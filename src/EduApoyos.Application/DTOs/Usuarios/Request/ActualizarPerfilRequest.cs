using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduApoyos.Application.DTOs.Usuarios.Request
{
    public class ActualizarPerfilRequest
    {
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
    }
}
