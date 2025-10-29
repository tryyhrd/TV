namespace TV.Classes.Common
{
    using Display;
    using System;

    public static class DisplayContentHelper
    {
        public static void FillFromDisplayContent(this Display display, DisplayContent content)
        {
            display.CurrentContent = content.Name ?? "Неизвестный контент";
            display.ContentType = GetContentTypeDisplay(content.ContentType);
            display.Status = GetStatusDisplay(content);
        }

        private static string GetContentTypeDisplay(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            return "Медиафайл";

            switch (contentType.ToLower())
            {
                case "video":
                    return "Видеофайл";
                case "image":
                    return "Изображение";
                case "audio":
                    return "Аудиофайл";
                case "web":
                    return "Веб-страница";
                default:
                    return "Медиафайл";
            }
        }

        private static string GetContentDisplayName(DisplayContent content)
        {
            if (content == null || string.IsNullOrEmpty(content.ContentValue))
                return "Неизвестный контент";

            var fileName = System.IO.Path.GetFileName(content.ContentValue);
            var contentType = content.ContentType?.ToLower();

            switch (contentType)
            {
                case "video":
                    return $"📹 {fileName}";
                case "image":
                    return $"🖼️ {fileName}";
                case "audio":
                    return $"🔊 {fileName}";
                case "web":
                    return $"🌐 {content.ContentValue}";
                default:
                    return $"📄 {fileName}";
            }
        }

        private static string GetStatusDisplay(DisplayContent content)
        {
            if (content.ContentMode == "SCHEDULE" && content.StartDateTime.HasValue && content.EndDateTime.HasValue)
            {
                var now = DateTime.Now;
                if (now >= content.StartDateTime.Value && now <= content.EndDateTime.Value)
                {
                    var timeLeft = content.EndDateTime.Value - now;
                    if (timeLeft.TotalHours >= 1)
                    {
                        return $"Расписание: {timeLeft.Hours}ч {timeLeft.Minutes}м осталось";
                    }
                    else
                    {
                        return $"Расписание: {timeLeft.Minutes}м осталось";
                    }
                }
                else if (now < content.StartDateTime.Value)
                {
                    return $"Начнется: {content.StartDateTime.Value:dd.MM.yyyy HH:mm}";
                }
                else
                {
                    return "Расписание завершено";
                }
            }
            else if (content.DisplayDuration.HasValue && content.DisplayDuration.Value == 0)
            {
                return "Бесконечное воспроизведение";
            }
            else if (content.DisplayDuration.HasValue && content.DisplayDuration.Value > 0)
            {
                return $"Воспроизведение: {content.DisplayDuration.Value} сек";
            }
            else if (content.ContentMode == "PLAYLIST")
            {
                return "Плейлист установлен";
            }
            else
            {
                return "Контент установлен";
            }
        }
    }
}
