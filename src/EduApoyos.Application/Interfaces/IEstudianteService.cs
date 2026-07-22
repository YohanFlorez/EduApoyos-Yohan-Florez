using EduApoyos.Application.Common;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.Interfaces
{
    public interface IEstudianteService
    {
        Task<EstudianteResponse> CrearAsync(CrearEstudianteRequest request, CancellationToken ct = default);
        Task<EstudianteResponse> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResponse<EstudianteResponse>> ListarAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<List<UsuarioPendienteResponse>> ObtenerPendientesAsync(string filtro, CancellationToken ct);

        Task<EstudianteResponse> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct);

        Task<EstudianteResponse> ActualizarAsync(Guid id, ActualizarEstudianteRequest request, CancellationToken ct);

        Task<List<EstudianteBusquedaResponse>> BuscarPorDocumentoAsync(string? filtro, CancellationToken ct = default);

        Task DesactivarAsync(Guid id, CancellationToken ct);

        Task ActivarAsync(Guid id, CancellationToken ct = default);

    }
}

    
