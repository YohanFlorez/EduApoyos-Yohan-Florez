using EduApoyos.Application.DTOs.Auth;
using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.DTOs.Auth.Response;
using EduApoyos.Application.DTOs.Usuarios.Request;
using EduApoyos.Application.DTOs.Usuarios.Response;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Interfaces;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EduApoyos.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReadOnlyDictionary<RolUsuario, IPostRegistroHandler> _postRegistroHandlers;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IEnumerable<IPostRegistroHandler> postRegistroHandlers)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;

    
        _postRegistroHandlers = postRegistroHandlers.ToDictionary(h => h.Rol);
    }

    public async Task<AuthResponse> RegistrarAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var usuarioExistente = await _userManager.FindByEmailAsync(request.Email);
        if (usuarioExistente is not null)
            throw new AuthException("Ya existe un usuario registrado con ese correo.");

        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            NombreCompleto = request.NombreCompleto,
            Rol = request.Rol,
            FechaRegistro = DateTime.UtcNow
        };

        // Toda la operación va en una sola transacción
        // para evitar  si algo falla 
        await _unitOfWork.IniciarTransaccionAsync(ct);
        try
        {
            var resultado = await _userManager.CreateAsync(usuario, request.Password);
            if (!resultado.Succeeded)
                throw new AuthException(string.Join(" | ", resultado.Errors.Select(e => e.Description)));

            var rolResult = await _userManager.AddToRoleAsync(usuario, request.Rol.ToString());
            if (!rolResult.Succeeded)
                throw new AuthException(string.Join(" | ", rolResult.Errors.Select(e => e.Description)));

            if (_postRegistroHandlers.TryGetValue(request.Rol, out var handler))
            {
                await handler.EjecutarAsync(usuario.Id, request, ct);
            }

            await _unitOfWork.GuardarCambiosAsync(ct);
            await _unitOfWork.ConfirmarTransaccionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RevertirTransaccionAsync(ct);
            throw;
        }

        var (token, expiraEn) = _jwtTokenGenerator.GenerarToken(usuario.Id, usuario.Email!, usuario.Rol, usuario.NombreCompleto);

        return new AuthResponse
        {
            Token = token,
            ExpiraEn = expiraEn,
            UsuarioId = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email!,
            Rol = usuario.Rol
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new AuthException("Credenciales inválidas.");

        var claveValida = await _userManager.CheckPasswordAsync(usuario, request.Password);
        if (!claveValida)
            throw new AuthException("Credenciales inválidas.");

        var (token, expiraEn) = _jwtTokenGenerator.GenerarToken(usuario.Id, usuario.Email!, usuario.Rol, usuario.NombreCompleto);

        return new AuthResponse
        {
            Token = token,
            ExpiraEn = expiraEn,
            UsuarioId = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email!,
            Rol = usuario.Rol
        };
    }

    public async Task<PerfilResponse> ObtenerPerfilAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new AuthException("Usuario no encontrado.");

        return MapearPerfil(usuario);
    }

    public async Task<PerfilResponse> ActualizarPerfilAsync(
        Guid usuarioId, ActualizarPerfilRequest request, CancellationToken ct = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new AuthException("Usuario no encontrado.");

        usuario.NombreCompleto = request.NombreCompleto.Trim();

        if (!string.Equals(usuario.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var usuarioConEseCorreo = await _userManager.FindByEmailAsync(request.Email);
            if (usuarioConEseCorreo is not null && usuarioConEseCorreo.Id != usuario.Id)
                throw new AuthException("Ya existe un usuario registrado con ese correo.");

            var emailResult = await _userManager.SetEmailAsync(usuario, request.Email);
            if (!emailResult.Succeeded)
                throw new AuthException(string.Join(" | ", emailResult.Errors.Select(e => e.Description)));

            var usernameResult = await _userManager.SetUserNameAsync(usuario, request.Email);
            if (!usernameResult.Succeeded)
                throw new AuthException(string.Join(" | ", usernameResult.Errors.Select(e => e.Description)));
        }

        var updateResult = await _userManager.UpdateAsync(usuario);
        if (!updateResult.Succeeded)
            throw new AuthException(string.Join(" | ", updateResult.Errors.Select(e => e.Description)));

        return MapearPerfil(usuario);
    }

    private static PerfilResponse MapearPerfil(ApplicationUser u) =>
        new PerfilResponse
        {
            Id = u.Id,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email!,
            Rol = u.Rol.ToString(),
            FechaRegistro = u.FechaRegistro
        };
}