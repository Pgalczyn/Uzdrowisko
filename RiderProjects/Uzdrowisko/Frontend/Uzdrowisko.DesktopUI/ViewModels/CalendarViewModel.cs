namespace Uzdrowisko.DesktopUI.ViewModels;
using System;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class CalendarViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Now;



}
