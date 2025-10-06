using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TV.Classes;

namespace TV.Pages.Media
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class CreatePlaylist : Page
    {
        private string connectionString = "server=localhost;port=3306;database=TVDisplay;uid=root;pwd=;charset=utf8;";
        public CreatePlaylist()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {

        }

        private void PlaylistName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string playlistName = playlistNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(playlistName))
            {
                validationText.Text = "Введите название плейлиста";
                validationText.Foreground = new SolidColorBrush(
                    Color.FromRgb(107, 114, 128));
                createButton.IsEnabled = false;
            }
            else if (playlistName.Length < 5)
            {
                validationText.Text = "Название должно содержать минимум 5 символа";
                validationText.Foreground = new SolidColorBrush(
                    Color.FromRgb(239, 68, 68));
                createButton.IsEnabled = false;
            }
            else if (playlistName.Length > 100)
            {
                validationText.Text = "Название не должно превышать 100 символов";
                validationText.Foreground = new SolidColorBrush(
                   Color.FromRgb(239, 68, 68));
                createButton.IsEnabled = false;
            }
            else
            {
                validationText.Text = "✓ Название корректно";
                validationText.Foreground = new SolidColorBrush(
                    Color.FromRgb(34, 197, 94));
                createButton.IsEnabled = true;
            }
        }

        private async void Create(object sender, RoutedEventArgs e)
        {
            try
            {
                string playlistName = playlistNameTextBox.Text.Trim();

                if (string.IsNullOrEmpty(playlistName) || playlistName.Length < 2)
                {
                    ShowErrorMessage("Некорректное название плейлиста");
                    return;
                }

                var newPlaylist = new Playlist
                {
                    Name = playlistName,
                    IsActive = false
                };

                bool success = CreatePlaylistInDatabase(playlistName);

                if (success)
                {
                    ShowSuccessMessage($"Плейлист \"{playlistName}\" успешно создан!");

                    await Task.Delay(500);
                    CloseWindow(sender, e);
                }
                else
                {
                    ShowErrorMessage("Ошибка при сохранении плейлиста в базу данных");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при создании плейлиста: {ex.Message}");
            }
        }

        private bool CreatePlaylistInDatabase(string name)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Playlists (Name, IsActive) 
                        VALUES (@Name, @IsActive)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@IsActive", false);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1062: 
                            ShowErrorMessage("Плейлист с таким названием уже существует");
                            break;
                        case 1045: 
                            ShowErrorMessage("Ошибка доступа к базе данных");
                            break;
                        case 1042: 
                            ShowErrorMessage("Не удалось подключиться к базе данных");
                            break;
                        default:
                            ShowErrorMessage($"Ошибка базы данных: {ex.Message}");
                            break;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Ошибка: {ex.Message}");
                    return false;
                }
            }
        }

        private void ShowSuccessMessage(string message)
        {
            validationText.Text = $"✓ {message}";
            validationText.Foreground = new SolidColorBrush(
                Color.FromRgb(34, 197, 94)); 

            createButton.Background = new SolidColorBrush(
                Color.FromRgb(34, 197, 94));
            createButton.Content = "✓ Успешно";
        }

        private void ShowErrorMessage(string message)
        {
            validationText.Text = $"✗ {message}";
            validationText.Foreground = new SolidColorBrush(
                Color.FromRgb(239, 68, 68)); 
        }
    }
}
