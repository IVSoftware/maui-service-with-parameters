You said:

>I would like to be able to register services in MAUI in such a way that I would be able to pass to their constructors' parameters upon their instantiation.

Ah! So maybe the first thing to do is _defer the instantiation_ until a later time when those parameters become known, but while we're building the DI pass a delegate to call when and if instantiation occurs and the constructor is invoked. For example, the code below gives `MainPage` the opportunity to initialize the service based on values at the point in time that the service is first utilized (but that aren't known 'now' as might be the case if something like a server or a root folder had to be chosen, at runtime, prior to invoking the service).

```
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

        // The "ServiceHelper" can be provided by any object you choose.
        EventHandler<ConstructorParametersRequiredEventArgs> ServiceHelperMethod = 
            MainPage.OnServiceConstructing;

        builder.Services.AddTransient(
            _ => new Lazy<ICounterService>(() => 
                     new CounterService(onConstructing: MainPage.OnServiceConstructing)));
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
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

    internal static void OnServiceConstructing(object? service, ConstructorParametersRequiredEventArgs e)
    {
        if (service is CounterService counterService)
        {
            foreach(var key in e.Keys)
            {
                switch (key)
                {
                    case "_count":  // This needs to be "one less" than the first click value we want to see.
                        e[key] = 4;
                        break;
                }
            }
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

public interface ILazyService { }
public interface ICounterService : ILazyService
{
    int IncrementCount();
    int GetCount();
}

public class CounterService : ICounterService
{
    private int _count;

    // Request initialization parametes in C'TOR
    public CounterService(EventHandler<ConstructorParametersRequiredEventArgs> onConstructing)
    {
        var e = new ConstructorParametersRequiredEventArgs
        {
            { nameof(_count), default}
        };
        onConstructing?.Invoke(this, e);
        foreach (var key in e.Keys)
        {
            switch (key)
            {
                case "_count":
                    e.TryGetValue<int>(key, out _count);
                    break;
            }
        }
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
}
public class ConstructorParametersRequiredEventArgs : EventArgs, IEnumerable<KeyValuePair<string, object?>>
{
    private Dictionary<string, object?> _parameters = new ();

    public void Add(string key, object? value)
    {
        _parameters.Add(key, value);
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public string[] Keys => _parameters.Keys.ToArray();
    public object? this[string key]
    {
        get => _parameters[key];
        set => _parameters[key] = value;
    }
    public bool TryGetValue<T>(string key, out T? value)
    {
        if (_parameters.TryGetValue(key, out var o) && o is T foundT)
        {
            value = foundT;
            return true;
        }
        value = default;
        return false;
    }
}
```


[![first click shows five times][1]][1]


  [1]: https://i.sstatic.net/wJdHwY8t.png