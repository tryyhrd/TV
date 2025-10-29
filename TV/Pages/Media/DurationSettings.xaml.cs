using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TV.Classes.Display;

namespace TV.Pages.Media
{
    /// <summary>
    /// Логика взаимодействия для DurationSettings.xaml
    /// </summary>
    public partial class DurationSettings : Page
    {
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public int Duration { get; set; }
        public bool IsInfinite { get; set; }
        public bool IsScheduled { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool LoopPlayback { get; set; }

        public event Action<DisplayContent> SettingsApplied;

        public DurationSettings(string filePath, string fileType)
        {
            InitializeComponent();

            FilePath = filePath;
            FileType = fileType;
            
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Теперь все элементы гарантированно инициализированы
            InitializeUI();

            // Отписываемся от события
            Loaded -= OnPageLoaded;
        }

        private void InitializeUI()
        {
            // Теперь можно безопасно работать с элементами
            fileInfoText.Text = $"Файл: {System.IO.Path.GetFileName(FilePath)}\nТип: {GetFileTypeDisplayName(FileType)}";

            videoWarningPanel.Visibility = FileType == "video" ? Visibility.Visible : Visibility.Collapsed;

            if (FileType == "image")
            {
                durationTextBox.Text = "5";
            }
            else if (FileType == "video")
            {
                durationTextBox.Text = "0";
                infiniteDurationRadio.IsChecked = true;
            }

            // Устанавливаем даты по умолчанию
            startDatePicker.SelectedDate = DateTime.Today;
            endDatePicker.SelectedDate = DateTime.Today;

            // Устанавливаем время по умолчанию
            startTimeTextBox.Text = DateTime.Now.ToString("HH:mm");
            endTimeTextBox.Text = DateTime.Now.ToString("HH:mm");

            // ПОДПИСКА НА СОБЫТИЯ RADIOBUTTON - ВАЖНО!
            finiteDurationRadio.Checked += RadioButton_Checked;
            infiniteDurationRadio.Checked += RadioButton_Checked;
            scheduledDurationRadio.Checked += RadioButton_Checked;

            // Подписываемся на другие события
            durationTextBox.PreviewTextInput += Duration_PreviewTextInput;
            startTimeTextBox.PreviewTextInput += Time_PreviewTextInput;
            endTimeTextBox.PreviewTextInput += Time_PreviewTextInput;

            startDatePicker.SelectedDateChanged += (s, e) => UpdateSummary();
            endDatePicker.SelectedDateChanged += (s, e) => UpdateSummary();
            startTimeTextBox.TextChanged += (s, e) => UpdateSummary();
            endTimeTextBox.TextChanged += (s, e) => UpdateSummary();
            durationTextBox.TextChanged += (s, e) => UpdateSummary();
            durationUnitCombo.SelectionChanged += (s, e) => UpdateSummary();
            loopCheckBox.Checked += (s, e) => UpdateSummary();
            loopCheckBox.Unchecked += (s, e) => UpdateSummary();

            // Находим кнопки и подписываемся на события
            var cancelButton = FindName("CancelButton") as Button;
            var applyButton = FindName("ApplyButton") as Button;

            if (cancelButton != null)
                cancelButton.Click += Cancel_Click;
            if (applyButton != null)
                applyButton.Click += Apply_Click;

            // Вызываем обновление для начального состояния
            UpdateDurationPanelVisibility();
            UpdateSummary();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDurationPanelVisibility();
            UpdateSummary();
        }

        private string GetFileTypeDisplayName(string fileType)
        {
            switch (fileType)
            {
                case "video":
                    return "Видео";
                case "image":
                    return "Изображение";
                case "audio":
                    return "Аудио";
                default:
                    return "Файл";
            }
        }

        private void DurationMode_Changed(object sender, RoutedEventArgs e)
        {
            UpdateDurationPanelVisibility();
            UpdateSummary();
        }

        private void UpdateDurationPanelVisibility()
        {
            if (infiniteDurationRadio.IsChecked == true)
            {
                // Бесконечный режим
                durationSettingsPanel.IsEnabled = false;
                durationSettingsPanel.Opacity = 0.5;
                periodSettingsPanel.Visibility = Visibility.Collapsed;
                durationTextBox.Text = "0";
            }
            else if (scheduledDurationRadio.IsChecked == true)
            {
                durationSettingsPanel.IsEnabled = true;
                durationSettingsPanel.Opacity = 0.5;
                periodSettingsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Ограниченное время
                durationSettingsPanel.IsEnabled = true;
                durationSettingsPanel.Opacity = 1.0;
                periodSettingsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Duration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void Time_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Проверяем формат времени ЧЧ:MM
            if (!System.Text.RegularExpressions.Regex.IsMatch(newText, @"^([0-1]?[0-9]|2[0-3]):?[0-5]?[0-9]?$"))
            {
                e.Handled = true;
            }
        }

        private void UpdateSummary()
        {
            string summary = "";

            if (scheduledDurationRadio.IsChecked == true)
            {
                // Режим по расписанию
                var startDate = startDatePicker.SelectedDate ?? DateTime.Today;
                var endDate = endDatePicker.SelectedDate ?? DateTime.Today.AddDays(7);

                string startTime = FormatTime(startTimeTextBox.Text);
                string endTime = FormatTime(endTimeTextBox.Text);

                if (DateTime.TryParseExact($"{startDate:yyyy-MM-dd} {startTime}", "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDateTime) &&
                    DateTime.TryParseExact($"{endDate:yyyy-MM-dd} {endTime}", "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDateTime))
                {
                    string loopText = loopCheckBox.IsChecked == true ? "с зацикливанием" : "однократно";

                    // Рассчитываем длительность периода
                    TimeSpan periodDuration = endDateTime - startDateTime;
                    string periodDurationText = GetPeriodDurationText(periodDuration);

                    // Проверяем, не в прошлом ли время
                    string timeWarning = "";
                    if (startDateTime < DateTime.Now)
                    {
                        timeWarning = "\n⚠️ Время начала уже прошло - будет автоматически скорректировано при применении";
                    }

                    summary = $"📅 Воспроизведение по расписанию:\n" +
                             $"• Период: {startDateTime:dd.MM.yyyy HH:mm} - {endDateTime:dd.MM.yyyy HH:mm}\n" +
                             $"• Длительность периода: {periodDurationText}\n" +
                             $"• Режим: {loopText}" +
                             timeWarning;
                }
                else
                {
                    summary = "❌ Укажите корректные дату и время для периода воспроизведения";
                }
            }
            else if (infiniteDurationRadio.IsChecked == true)
            {
                // Бесконечное воспроизведение
                summary = "♾️ Бесконечное воспроизведение\nФайл будет воспроизводиться пока не будет остановлен вручную";
            }
            else
            {
                // Ограниченное время
                string durationText = GetDurationText();
                string loopText = loopCheckBox.IsChecked == true ? "с зацикливанием" : "однократно";

                summary = $"⏱️ Ограниченное воспроизведение:\n" +
                         $"• Длительность показа: {durationText}\n" +
                         $"• Режим: {loopText}";
            }

            summaryText.Text = summary;
        }

        private string GetPeriodDurationText(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{duration.TotalDays:F1} дней";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{duration.TotalHours:F1} часов";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{duration.TotalMinutes:F0} минут";
            }
            else
            {
                return $"{duration.TotalSeconds:F0} секунд";
            }
        }

        private string FormatTime(string timeText)
        {
            // Форматируем время в правильный формат
            if (timeText.Contains(":"))
            {
                return timeText.Length == 4 ? "0" + timeText : timeText;
            }
            else if (timeText.Length == 3)
            {
                return "0" + timeText.Insert(1, ":");
            }
            else if (timeText.Length == 4)
            {
                return timeText.Insert(2, ":");
            }

            return "00:00";
        }

        private string GetDurationText()
        {
            if (scheduledDurationRadio.IsChecked == true)
            {
                return "определяется периодом"; // Для режима расписания
            }

            if (int.TryParse(durationTextBox.Text, out int durationValue))
            {
                int durationInSeconds;

                switch (durationUnitCombo.SelectedIndex)
                {
                    case 0: // секунды
                        durationInSeconds = durationValue;
                        break;
                    case 1: // минуты
                        durationInSeconds = durationValue * 60;
                        break;
                    case 2: // часы
                        durationInSeconds = durationValue * 3600;
                        break;
                    default:
                        durationInSeconds = durationValue;
                        break;
                }

                if (durationInSeconds == 0)
                    return "бесконечно";

                if (durationInSeconds < 60)
                    return $"{durationInSeconds} сек";

                if (durationInSeconds < 3600)
                    return $"{durationInSeconds / 60} мин";

                return $"{durationInSeconds / 3600} час";
            }

            return "не указано";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Автоматическая коррекция времени начала, если оно в прошлом
                CorrectPastStartTime();

                // Валидация данных
                if (!ValidateInput())
                    return;

                // Сохраняем настройки
                SaveSettings();

                // Создаем DisplayContent
                var displayContent = CreateDisplayContent();

                SettingsApplied?.Invoke(displayContent);

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CorrectPastStartTime()
        {
            if (scheduledDurationRadio.IsChecked == true)
            {
                if (DateTime.TryParseExact($"{startDatePicker.SelectedDate:yyyy-MM-dd} {FormatTime(startTimeTextBox.Text)}",
                    "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDateTime))
                {
                    var now = DateTime.Now;

                    if (startDateTime < now)
                    {
                        // Сохраняем разницу между началом и концом
                        if (DateTime.TryParseExact($"{endDatePicker.SelectedDate:yyyy-MM-dd} {FormatTime(endTimeTextBox.Text)}",
                            "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDateTime))
                        {
                            TimeSpan duration = endDateTime - startDateTime;

                            // Устанавливаем новое время начала - текущее время + 1 минута
                            var newStartDateTime = now.AddSeconds(15);
                            var newEndDateTime = newStartDateTime + duration;

                            // Обновляем элементы управления
                            startDatePicker.SelectedDate = newStartDateTime.Date;
                            startTimeTextBox.Text = newStartDateTime.ToString("HH:mm");
                            endDatePicker.SelectedDate = newEndDateTime.Date;
                            endTimeTextBox.Text = newEndDateTime.ToString("HH:mm");

                            // Показываем информационное сообщение
                            MessageBox.Show($"Время начала было автоматически скорректировано:\n" +
                                          $"Новое время: {newStartDateTime:dd.MM.yyyy HH:mm} - {newEndDateTime:dd.MM.yyyy HH:mm}",
                                          "Коррекция времени",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);

                            // Обновляем сводку
                            UpdateSummary();
                        }
                    }
                }
            }
        }

        private DisplayContent CreateDisplayContent()
        {
            var displayContent = new DisplayContent
            {
                ContentType = FileType,
                ContentValue = FilePath,
                Name = System.IO.Path.GetFileName(FilePath),
                IsLoop = LoopPlayback,
                Priority = 1,
                IsActive = true
            };

            // УНИФИЦИРОВАННАЯ ЛОГИКА РЕЖИМОВ
            if (IsScheduled)
            {
                displayContent.ContentMode = "SCHEDULE";
                displayContent.StartDateTime = StartDateTime;
                displayContent.EndDateTime = EndDateTime;
                // DisplayDuration = null - определяется периодом расписания
            }
            else if (IsInfinite)
            {
                displayContent.ContentMode = "SIMPLE";
                displayContent.DisplayDuration = 0; // 0 = бесконечное воспроизведение
                displayContent.StartDateTime = DateTime.Now;
                displayContent.EndDateTime = DateTime.MaxValue;
            }
            else
            {
                displayContent.ContentMode = "SIMPLE";
                displayContent.DisplayDuration = Duration;
                displayContent.StartDateTime = DateTime.Now;
                displayContent.EndDateTime = DateTime.Now.AddSeconds(Duration);
            }

            return displayContent;
        }

        private int? GetDisplayDuration()
        {
            if (IsScheduled)
            {
                // Для расписания длительность = null (определяется периодом)
                return null;
            }
            else if (IsInfinite)
            {
                // Бесконечное воспроизведение
                return 0;
            }
            else
            {
                // Ограниченное время
                if (Duration > 0)
                    return Duration;
                else
                    return null;
            }
        }

        private bool ValidateInput()
        {
            // Проверка периода для расписания
            if (scheduledDurationRadio.IsChecked == true)
            {
                if (!DateTime.TryParseExact($"{startDatePicker.SelectedDate:yyyy-MM-dd} {FormatTime(startTimeTextBox.Text)}",
                    "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start) ||
                    !DateTime.TryParseExact($"{endDatePicker.SelectedDate:yyyy-MM-dd} {FormatTime(endTimeTextBox.Text)}",
                    "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
                {
                    MessageBox.Show("Укажите корректные дату и время", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (start >= end)
                {
                    MessageBox.Show("Время окончания должно быть позже времени начала", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Проверяем, что период не слишком длинный (опционально)
                TimeSpan duration = end - start;
                if (duration.TotalDays > 365)
                {
                    MessageBox.Show("Период расписания не может превышать 1 год", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            else if (finiteDurationRadio.IsChecked == true)
            {
                if (!int.TryParse(durationTextBox.Text, out int durationValue) || durationValue <= 0)
                {
                    MessageBox.Show("Укажите корректную длительность воспроизведения", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        private void SaveSettings()
        {
            IsScheduled = scheduledDurationRadio.IsChecked == true;
            IsInfinite = infiniteDurationRadio.IsChecked == true;
            LoopPlayback = loopCheckBox.IsChecked == true;

            if (IsScheduled)
            {
                // Сохраняем период
                StartDateTime = DateTime.ParseExact($"{startDatePicker.SelectedDate:yyyy-MM-dd} {FormatTime(startTimeTextBox.Text)}",
                    "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                EndDateTime = DateTime.ParseExact($"{endDatePicker.SelectedDate:yyyy-MM-dd} {FormatTime(endTimeTextBox.Text)}",
                    "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                // Для режима расписания длительность = 0 (определяется периодом)
                Duration = 0;
            }
            else if (IsInfinite)
            {
                // Бесконечный режим
                Duration = 0;
                StartDateTime = null;
                EndDateTime = null;
            }
            else
            {
                // Ограниченное время - сохраняем длительность
                StartDateTime = null;
                EndDateTime = null;

                if (int.TryParse(durationTextBox.Text, out int durationValue))
                {
                    switch (durationUnitCombo.SelectedIndex)
                    {
                        case 0: // секунды
                            Duration = durationValue;
                            break;
                        case 1: // минуты
                            Duration = durationValue * 60;
                            break;
                        case 2: // часы
                            Duration = durationValue * 3600;
                            break;
                        default:
                            Duration = durationValue;
                            break;
                    }
                }
                else
                {
                    Duration = 0;
                }
            }
        }
    }
}
