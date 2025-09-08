using System.ComponentModel;
using System.Windows.Forms;

namespace TV.Classes
{
    public class Display: INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; } = "Неактивен";
        public Screen Screen { get; set; }

        private bool _isSelected;
        private string _currentContent = "Нет";
        private string _contentType = "Нет";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string CurrentContent
        {
            get => _currentContent;
            set
            {
                _currentContent = value;
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        public string ContentType
        {
            get => _contentType;
            set
            {
                _contentType = value;
                OnPropertyChanged(nameof(ContentType));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnSelectionChanged(bool isSelected)
        {
            isSelected = !isSelected;
        }
    }
}
