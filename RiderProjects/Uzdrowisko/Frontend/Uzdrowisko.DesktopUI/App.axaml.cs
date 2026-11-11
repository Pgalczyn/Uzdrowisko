using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Avalonia;
using HotAvalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.ViewModels;
using Uzdrowisko.Views;

namespace Uzdrowisko.DesktopUI;

public partial class App : Application
{
    // Umożliwia dostęp do kontenera z poziomu całej aplikacji, np. przy nawigacji
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        this.EnableHotReload(); 
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        
        ConfigureServices(serviceCollection);

        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);

        services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BackendApi"));
        
        services.AddHttpClient("BackendApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5290/");
        });
        
        services.AddHttpClient("WeatherApi",client =>
        {
            client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
        });


        services.AddTransient<ITreatmentService, TreatmentService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IPatientService, PatientService>();
        services.AddTransient<IRoomService, RoomService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IAppointmentService, AppointmentService>();
        services.AddTransient<IStayService, StayService>();
        services.AddTransient<IScheduleService, ScheduleService>();
        services.AddTransient<IWeatherService, WeatherService>();


        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();


        services.AddSingleton<MainWindow>(sp => new MainWindow
        {
            DataContext = sp.GetRequiredService<MainViewModel>()
        });
    }
    
    
}
