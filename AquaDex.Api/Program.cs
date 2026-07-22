using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace AquaDex.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<AquaDexDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Keep password rules reasonable for a capstone demo — not enterprise-grade, but not trivially weak either
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AquaDexDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddSignalR();
            builder.Services.AddScoped<PointsService>();
            builder.Services.AddHostedService<ReminderBackgroundService>();
            builder.Services.AddScoped<BadgeService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                await AquaDex.Infrastructure.Seed.RoleSeeder.SeedRolesAsync(scope.ServiceProvider);

                var dbContext = scope.ServiceProvider.GetRequiredService<AquaDexDbContext>();
                await AquaDex.Infrastructure.Seed.ForumCategorySeeder.SeedCategoriesAsync(dbContext);
            }



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "AquaDex API v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapHub<AquaDex.Api.Hubs.ForumHub>("/hubs/forum");



            app.Run();
        }
    }

    public class ReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReminderBackgroundService> _logger;

        public ReminderBackgroundService(ILogger<ReminderBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Your recurring task logic here
                    _logger.LogInformation("ReminderBackgroundService is running.");

                    // For example, send reminders or process tasks
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in ReminderBackgroundService.");
                }

                // Wait for a specific interval before executing the task again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
