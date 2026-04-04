using api.Data;
using api.Models.Users;
using api.Repositories.Base;
using api.Repositories.Interfaces.Base;
using api.Services.Base.Auth;
using api.Services.Base.Logs;
using api.Services.Base.Users;
using api.Services.Base.Zones;
using api.Services.Interfaces.Logs;
using api.Services.Interfaces.Users;
using api.Services.Interfaces.Zones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;


namespace api.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            // DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // JWT
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            // Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Services (REGISTER ALL HERE)
            services.AddScoped<IZoneService, ZoneService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<AuthService>();

            // Http Context
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Health Check
            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            return services;
        }
    }
}
