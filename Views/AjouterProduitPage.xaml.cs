using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class AjouterProduitPage : ContentPage
{
    public AjouterProduitPage(AjouterProduitViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
