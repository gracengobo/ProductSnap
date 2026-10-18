using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class InscriptionPage : ContentPage
{
    public InscriptionPage(InscriptionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}