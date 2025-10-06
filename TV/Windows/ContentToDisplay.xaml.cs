using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TV.Classes;

namespace TV.Windows
{
    /// <summary>
    /// Логика взаимодействия для ContentToDisplay.xaml
    /// </summary>
    public partial class ContentToDisplay : Window
    {
        public ContentToDisplay(ContentItem contentItem, Classes.Display.Display display)
        {
            InitializeComponent();

            SetupWindow(contentItem, display); 
        }

        private void SetupWindow(ContentItem contentItem, Classes.Display.Display targetDisplay)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Topmost = true;

            var screen = targetDisplay.Screen;
            Left = screen.Bounds.Left;
            Top = screen.Bounds.Top;
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;

            LoadContent(contentItem);
        }

        private void LoadContent(ContentItem contentItem)
        {
            switch (contentItem.Type)
            {
                case "image":
                    LoadImage(contentItem);
                    break;
                case "video":
                    LoadVideo(contentItem);
                    break;
                case "audio":
                    LoadAudio(contentItem);
                    break;
                default:
                    LoadUnknownContent(contentItem);
                    break;
            }
        }

        private void LoadImage(ContentItem contentItem)
        {
            var image = new Image();

            try
            {
                var bitmap = new BitmapImage(new Uri(contentItem.FilePath));
                image.Source = bitmap;
                image.Stretch = Stretch.Uniform;
            }
            catch (Exception ex)
            {
                image.Source = CreateErrorImage($"Ошибка загрузки: {ex.Message}");
            }

            this.Content = image;
        }

        private void LoadVideo(ContentItem contentItem)
        {
            var mediaElement = new MediaElement();
            mediaElement.Source = new Uri(contentItem.FilePath);
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            mediaElement.Play();

            mediaElement.MediaEnded += (s, e) => this.Close();

            this.Content = mediaElement;
        }

        private void LoadAudio(ContentItem contentItem)
        {
            var stackPanel = new StackPanel
            {
                Background = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var textBlock = new TextBlock
            {
                Text = $"🎵 {contentItem.Name}",
                Foreground = Brushes.White,
                FontSize = 24,
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(textBlock);
            this.Content = stackPanel;
        }

        private void LoadUnknownContent(ContentItem contentItem)
        {
            var textBlock = new TextBlock
            {
                Text = $"Неизвестный тип контента: {contentItem.Type}",
                Foreground = Brushes.White,
                Background = Brushes.Black,
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            this.Content = textBlock;
        }

        private BitmapImage CreateErrorImage(string errorMessage)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 400, 200));
                var text = new FormattedText(
                    errorMessage,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14,
                    Brushes.White,
                    1.0);
                context.DrawText(text, new Point(10, 10));
            }

            var bitmap = new RenderTargetBitmap(400, 200, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
    }
}
