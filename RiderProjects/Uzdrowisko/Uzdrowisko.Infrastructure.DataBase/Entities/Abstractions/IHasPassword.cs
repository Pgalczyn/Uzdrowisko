namespace Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

public interface IHasPassword
{
    string PasswordHash { get; set; }
}

