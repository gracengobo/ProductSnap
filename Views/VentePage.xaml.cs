using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class VentePage : ContentPage
{
    public VentePage(VenteViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}