using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class RecherchePage : ContentPage
{
    public RecherchePage(RechercheViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RechercheViewModel viewModel)
            await viewModel.ChargerAsync();
    }
}