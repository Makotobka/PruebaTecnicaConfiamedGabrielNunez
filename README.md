# Prueba técnica Confiamed

Solución backend desarrollada en C# con .NET 8 y SQL Server para gestionar usuarios e ítems de trabajo.

El sistema está dividido en dos microservicios:

- `Usuarios.Api`: administración y consulta de usuarios.
- `Trabajos.Api`: administración, asignación y consulta de ítems de trabajo.

## Arquitectura

Cada microservicio utiliza una estructura sencilla por capas:

- `Controladores`: exponen las rutas HTTP.
- `Servicios`: contienen las reglas de negocio.
- `Repositorios`: administran el acceso a SQL Server.
- `Modelos`: representan los datos y las solicitudes.

Las dependencias se definen mediante interfaces y se registran en `Program.cs`. Los microservicios se comunican mediante HTTP.

## Requisitos

- Visual Studio 2022 o .NET SDK 8.
- SQL Server.
- Una base de datos llamada `PR_ItemsClientes`.

## Base de datos

Ejecutar `CrearBases.sql` sobre la base de datos `PR_ItemsClientes` para crear las tablas e índices requeridos.
Las cadenas de conexión se encuentran en los archivos `appsettings.Development.json` de cada proyecto y deben ajustarse según la instancia local de SQL Server.

## Ejecución

Abrir en Visual Studio y configurar ambos proyectos como proyectos de inicio, o ejecutarlos desde dos terminales:

Direcciones locales:

- API de usuarios: http://localhost:5102
- API de trabajos: http://localhost:5101

## Documentación de las API

Swagger permite consultar y probar las operaciones disponibles:

- Usuarios: http://localhost:5102/swagger
- Trabajos: http://localhost:5101/swagger

Los archivos `.http` incluidos en cada proyecto contienen solicitudes de ejemplo para probar las API desde Visual Studio.