using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class FicheProduitViewModel : ObservableObject, IQueryAttributable
{
    private readonly LocalDataService _dataService;
    private int _productId;

    [ObservableProperty] public partial string ProduitNom { get; set; }
    [ObservableProperty] public partial string Categorie { get; set; }
    [ObservableProperty] public partial decimal PrixEffectif { get; set; }
    [ObservableProperty] public partial int Quantite { get; set; }
    [ObservableProperty] public partial int MaxStock { get; set; }
    [ObservableProperty] public partial decimal Total { get; set; }
    [ObservableProperty] public partial Produit? Produit { get; set; }

    public ObservableCollection<Lot> Lots { get; } = new();

    public FicheProduitViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        ProduitNom = "";
        Categorie = "";
        Quantite = 1;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) &&
            int.TryParse(value?.ToString(), out var productId))
        {
            _productId = productId;
        }
    }

    public async Task ChargerAsync()
    {
        if (_productId <= 0)
            return;

        try
        {
            Produit = await _dataService.GetProductAsync(_productId)
                ?? throw new InvalidOperationException("Le produit demandé est introuvable.");
            ProduitNom = Produit.Nom;
            Categorie = $"{Produit.Categorie} · CB : {Produit.CodeBarres}";
            PrixEffectif = Produit.Prix;
            MaxStock = Produit.Stock;
            Quantite = MaxStock > 0 ? Math.Min(Math.Max(Quantite, 1), MaxStock) : 0;
            Lots.Clear();
            foreach (var lot in await _dataService.GetLotsAsync(_productId))
                Lots.Add(lot);
            ActualiserTotal();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Produit indisponible", exception.Message, "OK");
        }
    }

    partial void OnQuantiteChanged(int value) => ActualiserTotal();

    [RelayCommand]
    private void Augmenter()
    {
        if (Quantite < MaxStock)
            Quantite++;
    }

    [RelayCommand]
    private void Diminuer()
    {
        if (Quantite > 1)
            Quantite--;
    }

    [RelayCommand]
    private async Task FermerAsync() => await Shell.Current.GoToAsync("//main/accueil");

    [RelayCommand]
    private async Task VoirDetailsAsync()
        => await Shell.Current.GoToAsync("historiquePrix");

    [RelayCommand]
    private async Task EnregistrerVenteAsync()
    {
        if (Produit is null || Quantite <= 0 || Quantite > MaxStock)
        {
            await Shell.Current.DisplayAlertAsync("Stock indisponible", "La quantité demandée n'est pas en stock.", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"vente?productId={_productId}&quantity={Quantite}");
    }

    [RelayCommand]
    private async Task AjouterStockAsync()
    {
        if (Produit is null)
            return;
        var quantityText = await Shell.Current.DisplayPromptAsync(
            "Réapprovisionner", "Quantité à ajouter", "Continuer", "Annuler", keyboard: Keyboard.Numeric);
        if (!int.TryParse(quantityText, out var quantity) || quantity <= 0)
            return;
        var priceText = await Shell.Current.DisplayPromptAsync(
            "Réapprovisionner", "Prix d'achat unitaire (FCFA)", "Continuer", "Annuler", keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(priceText, out var purchasePrice) || purchasePrice < 0)
            return;
        var supplier = await Shell.Current.DisplayPromptAsync(
            "Réapprovisionner", "Fournisseur (facultatif)", "Enregistrer", "Annuler");
        if (supplier is null)
            return;

        try
        {
            await _dataService.AddStockAsync(_productId, quantity, purchasePrice, supplier);
            await ChargerAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Réapprovisionnement impossible", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ModifierPrixAsync()
    {
        if (Produit is null)
            return;
        var value = await Shell.Current.DisplayPromptAsync(
            "Modifier le prix",
            $"Nouveau prix de vente pour {Produit.Nom} (FCFA)",
            "Enregistrer",
            "Annuler",
            initialValue: Produit.Prix.ToString(System.Globalization.CultureInfo.InvariantCulture),
            keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var price) || price <= 0)
        {
            if (value is not null)
                await Shell.Current.DisplayAlertAsync("Prix invalide", "Le prix doit être supérieur à zéro.", "OK");
            return;
        }

        try
        {
            await _dataService.UpdateProductPriceAsync(_productId, price);
            await ChargerAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Modification impossible", exception.Message, "OK");
        }
    }

    private void ActualiserTotal() => Total = Quantite * PrixEffectif;
}
