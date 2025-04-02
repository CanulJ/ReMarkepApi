using System;
using System.Collections.Generic;

namespace ReMarkepApi.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public string Categoria { get; set; } = null!;

    public string? ImagenUrl { get; set; }

    public DateOnly? FechaCreacion { get; set; }
}
