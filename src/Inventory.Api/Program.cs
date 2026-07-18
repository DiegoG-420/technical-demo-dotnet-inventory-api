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
    var activeProducts = products.Count(item => item.IsActive);
    var totalUnits = products.Sum(item => item.StockQuantity);
    var outOfStock = products.Count(item => item.StockQuantity <= 0);

    return Results.Ok(new StockSummaryDto(totalProducts, activeProducts, totalUnits, outOfStock));
})
.WithName("ObtenerResumenStock")
.WithTags("Stock");

app.MapPost("/api/movimientos", (CreateMovementRequest request) =>
{
    if (request.ProductId <= 0)
    {
        return Results.BadRequest(new { mensaje = "ProductId debe ser mayor que cero." });
    }

    if (request.Quantity <= 0)
    {
        return Results.BadRequest(new { mensaje = "Quantity debe ser mayor que cero." });
    }

    if (request.Type is not ("ENTRADA" or "SALIDA"))
    {
        return Results.BadRequest(new { mensaje = "Type debe ser ENTRADA o SALIDA." });
    }

    var product = products.FirstOrDefault(item => item.Id == request.ProductId);

    if (product is null)
    {
        return Results.NotFound(new { mensaje = "Producto no encontrado." });
    }

    var movement = new MovementDto(
        movements.Count + 1,
        request.ProductId,
        request.Type,
        request.Quantity,
        request.Description,
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
    string Name,
    string Sku,
    string Category,
    int StockQuantity,
    bool IsActive
);

public record StockSummaryDto(
    int TotalProducts,
    int ActiveProducts,
    int TotalUnits,
    int OutOfStockProducts
);

public record MovementDto(
    int Id,
    int ProductId,
    string Type,
    int Quantity,
    string Description,
    DateTime CreatedAtUtc
);

public record CreateMovementRequest(
    int ProductId,
    string Type,
    int Quantity,
    string Description
);
