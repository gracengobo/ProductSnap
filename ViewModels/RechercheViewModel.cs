using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class RechercheViewModel : ObservableObject
{
    private readonly MockDataService _mock;

    [ObservableProperty] private string recherche = "";
    [ObservableProperty] private int nbResultats;

    public ObservableCollection<Produit> Produits { get; } = new();

    public RechercheViewModel(MockDataService mock)
    {
        _mock = mock;
        ChargerProduits();
    }

    private void ChargerProduits()
    {
        Produits.Clear();
        foreach (var p in _mock.GetProduits())
            Produits.Add(p);
        NbResultats = Produits.Count;
    }

    [RelayCommand]
    private async Task Ajouter(Produit produit)
    {
        if (produit == null) return;
        await Shell.Current.DisplayAlert("Ajouté", $"{produit.Nom} ajouté", "OK");
    }
}