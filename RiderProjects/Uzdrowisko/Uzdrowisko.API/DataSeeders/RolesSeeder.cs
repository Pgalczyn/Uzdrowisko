using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.DataSeeders;

public static class RolesSeeder
{
    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        var predefinedRoles = new List<Roles>
        {
            new()
            {
                RoleName = "Fizjoterapeuta",
                RoleDescription = "Specjalista prowadzący kinezyterapię, rehabilitację ruchową oraz zaawansowaną fizykoterapię.",
                IsActive = true
            },
            new()
            {
                RoleName = "Technik Masażysta",
                RoleDescription = "Specjalista wykonujący masaże lecznicze, klasyczne, relaksacyjne oraz drenaż limfatyczny.",
                IsActive = true
            },
            new()
            {
                RoleName = "Technik Fizjoterapii / Pielęgniarka uzdrowiskowa",
                RoleDescription = "Personel medyczny wykonujący podstawowe zabiegi fizykalne, inhalacje oraz opiekę zabiegową.",
                IsActive = true
            },
            new()
            {
                RoleName = "Balneotechnik / Pracownik Hydroterapii",
                RoleDescription = "Pracownik obsługujący kąpiele lecznicze, borowiny, bicze wodne oraz zabiegi wodolecznicze.",
                IsActive = true
            },
            new()
            {
                RoleName = "Lekarz",
                RoleDescription = "Lekarz uzdrowiskowy przeprowadzający badania kwalifikacyjne, konsultacje oraz zlecający zabiegi.",
                IsActive = true
            },
            new()
            {
                RoleName = "Pracownik SPA / Obsługa",
                RoleDescription = "Personel obsługujący strefę wellness, zabiegi kosmetyczne oraz podstawową obsługę kuracjuszy.",
                IsActive = true
            }
        };

        var roleNames = predefinedRoles.Select(r => r.RoleName).ToList();

        var existingRoleNames = await dbContext.Roles
            .Where(r => roleNames.Contains(r.RoleName))
            .Select(r => r.RoleName)
            .ToListAsync();

        var rolesToAdd = predefinedRoles
            .Where(r => !existingRoleNames.Contains(r.RoleName))
            .ToList();

        if (rolesToAdd.Count > 0)
        {
            await dbContext.Roles.AddRangeAsync(rolesToAdd);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Zasilono bazę danych o {Count} ról pracowniczych.", rolesToAdd.Count);
        }
        else
        {
            logger.LogInformation("Wszystkie predefiniowane role znajdują się już w bazie danych.");
        }
    }
}
