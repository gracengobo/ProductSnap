namespace ProductSnap.Maui.Models;

public class Vente
{
    public int Id { get; set; }
    public string NomProduit { get; set; } = "";
    public string Emoji { get; set; } = "🛍️";
    public string Lot { get; set; } = "";
    public int Minutes { get; set; }
    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = "";
}