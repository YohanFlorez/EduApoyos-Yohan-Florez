using EduApoyos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Domain.Interfaces
{
    public interface IHistorialEstadoRepository
    {
        Task AgregarHistorialAsync(HistorialEstado historial, CancellationToken ct = default);
    }
}
