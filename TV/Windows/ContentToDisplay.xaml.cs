using System.Windows;
using System.Windows.Media.Imaging;

namespace TV.Windows
{
    /// <summary>
    /// Логика взаимодействия для ContentToDisplay.xaml
    /// </summary>
    public partial class ContentToDisplay : Window
    {
        public ContentToDisplay(Classes.Display display)
        {
            InitializeComponent();

            switch (display.ContentType)
            {
                case "Медиа":
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new System.Uri(display.CurrentContent, System.UriKind.Absolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    imageDisplay.Source = bitmapImage;
                    imageDisplay.Visibility = Visibility.Visible;
                    break;
            }

        }
    }
}
