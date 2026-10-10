using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;
using Uzdrowisko.Infrastructure.DataBase.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UzdrowiskoDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPasswordService,PasswordService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();