using Microsoft.OpenApi;

var constructor = WebApplication.CreateBuilder(args);

constructor.Services.AddControllers();
constructor.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de trabajos",
        Version = "v1"
    });
});

var aplicacion = constructor.Build();

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
