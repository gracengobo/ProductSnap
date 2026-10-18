using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class HistoriqueVentesPage : ContentPage
{
    public HistoriqueVentesPage(HistoriqueVentesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}