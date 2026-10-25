using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class RechercheViewModel : ObservableObject
{
    private readonly LocalDataService _dataService;
    private CancellationTokenSource? _searchCancellation;

    [ObservableProperty] public partial string Recherche { get; set; }
    [ObservableProperty] public partial string CategorieSelectionnee { get; set; }
    [ObservableProperty] public partial int NbResultats { get; set; }
    [ObservableProperty] public partial string Statut { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }

    public ObservableCollection<Produit> Produits { get; } = new();

    public RechercheViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        Recherche = "";
        CategorieSelectionnee = "Tous";
        Statut = "";
    }

    partial void OnRechercheChanged(string value) => _ = RechercherAsync(value);
    partial void OnCategorieSelectionneeChanged(string value) => _ = RechercherAsync(Recherche);

    public async Task ChargerAsync() => await RechercherAsync(Recherche);

    [RelayCommand]
    private async Task OuvrirProduitAsync(Produit produit)
    {
        if (produit is not null)
            await Shell.Current.GoToAsync($"ficheProduit?id={produit.Id}");
    }

    [RelayCommand]
    private async Task AjouterAsync(Produit produit)
    {
        if (produit is null)
            return;
        await Shell.Current.GoToAsync($"vente?productId={produit.Id}&quantity=1");
    }

    [RelayCommand]
    private async Task OuvrirScannerAsync() => await Shell.Current.GoToAsync("//main/scanner");

    [RelayCommand]
    private async Task AjouterNouveauAsync() => await Shell.Current.GoToAsync(nameof(Views.AjouterProduitPage));

    [RelayCommand]
    private void ChoisirCategorie(string categorie) => CategorieSelectionnee = categorie;

    private async Task RechercherAsync(string query)
    {
        var cancellation = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _searchCancellation, cancellation);
        previous?.Cancel();
        previous?.Dispose();

        try
        {
            await Task.Delay(180, cancellation.Token);
            IsBusy = true;
            var products = await _dataService.GetProductsAsync(query, CategorieSelectionnee);
            cancellation.Token.ThrowIfCancellationRequested();
            Produits.Clear();
            foreach (var product in products)
                Produits.Add(product);
            NbResultats = Produits.Count;
            Statut = Produits.Count == 0
                ? "Aucun produit dans le catalogue. Scannez un code-barres pour en ajouter."
                : "";
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Statut = $"Recherche impossible : {exception.Message}";
        }
        finally
        {
            if (ReferenceEquals(_searchCancellation, cancellation))
                IsBusy = false;
        }
    }
}
