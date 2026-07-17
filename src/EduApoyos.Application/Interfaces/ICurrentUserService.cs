using EduApoyos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UsuarioId { get; }
        RolUsuario? Rol { get; }
    }
}
