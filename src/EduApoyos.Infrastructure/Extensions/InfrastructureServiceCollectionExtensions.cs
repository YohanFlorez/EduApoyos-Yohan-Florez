using System.Text;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Domain.Services;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Persistence;
using EduApoyos.Infrastructure.Repositories;
using EduApoyos.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EduApoyos.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EduApoyosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<EduApoyosDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("La sección 'Jwt' no está configurada en appsettings.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddHttpContextAccessor();

        // repositorios / Unit of Work (patrón Repository)
        services.AddScoped<IEstudianteRepository, EstudianteRepository>();
        services.AddScoped<ISolicitudRepository, SolicitudRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // estrategia de transición de estados (patrón Strategy)
        services.AddScoped<IEstadoSolicitudStrategy, FlujoEstandarEstadoSolicitudStrategy>();

        // servicios de infraestructura
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioLookupService, UsuarioLookupService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
