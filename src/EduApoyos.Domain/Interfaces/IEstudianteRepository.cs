using EduApoyos.Domain.Common;
using EduApoyos.Domain.Entities;

namespace EduApoyos.Domain.Interfaces;

public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Estudiante?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Estudiante?> ObtenerPorNumeroDocumentoAsync(string numeroDocumento, CancellationToken ct = default);
    Task<PagedResult<Estudiante>> ListarAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default);
    Task<List<Guid>> ObtenerTodosLosUsuarioIdsAsync(CancellationToken ct = default);
    Task<List<Estudiante>> BuscarPorDocumentoAsync(string filtro, CancellationToken ct);
    void Eliminar(Estudiante estudiante);

}
