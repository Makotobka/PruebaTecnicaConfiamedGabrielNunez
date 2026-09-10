# Requiere PowerShell 7, sqlcmd, SQL Server local y la solucion compilada.
# Crea dos bases temporales con nombres aleatorios y las elimina al terminar.
param(
    [string]$Servidor = 'localhost',
    [int]$PuertoUsuarios = 5212,
    [int]$PuertoTrabajos = 5211
)

$ErrorActionPreference = 'Stop'
$raizProyecto = Split-Path $PSScriptRoot -Parent
$sufijo = [Guid]::NewGuid().ToString('N')
$baseUsuarios = "ConfiamedPruebasUsuarios_$sufijo"
$baseTrabajos = "ConfiamedPruebasTrabajos_$sufijo"
$carpetaTemporal = Join-Path ([IO.Path]::GetTempPath()) "ConfiamedPruebas_$sufijo"
$basesCreadas = [Collections.Generic.List[string]]::new()
$procesos = [Collections.Generic.List[Diagnostics.Process]]::new()
$variables = @('ASPNETCORE_ENVIRONMENT', 'ConnectionStrings__BaseDatos', 'Servicios__Usuarios')
$valoresAnteriores = @{}
foreach ($variable in $variables) {
    $valoresAnteriores[$variable] = [Environment]::GetEnvironmentVariable($variable, 'Process')
}

function EjecutarSql([string]$Texto) {
    $archivo = Join-Path $carpetaTemporal 'consulta.sql'
    Set-Content -LiteralPath $archivo -Value $Texto -Encoding utf8
    & sqlcmd -S $Servidor -E -C -b -l 10 -i $archivo
    if ($LASTEXITCODE -ne 0) { throw 'Fallo al ejecutar SQL de prueba.' }
}

function Comprobar([bool]$Condicion, [string]$Mensaje) {
    if (-not $Condicion) { throw $Mensaje }
}

function Solicitar([string]$Metodo, [string]$Url, [int]$Codigo, $Datos = $null) {
    $parametros = @{
        Method = $Metodo
        Uri = $Url
        SkipHttpErrorCheck = $true
        TimeoutSec = 20
    }
    if ($null -ne $Datos) {
        $parametros.ContentType = 'application/json; charset=utf-8'
        $parametros.Body = $Datos | ConvertTo-Json -Depth 5
    }
    $respuesta = Invoke-WebRequest @parametros
    Comprobar ($respuesta.StatusCode -eq $Codigo) "$Metodo $Url devolvio $($respuesta.StatusCode), se esperaba $Codigo. $($respuesta.Content)"
    if ($respuesta.Content) { return $respuesta.Content | ConvertFrom-Json }
}

function IniciarApi([string]$Proyecto, [string]$BaseDatos, [int]$Puerto) {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ConnectionStrings__BaseDatos = "Server=$Servidor;Database=$BaseDatos;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
    $env:Servicios__Usuarios = "http://localhost:$PuertoUsuarios/"
    $archivo = Join-Path $raizProyecto "src/$Proyecto/bin/Debug/net8.0/$Proyecto.dll"
    $proceso = Start-Process -FilePath 'dotnet' -ArgumentList @('"' + $archivo + '"', '--urls', "http://localhost:$Puerto") -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $carpetaTemporal "$Proyecto.log") -RedirectStandardError (Join-Path $carpetaTemporal "$Proyecto.errores.log")
    $procesos.Add($proceso)
    for ($intento = 0; $intento -lt 40; $intento++) {
        if ($proceso.HasExited) { throw "No se pudo iniciar $Proyecto. Ver $carpetaTemporal" }
        try {
            $estado = Invoke-RestMethod "http://localhost:$Puerto/estado" -TimeoutSec 1
            if ($estado.estado -eq 'disponible') { return $proceso }
        } catch { Start-Sleep -Milliseconds 250 }
    }
    throw "No respondio $Proyecto."
}

try {
    foreach ($puerto in @($PuertoUsuarios, $PuertoTrabajos)) {
        $escucha = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $puerto)
        try { $escucha.Start() } finally { $escucha.Stop() }
    }
    New-Item -ItemType Directory -Path $carpetaTemporal | Out-Null
    foreach ($base in @($baseUsuarios, $baseTrabajos)) {
        EjecutarSql "CREATE DATABASE [$base];"
        $basesCreadas.Add($base)
    }
    $esquema = Get-Content (Join-Path $raizProyecto 'base-datos/CrearBases.sql') -Raw
    EjecutarSql ($esquema.Replace('USE ConfiamedUsuarios;', "USE [$baseUsuarios];").Replace('USE ConfiamedTrabajos;', "USE [$baseTrabajos];"))

    $procesoUsuarios = IniciarApi 'Usuarios.Api' $baseUsuarios $PuertoUsuarios
    $null = IniciarApi 'Trabajos.Api' $baseTrabajos $PuertoTrabajos
    $usuarios = "http://localhost:$PuertoUsuarios/api/usuarios"
    $trabajos = "http://localhost:$PuertoTrabajos/api/items-trabajo"
    foreach ($puerto in @($PuertoUsuarios, $PuertoTrabajos)) {
        $definicion = Solicitar GET "http://localhost:$puerto/swagger/v1/swagger.json" 200
        Comprobar ($null -ne $definicion.paths) 'Falta la documentacion Swagger.'
    }

    $null = Solicitar POST $usuarios 400 @{ nick = ' '; nombre = 'Ana' }
    $null = Solicitar POST $usuarios 400 @{ nick = ('a' * 101); nombre = 'Ana' }
    $usuario = Solicitar POST $usuarios 201 @{ nick = 'ana'; nombre = "Ana O'Connor" }
    Comprobar ([Guid]::Parse($usuario.id) -ne [Guid]::Empty) 'El usuario no tiene GUID.'
    $consultado = Solicitar GET "$usuarios/$($usuario.id)" 200
    Comprobar ($consultado.nombre -eq "Ana O'Connor") 'No se guardo correctamente el nombre.'
    $lista = @(Solicitar GET $usuarios 200)
    Comprobar ($lista.Count -eq 1) 'La lista de usuarios es incorrecta.'
    $null = Solicitar PUT "$usuarios/$($usuario.id)" 204 @{ nick = 'ana'; nombre = 'Ana actualizada' }
    $consultado = Solicitar GET "$usuarios/$($usuario.id)" 200
    Comprobar ($consultado.nombre -eq 'Ana actualizada') 'No se actualizo el usuario.'
    $existe = Solicitar GET "$usuarios/existe?nick=ana" 200
    Comprobar $existe 'No se encontro el nick.'
    $inyeccion = [Uri]::EscapeDataString("' OR 1=1 --")
    $existe = Solicitar GET "$usuarios/existe?nick=$inyeccion" 200
    Comprobar (-not $existe) 'La consulta de nick no esta parametrizada.'

    $datos = @{ titulo = 'Revisar solicitud'; fechaEntrega = '2026-09-12T10:00:00-05:00'; relevancia = 'Alta'; estado = 'Pendiente'; nombreUsuarioAsignado = 'ana' }
    $item = Solicitar POST $trabajos 201 $datos
    Comprobar ([Guid]::Parse($item.idItem) -ne [Guid]::Empty) 'El item no tiene GUID.'
    $consultado = Solicitar GET "$trabajos/$($item.idItem)" 200
    Comprobar ($null -ne $consultado.fechaAsignacion) 'Falta la fecha de asignacion.'
    Comprobar (([DateTimeOffset]$consultado.fechaEntrega).UtcDateTime.Hour -eq 15) 'La fecha no se convirtio a UTC.'
    $lista = @(Solicitar GET $trabajos 200)
    Comprobar ($lista.Count -eq 1) 'La lista de items es incorrecta.'
    $datos.estado = 'Completado'
    $null = Solicitar PUT "$trabajos/$($item.idItem)" 204 $datos
    $completado = Solicitar GET "$trabajos/$($item.idItem)" 200
    Comprobar ($completado.estado -eq 'Completado' -and $null -ne $completado.fechaCompletado) 'No se registro la finalizacion.'
    Comprobar ($completado.fechaCreacion -eq $consultado.fechaCreacion) 'La actualizacion cambio la fecha de creacion.'
    $datos.estado = 'Pendiente'
    $datos.nombreUsuarioAsignado = $null
    $null = Solicitar PUT "$trabajos/$($item.idItem)" 204 $datos
    $pendiente = Solicitar GET "$trabajos/$($item.idItem)" 200
    Comprobar ($null -eq $pendiente.fechaAsignacion -and $null -eq $pendiente.fechaCompletado) 'No se limpiaron las fechas al reabrir y desasignar.'

    $datos.estado = 'Completado'
    $null = Solicitar POST $trabajos 400 $datos
    $datos.estado = 'Pendiente'
    $datos.nombreUsuarioAsignado = 'inexistente'
    $null = Solicitar POST $trabajos 400 $datos
    $datos.nombreUsuarioAsignado = $null
    $datos.relevancia = 'Media'
    $null = Solicitar POST $trabajos 400 $datos
    $datos.relevancia = 'Alta'
    $datos.fechaEntrega = $null
    $null = Solicitar POST $trabajos 400 $datos
    $datos.fechaEntrega = '2026-09-12T15:00:00Z'

    $inexistente = [Guid]::NewGuid()
    $null = Solicitar GET "$usuarios/$inexistente" 404
    $null = Solicitar PUT "$usuarios/$inexistente" 404 @{ nick = 'otro'; nombre = 'Otro' }
    $null = Solicitar DELETE "$usuarios/$inexistente" 404
    $null = Solicitar GET "$trabajos/$inexistente" 404
    $null = Solicitar PUT "$trabajos/$inexistente" 404 $datos
    $null = Solicitar DELETE "$trabajos/$inexistente" 404
    $null = Solicitar DELETE "$trabajos/$($item.idItem)" 204
    $null = Solicitar GET "$trabajos/$($item.idItem)" 404
    $null = Solicitar DELETE "$usuarios/$($usuario.id)" 204
    $null = Solicitar GET "$usuarios/$($usuario.id)" 404

    Stop-Process -Id $procesoUsuarios.Id
    $procesoUsuarios.WaitForExit()
    $datos.nombreUsuarioAsignado = 'ana'
    $null = Solicitar POST $trabajos 503 $datos
    Write-Output 'CRUD verificado: persistencia SQL, GUID, validaciones, fechas UTC, usuario asignado, Swagger y respuestas 201/204/400/404/503.'
} finally {
    foreach ($proceso in $procesos) {
        if (-not $proceso.HasExited) { Stop-Process -Id $proceso.Id; $proceso.WaitForExit() }
    }
    foreach ($variable in $variables) {
        [Environment]::SetEnvironmentVariable($variable, $valoresAnteriores[$variable], 'Process')
    }
    foreach ($base in $basesCreadas) {
        if ($base -notmatch '^ConfiamedPruebas(Usuarios|Trabajos)_[0-9a-f]{32}$') {
            throw 'Se rechazo eliminar una base que no corresponde a las pruebas.'
        }
        EjecutarSql "ALTER DATABASE [$base] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$base];"
    }
    Write-Output "Registros de las pruebas: $carpetaTemporal"
}
