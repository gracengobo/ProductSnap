using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class HistoriqueVentesViewModel : ObservableObject
{
    private readonly LocalDataService _dataService;
    [ObservableProperty] public partial string Erreur { get; set; }

    public ObservableCollection<Vente> Ventes { get; } = new();

    public HistoriqueVentesViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        Erreur = "";
    }

    public async Task ChargerAsync()
    {
        try
        {
            Ventes.Clear();
            foreach (var sale in await _dataService.GetSalesAsync())
                Ventes.Add(sale);
            Erreur = Ventes.Count == 0 ? "Aucune vente enregistrée." : "";
        }
        catch (Exception exception)
        {
            Erreur = $"Historique indisponible : {exception.Message}";
        }
    }
}
