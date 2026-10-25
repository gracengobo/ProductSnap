using ProductSnap.Maui.ViewModels;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.Views;

public partial class ConnexionPage : ContentPage
{
    private readonly LocalDataService _dataService;

    public ConnexionPage(ConnexionViewModel vm, LocalDataService dataService)
    {
        InitializeComponent();
        BindingContext = vm;
        _dataService = dataService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (await _dataService.GetCurrentUserAsync() is not null)
                await Shell.Current.GoToAsync("//main/accueil");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Session indisponible", exception.Message, "OK");
        }
    }
}