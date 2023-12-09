using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class TokenService
{
    private readonly byte[] _key;

    public TokenService(string keyText)
    {
        _key = GetHmacSha256Key(keyText);
    }

    private byte[] GetHmacSha256Key(string keyText)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyText));
            var key = new byte[32];
            Array.Copy(hash, key, Math.Min(hash.Length, key.Length));
            return key;
        }
    }

    public string GenerateToken(string userId, string username, int expirationMinutes = 60)
    {
        var claims = new List<Claim>
        {
            new Claim("user", userId),
            new Claim("name", username),
            new Claim("scope", "API"),
            new Claim("role","user")
            // Add custom claims here
            //new Claim("custom_claim_key", "custom_claim_value"),
        };
        DateTime tx = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            claims: claims,
            expires: tx.AddMinutes(expirationMinutes),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }


    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
