using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class ProfilViewModel : ObservableObject
{
    [ObservableProperty] private string nom = "";
    [ObservableProperty] private string initiale = "";
    [ObservableProperty] private string role = "";

    public ProfilViewModel(MockDataService mock)
    {
        var user = mock.GetUtilisateur();
        Nom = user.Nom;
        Initiale = user.Initiale;
        Role = user.Role;
    }

    [RelayCommand]
    private async Task SeDeconnecter()
    {
        bool ok = await Shell.Current.DisplayAlert("Déconnexion",
            "Voulez-vous vraiment vous déconnecter ?", "Oui", "Non");
        if (ok)
            await Shell.Current.GoToAsync("//connexion");
    }
}