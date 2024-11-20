using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public sealed class SedimentFractionsEditorViewModel : INotifyPropertyChanged
    {
        private IEventedList<ISedimentFraction> objectModelSedimentFractions = new EventedList<ISedimentFraction>();
        private IEventedList<ISedimentProperty> objectModelSedimentOverallProperties;
        private ISedimentFraction currentSedimentFraction;

        #region ObjectModel specific

        public IEventedList<ISedimentFraction> ObjectModelSedimentFractions
        {
            get
            {
                return objectModelSedimentFractions;
            }
            set
            {
                objectModelSedimentFractions = value;
                OnPropertyChanged("SedimentFractions");
                OnPropertyChanged("CurrentSedimentFraction");
                OnPropertyChanged("CurrentFractionName");
                OnPropertyChanged("FractionsVisible");
                OnPropertyChanged("FormulasVisible");
            }
        }

        public IEventedList<ISedimentProperty> ObjectModelSedimentOverallProperties
        {
            get
            {
                return objectModelSedimentOverallProperties;
            }
            set
            {
                objectModelSedimentOverallProperties = value;
                OnPropertyChanged("ObjectModelSedimentOverallProperties");
                OnPropertyChanged("CurrentSedimentFraction");
                OnPropertyChanged("CurrentFractionName");
                OnPropertyChanged("FractionsVisible");
                OnPropertyChanged("FormulasVisible");
            }
        }

        public ObservableCollection<ISedimentFraction> SedimentFractions
        {
            get
            {
                return new ObservableCollection<ISedimentFraction>(ObjectModelSedimentFractions);
            }
        }

        public ISedimentFraction CurrentSedimentFraction
        {
            get
            {
                if (currentSedimentFraction != null)
                {
                    return currentSedimentFraction;
                }

                ISedimentFraction firstSedimentFraction = SedimentFractions.FirstOrDefault();
                if (firstSedimentFraction == null)
                {
                    return null;
                }

                CurrentSedimentFraction = firstSedimentFraction;
                return currentSedimentFraction;
            }
            set
            {
                currentSedimentFraction = value;
                if (currentSedimentFraction != null)
                {
                    CurrentFractionName = currentSedimentFraction.Name;
                }

                OnPropertyChanged("CurrentFractionName");
                OnPropertyChanged("CurrentSedimentFraction");
                OnPropertyChanged("CurrentSedimentType");
                OnPropertyChanged("SedimentTypes");
                OnPropertyChanged("CurrentFormulaType");
                OnPropertyChanged("FormulaTypes");
                OnPropertyChanged("FormulasVisible");
                OnPropertyChanged("CurrentSedimentGuiProperties");
                OnPropertyChanged("CurrentFormulaGuiProperties");
            }
        }

        public List<ISedimentType> SedimentTypes
        {
            get
            {
                return CurrentSedimentFraction != null
                           ? CurrentSedimentFraction.AvailableSedimentTypes
                           : new List<ISedimentType>();
            }
        }

        public ISedimentType CurrentSedimentType
        {
            get
            {
                return CurrentSedimentFraction != null
                           ? CurrentSedimentFraction.CurrentSedimentType
                           : SedimentTypes.FirstOrDefault();
            }
            set
            {
                if (CurrentSedimentFraction != null)
                {
                    CurrentSedimentFraction.CurrentSedimentType = value;
                }

                OnPropertyChanged("CurrentSedimentType");
                OnPropertyChanged("CurrentFormulaType");
                OnPropertyChanged("FormulaTypes");
                OnPropertyChanged("FormulasVisible");
                OnPropertyChanged("CurrentSedimentGuiProperties");
                OnPropertyChanged("CurrentFormulaGuiProperties");
            }
        }

        public IEnumerable<ISedimentProperty> CurrentSedimentGuiProperties
        {
            get
            {
                return CurrentSedimentType != null
                           ? CurrentSedimentType.Properties.Where(p => p.IsVisible)
                           : Enumerable.Empty<ISedimentProperty>();
            }
        }

        public List<ISedimentFormulaType> FormulaTypes
        {
            get
            {
                return CurrentSedimentFraction != null
                           ? CurrentSedimentFraction.SupportedFormulaTypes
                           : new List<ISedimentFormulaType>();
            }
        }

        public ISedimentFormulaType CurrentFormulaType
        {
            get
            {
                return CurrentSedimentFraction != null
                           ? CurrentSedimentFraction.CurrentFormulaType
                           : FormulaTypes.FirstOrDefault();
            }
            set
            {
                if (CurrentSedimentFraction != null)
                {
                    CurrentSedimentFraction.CurrentFormulaType = value;
                }

                OnPropertyChanged("CurrentFormulaType");
                OnPropertyChanged("CurrentFormulaGuiProperties");
            }
        }

        public IEnumerable<ISedimentProperty> CurrentFormulaGuiProperties
        {
            get
            {
                return CurrentFormulaType != null
                           ? CurrentFormulaType.Properties.Where(p => p.IsVisible)
                           : Enumerable.Empty<ISedimentProperty>();
            }
        }

        public string CurrentFractionName { get; set; }

        #endregion

        #region ViewModel specific

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Visibility FractionsVisible
        {
            get
            {
                return SedimentFractions.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public Visibility FormulasVisible
        {
            get
            {
                return FormulaTypes.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        #endregion

        #region Add / Remove Fractions

        public ICommand OnAddCommand
        {
            get
            {
                return new SedimentFractionsEditorRelayCommand(AddFraction);
            }
        }

        public ICommand OnRemoveCommand
        {
            get
            {
                return new SedimentFractionsEditorRelayCommand(RemoveFraction);
            }
        }

        private void AddFraction()
        {
            if (string.IsNullOrEmpty(CurrentFractionName))
            {
                return;
            }

            List<string> names = SedimentFractions.Select(l => l.Name).ToList();
            if (names.Contains(CurrentFractionName))
            {
                var pattern = @"\d+$";
                var replacement = "";
                var rgx = new Regex(pattern);
                string result = rgx.Replace(CurrentFractionName, replacement);
                CurrentFractionName = NamingHelper.GenerateUniqueNameFromList(result + "{0:D2}", true, names);
            }

            var newFraction = new SedimentFraction() {Name = CurrentFractionName};
            ObjectModelSedimentFractions.Add(newFraction);

            OnPropertyChanged("SedimentFractions");
            OnPropertyChanged("CurrentFractionName");
            OnPropertyChanged("FractionsVisible");

            CurrentSedimentFraction = newFraction;
        }

        private void RemoveFraction()
        {
            if (CurrentSedimentFraction == null)
            {
                return;
            }

            ObjectModelSedimentFractions.Remove(CurrentSedimentFraction);
            CurrentFractionName = CurrentSedimentFraction == null ? string.Empty : CurrentSedimentFraction.Name;

            OnPropertyChanged("SedimentFractions");
            OnPropertyChanged("CurrentFractionName");
            OnPropertyChanged("FractionsVisible");

            CurrentSedimentFraction = SedimentFractions.LastOrDefault();
        }

        #endregion
    }
}