using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductSnap.Maui.ViewModels;

public partial class AccueilViewModel : ObservableObject
{
    [ObservableProperty] public partial string UserName { get; set; }
    [ObservableProperty] public partial string Initiale { get; set; }
    [ObservableProperty] public partial string Boutique { get; set; }
    [ObservableProperty] public partial string CaJour { get; set; }
    [ObservableProperty] public partial int NbReferences { get; set; }
    [ObservableProperty] public partial int NbAlertes { get; set; }

    public AccueilViewModel()
    {
        UserName = "Martial";
        Initiale = "M";
        Boutique = "Boutique Centrale";
        CaJour = "3 850 F CFA aujourd'hui";
        NbReferences = 142;
        NbAlertes = 3;
    }

    [RelayCommand]
    private async Task AllerScanner() => await Shell.Current.DisplayAlertAsync("Scanner", "Écran Scanner à venir", "OK");

    [RelayCommand]
    private async Task AllerVendre() => await Shell.Current.DisplayAlertAsync("Vendre", "Écran Vente à venir", "OK");

    [RelayCommand]
    private async Task AllerRecherche() => await Shell.Current.DisplayAlertAsync("Recherche", "Écran Recherche à venir", "OK");

    [RelayCommand]
    private async Task AllerStock() => await Shell.Current.DisplayAlertAsync("Stock", $"{NbReferences} références", "OK");

    [RelayCommand]
    private async Task OuvrirNotifications() => await Shell.Current.DisplayAlertAsync("Notifications", "Aucune notification", "OK");
}