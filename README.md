# Prueba técnica Confiamed

Dos microservicios en C# y .NET 8 con operaciones de creación, consulta, actualización y eliminación sobre SQL Server. 
Requiere el SDK de .NET 8 y una versión de Visual Studio 2022 compatible con .NET 8.

## Proyectos

- `src/Trabajos.Api`: API de ítems de trabajo, puerto 5101.
- `src/Usuarios.Api`: API de usuarios, puerto 5102.

Cada proyecto contiene una estructura sencilla:

- `Controladores`: rutas HTTP y validación de las solicitudes.
- `Servicios`: lógica de negocio.
- `Repositorios`: acceso a los datos.
- `Modelos`: modelos de datos o objetos de clases.

Las capas se organizan en carpetas dentro de cada API. 
Los controladores dependen de interfaces de servicios y los servicios de interfaces de repositorios. 

## Configurar SQL Server

El script crea las tablas e índices; no migra tablas de versiones anteriores. 
Los GUID los genera SQL Server mediante `NEWSEQUENTIALID()` cuando se inserta un registro.
Cada proyecto incluye `appsettings.Development.json` con una conexión local de autenticación de Windows.

## Swagger

Cada microservicio tiene su propia página Swagger para consultar y probar sus rutas:

- Trabajos: http://localhost:5101/swagger
- Usuarios: http://localhost:5102/swagger

Swagger está habilitado en el entorno `Development`, configurado en los perfiles de ejecución. 