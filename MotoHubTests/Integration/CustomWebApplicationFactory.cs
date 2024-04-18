using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotoHub.Data;

namespace MotoHubTests.Integration
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext configuration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add an in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestingDB");
                });
            });

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var integrationTestConfig = new Dictionary<string, string>
                {
                    {"JwtKey", "pnXhunyWll1LgERT86wXwMH5I6ieQC2M"}
                };

                configBuilder.Sources.Clear();

                configBuilder.AddInMemoryCollection(integrationTestConfig);
            });
        }
    }
}
