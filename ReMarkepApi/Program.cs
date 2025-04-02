using Microsoft.EntityFrameworkCore;
using ReMarkepApi.Models;

// Construcci�n de la aplicaci�n
var builder = WebApplication.CreateBuilder(args);

// Configuraci�n del servicio de env�o de correo
var smtpHost = builder.Configuration["EmailSettings:SmtpHost"]; // Ejemplo: smtp.gmail.com
var smtpPort = int.Parse(builder.Configuration["EmailSettings:SmtpPort"]); // Ejemplo: 587
var fromEmail = builder.Configuration["EmailSettings:FromEmail"];
var fromPassword = builder.Configuration["EmailSettings:FromPassword"];


// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ReMarketContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton(new EmailService(smtpHost, smtpPort, fromEmail, fromPassword)); // Agregamos el servicio de correo

var app = builder.Build();

// Configuraci�n de la canalizaci�n de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("NuevaPolitica");

app.UseAuthorization(); // Si usas autenticaci�n, d�jalo activado

app.MapControllers();

app.Run();
