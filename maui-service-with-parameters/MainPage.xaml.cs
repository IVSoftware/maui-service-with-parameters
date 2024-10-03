namespace maui_service_with_parameters
{
    public partial class MainPage : ContentPage
    {
        private readonly ICounterService _counterService;

        public MainPage(ICounterService counterService)
        {
            InitializeComponent();
            _counterService = counterService;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            int count = _counterService.IncrementCount();

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

    public interface ICounterService
    {
        int IncrementCount();
        int GetCount();
    }

    public class CounterService : ICounterService
    {
        private int _count;

        public CounterService(int initialCount = 0)
        {
            _count = initialCount;
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
}
