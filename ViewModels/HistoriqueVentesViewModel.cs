using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProductSnap.Maui.Models;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class HistoriqueVentesViewModel : ObservableObject
{
    public ObservableCollection<Vente> Ventes { get; } = new();

    public HistoriqueVentesViewModel(MockDataService mock)
    {
        foreach (var v in mock.GetDernieresVentes())
            Ventes.Add(v);
    }
}