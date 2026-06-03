using DeveloperStore.Infrastructure.Messaging.Consumers;
using DeveloperStore.Infrastructure.Persistence;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace DeveloperStore.UnitTests.Integration.Common;

public class SalesWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("developerstore_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    async Task IAsyncLifetime.InitializeAsync() => await _postgres.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString()
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove the real MassTransit hosted service so it does not try to connect to RabbitMQ
            var massTransitHostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService) &&
                            d.ImplementationType?.Namespace?.StartsWith("MassTransit") == true)
                .ToList();

            foreach (var descriptor in massTransitHostedServices)
                services.Remove(descriptor);

            // Replace with in-memory test harness
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<SaleCreatedConsumer>();
                x.AddConsumer<SaleModifiedConsumer>();
                x.AddConsumer<SaleCancelledConsumer>();
                x.AddConsumer<ItemCancelledConsumer>();
            });
        });
    }

    public async Task<AppDbContext> GetDbContextAsync()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        return db;
    }

    public ITestHarness GetTestHarness() =>
        Services.GetRequiredService<ITestHarness>();

    async Task IAsyncLifetime.DisposeAsync() => await _postgres.DisposeAsync();
}
