namespace ProductSnap.Maui.Models;

public class Vente
{
    public int Id { get; set; }
    public int ProduitId { get; set; }
    public string NomProduit { get; set; } = "";
    public string Lot { get; set; } = "";
    public int Quantite { get; set; }
    public DateTime DateVente { get; set; }
    public string Client { get; set; } = "";
    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = "";

    public int Minutes
    {
        get => Math.Max(0, (int)(DateTime.Now - DateVente).TotalMinutes);
        set => DateVente = DateTime.Now.AddMinutes(-value);
    }
}