using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public sealed class WeirViewModel : IDisposable, INotifyPropertyChanged
    {
        #region Fields

        private IWeir weir;
        private SelectableWeirFormulaType selectedWeirType;
        private bool previousCrestLevelTimeSeriesValue;
        private bool previousLowerEdgeLevelTimeSeriesValue;
        private bool gatedWeirPropertiesEnabled;
        private bool previousHorizontalDoorOpeningWidthTimeSeriesValue;
        private bool gateGroupBoxEnabled;
        private bool crestLevelEnabled;
        private bool lowerEdgeLevelEnabled;
        private bool horizontalDoorOpeningWidthEnabled;
        private bool timeSeriesEnabled;
        private bool coefficientsEnabled;

        #endregion

        #region Functions/Delegates

        public Func<IWeir, TimeSeries> GetTimeSeriesEditorForCrestLevel { get; set; }
        public Func<IGatedWeirFormula, TimeSeries> GetTimeSeriesEditorForEdgeLevel { get; set; }
        public Func<IGatedWeirFormula, TimeSeries> GetTimeSeriesEditorForHorizontalDoorOpeningWidth { get; set; }

        #endregion

        #region Commands

        public ICommand OnEditCrestLevelTimeSeries
        {
            get
            {
                return new RelayCommand(param => EditCrestLevelTimeSeries());
            }
        }

        public ICommand OnEditLowerEdgeLevelTimeSeries
        {
            get
            {
                return new RelayCommand(param => EditLowerEdgeLevelTimeSeries());
            }
        }

        public ICommand OnEditHorizontalDoorOpeningWidthTimeSeries
        {
            get
            {
                return new RelayCommand(param => EditHorizontalDoorOpeningWidthTimeSeries());
            }
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
                if (weir == value)
                {
                    return;
                }

                if (weir != null)
                {
                    UnSubscribe();
                }

                weir = value;

                if (weir != null)
                {
                    selectedWeirType = GetSelectableWeirFormulaType(weir.WeirFormula);

                    SetPreviousCrestLevelTimeSeries();

                    SetPreviousLowerEdgeLevelTimeSeries();

                    SetPreviousHorizontalDoorOpeningWidthTimeSeries();

                    UpdateControls();

                    OnPropertyChanged(nameof(Weir));

                    SynchronizeCommonProperties();

                    SynchronizeGatedOrGeneralStructureProperties();

                    Subscribe();
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

        public double GateOpeningHeight
        {
            get
            {
                double gateOpeningHeight = LowerEdgeLevel - BedLevelStructureCentre;
                return Math.Round(gateOpeningHeight, 2);
            }
        }

        public double DoorHeight
        {
            get
            {
                if (Weir == null || !(Weir.WeirFormula is IGatedWeirFormula weirFormula))
                {
                    return 0;
                }

                return weirFormula.DoorHeight;
            }
            set
            {
                if (!(Weir.WeirFormula is IGatedWeirFormula weirFormula))
                {
                    return;
                }

                if (Math.Abs(weirFormula.DoorHeight - value) <= double.Epsilon)
                {
                    return;
                }

                UnSubscribe();
                weirFormula.DoorHeight = value;
                Subscribe();
                OnPropertyChanged();
            }
        }

        public GateOpeningDirection SelectedDoorOpeningHeightDirectionType
        {
            get
            {
                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return GateOpeningDirection.Symmetric;
                }

                switch (gatedWeirFormula)
                {
                    case GatedWeirFormula _:
                        return gatedWeirFormula.HorizontalDoorOpeningDirection;
                    case GeneralStructureWeirFormula _:
                        return GateOpeningDirection.Symmetric;
                }

                return GateOpeningDirection.Symmetric;
            }
            set
            {
                var gatedWeirFormula = Weir?.WeirFormula as GatedWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return;
                }

                UnSubscribe();
                gatedWeirFormula.HorizontalDoorOpeningDirection = value;
                Subscribe();

                OnPropertyChanged(nameof(SelectedDoorOpeningHeightDirectionType));
            }
        }

        public double Upstream1CrestWidth
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.WidthLeftSideOfStructure;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthLeftSideOfStructure = value;
                }
            }
        }

        public double Upstream1CrestLevel
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.BedLevelLeftSideOfStructure;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelLeftSideOfStructure = value;
                }
            }
        }

        public double Upstream2CrestWidth
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.WidthStructureLeftSide;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthStructureLeftSide = value;
                }
            }
        }

        public double Upstream2CrestLevel
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.BedLevelLeftSideStructure;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelLeftSideStructure = value;
                }
            }
        }

        public double Downstream1CrestWidth
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.WidthStructureRightSide;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthStructureRightSide = value;
                }
            }
        }

        public double Downstream1CrestLevel
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.BedLevelRightSideStructure;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelRightSideStructure = value;
                }
            }
        }

        public double Downstream2CrestWidth
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.WidthRightSideOfStructure;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.WidthRightSideOfStructure = value;
                }
            }
        }

        public double Downstream2CrestLevel
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    return generalStructureFormula.BedLevelRightSideOfStructure;
                }

                return -1.0;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.BedLevelRightSideOfStructure = value;
                }
            }
        }

        public double BedLevelStructureCentre
        {
            get
            {
                if (Weir?.WeirFormula == null)
                {
                    return 0;
                }

                return Weir.CrestLevel;
            }
            set
            {
                if (Weir?.CrestLevel != null)
                {
                    UnSubscribe();
                }

                if (Weir?.WeirFormula != null)
                {
                    Weir.CrestLevel = value;

                    if (Weir?.CrestLevel != null)
                    {
                        Subscribe();
                    }
                }

                OnPropertyChanged(nameof(BedLevelStructureCentre));
                OnPropertyChanged(nameof(GateOpeningHeight));
            }
        }

        public double LowerEdgeLevel
        {
            get
            {
                if (!(Weir?.WeirFormula is IGatedWeirFormula gatedWeirFormula))
                {
                    return 0;
                }

                return gatedWeirFormula.LowerEdgeLevel;
            }
            set
            {
                var gatedWeirFormula = Weir?.WeirFormula as IGatedWeirFormula;

                if (gatedWeirFormula?.LowerEdgeLevel != null)
                {
                    UnSubscribe();
                }

                if (gatedWeirFormula != null)
                {
                    gatedWeirFormula.LowerEdgeLevel = value;
                    if (gatedWeirFormula?.LowerEdgeLevel != null)
                    {
                        Subscribe();
                    }
                }

                OnPropertyChanged(nameof(LowerEdgeLevel));
                OnPropertyChanged(nameof(GateOpeningHeight));
            }
        }

        public double HorizontalDoorOpeningWidth
        {
            get
            {
                if (!(Weir?.WeirFormula is IGatedWeirFormula gatedWeirFormula))
                {
                    return 0;
                }

                return gatedWeirFormula.HorizontalDoorOpeningWidth;
            }
            set
            {
                if (!(Weir?.WeirFormula is IGatedWeirFormula gatedWeirFormula))
                {
                    return;
                }

                UnSubscribe();
                gatedWeirFormula.HorizontalDoorOpeningWidth = value;
                Subscribe();

                OnPropertyChanged(nameof(HorizontalDoorOpeningWidth));
            }
        }

        public double ExtraResistance
        {
            get
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula == null)
                {
                    return 0;
                }

                return generalStructureFormula.ExtraResistance;
            }
            set
            {
                var generalStructureFormula = Weir?.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    generalStructureFormula.ExtraResistance = value;
                }

                OnPropertyChanged(nameof(GeneralStructurePropertiesVisibility));
                OnPropertyChanged(nameof(ExtraResistance));
            }
        }

        public bool LowerEdgeLevelEnabled
        {
            get
            {
                return lowerEdgeLevelEnabled;
            }
            set
            {
                lowerEdgeLevelEnabled = value;
                OnPropertyChanged(nameof(SimpleWeirPropertiesVisibility));
            }
        }

        public bool HorizontalDoorOpeningWidthEnabled
        {
            get
            {
                return horizontalDoorOpeningWidthEnabled;
            }
            set
            {
                horizontalDoorOpeningWidthEnabled = value;
                OnPropertyChanged(nameof(SimpleWeirPropertiesVisibility));
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
                if (Weir?.WeirFormula == null)
                {
                    return;
                }

                ((INotifyPropertyChanged) Weir.WeirFormula).PropertyChanged -= WeirFormulaPropertyChanged;

                switch (value)
                {
                    case SelectableWeirFormulaType.SimpleWeir:
                        Weir.WeirFormula = new SimpleWeirFormula();
                        SetSimpleWeirControls();
                        break;
                    case SelectableWeirFormulaType.SimpleGate:
                        Weir.WeirFormula = new GatedWeirFormula(true);
                        SetSimpleGateViewControls();
                        break;
                    case SelectableWeirFormulaType.GeneralStructure:
                        var generalStructureWeirFormula = new GeneralStructureWeirFormula
                        {
                            BedLevelStructureCentre = Weir.CrestLevel,
                            WidthStructureCentre = Weir.CrestWidth,
                            WidthStructureLeftSide = double.NaN,
                            WidthStructureRightSide = double.NaN,
                            WidthLeftSideOfStructure = double.NaN,
                            WidthRightSideOfStructure = double.NaN,
                        };

                        Weir.WeirFormula = generalStructureWeirFormula;
                        SetGeneralStructureViewControls();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The selected weir type does not exist. Please select simple weir, simple gate or general structure");
                }

                ((INotifyPropertyChanged) Weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;

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
            GateGroupBoxEnabled = true;
            CrestLevelEnabled = false;
            LowerEdgeLevelEnabled = true;
            HorizontalDoorOpeningWidthEnabled = true;
            CoefficientsEnabled = true;
            GatedWeirPropertiesEnabled = false;
            if (EnableCrestLevelTimeSeries != previousCrestLevelTimeSeriesValue)
            {
                previousCrestLevelTimeSeriesValue = EnableCrestLevelTimeSeries;
            }

            if (EnableLowerEdgeLevelTimeSeries != previousLowerEdgeLevelTimeSeriesValue)
            {
                previousLowerEdgeLevelTimeSeriesValue = EnableLowerEdgeLevelTimeSeries;
            }

            if (EnableHorizontalDoorOpeningWidthTimeSeries != previousHorizontalDoorOpeningWidthTimeSeriesValue)
            {
                previousHorizontalDoorOpeningWidthTimeSeriesValue = EnableHorizontalDoorOpeningWidthTimeSeries;
            }

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
            GateGroupBoxEnabled = true;
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
            GateGroupBoxEnabled = false;
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

        public Visibility GeneralStructurePropertiesVisibility
        {
            get
            {
                return Weir?.WeirFormula is GeneralStructureWeirFormula
                           ? Visibility.Visible
                           : Visibility.Collapsed;
            }
        }

        public bool GatedWeirPropertiesEnabled
        {
            get
            {
                return gatedWeirPropertiesEnabled;
            }
            set
            {
                gatedWeirPropertiesEnabled = value;
                OnPropertyChanged(nameof(GatedWeirPropertiesEnabled));
                OnPropertyChanged(nameof(GateOpeningHeight));
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

        private bool CrestLevelEnabled
        {
            get
            {
                return crestLevelEnabled;
            }
            set
            {
                crestLevelEnabled = value;
                OnPropertyChanged(nameof(SimpleWeirPropertiesVisibility));
            }
        }

        private bool TimeSeriesEnabled
        {
            get
            {
                return timeSeriesEnabled;
            }
            set
            {
                timeSeriesEnabled = value;
                OnPropertyChanged(nameof(GeneralStructurePropertiesVisibility));
            }
        }

        private bool CoefficientsEnabled
        {
            get
            {
                return coefficientsEnabled;
            }
            set
            {
                coefficientsEnabled = value;
                OnPropertyChanged(nameof(Coefficients));
            }
        }

        public bool GateGroupBoxEnabled
        {
            get
            {
                return gateGroupBoxEnabled;
            }
            set
            {
                if (!GateGroupBoxEnabled)
                {
                    UnSubscribe();
                }

                gateGroupBoxEnabled = value;
                if (!GateGroupBoxEnabled)
                {
                    Subscribe();
                }

                OnPropertyChanged(nameof(GateGroupBoxEnabled));
            }
        }

        public bool EnableCrestLevelTimeSeries
        {
            get
            {
                return Weir?.UseCrestLevelTimeSeries ?? false;
            }
            set
            {
                if (!EnableCrestLevelTimeSeries)
                {
                    UnSubscribe();
                }

                if (Weir.UseCrestLevelTimeSeries == value)
                {
                    return;
                }
                //Avoid useless propagation of events.

                Weir.UseCrestLevelTimeSeries = value;
                if (!EnableCrestLevelTimeSeries)
                {
                    Subscribe();
                }

                if (EnableLowerEdgeLevelTimeSeries || EnableCrestLevelTimeSeries)
                {
                    TimeSeriesEnabled = true;
                }
                else
                {
                    TimeSeriesEnabled = false;
                }

                OnPropertyChanged(nameof(EnableCrestLevelTimeSeries));
                OnPropertyChanged(nameof(GateOpeningHeight));
            }
        }

        public bool EnableLowerEdgeLevelTimeSeries
        {
            get
            {
                var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;
                return gatedWeir?.UseLowerEdgeLevelTimeSeries ?? false;
            }
            set
            {
                if (!EnableLowerEdgeLevelTimeSeries)
                {
                    UnSubscribe();
                }

                var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeir == null || gatedWeir.UseLowerEdgeLevelTimeSeries == value)
                {
                    return;
                }

                //Avoid useless propagation of events.
                try
                {
                    gatedWeir.UseLowerEdgeLevelTimeSeries = value;
                    if (!EnableLowerEdgeLevelTimeSeries)
                    {
                        Subscribe();
                    }
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

                OnPropertyChanged(nameof(EnableLowerEdgeLevelTimeSeries));
                OnPropertyChanged(nameof(GateOpeningHeight));
            }
        }

        public bool EnableHorizontalDoorOpeningWidthTimeSeries
        {
            get
            {
                var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;
                return gatedWeir?.UseHorizontalDoorOpeningWidthTimeSeries ?? false;
            }
            set
            {
                if (!EnableHorizontalDoorOpeningWidthTimeSeries)
                {
                    UnSubscribe();
                }

                var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;
                if (gatedWeir?.UseHorizontalDoorOpeningWidthTimeSeries == value)
                {
                    return;
                }

                //Avoid useless propagation of events.
                try
                {
                    if (gatedWeir != null)
                    {
                        gatedWeir.UseHorizontalDoorOpeningWidthTimeSeries = value;
                        if (!EnableHorizontalDoorOpeningWidthTimeSeries)
                        {
                            Subscribe();
                        }
                    }
                }
                catch (Exception)
                {
                    return;
                }

                OnPropertyChanged(nameof(EnableHorizontalDoorOpeningWidthTimeSeries));
            }
        }

        #endregion

        #region Methods: Timeseries

        private void EditLowerEdgeLevelTimeSeries()
        {
            var gatedWeir = Weir?.WeirFormula as IGatedWeirFormula;
            TimeSeries result = GetTimeSeriesEditorForEdgeLevel?.Invoke(gatedWeir);
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

            TimeSeries result = GetTimeSeriesEditorForHorizontalDoorOpeningWidth?.Invoke(gatedWeir);
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
            TimeSeries result = GetTimeSeriesEditorForCrestLevel?.Invoke(Weir);
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
            UnSubscribe();
        }

        private void UnSubscribe()
        {
            ((INotifyPropertyChanged) weir).PropertyChanged -= WeirPropertyChanged;
            ((INotifyPropertyChanged) weir.WeirFormula).PropertyChanged -= WeirFormulaPropertyChanged;
        }

        private void Subscribe()
        {
            ((INotifyPropertyChanged) weir).PropertyChanged += WeirPropertyChanged;
            ((INotifyPropertyChanged) weir.WeirFormula).PropertyChanged += WeirFormulaPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void WeirPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IWeir.WeirFormula))
            {
                selectedWeirType = GetSelectableWeirFormulaType(weir.WeirFormula);

                if (weir != null)
                {
                    UnSubscribe();
                }

                SelectedWeirType = selectedWeirType;

                if (weir != null)
                {
                    Subscribe();
                }
            }

            if (e.PropertyName == nameof(IWeir.UseCrestLevelTimeSeries))
            {
                OnPropertyChanged(nameof(EnableCrestLevelTimeSeries));
            }

            if (e.PropertyName == nameof(IWeir.CrestLevel))
            {
                OnPropertyChanged(nameof(BedLevelStructureCentre));
                OnPropertyChanged(nameof(GateOpeningHeight));
            }
        }

        private void WeirFormulaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeneralStructureWeirFormula.BedLevelStructureCentre))
            {
                var gatedWeirFormula = weir?.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return;
                }

                OnPropertyChanged(nameof(LowerEdgeLevel));
            }
        }

        private void SynchronizeCommonProperties()
        {
            OnPropertyChanged(nameof(SelectedWeirType));
            OnPropertyChanged(nameof(EnableCrestLevelTimeSeries));
            OnPropertyChanged(nameof(BedLevelStructureCentre));
            OnPropertyChanged(nameof(SimpleWeirPropertiesVisibility));
            OnPropertyChanged(nameof(GeneralStructurePropertiesVisibility));
        }

        private void SynchronizeGatedProperties()
        {
            OnPropertyChanged(nameof(GatedWeirPropertiesEnabled));
            OnPropertyChanged(nameof(GateOpeningHeight));
            OnPropertyChanged(nameof(DoorHeight));

            OnPropertyChanged(nameof(HorizontalDoorOpeningWidth));
            OnPropertyChanged(nameof(EnableHorizontalDoorOpeningWidthTimeSeries));

            OnPropertyChanged(nameof(LowerEdgeLevel));
            OnPropertyChanged(nameof(EnableLowerEdgeLevelTimeSeries));

            OnPropertyChanged(nameof(SelectedDoorOpeningHeightDirectionType));
        }

        private void SynchronizeUpAndDownStreamProperties()
        {
            OnPropertyChanged(nameof(Upstream1CrestWidth));
            OnPropertyChanged(nameof(Upstream2CrestWidth));
            OnPropertyChanged(nameof(Upstream1CrestLevel));
            OnPropertyChanged(nameof(Upstream2CrestLevel));
            OnPropertyChanged(nameof(Downstream1CrestWidth));
            OnPropertyChanged(nameof(Downstream2CrestWidth));
            OnPropertyChanged(nameof(Downstream1CrestLevel));
            OnPropertyChanged(nameof(Downstream2CrestLevel));
        }

        private void SynchronizeGeneralStructureProperties()
        {
            SynchronizeUpAndDownStreamProperties();
        }

        #endregion
    }
}