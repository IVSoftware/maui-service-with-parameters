
using Microsoft.Extensions.DependencyInjection;

namespace maui_service_with_parameters
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // The "ServiceHelper" can be provided by any object you liie.
            EventHandler<ConstructorParametersRequiredEventArgs> ServiceHelperMethod = 
                MainPage.OnServiceConstructing;

            builder.Services.AddTransient(
                _ => new Lazy<ICounterService>(() => new CounterService(onConstructing: ServiceHelperMethod)));
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
