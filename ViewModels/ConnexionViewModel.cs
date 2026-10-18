using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductSnap.Maui.ViewModels;

public partial class ConnexionViewModel : ObservableObject
{
    [ObservableProperty] public partial string Email { get; set; }
    [ObservableProperty] public partial string MotDePasse { get; set; }
    [ObservableProperty] public partial string MessageErreur { get; set; }
    [ObservableProperty] public partial bool HasErreur { get; set; }

    public ConnexionViewModel()
    {
        Email = "martial@boutique.cm";
        MotDePasse = "123456";
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

        HasErreur = false;
        await Task.Delay(500);

        
        await Shell.Current.GoToAsync("//accueil");
    }

    [RelayCommand]
    private async Task AllerInscription()
    {
        await Shell.Current.GoToAsync("//inscription");
    }
}