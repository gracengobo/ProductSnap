using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class HistoriquePrixPage : ContentPage
{
    public HistoriquePrixPage(HistoriquePrixViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HistoriquePrixViewModel viewModel)
            await viewModel.ChargerAsync();
    }
}