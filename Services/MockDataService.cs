using ProductSnap.Maui.Models;

namespace ProductSnap.Maui.Services;

public class MockDataService
{
    // === UTILISATEUR ===
    public Utilisateur GetUtilisateur()
    {
        return new Utilisateur
        {
            Nom = "Martial",
            Initiale = "M",
            Boutique = "Boutique Centrale",
            Role = "Gérant"
        };
    }

    // === PRODUITS ===
    public List<Produit> GetProduits()
    {
        return new List<Produit>
        {
            new() { Id = 1, Nom = "Riz parfumé 5kg", Categorie = "Alimentaire",
                    CodeBarres = "3024567890123", Prix = 250, Stock = 43,
                    Emoji = "🌾", StatutStock = "En stock" },
            new() { Id = 2, Nom = "Coca-Cola 1.5L", Categorie = "Boisson",
                    CodeBarres = "3024567890789", Prix = 900, Stock = 0,
                    Emoji = "🥤", StatutStock = "Rupture de stock" },
            new() { Id = 3, Nom = "Huile de palme raffinée 1L", Categorie = "Alimentaire",
                    CodeBarres = "3024567890456", Prix = 1400, Stock = 18,
                    Emoji = "🫒", StatutStock = "En stock" },
            new() { Id = 4, Nom = "Savon Marseille 200g", Categorie = "Hygiène",
                    CodeBarres = "3024567890333", Prix = 350, Stock = 3,
                    Emoji = "🧼", StatutStock = "Stock faible" },
        };
    }

    // === LOTS ===
    public List<Lot> GetLotsDuProduit(int produitId)
    {
        if (produitId == 1)
        {
            return new List<Lot>
            {
                new() { Id = 1, Reference = "#A-2025-09-20", Quantite = 33,
                        PrixUnitaire = 200, DateEntree = new DateTime(2025, 9, 20),
                        Fournisseur = "SABC Alimentation", Badge = "ANCIEN (FIFO)" },
                new() { Id = 2, Reference = "#B-2025-09-23", Quantite = 10,
                        PrixUnitaire = 250, DateEntree = new DateTime(2025, 9, 23),
                        Fournisseur = "SABC Alimentation", Badge = "NOUVEAU" },
                new() { Id = 3, Reference = "#C-2025-09-15", Quantite = 5,
                        PrixUnitaire = 180, DateEntree = new DateTime(2025, 9, 15),
                        Fournisseur = "Producteur Local", Badge = "Fin de série" },
            };
        }
        return new List<Lot>();
    }

    // === VENTES ===
    public List<Vente> GetDernieresVentes()
    {
        return new List<Vente>
        {
            new() { Id = 1, NomProduit = "Riz parfumé 5kg", Emoji = "🛍️",
                    Lot = "#A", Minutes = 12, Montant = 2500, ModePaiement = "Espèces" },
            new() { Id = 2, NomProduit = "Coca-Cola 1.5L", Emoji = "🥤",
                    Lot = "#B", Minutes = 60, Montant = 900, ModePaiement = "Orange Money" },
            new() { Id = 3, NomProduit = "Savon de ménage 400g", Emoji = "🧼",
                    Lot = "#C", Minutes = 120, Montant = 450, ModePaiement = "Wave" },
        };
    }

    // === STATS ===
    public (decimal caJour, int nbVentes, int nbAlertes) GetStats()
    {
        return (3850m, 12, 3);
    }

    public int GetNbReferences() => 142;
}