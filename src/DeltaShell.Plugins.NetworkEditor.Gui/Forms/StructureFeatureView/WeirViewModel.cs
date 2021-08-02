using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public class WeirViewModel : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WeirViewModel));
        private IWeir weir;
        private SelectableWeirFormulaType selectedWeirType;

        private double lowerEdgeLevel;
        private double previousCrestLevel;
        public Func<IWeir, TimeSeries> GetTimeSeriesEditor { get; set; }

        public IWeir Weir
        {
            get { return weir; }
            set
            {
                if (weir == value) return;

                if (weir != null)
                {
                    ((INotifyPropertyChanged)weir).PropertyChanged -= WeirPropertyChanged;
                    ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged -= WeirFormulaPropertyChanged;
                }

                weir = value;

                if (weir != null)
                {
                    selectedWeirType = GetSelectableWeirFormulaType(weir.WeirFormula);
                    previousCrestLevelTimeSeriesValue = weir.UseCrestLevelTimeSeries;
                    UpdateControls();
                    OnPropertyChanged(nameof(Weir));
                    OnPropertyChanged(nameof(HasWeir));
                    OnPropertyChanged(nameof(SelectedWeirType));
                    OnPropertyChanged(nameof(GateOpeningHeight));
                    OnPropertyChanged(nameof(GateOpeningWidth));
                    OnPropertyChanged(nameof(LowerEdgeLevel));
                    OnPropertyChanged(nameof(EnableCrestLevelTimeSeries));
                    OnPropertyChanged(nameof(IsCrestLevelConstantTime));
                    OnPropertyChanged(nameof(UseVelocityHeight));
                    OnPropertyChanged(nameof(SelectedGateOpeningHorizontalDirection));

                    ((INotifyPropertyChanged)weir).PropertyChanged += WeirPropertyChanged;
                    ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;

                }
            }
        }

        public bool HasWeir
        {
            get { return weir != null; }
        }

        public double GateOpeningHeight
        {
            get
            {
                if (!HasWeir) return 0;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return 0; 
                }
                return gatedWeirFormula.GateOpening;
            }
            set
            {
                if(!HasWeir) return;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return; 
                }
                
                if (gatedWeirFormula.GateOpening != value)
                {
                    gatedWeirFormula.GateOpening = value;

                    lowerEdgeLevel = value + BedLevelStructureCentre;

                    OnPropertyChanged(nameof(GateOpeningHeight));
                    OnPropertyChanged(nameof(LowerEdgeLevel));
                }
            }
        }

        public double GateHeight
        {
            get
            {
                if (!HasWeir) return 0;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return 0;
                }
                return gatedWeirFormula.GateHeight;
            }
            set
            {
                if (!HasWeir) return;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return;
                }

                if (gatedWeirFormula.GateHeight != value)
                {
                    gatedWeirFormula.GateHeight = value;

                    OnPropertyChanged(nameof(GateHeight));
                }
            }
        }

        public double GateOpeningWidth
        {
            get
            {
                if (!HasWeir) return 0;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null) return 0;

                return gatedWeirFormula.GateOpeningWidth;
            }
            set
            {
                if (!HasWeir) return;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null) return;
                if (gatedWeirFormula.GateOpeningWidth != value)
                {
                    gatedWeirFormula.GateOpeningWidth = value;

                    OnPropertyChanged(nameof(GateOpeningWidth));
                }

            }
        }

        public SelectableWeirFormulaType SelectedWeirType
        {
            get
            {
                return selectedWeirType;
            }
            set
            {
                if(!HasWeir || weir.WeirFormula == null) return;
                ((INotifyPropertyChanged) weir.WeirFormula).PropertyChanged -= WeirFormulaPropertyChanged;

                if (value == SelectableWeirFormulaType.SimpleWeir)
                {
                    weir.WeirFormula = new SimpleWeirFormula();
                    SetSimpleWeirControls();
                }
                if (value == SelectableWeirFormulaType.GeneralStructure)
                {
                    weir.WeirFormula = new GeneralStructureWeirFormula();
                    SetGeneralStructureViewControls();

                    lowerEdgeLevel = GateOpeningHeight + BedLevelStructureCentre;
                }

                ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;

                selectedWeirType = value;
                OnPropertyChanged(nameof(SelectedWeirType));
                OnPropertyChanged(nameof(GateOpeningHeight));
                OnPropertyChanged(nameof(GateOpeningWidth));
                OnPropertyChanged(nameof(LowerEdgeLevel));
            }
        }

        public GateOpeningDirection SelectedGateOpeningHorizontalDirection
        {
            get
            {
                if (!HasWeir) return 0;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null) return 0;

                return gatedWeirFormula.GateOpeningHorizontalDirection;
            }
            set
            {
                if (!HasWeir || weir.WeirFormula == null) return;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null) return;
                if (gatedWeirFormula.GateOpeningHorizontalDirection != value)
                {
                    gatedWeirFormula.GateOpeningHorizontalDirection = value;

                    OnPropertyChanged(nameof(SelectedGateOpeningHorizontalDirection));
                }
                
            }
        }

        public double BedLevelStructureCentre
        {
            get
            {
                if (!HasWeir) return 0;
                var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return 0;
                }
                return gatedWeirFormula.BedLevelStructureCentre;
            }
        }

        public double LowerEdgeLevel
        {
            get
            {
                lowerEdgeLevel = GateOpeningHeight + BedLevelStructureCentre;
                return lowerEdgeLevel;
            }
            set
            {
                if (lowerEdgeLevel != value)
                {
                    lowerEdgeLevel = value;

                    if(!HasWeir) return;
                    var gatedWeirFormula = Weir.WeirFormula as GeneralStructureWeirFormula;
                    if (gatedWeirFormula == null)
                    {
                        return;
                    }

                    GateOpeningHeight = lowerEdgeLevel - BedLevelStructureCentre;
                    OnPropertyChanged(nameof(LowerEdgeLevel));
                }
            }
        }

        #region Gui enabling controls

        private void SetGeneralStructureViewControls()
        {
            GateGroupboxEnabled = true;
            CrestLevelEnabled = false;
            if(EnableCrestLevelTimeSeries != previousCrestLevelTimeSeriesValue)
                previousCrestLevelTimeSeriesValue = EnableCrestLevelTimeSeries;
            EnableCrestLevelTimeSeries = false;
        }

        private void SetSimpleWeirControls()
        {
            GateGroupboxEnabled = false;
            CrestLevelEnabled = true;
            EnableCrestLevelTimeSeries = previousCrestLevelTimeSeriesValue;
        }

        public Visibility SimpleWeirPropertiesVisibility
        {
            get
            {
                if (CrestLevelEnabled)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private bool CrestLevelEnabled
        {
            get { return crestLevelEnabled; }
            set
            {
                crestLevelEnabled = value;
                OnPropertyChanged(nameof(SimpleWeirPropertiesVisibility));
            }
        }
        
        public bool GateGroupboxEnabled
        {
            get { return gateGroupboxEnabled; }
            set
            {
                gateGroupboxEnabled = value;
                OnPropertyChanged(nameof(GateGroupboxEnabled));
            }
        }

        public bool EnableAdvancedSettings
        {
            get { return enableAdvancedSettings; }
            set
            {
                enableAdvancedSettings = value;
                OnPropertyChanged(nameof(EnableAdvancedSettings));
            }
        }

        public bool UseVelocityHeight
        {
            get { return Weir?.UseVelocityHeight ?? true; }
            set
            {
                if (Weir.UseVelocityHeight == value) return;
                Weir.UseVelocityHeight = value;
                OnPropertyChanged(nameof(UseVelocityHeight));
            }
        }

        public bool IsCrestLevelConstantTime { get { return weir!= null && !weir.UseCrestLevelTimeSeries; } }

        private bool previousCrestLevelTimeSeriesValue;

        public bool EnableCrestLevelTimeSeries
        {
            get { return weir?.UseCrestLevelTimeSeries ?? false; }
            set
            {
                if (weir.UseCrestLevelTimeSeries == value) return; //Avoid useless propagation of events.
                try
                {
                    if (value && SelectedWeirType == SelectableWeirFormulaType.GeneralStructure)
                    {
                        //Log error
                        Log.ErrorFormat(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, Weir.Name);
                    }
                    else
                    {
                        weir.UseCrestLevelTimeSeries = value;
                    }
                }
                catch (Exception)
                {
                    //Log error
                    Log.ErrorFormat(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, Weir.Name);
                }

                OnPropertyChanged(nameof(EnableCrestLevelTimeSeries));
                OnPropertyChanged(nameof(IsCrestLevelConstantTime));
            }
        }

        private bool gateGroupboxEnabled;
        private bool crestLevelEnabled;
        private bool enableAdvancedSettings;

        #endregion

        public ICommand OnEditCrestLevelTimeSeries
        {
            get { return new RelayCommand(param => EditCrestLevelTimeSeries()); }
        }

        private void EditCrestLevelTimeSeries()
        {
            if(!Weir.CanBeTimedependent)
                Log.ErrorFormat(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, Weir.Name);
            
            var result = GetTimeSeriesEditor?.Invoke(Weir);
            if (result != null)
            {
                Weir.CrestLevelTimeSeries.Time.Clear();
                Weir.CrestLevelTimeSeries.Components[0].Clear();
                Weir.CrestLevelTimeSeries.Time.SetValues(result.Time.Values);
                Weir.CrestLevelTimeSeries.Components[0].SetValues(result.Components[0].Values.Cast<double>());
            }
        }

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void WeirPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IWeir.WeirFormula))
            {
                selectedWeirType = GetSelectableWeirFormulaType(weir.WeirFormula);
                OnPropertyChanged(nameof(SelectedWeirType));
            }

            if (e.PropertyName == nameof(IWeir.UseCrestLevelTimeSeries))
            {
                OnPropertyChanged(nameof(EnableCrestLevelTimeSeries));
                OnPropertyChanged(nameof(IsCrestLevelConstantTime));
            }
        }

        private void WeirFormulaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeneralStructureWeirFormula.BedLevelStructureCentre))
            {
                var gatedWeirFormula = weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return;
                }
                lowerEdgeLevel += (BedLevelStructureCentre - previousCrestLevel);
                previousCrestLevel = BedLevelStructureCentre;

                OnPropertyChanged(nameof(LowerEdgeLevel));
            }
        }

        private void UpdateControls()
        {
            if (selectedWeirType == SelectableWeirFormulaType.GeneralStructure)
            {
                SetGeneralStructureViewControls();
            }
            if (selectedWeirType == SelectableWeirFormulaType.SimpleWeir)
            {
                SetSimpleWeirControls();
            }
        }

        private SelectableWeirFormulaType GetSelectableWeirFormulaType(IWeirFormula formula)
        {
            if (formula is SimpleWeirFormula)
            {
                return SelectableWeirFormulaType.SimpleWeir;
            }
            if (formula is GeneralStructureWeirFormula)
            {
                return SelectableWeirFormulaType.GeneralStructure;
            }

            throw new NotSupportedException(string.Format(Resources.WeirViewModel_GetSelectableWeirFormulaType_This_formula_type____0___is_not__yet__supported, formula.Name));
        }
    }
}
