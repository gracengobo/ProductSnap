using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class ConnexionViewModel : ObservableObject
{
    [ObservableProperty] public partial string Email { get; set; }
    [ObservableProperty] public partial string MotDePasse { get; set; }
    [ObservableProperty] public partial string MessageErreur { get; set; }
    [ObservableProperty] public partial bool HasErreur { get; set; }

    private readonly LocalDataService _dataService;

    public ConnexionViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        Email = "";
        MotDePasse = "";
        MessageErreur = "";
        HasErreur = false;
    }

    [RelayCommand]
    private async Task SeConnecter()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            MessageErreur = "Veuillez remplir tous les champs.";
            HasErreur = true;
            return;
        }

        try
        {
            HasErreur = false;
            var connected = await _dataService.SignInAsync(Email.Trim(), MotDePasse);
            if (!connected)
            {
                MessageErreur = "Identifiant ou mot de passe incorrect.";
                HasErreur = true;
                return;
            }

            MotDePasse = "";
            await Shell.Current.GoToAsync("//main/accueil");
        }
        catch (Exception exception)
        {
            MessageErreur = $"Connexion impossible : {exception.Message}";
            HasErreur = true;
        }
    }

    [RelayCommand]
    private async Task AllerInscription()
    {
        await Shell.Current.GoToAsync("//inscription");
    }
}