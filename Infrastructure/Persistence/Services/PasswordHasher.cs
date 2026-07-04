using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Infrastructure.Persistence.Services
{
    
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        // Formato: iteraciones.salt.hash (todo en Base64)
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verificar(string password, string hashAlmacenado)
    {
        var partes = hashAlmacenado.Split('.', 3);
        if (partes.Length != 3) return false;

        var iterations = int.Parse(partes[0]);
        var salt = Convert.FromBase64String(partes[1]);
        var keyAlmacenada = Convert.FromBase64String(partes[2]);

        var keyIngresada = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(keyIngresada, keyAlmacenada);
    }
}
}