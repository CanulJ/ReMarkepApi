using Microsoft.EntityFrameworkCore;
using ReMarkepApi.Models;

// Construcción de la aplicación
var builder = WebApplication.CreateBuilder(args);

// Configuración del servicio de envío de correo
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

// Configuración de la canalización de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("NuevaPolitica");

app.UseAuthorization(); // Si usas autenticación, déjalo activado

app.MapControllers();

app.Run();
