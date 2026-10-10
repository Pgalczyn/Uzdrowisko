namespace Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

public interface IPasswordService
{
    string HashPassword<T>(T user,string rawPassword) where T: class, IHasPassword;
    bool VerifyHashedPassword<T>(T user, string rawPassword) where T: class, IHasPassword;
}