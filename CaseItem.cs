using System.Windows.Media;
using System.ComponentModel;

namespace GradescopeIOViewer
{
    public class CaseItem : INotifyPropertyChanged
    {
        private string _name;
        private Brush _color;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public Brush Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        public CaseItem(string name, Brush color)
        {
            _name = name;
            _color = color;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
