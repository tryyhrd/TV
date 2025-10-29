using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TV.Classes.Display
{
    public class Display: INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; } = "Неактивен";
        public Screen Screen { get; set; }

        public bool IsPrimary { get; set; }

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
                new DisplayViewModel().UpdateActiveDisplaysString();
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

        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
