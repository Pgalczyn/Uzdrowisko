using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.Infrastructure.DataBase.Context;

public class UzdrowiskoDbContext : DbContext
{
    public UzdrowiskoDbContext(DbContextOptions<UzdrowiskoDbContext> options) : base(options){}
    
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Treatment> Treatments => Set<Treatment>();
    public DbSet<Stay> Stays => Set<Stay>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Schedule> Schedules => Set<Schedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Room)
            .WithMany()
            .HasForeignKey(a => a.RoomId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}