using System.Net;
using System.Net.Http.Json;
using DeveloperStore.Application.Events;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Application.Sales.Commands.UpdateSale;
using DeveloperStore.Application.Sales.DTOs;
using DeveloperStore.UnitTests.Integration.Common;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.UnitTests.Integration.Sales;

public class SalesControllerIntegrationTests : IClassFixture<SalesWebApplicationFactory>
{
    private readonly SalesWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SalesControllerIntegrationTests(SalesWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreateSale_ReturnsCreatedSaleIsSavedInDatabaseAndSaleCreatedEventIsConsumed()
    {
        // Arrange
        var command = BuildCreateCommand("SALE-INT-001");

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", command);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateSaleResponse>();
        body!.Id.Should().NotBe(Guid.Empty);

        // Assert — Database
        var db = await _factory.GetDbContextAsync();
        var sale = await db.Sales.Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == body.Id);

        sale.Should().NotBeNull();
        sale!.SaleNumber.Should().Be("SALE-INT-001");
        sale.Items.Should().HaveCount(1);
        sale.Items[0].Quantity.Should().Be(2);
        sale.Items[0].UnitPrice.Should().Be(100m);

        // Assert — Event
        var harness = _factory.GetTestHarness();
        (await harness.Published.Any<SaleCreated>()).Should().BeTrue();
        (await harness.Consumed.Any<SaleCreated>()).Should().BeTrue();
    }

    [Fact]
    public async Task Get_GetSaleById_ReturnsOkWithCorrectData()
    {
        // Arrange — create a sale first via the API
        var created = await _client.PostAsJsonAsync("/api/sales", BuildCreateCommand("SALE-INT-002"));
        var body = await created.Content.ReadFromJsonAsync<CreateSaleResponse>();

        // Act
        var response = await _client.GetAsync($"/api/sales/{body!.Id}");

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sale = await response.Content.ReadFromJsonAsync<SaleDto>();

        sale.Should().NotBeNull();
        sale!.Id.Should().Be(body.Id);
        sale.SaleNumber.Should().Be("SALE-INT-002");
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(200m); // qty=2, price=100, discount=0 (qty<4)
    }

    [Fact]
    public async Task Patch_UpdateSale_ReturnsOkDatabaseReflectsChangesAndSaleModifiedEventIsConsumed()
    {
        // Arrange — create then update
        var created = await _client.PostAsJsonAsync("/api/sales", BuildCreateCommand("SALE-INT-003"));
        var body = await created.Content.ReadFromJsonAsync<CreateSaleResponse>();

        var updateCommand = BuildUpdateCommand(body!.Id, "SALE-INT-003-UPDATED");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/sales/{body.Id}", updateCommand);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<SaleDto>();
        updated!.SaleNumber.Should().Be("SALE-INT-003-UPDATED");

        // Assert — Database
        var db = await _factory.GetDbContextAsync();
        var sale = await db.Sales.FirstOrDefaultAsync(s => s.Id == body.Id);

        sale.Should().NotBeNull();
        sale!.SaleNumber.Should().Be("SALE-INT-003-UPDATED");
        sale.UpdatedAt.Should().NotBeNull();

        // Assert — Event
        var harness = _factory.GetTestHarness();
        (await harness.Published.Any<SaleModified>()).Should().BeTrue();
        (await harness.Consumed.Any<SaleModified>()).Should().BeTrue();
    }

    [Fact]
    public async Task Delete_DeleteSale_ReturnsNoContentSaleIsRemovedFromDatabaseAndSaleCancelledEventIsConsumed()
    {
        // Arrange — create a sale to delete
        var created = await _client.PostAsJsonAsync("/api/sales", BuildCreateCommand("SALE-INT-004"));
        var body = await created.Content.ReadFromJsonAsync<CreateSaleResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{body!.Id}");

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert — Database
        var db = await _factory.GetDbContextAsync();
        var sale = await db.Sales.FirstOrDefaultAsync(s => s.Id == body.Id);
        sale.Should().BeNull();

        // Assert — Events (ItemCancelled per item + SaleCancelled)
        var harness = _factory.GetTestHarness();
        (await harness.Published.Any<ItemCancelled>()).Should().BeTrue();
        (await harness.Published.Any<SaleCancelled>()).Should().BeTrue();
        (await harness.Consumed.Any<ItemCancelled>()).Should().BeTrue();
        (await harness.Consumed.Any<SaleCancelled>()).Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────

    private static CreateSaleCommand BuildCreateCommand(string saleNumber) =>
        new(
            SaleNumber: saleNumber,
            SaleDate: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CustomerId: Guid.NewGuid(),
            CustomerName: "John Doe",
            BranchId: Guid.NewGuid(),
            BranchName: "Main Branch",
            Items:
            [
                new CreateSaleItemDto(
                    ProductId: Guid.NewGuid(),
                    ProductName: "Widget A",
                    Quantity: 2,
                    UnitPrice: 100m,
                    Discount: 0m)
            ]);

    private static UpdateSaleCommand BuildUpdateCommand(Guid id, string saleNumber) =>
        new(
            Id: id,
            SaleNumber: saleNumber,
            SaleDate: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            CustomerId: Guid.NewGuid(),
            CustomerName: "Jane Doe",
            BranchId: Guid.NewGuid(),
            BranchName: "Updated Branch",
            Items:
            [
                new CreateSaleItemDto(
                    ProductId: Guid.NewGuid(),
                    ProductName: "Widget B",
                    Quantity: 5,
                    UnitPrice: 50m,
                    Discount: 0m)
            ]);

    private sealed record CreateSaleResponse(Guid Id);
}
