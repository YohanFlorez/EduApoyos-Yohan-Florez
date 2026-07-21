using EduApoyos.Application.Exceptions;
using EduApoyos.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Middleware;

/// <summary>
/// captura cualquier excepción no manejada y  para mapearlo en la clase ProblemDetails 
/// evitando exponer stack traces y centralizando el mapeo de excepciones a códigos HTTP.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Acceso no autorizado"),
            AuthException => (StatusCodes.Status400BadRequest, "Error de autenticación"),
            DomainException => (StatusCodes.Status409Conflict, "Regla de negocio violada"),
            ValidationException => (StatusCodes.Status400BadRequest, "Error de validación"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado procesando {Path}", context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception is ValidationException validationEx
                ? string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage))
                : exception.Message,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.io/{statusCode}"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
