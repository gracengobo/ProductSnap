using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class AccueilViewModel : ObservableObject
{
    [ObservableProperty] public partial string UserName { get; set; }
    [ObservableProperty] public partial string Initiale { get; set; }
    [ObservableProperty] public partial string Boutique { get; set; }
    [ObservableProperty] public partial string CaJour { get; set; }
    [ObservableProperty] public partial int NbReferences { get; set; }
    [ObservableProperty] public partial int NbAlertes { get; set; }

    private readonly LocalDataService _dataService;

    public ObservableCollection<Vente> DernieresVentes { get; } = new();

    public AccueilViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        UserName = "";
        Initiale = "";
        Boutique = "";
        CaJour = "0 FCFA aujourd'hui";
        NbReferences = 0;
        NbAlertes = 0;
    }

    public async Task ChargerAsync()
    {
        try
        {
            var user = await _dataService.GetCurrentUserAsync()
                ?? throw new InvalidOperationException("Aucun compte connecté.");
            var stats = await _dataService.GetStatsAsync();
            var sales = await _dataService.GetSalesAsync();
            UserName = user.Nom;
            Initiale = user.Initiale;
            Boutique = user.Boutique;
            CaJour = $"{stats.caJour:N0} FCFA aujourd'hui";
            NbReferences = stats.nbReferences;
            NbAlertes = stats.nbAlertes;
            DernieresVentes.Clear();
            foreach (var sale in sales.Take(3))
                DernieresVentes.Add(sale);
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Données indisponibles", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task AllerScanner() => await Shell.Current.GoToAsync("//main/scanner");

    [RelayCommand]
    private async Task AllerVendre() => await Shell.Current.GoToAsync(nameof(Views.RecherchePage));

    [RelayCommand]
    private async Task AllerRecherche() => await Shell.Current.GoToAsync(nameof(Views.RecherchePage));

    [RelayCommand]
    private async Task AllerStock() => await Shell.Current.GoToAsync(nameof(Views.RecherchePage));

    [RelayCommand]
    private async Task OuvrirNotifications()
    {
        var products = await _dataService.GetProductsAsync();
        var lowStock = products.Where(product => product.Stock <= 3).ToList();
        var message = lowStock.Count == 0
            ? "Aucun produit en alerte de stock."
            : string.Join(Environment.NewLine, lowStock.Select(product => $"{product.Nom} : {product.Stock}"));
        await Shell.Current.DisplayAlertAsync("Alertes de stock", message, "OK");
    }
}