using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class AccueilPage : ContentPage
{
    public AccueilPage(AccueilViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}