using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Algorithm;

public class TestWithDB {
	static void Main() { // Make static to run
		UzdrowiskoDbContext ctx = new UzdrowiskoDbContext(
			new DbContextOptions<UzdrowiskoDbContext>()
		);

		int len = ctx.Employees.ToList().Count;
		Console.WriteLine(len);
	}
}