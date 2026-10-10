using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

// Indeks unikalny: Numer gabinetu/pokoju nie może się powtórzyć w uzdrowisku
[Index(nameof(Number), IsUnique = true)]
public class Room
{
    // EF Core automatycznie rozpoznaje "Id" jako Primary Key + IDENTITY
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Number { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public bool Active { get; set; } = true;
}