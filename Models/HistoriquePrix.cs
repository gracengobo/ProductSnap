namespace ProductSnap.Maui.Models;

public class HistoriquePrix
{
    public int Id { get; set; }
    public int ProduitId { get; set; }
    public string NomProduit { get; set; } = "";
    public decimal Prix { get; set; }
    public DateTime DateModification { get; set; }
}
