using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Inventory.Api.Tests;

public class InventoryApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InventoryApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Should_Return_Healthy_Status()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal("Saludable", root.GetProperty("estado").GetString());
        Assert.Equal("Inventory.Api", root.GetProperty("servicio").GetString());
    }

    [Fact]
    public async Task Productos_Should_Return_Product_List()
    {
        var response = await _client.GetAsync("/api/productos");

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var products = document.RootElement;

        Assert.Equal(JsonValueKind.Array, products.ValueKind);
        Assert.True(products.GetArrayLength() >= 4);
        Assert.Equal("Sensor industrial", products[0].GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task Producto_By_Id_Should_Return_Product()
    {
        var response = await _client.GetAsync("/api/productos/1");

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var product = document.RootElement;

        Assert.Equal(1, product.GetProperty("id").GetInt32());
        Assert.Equal("Sensor industrial", product.GetProperty("nombre").GetString());
        Assert.Equal("SEN-001", product.GetProperty("sku").GetString());
    }

    [Fact]
    public async Task Producto_Not_Found_Should_Return_404()
    {
        var response = await _client.GetAsync("/api/productos/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal("Producto no encontrado.", root.GetProperty("mensaje").GetString());
    }

    [Fact]
    public async Task Stock_Resumen_Should_Return_Expected_Totals()
    {
        var response = await _client.GetAsync("/api/stock/resumen");

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var summary = document.RootElement;

        Assert.Equal(4, summary.GetProperty("totalProductos").GetInt32());
        Assert.Equal(3, summary.GetProperty("productosActivos").GetInt32());
        Assert.Equal(45, summary.GetProperty("unidadesTotales").GetInt32());
        Assert.Equal(1, summary.GetProperty("productosSinStock").GetInt32());
    }

    [Fact]
    public async Task Movimiento_Valido_Should_Return_201()
    {
        var request = new
        {
            productoId = 1,
            tipo = "ENTRADA",
            cantidad = 5,
            descripcion = "Ajuste de inventario de prueba"
        };

        var response = await _client.PostAsJsonAsync("/api/movimientos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var movement = document.RootElement;

        Assert.Equal(1, movement.GetProperty("productoId").GetInt32());
        Assert.Equal("ENTRADA", movement.GetProperty("tipo").GetString());
        Assert.Equal(5, movement.GetProperty("cantidad").GetInt32());
    }

    [Fact]
    public async Task Movimiento_Invalido_Should_Return_400()
    {
        var request = new
        {
            productoId = 0,
            tipo = "ENTRADA",
            cantidad = 5,
            descripcion = "Movimiento inválido"
        };

        var response = await _client.PostAsJsonAsync("/api/movimientos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal("ProductoId debe ser mayor que cero.", root.GetProperty("mensaje").GetString());
    }
}
