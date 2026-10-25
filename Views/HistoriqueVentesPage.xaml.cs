using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class HistoriqueVentesPage : ContentPage
{
    public HistoriqueVentesPage(HistoriqueVentesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HistoriqueVentesViewModel viewModel)
            await viewModel.ChargerAsync();
    }
}