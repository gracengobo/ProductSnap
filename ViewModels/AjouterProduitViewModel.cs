using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class AjouterProduitViewModel : ObservableObject, IQueryAttributable
{
    private readonly LocalDataService _dataService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial string CodeBarres { get; set; }
    [ObservableProperty] public partial string Nom { get; set; }
    [ObservableProperty] public partial string Categorie { get; set; }
    [ObservableProperty] public partial string PrixVente { get; set; }
    [ObservableProperty] public partial string StockInitial { get; set; }
    [ObservableProperty] public partial string PrixAchat { get; set; }

    public bool IsNotBusy => !IsBusy;

    public AjouterProduitViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        CodeBarres = Nom = Categorie = PrixVente = StockInitial = PrixAchat = "";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("barcode", out var value))
            CodeBarres = Uri.UnescapeDataString(value?.ToString() ?? "");
    }

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Nom) ||
            !TryParseDecimal(PrixVente, out var sellingPrice) || sellingPrice <= 0 ||
            !int.TryParse(StockInitial, NumberStyles.Integer, CultureInfo.CurrentCulture, out var stock) || stock < 0 ||
            !TryParseDecimal(PrixAchat, out var purchasePrice) || purchasePrice < 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Informations requises",
                "Saisissez un nom, un prix de vente supérieur à zéro, un stock entier positif ou nul et un prix d'achat positif ou nul.",
                "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var product = await _dataService.AddProductAsync(
                new Produit
                {
                    Nom = Nom.Trim(),
                    Categorie = Categorie.Trim(),
                    CodeBarres = CodeBarres.Trim(),
                    Prix = sellingPrice
                },
                stock,
                purchasePrice);
            await Shell.Current.GoToAsync($"ficheProduit?id={product.Id}");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Enregistrement impossible", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task AnnulerAsync() => Shell.Current.GoToAsync("..");

    private static bool TryParseDecimal(string value, out decimal result)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
           decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
}
