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
            Windows.MainSettings window = new Windows.MainSettings();

            window.Show();
            Close();
        }
    }
}
