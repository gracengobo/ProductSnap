namespace ProductSnap.Maui.Models;

public class Lot
{
    public int Id { get; set; }
    public int ProduitId { get; set; }
    public string Reference { get; set; } = "";   // ex: #A-2025-09-20
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public DateTime DateEntree { get; set; }
    public string Fournisseur { get; set; } = "";
    public string Badge { get; set; } = "";       // "ANCIEN", "NOUVEAU", etc.
}