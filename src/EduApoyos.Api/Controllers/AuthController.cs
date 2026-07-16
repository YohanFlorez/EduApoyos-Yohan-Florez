using EduApoyos.Application.DTOs.Auth;
using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.DTOs.Auth.Response;
using EduApoyos.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(IAuthService authService, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    ///Registra un nuevo usuario (Asesor o Estudiante) y devuelve un jwt
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        await _registerValidator.ValidateAndThrowAsync(request, ct);
        var respuesta = await _authService.RegistrarAsync(request, ct);
        return Created(string.Empty, respuesta);
    }

    /// Autentica un usuario existente y devuelve un jwt.
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        await _loginValidator.ValidateAndThrowAsync(request, ct);
        var respuesta = await _authService.LoginAsync(request, ct);
        return Ok(respuesta);
    }
}
