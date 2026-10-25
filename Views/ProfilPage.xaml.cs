using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class ProfilPage : ContentPage
{
    public ProfilPage(ProfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfilViewModel viewModel)
            await viewModel.ChargerAsync();
    }
}