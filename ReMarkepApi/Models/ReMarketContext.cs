using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ReMarkepApi.Models;

public partial class ReMarketContext : DbContext
{
    public ReMarketContext()
    {
    }

    public ReMarketContext(DbContextOptions<ReMarketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__Producto__69E6E154B15507A9");

            entity.Property(e => e.ProductoId).HasColumnName("productoId");
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasColumnName("categoria");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.ImagenUrl).HasColumnName("imagenURL");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Usuarios__CB9A1CFF174F14DE");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__AB6E61642D7F55A2").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Usuarios__F3DBC572754393CB").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.TokenExpiration).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
