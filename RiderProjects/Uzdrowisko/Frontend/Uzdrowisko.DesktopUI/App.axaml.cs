using System;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.ViewModels;
using Uzdrowisko.Services;
using Uzdrowisko.Views;

namespace Uzdrowisko.DesktopUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5290/")
        };
        services.AddSingleton(httpClient);

        services.AddTransient<ITreatmentService, TreatmentService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IPatientService, PatientService>();
        services.AddTransient<IRoomService, RoomService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IAppointmentService, AppointmentService>();
        services.AddTransient<IStayService, StayService>();
        services.AddTransient<IScheduleService, ScheduleService>();
    }


}