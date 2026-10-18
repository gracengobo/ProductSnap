using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class FicheProduitViewModel : ObservableObject
{
    private readonly MockDataService _mock;

    [ObservableProperty] private string produitNom = "Riz parfumé 5kg";
    [ObservableProperty] private string categorie = "Alimentaire · CB : 3024567890123";
    [ObservableProperty] private decimal prixEffectif = 200;
    [ObservableProperty] private int quantite = 1;
    [ObservableProperty] private int maxStock = 33;
    [ObservableProperty] private decimal total = 200;

    public ObservableCollection<Lot> Lots { get; } = new();

    public FicheProduitViewModel(MockDataService mock)
    {
        _mock = mock;
        var lots = _mock.GetLotsDuProduit(1);
        foreach (var lot in lots)
            Lots.Add(lot);
    }

    partial void OnQuantiteChanged(int value)
    {
        Total = value * PrixEffectif;
    }

    [RelayCommand]
    private void Augmenter()
    {
        if (Quantite < MaxStock) Quantite++;
    }

    [RelayCommand]
    private void Diminuer()
    {
        if (Quantite > 1) Quantite--;
    }

    [RelayCommand]
    private async Task Fermer()
        => await Shell.Current.GoToAsync("//main/accueil");

    [RelayCommand]
    private async Task VoirDetails()
        => await Shell.Current.GoToAsync("//main/accueil/historiquePrix");

    [RelayCommand]
    private async Task EnregistrerVente()
        => await Shell.Current.GoToAsync("//main/accueil/vente");
}