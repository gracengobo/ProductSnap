using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class VentePage : ContentPage
{
    public VentePage(VenteViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VenteViewModel viewModel)
            await viewModel.ChargerAsync();
    }
}