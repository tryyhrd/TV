using System.Windows;

namespace TV
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow main;
        public MainWindow()
        {
            InitializeComponent();

            main = this;

            mainFrame.Navigate(new Pages.Main());
        }

        private void NavigateToDisplays(object sender, RoutedEventArgs e)
            => mainFrame.Navigate(new Pages.Main());

        private void NavigateToMedia(object sender, RoutedEventArgs e)
            => mainFrame.Navigate(new Pages.Media.Main());

        private void NavigateToSchedules(object sender, RoutedEventArgs e)
            => mainFrame.Navigate(new Pages.Schedules.Main());
    }
}
    