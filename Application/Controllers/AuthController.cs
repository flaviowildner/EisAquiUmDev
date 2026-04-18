using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Data;
using Application.Models;
using Application.Services;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Name))
            return BadRequest("Email, senha e nome s�o obrigat�rios");

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            return BadRequest("Usu�rio j� existe");

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = _authService.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);
        return Ok(new
        {
            token,
            user = new { user.Id, user.Email, user.Name }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email e senha s�o obrigat�rios");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Email ou senha inv�lidos");

        var token = _authService.GenerateJwtToken(user);
        return Ok(new
        {
            token,
            user = new { user.Id, user.Email, user.Name }
        });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
            return BadRequest("ID Token � obrigat�rio");

        try
        {
            // Validar token do Google
            var googlePayload = await _authService.VerifyGoogleTokenAsync(request.IdToken);
            if (googlePayload == null)
                return Unauthorized("Token do Google inv�lido");

            // Procurar usu�rio existente
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googlePayload.Subject || u.Email == googlePayload.Email);

            if (user == null)
            {
                // Criar novo usuário
                user = new User
                {
                    GoogleId = googlePayload.Subject,
                    Email = googlePayload.Email,
                    Name = googlePayload.Name ?? googlePayload.Email.Split('@')[0],
                    PasswordHash = "" // Usu�rio autenticado via Google n�o precisa de senha
                };
                _context.Users.Add(user);
            }
            else if (string.IsNullOrEmpty(user.GoogleId))
            {
                // Vincular conta Google a usu�rio existente
                user.GoogleId = googlePayload.Subject;
            }

            await _context.SaveChangesAsync();

            // Gerar JWT da API
            var token = _authService.GenerateJwtToken(user);
            return Ok(new
            {
                token,
                user = new { user.Id, user.Email, user.Name }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized($"Erro ao autenticar com Google: {ex.Message}");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("Usu�rio n�o encontrado");

        return Ok(new { user.Id, user.Email, user.Name, user.CreatedAt });
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
