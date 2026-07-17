using EduApoyos.Application.DTOs.Solicitudes;
using EduApoyos.Application.DTOs.Solicitudes.Request;
using EduApoyos.Application.DTOs.Solicitudes.Response;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

[ApiController]
[Route("api/solicitudes")]
[Produces("application/json")]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudService _solicitudService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CrearSolicitudRequest> _crearValidator;
    private readonly IValidator<CambiarEstadoRequest> _cambiarEstadoValidator;

    public SolicitudesController(
        ISolicitudService solicitudService,
        ICurrentUserService currentUser,
        IValidator<CrearSolicitudRequest> crearValidator,
        IValidator<CambiarEstadoRequest> cambiarEstadoValidator)
    {
        _solicitudService = solicitudService;
        _currentUser = currentUser;
        _crearValidator = crearValidator;
        _cambiarEstadoValidator = cambiarEstadoValidator;
    }

    /// Listado paginado de solicitudes con filtros por estado, tipo y fecha .
    [HttpGet]
    [Authorize(Roles = "Asesor")]
    public async Task<IActionResult> Listar(
        [FromQuery] EstadoSolicitud? estado,
        [FromQuery] TipoApoyo? tipoApoyo,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var resultado = await _solicitudService.ListarAsync(estado, tipoApoyo, fechaDesde, fechaHasta, pageNumber, pageSize, ct);
        return Ok(resultado);
    }

    /// Crea una solicitud de apoyo . Un estudiante crea la suya propia; un asesor puede crearla en nombre de un estudiante.
    [HttpPost]
    [Authorize(Roles = "Asesor,Estudiante")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<SolicitudDetalleResponse>> Crear(CrearSolicitudRequest request, CancellationToken ct)
    {
        await _crearValidator.ValidateAndThrowAsync(request, ct);
        var usuarioId = _currentUser.UsuarioId!.Value;
        var creado = await _solicitudService.CrearAsync(request, usuarioId, ct);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id = creado.Id }, creado);
    }

    /// Detalle de una solicitud con su historial de estados. El estudiante solo puede ver la suya 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SolicitudDetalleResponse>> ObtenerDetalle(Guid id, CancellationToken ct)
    {
        var usuarioId = _currentUser.UsuarioId!.Value;
        var rol = _currentUser.Rol!.Value;
        var detalle = await _solicitudService.ObtenerDetalleAsync(id, usuarioId, rol, ct);
        return Ok(detalle);
    }

    /// Cambia el estado de una solicitud (solo Asesor). Valida el flujo Pendiente -EnRevision  - Aprobada o Rechazada.
    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = "Asesor")]
    public async Task<ActionResult<SolicitudDetalleResponse>> CambiarEstado(Guid id, CambiarEstadoRequest request, CancellationToken ct)
    {
        await _cambiarEstadoValidator.ValidateAndThrowAsync(request, ct);
        var asesorId = _currentUser.UsuarioId!.Value;
        var actualizado = await _solicitudService.CambiarEstadoAsync(id, request, asesorId, ct);
        return Ok(actualizado);
    }
}
