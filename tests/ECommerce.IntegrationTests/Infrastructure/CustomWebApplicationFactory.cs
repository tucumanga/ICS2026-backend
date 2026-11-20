using System.Data.Common;
using Dsw2025Tpi.Api;
using Dsw2025Tpi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DB_CONNECTION_STRING", "Data Source=InMemoryDb;Mode=Memory;Cache=Shared");

        builder.ConfigureServices(services =>
        {
            // 1. Crear y mantener abierta la conexión SQLite en memoria compartida
            _connection = new SqliteConnection("Data Source=InMemoryDb;Mode=Memory;Cache=Shared");
            _connection.Open();

            // 2. Registrar los DbContexts usando SQLite para el entorno de pruebas
            services.AddDbContext<AuthenticateContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddDbContext<Dsw2025TpiContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
