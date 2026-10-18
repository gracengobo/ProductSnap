using ProductSnap.Maui.ViewModels;

namespace ProductSnap.Maui.Views;

public partial class ScannerPage : ContentPage
{
    public ScannerPage(ScannerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}