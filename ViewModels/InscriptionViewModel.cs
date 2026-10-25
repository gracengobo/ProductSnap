using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Services;

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

    private readonly LocalDataService _dataService;

    public InscriptionViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
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
            MessageConfirmation = "Conforme";
            CouleurConfirmation = Colors.Green;
        }
        else
        {
            MessageConfirmation = "Non identique";
            CouleurConfirmation = Colors.Red;
        }
    }

    [RelayCommand]
    private async Task CreerCompte()
    {
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Telephone) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            await ShowErrorAsync("Veuillez remplir tous les champs obligatoires.");
            return;
        }

        if (!System.Net.Mail.MailAddress.TryCreate(Email.Trim(), out _))
        {
            await ShowErrorAsync("Veuillez saisir une adresse e-mail valide.");
            return;
        }

        if (MotDePasse.Length < 8)
        {
            await ShowErrorAsync("Le mot de passe doit contenir au moins 8 caractères.");
            return;
        }

        if (MotDePasse != ConfirmationMotDePasse)
        {
            await ShowErrorAsync("Les mots de passe ne correspondent pas.");
            return;
        }

        if (!AccepteCGU)
        {
            await ShowErrorAsync("Vous devez accepter les CGU.");
            return;
        }

        try
        {
            var created = await _dataService.RegisterAsync(
                Nom, Email, Telephone, BoutiqueSelectionnee, MotDePasse);
            if (!created)
            {
                await ShowErrorAsync("Un compte existe déjà avec cet e-mail ou ce téléphone.");
                return;
            }

            await Shell.Current.GoToAsync("//main/accueil");
        }
        catch (Exception exception)
        {
            await ShowErrorAsync($"Création du compte impossible : {exception.Message}");
        }
    }

    [RelayCommand]
    private async Task Retour()
    {
        await Shell.Current.GoToAsync("//connexion");
    }

    private static Task ShowErrorAsync(string message)
        => Shell.Current.DisplayAlertAsync("Erreur", message, "OK");
}