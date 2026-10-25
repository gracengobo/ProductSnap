using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui.ViewModels;

public partial class ProfilViewModel : ObservableObject
{
    private readonly LocalDataService _dataService;

    [ObservableProperty] public partial string Nom { get; set; }
    [ObservableProperty] public partial string Initiale { get; set; }
    [ObservableProperty] public partial string Role { get; set; }
    [ObservableProperty] public partial string Email { get; set; }
    [ObservableProperty] public partial string Telephone { get; set; }
    [ObservableProperty] public partial string Boutique { get; set; }

    public ProfilViewModel(LocalDataService dataService)
    {
        _dataService = dataService;
        Nom = Initiale = Role = Email = Telephone = Boutique = "";
    }

    public async Task ChargerAsync()
    {
        try
        {
            var user = await _dataService.GetCurrentUserAsync()
                ?? throw new InvalidOperationException("Aucun compte connecté.");
            Nom = user.Nom;
            Initiale = user.Initiale;
            Role = user.Role;
            Email = user.Email;
            Telephone = user.Telephone;
            Boutique = user.Boutique;
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Profil indisponible", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SeDeconnecterAsync()
    {
        var ok = await Shell.Current.DisplayAlertAsync(
            "Déconnexion", "Voulez-vous vraiment vous déconnecter ?", "Oui", "Non");
        if (!ok)
            return;

        try
        {
            await _dataService.SignOutAsync();
            await Shell.Current.GoToAsync("//connexion");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Déconnexion impossible", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ChangerMotDePasseAsync()
    {
        var currentPassword = await Shell.Current.DisplayPromptAsync(
            "Changer le mot de passe", "Mot de passe actuel", "Continuer", "Annuler",
            keyboard: Keyboard.Default, maxLength: 128);
        if (currentPassword is null)
            return;
        var newPassword = await Shell.Current.DisplayPromptAsync(
            "Changer le mot de passe", "Nouveau mot de passe (8 caractères minimum)",
            "Continuer", "Annuler", keyboard: Keyboard.Default, maxLength: 128);
        if (newPassword is null)
            return;
        var confirmation = await Shell.Current.DisplayPromptAsync(
            "Confirmer le mot de passe", "Saisissez-le une nouvelle fois",
            "Enregistrer", "Annuler", keyboard: Keyboard.Default, maxLength: 128);
        if (confirmation is null)
            return;
        if (newPassword != confirmation)
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Les mots de passe ne correspondent pas.", "OK");
            return;
        }

        try
        {
            await _dataService.ChangePasswordAsync(currentPassword, newPassword);
            await Shell.Current.DisplayAlertAsync("Succès", "Mot de passe mis à jour.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Modification impossible", exception.Message, "OK");
        }
    }
}
