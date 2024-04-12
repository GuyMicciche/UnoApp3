namespace UnoApp3.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public MainViewModel? ViewModel
    {
        get { return DataContext as MainViewModel; }
    }
}
