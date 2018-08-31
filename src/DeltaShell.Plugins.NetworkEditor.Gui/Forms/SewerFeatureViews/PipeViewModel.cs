using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    //[Entity]
    public class PipeViewModel : INotifyPropertyChanged
    {
        private Pipe pipe;

        public Pipe Pipe
        {
            get { return pipe; }
            set
            {
                if (pipe != null)
                {
                    ((INotifyPropertyChanged)pipe).PropertyChanged -= OnPropertyChanged;
                }
                pipe = value;
                
                if (pipe != null)
                {
                    ((INotifyPropertyChanged)pipe).PropertyChanged += OnPropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName<Pipe>(p => p.LevelTarget) ||
                e.PropertyName == TypeUtils.GetMemberName<Pipe>(p => p.LevelSource) ||
                e.PropertyName == TypeUtils.GetMemberName<Pipe>(p => p.Length))
            {
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        public double PipeSlope
        {
            get { return pipe?.Slope() ?? 0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}