using System;
using System.Collections.Generic;

namespace ReMarkepApi.Models;

public partial class Usuario
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? TokenExpiration { get; set; }
}
