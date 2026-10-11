namespace WareFlow.Core.Models;

/// <summary>
/// Refresh token de un usuario (Fase B.3a).
/// Se persiste siempre hasheado con SHA256: el valor en claro sólo viaja al cliente
/// en el login o en el refresh. Cada uso rota el token (se revoca el usado y se
/// emite uno nuevo) para que un token filtrado sirva una sola vez.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>Hash SHA256 (Base64) del token entregado al cliente.</summary>
    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash del token que reemplazó a éste al rotar.</summary>
    public string? ReplacedByToken { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt != null;

    public bool IsActive => !IsRevoked && !IsExpired;
}
