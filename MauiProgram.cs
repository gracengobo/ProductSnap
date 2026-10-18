using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ProductSnap.Maui.Services;

namespace ProductSnap.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<MockDataService>();

        // ViewModels
        builder.Services.AddTransient<ViewModels.ConnexionViewModel>();
        builder.Services.AddTransient<ViewModels.InscriptionViewModel>();
        builder.Services.AddTransient<ViewModels.AccueilViewModel>();
        builder.Services.AddTransient<ViewModels.ScannerViewModel>();
        builder.Services.AddTransient<ViewModels.FicheProduitViewModel>();
        builder.Services.AddTransient<ViewModels.VenteViewModel>();
        builder.Services.AddTransient<ViewModels.RechercheViewModel>();
        builder.Services.AddTransient<ViewModels.HistoriquePrixViewModel>();
        builder.Services.AddTransient<ViewModels.HistoriqueVentesViewModel>();
        builder.Services.AddTransient<ViewModels.ProfilViewModel>();

        // Views
        builder.Services.AddTransient<Views.ConnexionPage>();
        builder.Services.AddTransient<Views.InscriptionPage>();
        builder.Services.AddTransient<Views.AccueilPage>();
        builder.Services.AddTransient<Views.ScannerPage>();
        builder.Services.AddTransient<Views.FicheProduitPage>();
        builder.Services.AddTransient<Views.VentePage>();
        builder.Services.AddTransient<Views.RecherchePage>();
        builder.Services.AddTransient<Views.HistoriquePrixPage>();
        builder.Services.AddTransient<Views.HistoriqueVentesPage>();
        builder.Services.AddTransient<Views.ProfilPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}