using Trabajos.Api.Errores;
using Trabajos.Api.Repositorios;
using Trabajos.Api.Servicios;
using Microsoft.OpenApi;

var constructor = WebApplication.CreateBuilder(args);

constructor.Services.AddControllers();
constructor.Services.AddProblemDetails();


constructor.Services.AddScoped<IRepositorioItemsTrabajo, RepositorioItemsTrabajo>();
constructor.Services.AddScoped<IServicioItemsTrabajo, ServicioItemsTrabajo>();
//Conexion directa a otro microservicio.
constructor.Services.AddHttpClient<IConsultaUsuarios, ConsultaUsuarios>(cliente =>
{
    cliente.BaseAddress = new Uri(constructor.Configuration["ServiciosExternos:Usuarios"]
        ?? throw new InvalidOperationException("Falta configurar la dirección del servicio de usuarios."));
    cliente.Timeout = TimeSpan.FromSeconds(10);
});

//Swagger.
constructor.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de trabajos",
        Version = "v1"
    });
});

var aplicacion = constructor.Build();

aplicacion.UseMiddleware<ManejadorErrores>();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "API de trabajos v1");
        opciones.DocumentTitle = "API de trabajos";
    });
}

aplicacion.MapControllers();

aplicacion.Run();
