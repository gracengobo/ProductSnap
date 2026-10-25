using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class ScannerViewModel : ObservableObject
{
    private readonly LocalDataService _dataService;
    private readonly IBarcodeImageDecoder _decoder;

    [ObservableProperty] public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial string Message { get; set; }

    public ScannerViewModel(LocalDataService dataService, IBarcodeImageDecoder decoder)
    {
        _dataService = dataService;
        _decoder = decoder;
        Message = "Photographiez un code-barres EAN-13 / UPC-A pour retrouver le produit dans votre catalogue.";
    }

    [RelayCommand]
    private Task PrendrePhotoAsync() => LireImageAsync(capture: true);

    [RelayCommand]
    private Task OuvrirGalerieAsync() => LireImageAsync(capture: false);

    [RelayCommand]
    private async Task SaisieManuelleAsync()
    {
        var value = await Shell.Current.DisplayPromptAsync(
            "Saisie du code-barres",
            "Entrez les 12 chiffres UPC-A ou les 13 chiffres EAN-13.",
            "Rechercher",
            "Annuler",
            keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(value))
            return;

        var barcode = new string(value.Where(char.IsAsciiDigit).ToArray());
        if (barcode.Length == 12)
            barcode = $"0{barcode}";
        if (!EanBarcodeDecoder.IsValid(barcode))
        {
            await Shell.Current.DisplayAlertAsync(
                "Code invalide", "Le code doit être un UPC-A/EAN-13 valide.", "OK");
            return;
        }

        await OuvrirProduitAsync(barcode);
    }

    private async Task LireImageAsync(bool capture)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            Message = capture ? "Ouverture de l'appareil photo…" : "Sélectionnez une photo contenant le code-barres.";
            FileResult? file = capture
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Choisir une photo de code-barres",
                    FileTypes = FilePickerFileType.Images
                });
            if (file is null)
                return;

            await using var stream = await file.OpenReadAsync();
            var barcode = await _decoder.DecodeAsync(stream);
            if (barcode is null)
            {
                Message = "Code non détecté. Cadrez le code bien à plat, net et suffisamment éclairé, puis réessayez.";
                await Shell.Current.DisplayAlertAsync("Aucun code détecté", Message, "OK");
                return;
            }

            await OuvrirProduitAsync(barcode);
        }
        catch (OperationCanceledException)
        {
            Message = "Scan annulé.";
        }
        catch (NotSupportedException exception)
        {
            Message = exception.Message;
            await Shell.Current.DisplayAlertAsync("Fonction non disponible", exception.Message, "OK");
        }
        catch (Exception exception)
        {
            Message = $"Impossible de lire l'image : {exception.Message}";
            await Shell.Current.DisplayAlertAsync("Erreur de scan", Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OuvrirProduitAsync(string barcode)
    {
        var product = await _dataService.GetProductByBarcodeAsync(barcode);
        if (product is null)
        {
            Message = $"Le code {barcode} est valide, mais absent du catalogue local.";
            var add = await Shell.Current.DisplayAlertAsync(
                "Produit inconnu",
                $"{Message}{Environment.NewLine}Voulez-vous l'ajouter maintenant ?",
                "Ajouter",
                "Annuler");
            if (add)
                await Shell.Current.GoToAsync(
                    $"{nameof(Views.AjouterProduitPage)}?barcode={Uri.EscapeDataString(barcode)}");
            return;
        }

        Message = $"Produit trouvé : {product.Nom}";
        await Shell.Current.GoToAsync($"ficheProduit?id={product.Id}");
    }
}
