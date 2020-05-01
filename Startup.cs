using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecksExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<MyDbContext>(o => o.UseSqlServer(Configuration["ConnectionString"]));

            services.AddHealthChecks()
                .AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024))
                .AddProcessAllocatedMemoryHealthCheck(512)
                .AddProcessHealthCheck("ProcessName", p => p.Length > 0)
                .AddWindowsServiceHealthCheck("someservice", s => true)
                .AddUrlGroup(new Uri("https://localhost:44318/weatherforecast"), "Example endpoint")
                .AddSqlServer(Configuration["ConnectionString"]);

            services
                .AddHealthChecksUI(s =>
                {
                    s.AddHealthCheckEndpoint("endpoint1", "https://localhost:44318/health");
                })
            .AddSqliteStorage("Data Source = healthchecks.db");
            //.AddInMemoryStorage();

            services.AddScoped<IMyCustomService, MyCustomService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecksUI();

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }

    public class MyCustomCheck : IHealthCheck
    {
        private readonly IMyCustomService _customService;

        public MyCustomCheck(IMyCustomService customService)
        {
            _customService = customService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var result = _customService.IsHealthy() ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
            return Task.FromResult(result);
        }

    }

    public interface IMyCustomService
    {

        public bool IsHealthy();

    }

    public class MyCustomService : IMyCustomService
    {

        public bool IsHealthy()
        {
            return new Random().NextDouble() > 0.5;
        }

    }
}
