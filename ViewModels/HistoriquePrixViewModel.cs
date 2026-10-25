using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class HistoriquePrixViewModel : ObservableObject
{
    private readonly LocalDataService _dataService;
    [ObservableProperty] public partial string Message { get; set; }

    public ObservableCollection<HistoriquePrix> Historique { get; } = new();

    public HistoriquePrixViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        Message = "";
    }

    public async Task ChargerAsync()
    {
        try
        {
            Historique.Clear();
            foreach (var item in await _dataService.GetPriceHistoryAsync())
                Historique.Add(item);
            Message = Historique.Count == 0 ? "Aucun historique de prix." : "";
        }
        catch (Exception exception)
        {
            Message = $"Historique indisponible : {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task RetourAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task PartagerAsync()
    {
        var path = await ExporterCsvAsync();
        await Share.Default.RequestAsync(new ShareFileRequest("Historique des prix", new ShareFile(path)));
    }

    [RelayCommand]
    private async Task ExporterAsync()
    {
        var path = await ExporterCsvAsync();
        await Shell.Current.DisplayAlertAsync("Export terminé", $"Fichier créé : {path}", "OK");
    }

    private async Task<string> ExporterCsvAsync()
    {
        var entries = await _dataService.GetPriceHistoryAsync();
        var path = Path.Combine(FileSystem.CacheDirectory, $"productsnap-prix-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        await using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true));
        await writer.WriteLineAsync("Produit;Prix FCFA;Date");
        foreach (var item in entries)
        {
            var safeName = item.NomProduit.Replace("\"", "\"\"", StringComparison.Ordinal);
            await writer.WriteLineAsync($"\"{safeName}\";{item.Prix.ToString(System.Globalization.CultureInfo.InvariantCulture)};{item.DateModification:O}");
        }
        return path;
    }
}
