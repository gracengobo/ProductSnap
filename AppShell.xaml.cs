using ProductSnap.Maui.Views;

namespace ProductSnap.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Routes secondaires (non présentes dans la TabBar)
        Routing.RegisterRoute("ficheProduit", typeof(FicheProduitPage));
        Routing.RegisterRoute("vente", typeof(VentePage));
        Routing.RegisterRoute(nameof(AjouterProduitPage), typeof(AjouterProduitPage));
        Routing.RegisterRoute("recherche", typeof(RecherchePage));
        Routing.RegisterRoute("historiquePrix", typeof(HistoriquePrixPage));
    }
}