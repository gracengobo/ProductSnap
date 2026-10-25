using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class FicheProduitPage : ContentPage
{
    public FicheProduitPage(FicheProduitViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FicheProduitViewModel viewModel)
            await viewModel.ChargerAsync();
    }
}