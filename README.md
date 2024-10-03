You said:

>I would like to be able to register services in MAUI in such a way that I would be able to pass to their constructors' parameters upon their instantiation.

Ah! So maybe the first thing to do is defer the instantiation until its first used, but pass it a delegate to call when and if that occurs? For example, the code below gives `MainPage` the opportunity to initialize the service based on values at the point in time that the service is first invoked (but that aren't known 'now').

```
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
```


___

#### Minimal reproducible example

```
public partial class MainPage : ContentPage
{
    private readonly Lazy<ICounterService> _counterService;
    public MainPage(Lazy<ICounterService> counterService)
    {
        InitializeComponent();
        _counterService = counterService;
    }
    internal static void OnServiceInitialize(ILazyService service)
    {
        if(service is CounterService counterService)
        {
            service.InitializeWithParameters(new Dictionary<string, object>
            {
                { "Count", 4 } // This needs to be "one less" than the first click value we want to see.
            });
        }
    }
    private void OnCounterClicked(object sender, EventArgs e)
    {
        int count = _counterService.Value.IncrementCount();

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}

public interface ILazyService
{
    void InitializeWithParameters(Dictionary<string, object> parameters);
}
public interface ICounterService : ILazyService
{
    int IncrementCount();
    int GetCount();
}

public class CounterService : ICounterService
{
    private int _count;
    public CounterService(Action<CounterService>? onInit = null)
    {
        onInit?.Invoke(this);
    }
    public int IncrementCount()
    {
        _count++;
        return _count;
    }
    public int GetCount()
    {
        return _count;
    }
    public void InitializeWithParameters(Dictionary<string, object> parameters)
    {
        foreach (var key in parameters.Keys)
        {
            switch (key)
            {
                case "Count":
                    _count = (int)parameters[key];
                    break;
            }
        }
    }
}
```
