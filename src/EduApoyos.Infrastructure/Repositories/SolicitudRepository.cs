using EduApoyos.Domain.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EduApoyos.Infrastructure.Repositories;
public class SolicitudRepository : ISolicitudRepository
{
    private readonly EduApoyosDbContext _context;
    public SolicitudRepository(EduApoyosDbContext context)
    {
        _context = context;
    }
    public async Task<SolicitudApoyo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == id, ct);
    public async Task<SolicitudApoyo?> ObtenerPorIdConHistorialAsync(Guid id, CancellationToken ct = default) =>
        await _context.Solicitudes.Include(s => s.Historial).FirstOrDefaultAsync(s => s.Id == id, ct);
    public async Task<PagedResult<SolicitudApoyo>> ListarAsync(
        FiltroSolicitudes filtro, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Solicitudes.AsNoTracking().AsQueryable();
        if (filtro.Estado is not null)
            query = query.Where(s => s.Estado == filtro.Estado);
        if (filtro.TipoApoyo is not null)
            query = query.Where(s => s.TipoApoyo == filtro.TipoApoyo);
        if (filtro.FechaDesde is not null)
            query = query.Where(s => s.FechaSolicitud >= filtro.FechaDesde);
        if (filtro.FechaHasta is not null)
            query = query.Where(s => s.FechaSolicitud <= filtro.FechaHasta);
        query = query.OrderByDescending(s => s.FechaSolicitud);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<SolicitudApoyo>(items, total, pageNumber, pageSize);
    }
    public async Task<PagedResult<SolicitudApoyo>> ListarPorEstudianteAsync(
        Guid estudianteId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Solicitudes.AsNoTracking()
            .Where(s => s.EstudianteId == estudianteId)
            .OrderByDescending(s => s.FechaSolicitud);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<SolicitudApoyo>(items, total, pageNumber, pageSize);
    }
    public async Task AgregarAsync(SolicitudApoyo solicitud, CancellationToken ct = default)
    {
        await _context.Solicitudes.AddAsync(solicitud, ct);
    }
    public async Task<bool> TieneSolicitudesActivasAsync(Guid estudianteId, CancellationToken ct = default)
    {
        var estadosActivos = new[] { EstadoSolicitud.Pendiente, EstadoSolicitud.EnRevision };
        return await _context.Solicitudes
            .AnyAsync(s => s.EstudianteId == estudianteId && estadosActivos.Contains(s.Estado), ct);
    }
}