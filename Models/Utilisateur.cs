namespace ProductSnap.Maui.Models;

public class Utilisateur
{
    public string Id { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Initiale { get; set; } = "";
    public string Boutique { get; set; } = "";
    public string Role { get; set; } = "Gérant";
    public string Email { get; set; } = "";
    public string Telephone { get; set; } = "";
}