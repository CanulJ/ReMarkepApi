using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReMarkepApi.Models;
using BCrypt.Net;

namespace ReMarkepApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ReMarketContext _context;
        private readonly EmailService _emailService; // Agregamos el servicio de correo

        public UsuariosController(ReMarketContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService; // Inicializamos el servicio de correo
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.UserId)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash); // Hash password before saving
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.UserId }, usuario);
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UserId == id);
        }

        // POST: api/Usuarios/verificarUsuario
        [HttpPost("verificarUsuario")]
        public async Task<IActionResult> VerificarUsuario([FromBody] UsuarioDTO credenciales)
        {
            if (credenciales == null || string.IsNullOrWhiteSpace(credenciales.Username) || string.IsNullOrWhiteSpace(credenciales.PasswordHash))
            {
                return BadRequest(new { mensaje = "Credenciales inválidas." });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == credenciales.Username);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(credenciales.PasswordHash, usuario.PasswordHash))
            {
                return Unauthorized(new { autenticado = false, mensaje = "Usuario o contraseña incorrectos." });
            }

            // Incluir el correo en la respuesta
            return Ok(new
            {
                autenticado = true,
                mensaje = "Inicio de sesión exitoso.",
                email = usuario.Email // Añadimos el correo aquí
            });
        }


        [HttpPost("solicitarRestablecimientoContraseña")]
        public async Task<IActionResult> SolicitarRestablecimientoContraseña([FromBody] ForgotPasswordRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
            {
                return BadRequest(new { mensaje = "No se encontró ningún usuario con ese correo electrónico." });
            }

            // Genera un token único para el restablecimiento de la contraseña
            var resetToken = Guid.NewGuid().ToString();
            usuario.PasswordResetToken = resetToken;
            usuario.TokenExpiration = DateTime.UtcNow.AddHours(1); // El token vence en 1 hora

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // Enviar el token directamente en el correo
            await _emailService.SendEmailAsync(usuario.Email, "Restablecimiento de Contraseña",
                $"Tu token de restablecimiento de contraseña es: {resetToken}. Este token expira en 1 hora.");

            return Ok(new { mensaje = "El token de restablecimiento de contraseña ha sido enviado a tu correo." });
        }


        // POST: api/Usuarios/restablecerContraseña
        [HttpPost("restablecerContraseña")]
        public async Task<IActionResult> RestablecerContraseña([FromBody] ResetPasswordRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token && u.TokenExpiration > DateTime.UtcNow);

            if (usuario == null)
            {
                return BadRequest(new { mensaje = "Token inválido o ha expirado." });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { mensaje = "Las contraseñas no coinciden." });
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            usuario.PasswordResetToken = null;
            usuario.TokenExpiration = null;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña restablecida exitosamente." });
        }

        // DTO para las credenciales de inicio de sesión
        public class UsuarioDTO
        {
            public required string Username { get; set; }
            public required string PasswordHash { get; set; }
        }

        // DTO para la solicitud de restablecimiento de contraseña
        public class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }

        // DTO para el restablecimiento de la contraseña
        public class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        [HttpPost("enviarCorreoContacto")]
        public async Task<IActionResult> EnviarCorreoContacto([FromBody] ContactoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return BadRequest(new { mensaje = "Todos los campos son obligatorios." });
            }

            // Asunto y cuerpo del correo
            string subject = "Nuevo mensaje de contacto";
            string body = $"Nombre: {request.Nombre}\n" +
                          $"Correo: {request.Email}\n" +
                          $"Mensaje:\n{request.Mensaje}";

            await _emailService.SendEmailAsync("jesus34canul@gmail.com", subject, body);

            return Ok(new { mensaje = "El mensaje ha sido enviado exitosamente." });
        }

        // DTO para recibir la solicitud del frontend
        public class ContactoRequest
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string Mensaje { get; set; }
        }

        [HttpPost("enviarCorreoBuzon")]
        public async Task<IActionResult> EnviarCorreoBuzon([FromBody] BuzonRequest request)
        {
            // Validación de campos
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Mensaje) ||
                string.IsNullOrWhiteSpace(request.Tipo))
            {
                return BadRequest(new { mensaje = "Todos los campos son obligatorios." });
            }

            // Asunto y cuerpo del correo
            string subject = $"Nuevo mensaje de {request.Tipo} en el Buzón";
            string body = $"Nombre: {request.Nombre}\n" +
                          $"Correo: {request.Email}\n" +
                          $"Tipo de mensaje: {request.Tipo}\n" +
                          $"Mensaje:\n{request.Mensaje}";

            // Enviar correo
            await _emailService.SendEmailAsync("jesus34canul@gmail.com", subject, body);

            return Ok(new { mensaje = "El mensaje ha sido enviado exitosamente." });
        }
    }

    // DTO para recibir la solicitud del frontend
    public class BuzonRequest
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }  // "Queja" o "Sugerencia"
    }

}

