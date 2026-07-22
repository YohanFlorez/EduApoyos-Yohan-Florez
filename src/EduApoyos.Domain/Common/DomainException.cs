namespace EduApoyos.Domain.Common;

/// <summary>
/// Excepción lanzada cuando se viola una regla de negocio del dominio.
/// Se traduce a un ProblemDetails 400/409 en la capa de API.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
