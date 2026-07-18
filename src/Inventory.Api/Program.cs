using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Technical Demo Inventory API",
        Version = "v1",
        Description = "Demo REST API for inventory, stock and movement operations built with .NET."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Technical Demo Inventory API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Inventory.Api",
    timestampUtc = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");

var products = new List<ProductDto>
{
    new(1, "Industrial Sensor", "SEN-001", "Electronics", 25, true),
    new(2, "Hydraulic Valve", "VAL-204", "Mechanical", 12, true),
    new(3, "Control Panel Module", "CPM-778", "Automation", 8, true),
    new(4, "Safety Relay", "SR-440", "Electrical", 0, false)
};

var movements = new List<MovementDto>
{
    new(1, 1, "IN", 10, "Initial stock load", DateTime.UtcNow.AddDays(-4)),
    new(2, 2, "OUT", 2, "Maintenance dispatch", DateTime.UtcNow.AddDays(-2)),
    new(3, 3, "IN", 5, "Warehouse adjustment", DateTime.UtcNow.AddDays(-1))
};

app.MapGet("/api/products", () => Results.Ok(products))
    .WithName("GetProducts")
    .WithTags("Products");

app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(item => item.Id == id);
    return product is null ? Results.NotFound(new { message = "Product not found." }) : Results.Ok(product);
})
.WithName("GetProductById")
.WithTags("Products");

app.MapGet("/api/stock/summary", () =>
{
    var totalProducts = products.Count;
    var activeProducts = products.Count(item => item.IsActive);
    var totalUnits = products.Sum(item => item.StockQuantity);
    var outOfStock = products.Count(item => item.StockQuantity <= 0);

    return Results.Ok(new StockSummaryDto(totalProducts, activeProducts, totalUnits, outOfStock));
})
.WithName("GetStockSummary")
.WithTags("Stock");

app.MapPost("/api/movements", (CreateMovementRequest request) =>
{
    if (request.ProductId <= 0)
    {
        return Results.BadRequest(new { message = "ProductId must be greater than zero." });
    }

    if (request.Quantity <= 0)
    {
        return Results.BadRequest(new { message = "Quantity must be greater than zero." });
    }

    if (request.Type is not ("IN" or "OUT"))
    {
        return Results.BadRequest(new { message = "Type must be IN or OUT." });
    }

    var product = products.FirstOrDefault(item => item.Id == request.ProductId);

    if (product is null)
    {
        return Results.NotFound(new { message = "Product not found." });
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

    return Results.Created($"/api/movements/{movement.Id}", movement);
})
.WithName("CreateMovement")
.WithTags("Movements");

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
