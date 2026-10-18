using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductSnap.Maui.ViewModels;

public partial class ScannerViewModel : ObservableObject
{
    [ObservableProperty] private bool flashActif;

    [RelayCommand]
    private void ToggleFlash()
    {
        FlashActif = !FlashActif;
    }

    [RelayCommand]
    private async Task PrendrePhoto()
    {
        await Task.Delay(800);
        await Shell.Current.GoToAsync("//main/accueil/ficheProduit");
    }

    [RelayCommand]
    private async Task OuvrirGalerie()
    {
        await Shell.Current.DisplayAlert("Galerie", "Sélection depuis la galerie", "OK");
    }

    [RelayCommand]
    private async Task SaisieManuelle()
    {
        await Shell.Current.GoToAsync("//main/accueil/recherche");
    }
}