using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class LeveeBreachViewModel : INotifyPropertyChanged
    {
        private bool useActive = true;
        private LeveeBreach leveeBreach;
        
        public LeveeBreach LeveeBreach
        {
            get { return leveeBreach; }
            set
            {
                leveeBreach = value;
                if (leveeBreach != null)
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedGrowthFormula));
                    OnPropertyChanged(nameof(LeveeBreachSettings));
                    OnPropertyChanged(nameof(UseActive));
                }
            }
        }

        public LeveeBreachGrowthFormula SelectedGrowthFormula
        {
            get { return LeveeBreach?.LeveeBreachFormula ?? LeveeBreachGrowthFormula.VerweijvdKnaap2002; }
            set
            {
                if (LeveeBreach == null) return;
                LeveeBreach.LeveeBreachFormula = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LeveeBreachSettings));
            }
        }

        [ExcludeFromCodeCoverage]
        public bool UseActive
        {
            get { return useActive; }
            set
            {
                useActive = value;
                OnPropertyChanged();
            }
        }

        public LeveeBreachSettings LeveeBreachSettings
        {
            get { return LeveeBreach?.GetLeveeBreachSettings(); }
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}