using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Infrastructure.Repositories
{
    public class HistorialEstadoRepository : IHistorialEstadoRepository
    {

        private readonly EduApoyosDbContext _context;
        public HistorialEstadoRepository(EduApoyosDbContext context)
        {
            _context = context;
        }

        public async Task AgregarHistorialAsync(Domain.Entities.HistorialEstado historial, CancellationToken ct = default)
        {
            await _context.HistorialEstados.AddAsync(historial, ct);
        }
    }
}
