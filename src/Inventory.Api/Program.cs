using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Demo Técnica API de Inventario",
        Version = "v1",
        Description = "API REST de demostración para operaciones de inventario, stock y movimientos construida con .NET."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo Técnica API de Inventario v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapGet("/health", () => Results.Ok(new
{
    estado = "Saludable",
    servicio = "Inventory.Api",
    fechaHoraUtc = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Salud");

var products = new List<ProductDto>
{
    new(1, "Sensor industrial", "SEN-001", "Electrónica", 25, true),
    new(2, "Válvula hidráulica", "VAL-204", "Mecánica", 12, true),
    new(3, "Módulo de panel de control", "MPC-778", "Automatización", 8, true),
    new(4, "Relevador de seguridad", "REL-440", "Eléctrico", 0, false)
};

var movements = new List<MovementDto>
{
    new(1, 1, "ENTRADA", 10, "Carga inicial de inventario", DateTime.UtcNow.AddDays(-4)),
    new(2, 2, "SALIDA", 2, "Despacho para mantenimiento", DateTime.UtcNow.AddDays(-2)),
    new(3, 3, "ENTRADA", 5, "Ajuste de almacén", DateTime.UtcNow.AddDays(-1))
};

app.MapGet("/api/productos", () => Results.Ok(products))
    .WithName("ObtenerProductos")
    .WithTags("Productos");

app.MapGet("/api/productos/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(item => item.Id == id);
    return product is null
        ? Results.NotFound(new { mensaje = "Producto no encontrado." })
        : Results.Ok(product);
})
.WithName("ObtenerProductoPorId")
.WithTags("Productos");

app.MapGet("/api/stock/resumen", () =>
{
    var totalProducts = products.Count;
    var activeProducts = products.Count(item => item.Activo);
    var totalUnits = products.Sum(item => item.CantidadStock);
    var outOfStock = products.Count(item => item.CantidadStock <= 0);

    return Results.Ok(new StockSummaryDto(totalProducts, activeProducts, totalUnits, outOfStock));
})
.WithName("ObtenerResumenStock")
.WithTags("Stock");

app.MapPost("/api/movimientos", (CreateMovementRequest request) =>
{
    if (request.ProductoId <= 0)
    {
        return Results.BadRequest(new { mensaje = "ProductoId debe ser mayor que cero." });
    }

    if (request.Cantidad <= 0)
    {
        return Results.BadRequest(new { mensaje = "Cantidad debe ser mayor que cero." });
    }

    if (request.Tipo is not ("ENTRADA" or "SALIDA"))
    {
        return Results.BadRequest(new { mensaje = "Tipo debe ser ENTRADA o SALIDA." });
    }

    var product = products.FirstOrDefault(item => item.Id == request.ProductoId);

    if (product is null)
    {
        return Results.NotFound(new { mensaje = "Producto no encontrado." });
    }

    var movement = new MovementDto(
        movements.Count + 1,
        request.ProductoId,
        request.Tipo,
        request.Cantidad,
        request.Descripcion,
        DateTime.UtcNow
    );

    movements.Add(movement);

    return Results.Created($"/api/movimientos/{movement.Id}", movement);
})
.WithName("CrearMovimiento")
.WithTags("Movimientos");

app.Run();

public record ProductDto(
    int Id,
    string Nombre,
    string Sku,
    string Categoria,
    int CantidadStock,
    bool Activo
);

public record StockSummaryDto(
    int TotalProductos,
    int ProductosActivos,
    int UnidadesTotales,
    int ProductosSinStock
);

public record MovementDto(
    int Id,
    int ProductoId,
    string Tipo,
    int Cantidad,
    string Descripcion,
    DateTime FechaCreacionUtc
);

public record CreateMovementRequest(
    int ProductoId,
    string Tipo,
    int Cantidad,
    string Descripcion
);

public partial class Program { }

