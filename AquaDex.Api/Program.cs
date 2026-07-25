using System.Threading.RateLimiting;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace AquaDex.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, config) =>
            {
                config
                    .WriteTo.Console()
                    .WriteTo.File("Logs/aquadex-.log", rollingInterval: RollingInterval.Day)
                    .Enrich.FromLogContext();
            });



            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<AquaDexDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AquaDexDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please slow down.", token);
                };

                options.AddPolicy("auth", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer();

            builder.Services.AddSignalR();
            builder.Services.AddScoped<PointsService>();
            builder.Services.AddHostedService<ReminderBackgroundService>();
            builder.Services.AddScoped<BadgeService>();
            builder.Services.AddScoped<AuditService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<FishIdSuggestionService>();
            builder.Services.AddScoped<BiteAlertCleanupJob>();

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            using (var scope = app.Services.CreateScope())
            {
                await AquaDex.Infrastructure.Seed.RoleSeeder.SeedRolesAsync(scope.ServiceProvider);

                var dbContext = scope.ServiceProvider.GetRequiredService<AquaDexDbContext>();
                await AquaDex.Infrastructure.Seed.ForumCategorySeeder.SeedCategoriesAsync(dbContext);
            }



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
            app.UseRateLimiter();


            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAdminAuthFilter() }
            });

            RecurringJob.AddOrUpdate<BiteAlertCleanupJob>(
            "cleanup-expired-bite-alerts",
            job => job.RunAsync(),
            Cron.Hourly
            );

            app.MapControllers();
            app.MapHub<AquaDex.Api.Hubs.ForumHub>("/hubs/forum");



            app.Run();
        }
    }

}
