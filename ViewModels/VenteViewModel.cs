using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class VenteViewModel : ObservableObject, IQueryAttributable
{
    private readonly LocalDataService _dataService;
    private int _productId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial string NomProduit { get; set; }
    [ObservableProperty] public partial string LotReference { get; set; }
    [ObservableProperty] public partial decimal PrixUnitaire { get; set; }
    [ObservableProperty] public partial int Quantite { get; set; }
    [ObservableProperty] public partial decimal Total { get; set; }
    [ObservableProperty] public partial string Client { get; set; }
    [ObservableProperty] public partial string ModePaiement { get; set; }
    [ObservableProperty] public partial int StockDisponible { get; set; }

    public bool IsNotBusy => !IsBusy;

    public VenteViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        NomProduit = "";
        LotReference = "";
        Quantite = 1;
        Client = "";
        ModePaiement = "Espèces";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("productId", out var productValue))
            int.TryParse(productValue?.ToString(), out _productId);
        if (query.TryGetValue("quantity", out var quantityValue) &&
            int.TryParse(quantityValue?.ToString(), out var requestedQuantity))
        {
            Quantite = requestedQuantity;
        }
    }

    public async Task ChargerAsync()
    {
        try
        {
            var product = await _dataService.GetProductAsync(_productId)
                ?? throw new InvalidOperationException("Le produit demandé est introuvable.");
            NomProduit = product.Nom;
            PrixUnitaire = product.Prix;
            StockDisponible = product.Stock;
            var lot = (await _dataService.GetLotsAsync(_productId)).FirstOrDefault(item => item.Quantite > 0);
            LotReference = lot?.Reference ?? "—";
            if (Quantite <= 0)
                Quantite = 1;
            RecalculerTotal();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Vente indisponible", exception.Message, "OK");
        }
    }

    partial void OnQuantiteChanged(int value) => RecalculerTotal();

    [RelayCommand] private void ChoisirEspeces() => ModePaiement = "Espèces";
    [RelayCommand] private void ChoisirMobileMoney() => ModePaiement = "Mobile Money";
    [RelayCommand] private void ChoisirCarte() => ModePaiement = "Carte";
    [RelayCommand] private void ChoisirCredit() => ModePaiement = "Crédit";

    [RelayCommand]
    private async Task ValiderAsync()
    {
        if (IsBusy)
            return;
        if (_productId <= 0 || Quantite <= 0 || Quantite > StockDisponible)
        {
            await Shell.Current.DisplayAlertAsync(
                "Stock insuffisant", $"La quantité doit être comprise entre 1 et {StockDisponible}.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _dataService.RecordSaleAsync(_productId, Quantite, ModePaiement, Client);
            await Shell.Current.DisplayAlertAsync(
                "Vente enregistrée", $"{Total:N0} FCFA · {ModePaiement}", "OK");
            await Shell.Current.GoToAsync("//main/accueil");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Vente non enregistrée", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task FermerAsync() => await Shell.Current.GoToAsync("..");

    private void RecalculerTotal()
    {
        Total = PrixUnitaire * Math.Max(0, Quantite);
    }
}
