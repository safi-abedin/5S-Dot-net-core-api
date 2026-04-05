using api.Data;
using api.Models.Users;
using api.Repositories.Base;
using api.Repositories.Interfaces.Base;
using api.Services.Base.Auth;
using api.Services.Base.Checklists;
using api.Services.Base.Companies;
using api.Services.Base.Logs;
using api.Services.Base.Users;
using api.Services.Base.Zones;
using api.Services.Interfaces.Auth;
using api.Services.Interfaces.Checklists;
using api.Services.Interfaces.Companies;
using api.Services.Interfaces.Logs;
using api.Services.Interfaces.Users;
using api.Services.Interfaces.Zones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace api.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
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

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IZoneService, ZoneService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IChecklistService, ChecklistService>();
            services.AddScoped<IChecklistCategoryService, ChecklistCategoryService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            return services;
        }
    }
}
