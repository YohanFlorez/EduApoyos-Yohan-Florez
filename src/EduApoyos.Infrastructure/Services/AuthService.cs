using EduApoyos.Application.DTOs.Auth;
using EduApoyos.Application.DTOs.Auth.Request;
using EduApoyos.Application.DTOs.Auth.Response;
using EduApoyos.Application.Exceptions;
using EduApoyos.Application.Interfaces;
using EduApoyos.Domain.Entities;
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

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
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

        var resultado = await _userManager.CreateAsync(usuario, request.Password);
        if (!resultado.Succeeded)
            throw new AuthException(string.Join(" | ", resultado.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(usuario, request.Rol.ToString());

        // Si el rol es Estudiante, se crea automáticamente el registro académico asociado
        if (request.Rol == RolUsuario.Estudiante)
        {
            var estudiante = new Estudiante(
                usuario.Id,
                request.NumeroDocumento!,
                request.TipoDocumento!,
                request.ProgramaAcademico!,
                request.Semestre!.Value);

            await _unitOfWork.Estudiantes.AgregarAsync(estudiante, ct);
            await _unitOfWork.GuardarCambiosAsync(ct);
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
}
