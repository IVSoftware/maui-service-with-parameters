
using System.Collections;
using static System.Collections.Generic.Dictionary<string, object?>;

namespace maui_service_with_parameters
{
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

        // Request initialization parameters in C'TOR.
        // This allows initialization of properties and fields that are readonly, get only, and init
        public CounterService(EventHandler<ConstructorParametersRequiredEventArgs>? onConstructing = null)
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
}
