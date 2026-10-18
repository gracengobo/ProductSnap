using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class ProfilPage : ContentPage
{
    public ProfilPage(ProfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}