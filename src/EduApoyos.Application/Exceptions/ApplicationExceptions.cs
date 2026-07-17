namespace EduApoyos.Application.Exceptions;

/// Se dispara cuando un recurso solicitado no existe. Se traduce a HTTP 404
public class NotFoundException : Exception
{
    public NotFoundException(string entidad, object id)
        : base($"{entidad} con id '{id}' no fue encontrado(a).")
    {
    }
}

/// Se lanza cuando el usuario autenticado no tiene permiso sobre el recurso. Se traduce a HTTP 403
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string mensaje = "No tiene permisos para acceder a este recurso.")
        : base(mensaje)
    {
    }
}

/// Se dispara en fallos de autenticación (credenciales inválidas, usuario duplicado). Se traduce a HTTP 400/401
public class AuthException : Exception
{
    public AuthException(string mensaje) : base(mensaje)
    {
    }
}
