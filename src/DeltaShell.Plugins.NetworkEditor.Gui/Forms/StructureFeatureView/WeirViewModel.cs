using System;
using System.ComponentModel;
using System.Windows;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public class WeirViewModel : IDisposable, INotifyPropertyChanged
    {
        private IWeir weir;
        private SelectableWeirFormulaType selectedWeirType;

        private double lowerEdgeLevel;
        private double previousCrestLevel;

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
                    UpdateControls();
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.Weir));
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.HasWeir));
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SelectedWeirType));
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));

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

                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
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
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SelectedWeirType));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.GateOpeningHeight));
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
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
                    OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
                }
            }
        }

        #region Gui enabling controls

        private void SetGeneralStructureViewControls()
        {
            GateGroupboxEnabled = true;
            CrestLevelEnabled = false;
        }

        private void SetSimpleWeirControls()
        {
            GateGroupboxEnabled = false;
            CrestLevelEnabled = true;
        }

        public Visibility CrestLevelVisibility
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
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.CrestLevelVisibility));
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

        private bool gateGroupboxEnabled;
        private bool crestLevelEnabled;
        private bool enableAdvancedSettings;

        #endregion

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
            if (e.PropertyName == TypeUtils.GetMemberName<IWeir>(vm => vm.WeirFormula))
            {
                selectedWeirType = GetSelectableWeirFormulaType(weir.WeirFormula);

                UpdateControls();
                
                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.SelectedWeirType));
            }
        }

        private void WeirFormulaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName<GeneralStructureWeirFormula>(f => f.BedLevelStructureCentre))
            {
                var gatedWeirFormula = weir.WeirFormula as GeneralStructureWeirFormula;
                if (gatedWeirFormula == null)
                {
                    return;
                }
                lowerEdgeLevel += (BedLevelStructureCentre - previousCrestLevel);
                previousCrestLevel = BedLevelStructureCentre;

                OnPropertyChanged(TypeUtils.GetMemberName<WeirViewModel>(vm => vm.LowerEdgeLevel));
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
