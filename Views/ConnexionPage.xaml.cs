using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage(ConnexionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}