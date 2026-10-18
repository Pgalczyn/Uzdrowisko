using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

namespace Uzdrowisko.API.DataSeeders;

public static class DataSeeder
{
    public static async Task SeedDataBase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
        var dbContext = services.GetRequiredService<UzdrowiskoDbContext>();
        var passwordService = services.GetRequiredService<IPasswordService>();

        try
        {
            await dbContext.Database.MigrateAsync();
            await EmployeeSeeder.SeedAsync(dbContext,logger,passwordService);
            //await TreatmentSeeder.SeedAsync(dbContext, logger);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}