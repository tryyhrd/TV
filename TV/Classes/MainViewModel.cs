using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TV.Classes
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public ObservableCollection<Display> Displays { get; } = new ObservableCollection<Display>();

        private string activeDisplaysString = "Выбрано: 0";
        public string ActiveDisplaysString
        {
            get => activeDisplaysString;
            set
            {
                activeDisplaysString = value;
                OnPropertyChanged(nameof(ActiveDisplaysString));
            }
        }

        public MainViewModel()
        {
            Displays.CollectionChanged += Displays_CollectionChanged;
        }

        private void Displays_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Display display in e.NewItems)
                {
                    display.PropertyChanged += Display_PropertyChanged;
                }
            }

            UpdateActiveDisplaysString();
        }

        private void Display_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Display.IsSelected))
            {
                UpdateActiveDisplaysString();
            }
        }

        public void UpdateActiveDisplaysString()
        {
            ActiveDisplaysString = $"Выбрано: {Displays.Count(x => x.IsSelected)}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
