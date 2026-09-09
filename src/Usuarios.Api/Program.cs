using Microsoft.OpenApi;

var constructor = WebApplication.CreateBuilder(args);

constructor.Services.AddControllers();
constructor.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de usuarios",
        Version = "v1"
    });
});

var aplicacion = constructor.Build();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "API de usuarios v1");
        opciones.DocumentTitle = "API de usuarios";
    });
}

aplicacion.MapControllers();

aplicacion.Run();
