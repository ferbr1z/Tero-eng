using MbyteWeb.Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Backend.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:16-alpine").Build();
    public string ConnectionString => container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options;
        await using var context = new AppDbContext(options, new HttpContextAccessor());
        await context.Database.MigrateAsync();
        _ = Services;
    }

    public new async Task DisposeAsync()
    {
        await container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services.Where(item => item.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || item.ServiceType == typeof(DbContextOptions)
                         || item.ServiceType == typeof(AppDbContext)).ToList())
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(ConnectionString));
        });
    }
}

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "Integration";
}
