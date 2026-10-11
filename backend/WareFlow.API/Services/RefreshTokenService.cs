using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Models;
using WareFlow.Infrastructure.Data;

namespace WareFlow.API.Services;

/// <summary>
/// Emisión, validación y rotación de refresh tokens (Fase B.3a).
/// En la base de datos sólo se guarda el hash SHA256 del token: el valor en claro
/// existe únicamente en la respuesta al cliente.
/// </summary>
public class RefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>Genera un refresh token para el usuario y devuelve el valor sin hashear.</summary>
    public async Task<string> GenerateAsync(string userId)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);

        var hash = HashToken(token);

        var expiryDays = int.Parse(_configuration["Jwt:RefreshExpireDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            Token = hash,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return token;
    }

    /// <summary>
    /// Devuelve el token almacenado si existe y está activo (ni revocado ni expirado).
    /// El usuario se incluye para poder emitir un access token nuevo sin otra consulta.
    /// </summary>
    public async Task<RefreshToken?> ValidateAsync(string token)
    {
        var hash = HashToken(token);
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == hash);

        if (refreshToken == null || !refreshToken.IsActive)
            return null;

        return refreshToken;
    }

    /// <summary>
    /// Rota el token: revoca el usado, emite uno nuevo y deja la trazabilidad en
    /// <see cref="RefreshToken.ReplacedByToken"/>. Devuelve el token nuevo sin hashear.
    /// </summary>
    public async Task<string> RotateAsync(RefreshToken oldToken)
    {
        oldToken.RevokedAt = DateTime.UtcNow;

        var newToken = await GenerateAsync(oldToken.UserId);
        oldToken.ReplacedByToken = HashToken(newToken);

        await _context.SaveChangesAsync();
        return newToken;
    }

    /// <summary>Revoca todos los refresh tokens activos del usuario (logout).</summary>
    public async Task RevokeAllForUserAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
