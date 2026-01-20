using Microsoft.UI.Xaml;
using IotDashboard.App.ViewModels;

namespace IotDashboard.App
{
    /// <summary>
    /// Main window of the IoT Dashboard application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            
            // Set DataContext for Binding to work
            RootGrid.DataContext = this;
            
            // Set window size
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(900, 700));
            this.Title = "IoT Device Dashboard";
        }
    }
}
