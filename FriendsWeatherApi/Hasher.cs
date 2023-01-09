using System.Security.Cryptography;

namespace FriendsWeatherApi;

public class Hasher
{
    private readonly int _size;
    private readonly int _iterations;
    private readonly HashAlgorithmName _algorithm;

    public Hasher(int size, int iterations, HashAlgorithmName algorithm)
    {
        _size = size;
        _iterations = iterations;
        _algorithm = algorithm;
    }
    
    public byte[] HashPassword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(_size);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _iterations,
            _algorithm,
            _size);

        return hash;
    }
    
    public bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        var passwordHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _iterations,
            _algorithm,
            _size);

        return passwordHash.SequenceEqual(hash);
    }
}