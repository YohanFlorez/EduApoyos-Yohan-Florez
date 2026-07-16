using EduApoyos.Application.Common;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.DTOs.Estudiantes.Response;
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
    }
}
