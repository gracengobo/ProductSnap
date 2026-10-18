using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductSnap.Maui.ViewModels;

public partial class HistoriquePrixViewModel : ObservableObject
{
    [RelayCommand]
    private async Task Retour()
        => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task Partager()
        => await Shell.Current.DisplayAlert("Partager", "Rapport partagé", "OK");

    [RelayCommand]
    private async Task Exporter()
        => await Shell.Current.DisplayAlert("Export", "Rapport exporté en CSV/PDF", "OK");
}