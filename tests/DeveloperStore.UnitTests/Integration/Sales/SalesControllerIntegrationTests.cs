using System.Net;
using System.Net.Http.Json;
using DeveloperStore.Application.Events;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Application.Sales.Commands.UpdateSale;
using DeveloperStore.Application.Sales.DTOs;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using DeveloperStore.Tests.Integration.Common;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Tests.Integration.Sales;

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

        // Assert — Event (filter by SaleId to isolate this test from the shared harness)
        var harness = _factory.GetTestHarness();
        (await harness.Published.Any<SaleCreated>(x => x.Context.Message.SaleId == body.Id)).Should().BeTrue();
        (await harness.Consumed.Any<SaleCreated>(x => x.Context.Message.SaleId == body.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Get_GetSaleById_ReturnsOkWithCorrectDataAndNoEventsAreConsumed()
    {
        // Arrange — seed directly via context, bypassing the API handler
        var seed = BuildSale("SALE-INT-002");
        var db = await _factory.GetDbContextAsync();
        await db.Sales.AddAsync(seed);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/sales/{seed.Id}");

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sale = await response.Content.ReadFromJsonAsync<SaleDto>();

        sale.Should().NotBeNull();
        sale!.Id.Should().Be(seed.Id);
        sale.SaleNumber.Should().Be("SALE-INT-002");
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(200m); // qty=2, price=100, discount=0 (qty<4)

        // Assert — Event (GET is read-only: no events should be published or consumed for this sale)
        var harness = _factory.GetTestHarness();
        harness.Consumed.Select<SaleCreated>(x => x.Context.Message.SaleId == seed.Id)
            .Should().BeEmpty();
        harness.Consumed.Select<SaleModified>(x => x.Context.Message.SaleId == seed.Id)
            .Should().BeEmpty();
        harness.Consumed.Select<SaleCancelled>(x => x.Context.Message.SaleId == seed.Id)
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Patch_UpdateSale_ReturnsOkDatabaseReflectsChangesAndSaleModifiedEventIsConsumed()
    {
        // Arrange — seed directly via context
        var seed = BuildSale("SALE-INT-003");
        var db = await _factory.GetDbContextAsync();
        await db.Sales.AddAsync(seed);
        await db.SaveChangesAsync();

        var updateCommand = BuildUpdateCommand(seed.Id, "SALE-INT-003-UPDATED");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/sales/{seed.Id}", updateCommand);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<SaleDto>();
        updated!.SaleNumber.Should().Be("SALE-INT-003-UPDATED");

        // Assert — Database
        var freshDb = await _factory.GetDbContextAsync();
        var sale = await freshDb.Sales.FirstOrDefaultAsync(s => s.Id == seed.Id);

        sale.Should().NotBeNull();
        sale!.SaleNumber.Should().Be("SALE-INT-003-UPDATED");
        sale.UpdatedAt.Should().NotBeNull();

        // Assert — Event (filter by SaleId to isolate this test from the shared harness)
        var harness = _factory.GetTestHarness();
        (await harness.Published.Any<SaleModified>(x => x.Context.Message.SaleId == seed.Id)).Should().BeTrue();
        (await harness.Consumed.Any<SaleModified>(x => x.Context.Message.SaleId == seed.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Delete_DeleteSale_ReturnsNoContentSaleIsRemovedFromDatabaseAndCancelledEventsAreConsumed()
    {
        // Arrange — seed directly via context
        var seed = BuildSale("SALE-INT-004");
        var db = await _factory.GetDbContextAsync();
        await db.Sales.AddAsync(seed);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{seed.Id}");

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert — Database
        var freshDb = await _factory.GetDbContextAsync();
        var sale = await freshDb.Sales.FirstOrDefaultAsync(s => s.Id == seed.Id);
        sale.Should().BeNull();

        // Assert — Events (filter by SaleId to distinguish between multiple events of the same type)
        var harness = _factory.GetTestHarness();
        (await harness.Published.Any<ItemCancelled>(x => x.Context.Message.SaleId == seed.Id)).Should().BeTrue();
        (await harness.Published.Any<SaleCancelled>(x => x.Context.Message.SaleId == seed.Id)).Should().BeTrue();
        (await harness.Consumed.Any<ItemCancelled>(x => x.Context.Message.SaleId == seed.Id)).Should().BeTrue();
        (await harness.Consumed.Any<SaleCancelled>(x => x.Context.Message.SaleId == seed.Id)).Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────

    private static Sale BuildSale(string saleNumber)
    {
        var sale = Sale.Create(
            saleNumber,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new ExternalIdentity(Guid.NewGuid(), "John Doe"),
            new ExternalIdentity(Guid.NewGuid(), "Main Branch"));

        sale.AddItem(
            new ExternalIdentity(Guid.NewGuid(), "Widget A"),
            quantity: 2,
            unitPrice: 100m,
            discount: 0m);

        return sale;
    }

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
