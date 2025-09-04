using System.Windows;

namespace TV
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigateToDisplays(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.Main());
        }

        private void NavigateToMedia(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.Media());
        }

        private void NavigateToSchedules(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.Schedule());
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.Displays());
        }
    }
}
    