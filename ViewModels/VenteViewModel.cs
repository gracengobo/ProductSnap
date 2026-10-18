using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductSnap.Maui.ViewModels;

public partial class VenteViewModel : ObservableObject
{
    [ObservableProperty] private string nomProduit = "Riz parfumé 5kg";
    [ObservableProperty] private string lotReference = "#A-2025-09-20";
    [ObservableProperty] private decimal prixUnitaire = 200;
    [ObservableProperty] private int quantite = 10;
    [ObservableProperty] private decimal total = 2000;
    [ObservableProperty] private string client = "";
    [ObservableProperty] private string modePaiement = "Espèces";

    [RelayCommand] private void ChoisirEspeces() => ModePaiement = "Espèces";
    [RelayCommand] private void ChoisirMobileMoney() => ModePaiement = "Mobile Money";
    [RelayCommand] private void ChoisirCarte() => ModePaiement = "Carte";
    [RelayCommand] private void ChoisirCredit() => ModePaiement = "Crédit";

    [RelayCommand]
    private async Task Valider()
    {
        await Task.Delay(600);
        await Shell.Current.DisplayAlert("Succès",
            $"Vente de {Total:N0} FCFA enregistrée ({ModePaiement})", "OK");
        await Shell.Current.GoToAsync("//main/accueil");
    }

    [RelayCommand]
    private async Task Fermer()
        => await Shell.Current.GoToAsync("//main/accueil");
}