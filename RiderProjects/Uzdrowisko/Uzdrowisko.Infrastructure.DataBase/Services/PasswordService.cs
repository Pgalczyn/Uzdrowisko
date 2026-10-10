using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;
using Microsoft.AspNetCore.Identity;
namespace Uzdrowisko.Infrastructure.DataBase.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword<T>(T user, string rawPassword) where T : class, IHasPassword
    {
        var hasher = new PasswordHasher<T>();
        return hasher.HashPassword(user, rawPassword);
    }

    public bool VerifyHashedPassword<T>(T user, string rawPassword) where T : class, IHasPassword
    {
        var hasher = new PasswordHasher<T>();
        
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || string.IsNullOrWhiteSpace(rawPassword))
        {
            return false;
        }
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, rawPassword);
        return result == PasswordVerificationResult.Success;
    }
}