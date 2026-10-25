namespace ProductSnap.Maui.Models;

public class Produit
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public string Categorie { get; set; } = "";
    public string CodeBarres { get; set; } = "";
    public decimal Prix { get; set; }
    public int Stock { get; set; }
    public string StatutStock { get; set; } = "En stock";

    public void ActualiserStatutStock()
    {
        StatutStock = Stock switch
        {
            <= 0 => "Rupture de stock",
            <= 3 => "Stock faible",
            _ => "En stock"
        };
    }
}