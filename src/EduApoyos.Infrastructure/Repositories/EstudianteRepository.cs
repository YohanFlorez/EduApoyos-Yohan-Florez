using EduApoyos.Domain.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

public class EstudianteRepository : IEstudianteRepository
{
    private readonly EduApoyosDbContext _context;

    public EstudianteRepository(EduApoyosDbContext context)
    {
        _context = context;
    }

    public async Task<Estudiante?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Estudiantes.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Estudiante?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default) =>
        await _context.Estudiantes.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId, ct);

    public async Task<Estudiante?> ObtenerPorNumeroDocumentoAsync(string numeroDocumento, CancellationToken ct = default) =>
        await _context.Estudiantes.FirstOrDefaultAsync(e => e.NumeroDocumento == numeroDocumento, ct);

    public async Task<PagedResult<Estudiante>> ListarAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Estudiantes.AsNoTracking().OrderBy(e => e.ProgramaAcademico).ThenBy(e => e.NumeroDocumento);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<Estudiante>(items, total, pageNumber, pageSize);
    }

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default) =>
        await _context.Estudiantes.AddAsync(estudiante, ct);


    public async Task<List<Guid>> ObtenerTodosLosUsuarioIdsAsync(CancellationToken ct = default)
    {

        return await _context.Estudiantes
       .Where(e => !e.UsuarioId.HasValue)
       .Select(e => e.Id)
       .ToListAsync(ct);

        //return await _context.Estudiantes
        //    .Where(e => e.UsuarioId!.HasValue)
        //    .Select(e => e.UsuarioId!.Value)
        //    .ToListAsync(ct);
    }

    public void Eliminar(Estudiante estudiante)
    {
        _context.Estudiantes.Remove(estudiante);
    }

}
