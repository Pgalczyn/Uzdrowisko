using System;
using System.Net.Http;
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
        // Krok 1: Tworzymy kolekcję serwisów
        var serviceCollection = new ServiceCollection();

        // Krok 2: Rejestrujemy wszystkie zależności
        ConfigureServices(serviceCollection);

        // Krok 3: Budujemy kontener IServiceProvider
        Services = serviceCollection.BuildServiceProvider();

        // Krok 4: Pobieramy okno z kontenera - DI sam wstrzyknie MainViewModel i wszystkie serwisy
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. Rejestracja HttpClient dedykowanego do backendu
        services.AddHttpClient("BackendApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5290/");
        });

        // Rejestracja domyślnego HttpClient pobieranego przez konstruktory serwisów
        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BackendApi"));

        // 2. Rejestracja serwisów domenowych
        services.AddTransient<ITreatmentService, TreatmentService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IPatientService, PatientService>();
        services.AddTransient<IRoomService, RoomService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IAppointmentService, AppointmentService>();
        services.AddTransient<IStayService, StayService>();
        services.AddTransient<IScheduleService, ScheduleService>();
        services.AddHttpClient<IWeatherService, WeatherService>(client =>
        {
            client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
        });
        // 3. Rejestracja ViewModeli
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>(); // <-- DOPISZ TĘ LINIJKĘ

        // 4. Rejestracja Widoków z automatycznym powiązaniem DataContext
        services.AddSingleton<MainWindow>(sp => new MainWindow
        {
            DataContext = sp.GetRequiredService<MainViewModel>()
        });
    }
    
    
}
