using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    public class BreachGrowthSetting : INotifyPropertyChanged
    {
        private double height;
        private double width;
        private TimeSpan timeSpan;

        public TimeSpan TimeSpan
        {
            get { return timeSpan; }
            set
            {
                timeSpan = value;
                OnPropertyChanged();
            }
        }

        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Area));
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Area));
            }
        }

        public double Area
        {
            get { return Width * Height; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}