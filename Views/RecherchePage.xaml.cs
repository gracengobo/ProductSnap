using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class RecherchePage : ContentPage
{
    public RecherchePage(RechercheViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}