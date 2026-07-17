using System.Reflection;
using EduApoyos.Application.Interfaces;
using EduApoyos.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EduApoyos.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEstudianteService, EstudianteService>();
        services.AddScoped<ISolicitudService, SolicitudService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
