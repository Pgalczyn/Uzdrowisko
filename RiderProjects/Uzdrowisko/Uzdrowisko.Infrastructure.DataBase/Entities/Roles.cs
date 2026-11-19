using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

public class Roles
{
    public int Id { get; set; }
    [Required]
    [MaxLength(60)]
    public string RoleName { get; set; }
    public string RoleDescription { get; set; }

    [Required]
    public bool IsActive { get; set; }
    public ICollection<Employee> EmployeesCollection{ get; set; } = new List<Employee>();
    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();

    
}

/*
1. Fizjoterapeuta
2. Technik Masażysta
3. Technik Fizjoterapii / Pielęgniarka uzdrowiskowa
4. Balneotechnik / Pracownik Hydroterapii
5. Lekarz
6. Pracownik SPA / Obsługa
*/