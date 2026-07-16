using EduApoyos.Application.DTOs.Estudiantes;
using EduApoyos.Application.DTOs.Estudiantes.Request;
using EduApoyos.Application.DTOs.Estudiantes.Response;
using EduApoyos.Application.DTOs.Paginacion.Request;
using EduApoyos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Gestión de estudiantes y sus solicitudes asociadas.
/// </summary>
[ApiController]
[Route("api/estudiantes")]
[Produces("application/json")]
[Authorize]
public class EstudiantesController : ControllerBase
{
    private readonly IEstudianteService _estudianteService;
    private readonly ISolicitudService _solicitudService;
    private readonly ICurrentUserService _currentUser;

    public EstudiantesController(
        IEstudianteService estudianteService,
        ISolicitudService solicitudService,
        ICurrentUserService currentUser)
    {
        _estudianteService = estudianteService;
        _solicitudService = solicitudService;
        _currentUser = currentUser;
    }

    /// <summary>Listado paginado de estudiantes.</summary>
    [HttpGet]
    [Authorize(Roles = "Asesor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] PaginacionRequest paginacion,
        CancellationToken ct)
    {
        var resultado = await _estudianteService.ListarAsync(
            paginacion.PageNumber, paginacion.PageSize, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Crea un estudiante. Normalmente se crea junto con el registro;
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Asesor")]
    [ProducesResponseType(typeof(EstudianteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstudianteResponse>> Crear(
        CrearEstudianteRequest request,
        CancellationToken ct)
    {
        // Validación la ejecuta ValidationFilter global antes de llegar aquí.
        var creado = await _estudianteService.CrearAsync(request, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>Obtiene un estudiante por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Asesor")]
    [ProducesResponseType(typeof(EstudianteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteResponse>> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var estudiante = await _estudianteService.ObtenerPorIdAsync(id, ct);
        return Ok(estudiante);
    }

    /// <summary>
    /// Solicitudes de un estudiante. Accesible por el propio estudiante
    /// </summary>
    [HttpGet("{id:guid}/solicitudes")]
    [Authorize(Policy = "MismoEstudianteOAsesor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarSolicitudes(
        Guid id,
        [FromQuery] PaginacionRequest paginacion,
        CancellationToken ct)
    {
        if (_currentUser.UsuarioId is not { } usuarioId || _currentUser.Rol is not { } rol)
            return Unauthorized();

        var resultado = await _solicitudService.ListarPorEstudianteAsync(
            id, usuarioId, rol, paginacion.PageNumber, paginacion.PageSize, ct);

        return Ok(resultado);
    }
}