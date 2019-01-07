using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public class WeirViewModel : IDisposable, INotifyPropertyChanged
    {
        #region Fields
        private IWeir weir;
        private SelectableWeirFormulaType selectedWeirType;
        private double lowerEdgeLevel;
        private double upstream1CrestLevel;
        private double upstream2CrestLevel;
        private double upstream1CrestWidth;
        private double upstream2CrestWidth;
        private double downstream1CrestLevel;
        private double downstream2CrestLevel;
        private double downstream1CrestWidth;
        private double downstream2CrestWidth;
        private double horizontalDoorOpeningWidth;
        private double previousCrestLevel;
        private bool previousCrestLevelTimeSeriesValue;
        private bool previousLowerEdgeLevelTimeSeriesValue;
        private bool gatedWeirPropertiesEnabled;
        private bool previousHorizontalDoorOpeningWidthTimeSeriesValue;
        private bool gateGroupboxEnabled;
        private bool simpleGateGroupBoxEnabled;
        private bool crestLevelEnabled;
        private bool lowerEdgeLevelEnabled;
        private bool horizontalDoorOpeningWidthEnabled;
        private bool enableAdvancedSettings;
        private bool _generalStructureVisibility;
        private bool generalStructureEnabled;
        private bool timeSeriesEnabled;
        private double gateOpeningHeight;
        public bool coefficientsEnabled;
        #endregion

        #region Functions/Delegates
        public Func<IWeir, TimeSeries> GetTimeSeriesEditorForCrestLevel { get; set; }
        public Func<IGatedWeirFormula, TimeSeries> GetTimeSeriesEditorForEdgeLevel { get; set; }
        public Func<IGatedWeirFormula, TimeSeries> GetTimeSeriesEditorForHorizontalDoorOpeningWidth { get; set; }

        #endregion

        #region Commands
        public ICommand OnEditCrestLevelTimeSeries
        {
            get { return new RelayCommand(param => EditCrestLevelTimeSeries()); }
        }
        public ICommand OnEditLowerEdgeLevelTimeSeries
        {
            get { return new RelayCommand(param => EditLowerEdgeLevelTimeSeries()); }
        }
        public ICommand OnEditHorizontalDoorOpeningWidthTimeSeries
        {
            get { return new RelayCommand(param => EditHorizontalDoorOpeningWidthTimeSeries()); }
        }

        #endregion

        #region Model Properties

        public IWeir Weir
        {
            get
            {
                return weir;
            }
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

                    SetPreviousCrestLevelTimeSeries();

                    SetPreviousLowerEdgeLevelTimeSeries();

                    SetPreviousHorizontalDoorOpeningWidthTimeSeries();

                    UpdateControls();

                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Weir));
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.HasWeir));

                    SynchronizeCommonProperties();

                    SynchronizeGatedOrGeneralStructureProperties();

                    ((INotifyPropertyChanged)weir).PropertyChanged += WeirPropertyChanged;
                    ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;
                }
            }
        }

        private void SetPreviousCrestLevelTimeSeries()
        {
            if (EnableCrestLevelTimeSeries)
            {
                previousCrestLevelTimeSeriesValue = weir.UseCrestLevelTimeSeries;
            }
        }

        private void SynchronizeGatedOrGeneralStructureProperties()
        {
            if (weir?.WeirFormula is GatedWeirFormula)
            {
                SynchronizeGatedProperties();
            }
            else if (weir?.WeirFormula is GeneralStructureWeirFormula)
            {
                SynchronizeGatedProperties();
                SynchronizeGeneralStructureProperties();
            }
        }

        private void SetPreviousHorizontalDoorOpeningWidthTimeSeries()
        {
            if (EnableHorizontalDoorOpeningWidthTimeSeries)
            {
                var gatedWeir = weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeir != null)
                {
                    previousHorizontalDoorOpeningWidthTimeSeriesValue = gatedWeir.UseHorizontalDoorOpeningWidthTimeSeries;
                }
            }
        }

        private void SetPreviousLowerEdgeLevelTimeSeries()
        {
            if (EnableLowerEdgeLevelTimeSeries)
            {
                var gatedWeir = weir?.WeirFormula as IGatedWeirFormula;

                if (gatedWeir != null)
                {
                    previousLowerEdgeLevelTimeSeriesValue = gatedWeir.UseLowerEdgeLevelTimeSeries;
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
                gateOpeningHeight = LowerEdgeLevel - BedLevelStructureCentre;
                return gateOpeningHeight;
            }

            set
            {
                gateOpeningHeight = value;
                lowerEdgeLevel = value + BedLevelStructureCentre;

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
            }
        }


        public double DoorHeight
        {
            get
            {
                if(weir?.WeirFormula == null) {return 0;}

                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;

                return gatedWeirFormula.DoorHeight;
            }
            set
            {
                if (weir?.WeirFormula == null) { return; }

                var gatedWeirFormula = Weir.WeirFormula as IGatedWeirFormula;

                if (gatedWeirFormula == null)
                {
                    return;
                }

                if (gatedWeirFormula.DoorHeight != value)
                {
                    gatedWeirFormula.DoorHeight = value;

                    OnPropertyChanged();
                }
            }
        }

        private IGatedWeirFormula gatedWeirFormula;
        public GateOpeningDirection SelectedDoorOpeningHeightDirectionType
        {
            get
            {
                gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula != null)
                {
                    if (gatedWeirFormula is GatedWeirFormula)
                    {
                        return gatedWeirFormula.HorizontalDoorOpeningDirection;
                    }

                    if (gatedWeirFormula is GeneralStructureWeirFormula)
                    {
                        return GateOpeningDirection.Symmetric;
                    }
                }

                return GateOpeningDirection.Symmetric;
            }
            set
            {
                gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula != null)
                {
                    if (gatedWeirFormula is GatedWeirFormula)
                    {
                        gatedWeirFormula.HorizontalDoorOpeningDirection = value;
                    }
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SelectedDoorOpeningHeightDirectionType));
            }

        }

        public double Upstream1CrestWidth
        {
            get { return upstream1CrestWidth; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthLeftSideOfStructure = value;
                    upstream1CrestWidth = value;
                }
            }
        }

        public double Upstream1CrestLevel
        {
            get { return upstream1CrestLevel; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelLeftSideOfStructure = value;
                    upstream1CrestLevel = value;
                }
            }
        }

        public double Upstream2CrestWidth
        {
            get { return upstream2CrestWidth; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthStructureLeftSide = value;
                    upstream2CrestWidth = value;
                }
            }
        }

        public double Upstream2CrestLevel
        {
            get { return upstream2CrestLevel; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelLeftSideStructure = value;
                    upstream2CrestLevel = value;
                }
            }
        }

        public double Downstream1CrestWidth
        {
            get { return downstream1CrestWidth; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthStructureRightSide = value;
                    downstream1CrestWidth = value;
                }
            }
        }

        public double Downstream1CrestLevel
        {
            get { return downstream1CrestLevel; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelRightSideStructure = value;
                    downstream1CrestLevel = value;
                }
            }
        }

        public double Downstream2CrestWidth
        {
            get { return downstream2CrestWidth; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthRightSideOfStructure = value;
                    downstream2CrestWidth = value;
                }
            }
        }

        public double Downstream2CrestLevel
        {
            get { return downstream2CrestLevel; }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelRightSideOfStructure = value;
                    downstream2CrestLevel = value;
                }
            }
        }

        public double BedLevelStructureCentre
        {
            get
            {
                if (weir?.WeirFormula == null) { return 0; }

                return Weir.CrestLevel;
            }
            set
            {
                if (weir?.WeirFormula != null)
                {
                    Weir.CrestLevel = value;
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.BedLevelStructureCentre));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
            }
        }

        public double LowerEdgeLevel
        {
            get
            {
                if (weir?.WeirFormula == null) { return 0; }

                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula == null) return 0;



                return gatedWeirFormula.LowerEdgeLevel;
            }
            set
            {
                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula != null)
                {
                    gatedWeirFormula.LowerEdgeLevel = value;
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));

            }
        }

        public double HorizontalDoorOpeningWidth
        {
            get
            {
                if (weir?.WeirFormula == null) { return 0; }


                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula == null) return 0;

                return gatedWeirFormula.HorizontalDoorOpeningWidth;
            }
            set
            {
                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula != null)
                {
                    gatedWeirFormula.HorizontalDoorOpeningWidth = value;

                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.HorizontalDoorOpeningWidth));
                }
            }
        }

        public double ExtraResistance
        {
            get
            {
                if (weir?.WeirFormula == null) { return 0; }
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula == null) return 0;


                return generalStructureFormula.ExtraResistance;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.ExtraResistance = value;
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GeneralStructurePropertiesVisibility));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.ExtraResistance));

            }
        }

        public bool LowerEdgeLevelEnabled
        {
            get { return lowerEdgeLevelEnabled; }
            set
            {
                lowerEdgeLevelEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SimpleWeirPropertiesVisibility));
            }
        }

        public bool HorizontalDoorOpeningWidthEnabled
        {
            get { return horizontalDoorOpeningWidthEnabled; }
            set
            {
                horizontalDoorOpeningWidthEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SimpleWeirPropertiesVisibility));
            }
        }
        #endregion

        #region Methods: Gui enabling controls
        public SelectableWeirFormulaType SelectedWeirType
        {
            get
            {
                return selectedWeirType;
            }
            set
            {
                if (weir?.WeirFormula == null) return;

                ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged -= WeirFormulaPropertyChanged;

                switch (value)
                {
                    case SelectableWeirFormulaType.SimpleWeir:
                        weir.WeirFormula = new SimpleWeirFormula();
                        SetSimpleWeirControls();
                        break;
                    case SelectableWeirFormulaType.SimpleGate:
                        weir.WeirFormula = new GatedWeirFormula(true);
                        SetSimpleGateViewControls();
                        break;
                    case SelectableWeirFormulaType.GeneralStructure:
                        var generalStructureWeirFormula = new GeneralStructureWeirFormula
                        {
                            BedLevelStructureCentre = weir.CrestLevel,
                            WidthStructureCentre = weir.CrestWidth,
                        };

                        weir.WeirFormula = generalStructureWeirFormula;
                        SetGeneralStructureViewControls();

                        lowerEdgeLevel = GateOpeningHeight + BedLevelStructureCentre;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The selected weir type does not exist. Please select simple weir, simple gate or general structure");
                }

                ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;

                selectedWeirType = value;

                SynchronizeCommonProperties();

                if (value == SelectableWeirFormulaType.SimpleGate)
                {
                    SynchronizeGatedProperties();
                }
                else if (value == SelectableWeirFormulaType.GeneralStructure)
                {
                    SynchronizeGatedProperties();
                    SynchronizeGeneralStructureProperties();
                }
            }
        }

        private void UpdateControls()
        {
            if (selectedWeirType == SelectableWeirFormulaType.SimpleGate)
            {
                PersistValuesAfterOpeningSimpleGateView();
                if (Weir?.WeirFormula != null)

                {
                    return;
                }
                SetSimpleGateViewControls();
            }

            if (selectedWeirType == SelectableWeirFormulaType.GeneralStructure)
            {
                PersistValuesAfterOpeningGeneralStructureView();

                if (Weir.WeirFormula != null)
                {
                    return;
                }
                SetGeneralStructureViewControls();
            }

            if (selectedWeirType == SelectableWeirFormulaType.SimpleWeir)
            {
                PersistValuesAfterOpeningSimpleWeirView();

                if (Weir.WeirFormula != null)
                {
                    return;
                }
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

            if (formula is GatedWeirFormula)
            {
                return SelectableWeirFormulaType.SimpleGate;
            }

            throw new NotSupportedException(string.Format(
                Resources.WeirViewModel_GetSelectableWeirFormulaType_This_formula_type____0___is_not__yet__supported,
                formula.Name));
        }

        private void SetGeneralStructureViewControls()
        {
            ResetValuesAfterStructureTypeSwitch();
            PersistValuesAfterOpeningGeneralStructureView();
        }

        private void PersistValuesAfterOpeningGeneralStructureView()
        {
            ExtraResistanceEnabled = true;
            GateGroupboxEnabled = true;
            CrestLevelEnabled = false;
            LowerEdgeLevelEnabled = true;
            HorizontalDoorOpeningWidthEnabled = true;
            CoefficientsEnabled = true;
            GatedWeirPropertiesEnabled = false;
            if (EnableCrestLevelTimeSeries != previousCrestLevelTimeSeriesValue)
                previousCrestLevelTimeSeriesValue = EnableCrestLevelTimeSeries;
            if (EnableLowerEdgeLevelTimeSeries != previousLowerEdgeLevelTimeSeriesValue)
                previousLowerEdgeLevelTimeSeriesValue = EnableLowerEdgeLevelTimeSeries;
            if (EnableHorizontalDoorOpeningWidthTimeSeries != previousHorizontalDoorOpeningWidthTimeSeriesValue)
                previousHorizontalDoorOpeningWidthTimeSeriesValue = EnableHorizontalDoorOpeningWidthTimeSeries;
            EnableCrestLevelTimeSeries = previousCrestLevelTimeSeriesValue;
        }

        private void SetSimpleGateViewControls()
        {
            ResetValuesAfterStructureTypeSwitch();
            PersistValuesAfterOpeningSimpleGateView();
        }

        private void PersistValuesAfterOpeningSimpleGateView()
        {
            GatedWeirPropertiesEnabled = true;
            GateGroupboxEnabled = true;
            CrestLevelEnabled = false;
            LowerEdgeLevelEnabled = true;
            HorizontalDoorOpeningWidthEnabled = true;
            CoefficientsEnabled = false;
            EnableCrestLevelTimeSeries = previousCrestLevelTimeSeriesValue;
            EnableLowerEdgeLevelTimeSeries = previousLowerEdgeLevelTimeSeriesValue;
            EnableHorizontalDoorOpeningWidthTimeSeries = previousHorizontalDoorOpeningWidthTimeSeriesValue;
        }

        private void SetSimpleWeirControls()
        {
            ResetValuesAfterStructureTypeSwitch();
            PersistValuesAfterOpeningSimpleWeirView();
        }

        private void PersistValuesAfterOpeningSimpleWeirView()
        {
            CoefficientsEnabled = true;
            GateGroupboxEnabled = false;
            CrestLevelEnabled = true;
            LowerEdgeLevelEnabled = true;
            HorizontalDoorOpeningWidthEnabled = true;
            GatedWeirPropertiesEnabled = false;
            EnableCrestLevelTimeSeries = previousCrestLevelTimeSeriesValue;
            EnableLowerEdgeLevelTimeSeries = previousLowerEdgeLevelTimeSeriesValue;
            EnableHorizontalDoorOpeningWidthTimeSeries = previousHorizontalDoorOpeningWidthTimeSeriesValue;
        }

        private void ResetValuesAfterStructureTypeSwitch()
        {
            ExtraResistance = 0.0;
        }

        public Visibility SimpleWeirPropertiesVisibility
        {
            get
            {
                if (CrestLevelEnabled || LowerEdgeLevelEnabled || HorizontalDoorOpeningWidthEnabled)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }
        public Visibility GatedVisibility
        {
            get
            {
                if (!TimeSeriesEnabled)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public Visibility GeneralStructurePropertiesVisibility
        {
            get
            {
                if (weir != null && weir.WeirFormula != null && Weir.WeirFormula is GeneralStructureWeirFormula)
                {
                    return Visibility.Visible;

                }

                return Visibility.Collapsed;
            }
        }

        public bool GatedWeirPropertiesEnabled
        {
            get { return gatedWeirPropertiesEnabled; }
            set
            {
                gatedWeirPropertiesEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GatedWeirPropertiesEnabled));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
            }
        }

        public Visibility Coefficients
        {
            get
            {
                if (CoefficientsEnabled)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public bool CrestLevelEnabled
        {
            get { return crestLevelEnabled; }
            set
            {
                crestLevelEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SimpleWeirPropertiesVisibility));
            }
        }

        private bool TimeSeriesEnabled
        {
            get { return timeSeriesEnabled; }
            set
            {
                timeSeriesEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GeneralStructurePropertiesVisibility));
            }
        }

        private bool CoefficientsEnabled
        {
            get { return coefficientsEnabled; }
            set
            {
                coefficientsEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Coefficients));
            }
        }

        private bool extraResistance;
        private bool ExtraResistanceEnabled
        {
            get { return extraResistance; }
            set
            {
                extraResistance = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.ExtraResistance));
            }
        }

        public bool GeneralStructureVisibility
        {
            get { return _generalStructureVisibility; }
            set
            {
                _generalStructureVisibility = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GeneralStructureVisibility));
            }
        }

        public bool GateGroupboxEnabled
        {
            get { return gateGroupboxEnabled; }
            set
            {
                gateGroupboxEnabled = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateGroupboxEnabled));
            }
        }

        public bool EnableAdvancedSettings
        {
            get { return enableAdvancedSettings; }
            set
            {
                enableAdvancedSettings = value;
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableAdvancedSettings));
            }
        }

        public bool IsLowerEdgeConstantTime
        {
            get
            {
                var gatedWeir = Weir.WeirFormula as GatedWeirFormula;
                return !gatedWeir.UseLowerEdgeLevelTimeSeries;
            }
        }
        public bool IsHorizontalDoorOpeningWidthConstantTime
        {
            get
            {
                var gatedWeir = Weir.WeirFormula as GatedWeirFormula;
                return !gatedWeir.UseLowerEdgeLevelTimeSeries;
            }
        }

        public bool EnableCrestLevelTimeSeries
        {
            get { return weir?.UseCrestLevelTimeSeries ?? false; }
            set
            {
                if (weir.UseCrestLevelTimeSeries == value) { return;}
                //Avoid useless propagation of events.
             
                 weir.UseCrestLevelTimeSeries = value;

                if (EnableLowerEdgeLevelTimeSeries || EnableCrestLevelTimeSeries)
                {
                    TimeSeriesEnabled = true;
                }
                else
                {
                    TimeSeriesEnabled = false;
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableCrestLevelTimeSeries));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GatedVisibility));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
            }
        }

        public bool EnableLowerEdgeLevelTimeSeries
        {
            get
            {
                var gatedWeir = weir?.WeirFormula as IGatedWeirFormula;
                return gatedWeir?.UseLowerEdgeLevelTimeSeries?? false;
            }
            set
            {
                var gatedWeir = weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeir?.UseLowerEdgeLevelTimeSeries == value) return;
                
                //Avoid useless propagation of events.
                try
                {
                    gatedWeir.UseLowerEdgeLevelTimeSeries = value;

                }
                catch (Exception)
                {
                    return;
                }

                if (EnableLowerEdgeLevelTimeSeries || EnableCrestLevelTimeSeries)
                {
                    TimeSeriesEnabled = true;
                }
                else
                {
                    TimeSeriesEnabled = false;
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableLowerEdgeLevelTimeSeries));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GatedVisibility));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));

            }
        }
        public bool EnableHorizontalDoorOpeningWidthTimeSeries
        {
            get
            {
                var gatedWeir = weir?.WeirFormula as IGatedWeirFormula;
                return gatedWeir?.UseHorizontalDoorOpeningWidthTimeSeries?? false;
            }
            set
            {
                var gatedWeir = weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeir?.UseHorizontalDoorOpeningWidthTimeSeries == value) return;
                
                //Avoid useless propagation of events.
                try
                {
                    if (gatedWeir != null)
                    {
                        gatedWeir.UseHorizontalDoorOpeningWidthTimeSeries = value;
                    }
                }
                catch (Exception)
                {
                    return;
                }

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableHorizontalDoorOpeningWidthTimeSeries));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.IsHorizontalDoorOpeningWidthConstantTime));
            }
        }
        #endregion

        #region Methods: Timeseries
        private void EditLowerEdgeLevelTimeSeries()
        {
            var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;
            var result = GetTimeSeriesEditorForEdgeLevel?.Invoke(gatedWeir);
            if (result != null)
            {
                gatedWeir?.LowerEdgeLevelTimeSeries.Time.Clear();
                gatedWeir?.LowerEdgeLevelTimeSeries.Components[0].Clear();
                gatedWeir?.LowerEdgeLevelTimeSeries.Time.SetValues(result.Time.Values);
                gatedWeir?.LowerEdgeLevelTimeSeries.Components[0].SetValues(result.Components[0].Values.Cast<double>());
            }

            TimeSeriesEnabled = true;
            OnPropertyChanged(nameof(TimeSeriesEnabled));
        }

        private void EditHorizontalDoorOpeningWidthTimeSeries()
        {
            var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;

            var result = GetTimeSeriesEditorForHorizontalDoorOpeningWidth?.Invoke(gatedWeir);
            if (result != null)
            {
                gatedWeir?.HorizontalDoorOpeningWidthTimeSeries.Time.Clear();
                gatedWeir?.HorizontalDoorOpeningWidthTimeSeries.Components[0].Clear();
                gatedWeir?.HorizontalDoorOpeningWidthTimeSeries.Time.SetValues(result.Time.Values);
                gatedWeir?.HorizontalDoorOpeningWidthTimeSeries.Components[0].SetValues(result.Components[0].Values.Cast<double>());
            }
        }

        private void EditCrestLevelTimeSeries()
        {
            var result = GetTimeSeriesEditorForCrestLevel?.Invoke(Weir);
            if (result != null)
            {
                Weir?.CrestLevelTimeSeries.Time.Clear();
                Weir?.CrestLevelTimeSeries.Components[0].Clear();
                Weir?.CrestLevelTimeSeries.Time.SetValues(result.Time.Values);
                Weir?.CrestLevelTimeSeries.Components[0].SetValues(result.Components[0].Values.Cast<double>());
            }

            TimeSeriesEnabled = true;
            OnPropertyChanged(nameof(TimeSeriesEnabled));
        }
        #endregion

        #region Methods: Eventing
        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void WeirPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName<IWeir>(vm => vm.WeirFormula))
            {
                selectedWeirType = GetSelectableWeirFormulaType(weir.WeirFormula);

                ((INotifyPropertyChanged)weir).PropertyChanged -= WeirPropertyChanged;
                ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged -= WeirFormulaPropertyChanged;

                SelectedWeirType = selectedWeirType;

                ((INotifyPropertyChanged)weir).PropertyChanged += WeirPropertyChanged;
                ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;
            }

            if (e.PropertyName == TypeUtils.GetMemberName<IWeir>(vm => vm.UseCrestLevelTimeSeries))
            {
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableCrestLevelTimeSeries));
            }

            if (e.PropertyName == TypeUtils.GetMemberName<IWeir>(vm => vm.CrestLevel))
            {
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.BedLevelStructureCentre));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
            }
        }

        private void WeirFormulaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName<GeneralStructureWeirFormula>(f => f.BedLevelStructureCentre))
            {
                var gatedWeirFormula = weir?.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return;
                }

                lowerEdgeLevel += (BedLevelStructureCentre - previousCrestLevel);
                previousCrestLevel = BedLevelStructureCentre;

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
            }
        }

        private void SynchronizeCommonProperties()
        {
            OnPropertyChanged(nameof(SelectedWeirType));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableCrestLevelTimeSeries));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SimpleWeirPropertiesVisibility));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GeneralStructurePropertiesVisibility));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GeneralStructureVisibility));
        }

        private void SynchronizeGatedProperties()
        {
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GatedWeirPropertiesEnabled));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.DoorHeight));

            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.HorizontalDoorOpeningWidth));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableHorizontalDoorOpeningWidthTimeSeries));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.IsHorizontalDoorOpeningWidthConstantTime));

            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.EnableLowerEdgeLevelTimeSeries));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.IsLowerEdgeConstantTime));

            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SelectedDoorOpeningHeightDirectionType));
        }

        private void SynchronizeUpAndDownStreamProperties()
        {
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Upstream1CrestWidth));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Upstream2CrestWidth));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Upstream1CrestLevel));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Upstream2CrestLevel));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Downstream1CrestWidth));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Downstream2CrestWidth));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Downstream1CrestLevel));
            OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Downstream2CrestLevel));
        }

        private void SynchronizeGeneralStructureProperties()
        {
            SynchronizeUpAndDownStreamProperties();
        }
        #endregion
    }
}
