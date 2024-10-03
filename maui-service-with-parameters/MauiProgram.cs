
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
            builder.Services.AddTransient(
                _ => new Lazy<ICounterService>(() => new CounterService(onInit: MainPage.OnServiceInitialize)));
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
