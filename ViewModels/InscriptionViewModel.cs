using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductSnap.Maui.ViewModels;

public partial class InscriptionViewModel : ObservableObject
{
    [ObservableProperty] public partial string Nom { get; set; }
    [ObservableProperty] public partial string Email { get; set; }
    [ObservableProperty] public partial string Telephone { get; set; }
    [ObservableProperty] public partial string MotDePasse { get; set; }
    [ObservableProperty] public partial string ConfirmationMotDePasse { get; set; }
    [ObservableProperty] public partial string BoutiqueSelectionnee { get; set; }
    [ObservableProperty] public partial bool AccepteCGU { get; set; }
    [ObservableProperty] public partial string MessageConfirmation { get; set; }
    [ObservableProperty] public partial Color CouleurConfirmation { get; set; }
    [ObservableProperty] public partial bool HasMessageConfirmation { get; set; }

    public InscriptionViewModel()
    {
        Nom = ""; Email = ""; Telephone = ""; MotDePasse = "";
        ConfirmationMotDePasse = ""; BoutiqueSelectionnee = "";
        AccepteCGU = false; MessageConfirmation = "";
        CouleurConfirmation = Colors.Green; HasMessageConfirmation = false;
    }

    partial void OnConfirmationMotDePasseChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) { HasMessageConfirmation = false; return; }
        HasMessageConfirmation = true;
        if (value == MotDePasse)
        {
            MessageConfirmation = "✓ Conforme";
            CouleurConfirmation = Colors.Green;
        }
        else
        {
            MessageConfirmation = "✗ Non identique";
            CouleurConfirmation = Colors.Red;
        }
    }

    [RelayCommand]
    private async Task CreerCompte()
    {
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Veuillez remplir les champs obligatoires.", "OK");
            return;
        }

        if (MotDePasse != ConfirmationMotDePasse)
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Les mots de passe ne correspondent pas.", "OK");
            return;
        }

        if (!AccepteCGU)
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Vous devez accepter les CGU.", "OK");
            return;
        }

        await Shell.Current.DisplayAlertAsync("Succès", $"Compte créé pour {Nom} !", "OK");
        await Shell.Current.GoToAsync("//connexion");
    }

    [RelayCommand]
    private async Task Retour()
    {
        await Shell.Current.GoToAsync("//connexion");
    }
}