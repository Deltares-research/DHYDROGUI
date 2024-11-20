using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class PavedDataView : UserControl, IView, IRRUnitAwareView, IRRMeteoStationAwareView
    {
        private RainfallRunoffEnums.AreaUnit areaUnit;
        private PavedData data;
        private bool updatingRadioButtons;

        public PavedDataView()
        {
            InitializeComponent();
            RainfallRunoffFormsHelper.ApplyRealNumberFormatToDataBinding(this);
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        private PavedDataViewModel ViewModel { get; set; }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return areaUnit; }
            set
            {
                ViewModel.AreaUnit = value;
                areaUnit = value;
            }
        }

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    bindingSourcePaved.DataSource = typeof (PavedData);
                    bindingSourcePavedViewModel.DataSource = typeof (PavedDataViewModel);
                    ((INotifyPropertyChanged) data).PropertyChanged -= PavedDataViewPropertyChanged;

                    catchmentMeteoStationSelection1.CatchmentModelData = null;
                    catchmentMeteoStationSelection1.MeteoStations = null;
                }

                data = (PavedData) value;

                if (data != null)
                {
                    Text = "Paved data: " + data.Name;
                    ViewModel = new PavedDataViewModel(data, AreaUnit);

                    ((INotifyPropertyChanged) data).PropertyChanged += PavedDataViewPropertyChanged;
                    bindingSourcePaved.DataSource = data;
                    bindingSourcePavedViewModel.DataSource = ViewModel;
                    
                    Initialize();
                }
            }
        }

        private void PavedDataViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(data.SewerType) ||
                e.PropertyName == nameof(data.SpillingDefinition) ||
                e.PropertyName == nameof(data.IsSewerPumpCapacityFixed))
            {
                UpdateRadioButtons();
            }
        }

        private void Initialize()
        {
            sewerTypeComboBox.DataSource = Enum.GetValues(typeof (PavedEnums.SewerType));
            sewerPumpCapacityUnitComboBox.DataSource = Enum.GetValues(typeof (PavedEnums.SewerPumpCapacityUnit));
            mixedAndOrRainfallPumpDischargeTargetcomboBox.DataSource =
                Enum.GetValues(typeof (PavedEnums.SewerPumpDischargeTarget));
            dryWeatherFlowPumpDischargeTargetcomboBox.DataSource =
                Enum.GetValues(typeof (PavedEnums.SewerPumpDischargeTarget));
            storageUnitComboBox.DataSource = Enum.GetValues(typeof (RainfallRunoffEnums.StorageUnit));
            dryWeatherFlowOptionsTypeComboBox.DataSource = Enum.GetValues(typeof (PavedEnums.DryWeatherFlowOptions));
            waterUseComboBox.DataSource = Enum.GetValues(typeof (PavedEnums.WaterUseUnit));

            catchmentMeteoStationSelection1.CatchmentModelData = data;

            UpdateRadioButtons();
        }

        private void UpdateRadioButtons()
        {
            //manual synchronization..crappy but true:
            updatingRadioButtons = true;

            rbNoDelay.Checked = ViewModel.SplittingDefinitionIsNoDelay;
            rbUseRunoffCoefficient.Checked = ViewModel.SplittingDefinitionUseRunoffCoefficient;
            rbFixedCapacity.Checked = !ViewModel.SewerPumpCapacityIsVariable;
            rbVariableCapacity.Checked = ViewModel.SewerPumpCapacityIsVariable;

            updatingRadioButtons = false;
        }

        private void dryWeatherFlowOptionsTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((PavedEnums.DryWeatherFlowOptions) dryWeatherFlowOptionsTypeComboBox.SelectedItem)
            {
                case PavedEnums.DryWeatherFlowOptions.ConstantDWF:
                    lblWaterUse.Text = "Total water use";
                    variableWaterUseFunctionButton.Enabled = false;
                    break;
                case PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesConstantDWF:
                    lblWaterUse.Text = "Water use per inhabitant";
                    variableWaterUseFunctionButton.Enabled = false;
                    break;
                case PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesVariableDWF:
                    lblWaterUse.Text = "Water use per inhabitant";
                    variableWaterUseFunctionButton.Enabled = true;
                    break;
                case PavedEnums.DryWeatherFlowOptions.VariableDWF:
                    lblWaterUse.Text = "Total water use";
                    variableWaterUseFunctionButton.Enabled = true;
                    break;
            }
        }

        private void groundwaterSeriesButtonClick(object sender, EventArgs e)
        {
            RainfallRunoffFormsHelper.ShowTableEditor(this, data.MixedSewerPumpVariableCapacitySeries);
        }

        private void variableWaterUseFunctionButtonClick(object sender, EventArgs e)
        {
            RainfallRunoffFormsHelper.ShowTableEditor(this, data.VariableWaterUseFunction);
        }

        private void dwfVariableCapacityButton_Click(object sender, EventArgs e)
        {
            RainfallRunoffFormsHelper.ShowTableEditor(this, data.DwfSewerPumpVariableCapacitySeries);
        }
        
        #region Manual RadioButton Binding

        private void SplittingDefinitionRadioCheckedChanged(object sender, EventArgs e)
        {
            //databinding doesn't work well for >2 radio buttons
            var radioButton = (RadioButton) sender;

            if (!radioButton.Checked)
            {
                return;
            }

            bindingSourcePavedViewModel.SuspendBinding();

            if (!updatingRadioButtons)
            {
                if (sender == rbNoDelay)
                {
                    ViewModel.SplittingDefinitionIsNoDelay = true;
                }
                if (sender == rbUseRunoffCoefficient)
                {
                    ViewModel.SplittingDefinitionUseRunoffCoefficient = true;
                }
            }

            bindingSourcePavedViewModel.ResumeBinding();
        }

        private void SewerPumpCapacityRadioCheckedChanged(object sender, EventArgs e)
        {
            //databinding doesn't work well for >2 radio buttons
            var radioButton = (RadioButton) sender;

            if (!radioButton.Checked)
            {
                return;
            }

            bindingSourcePavedViewModel.SuspendBinding();

            if (!updatingRadioButtons)
            {
                if (sender == rbFixedCapacity)
                {
                    data.IsSewerPumpCapacityFixed = true;
                }
                if (sender == rbVariableCapacity)
                {
                    data.IsSewerPumpCapacityFixed = false;
                }
            }

            bindingSourcePavedViewModel.ResumeBinding();
        }

        #endregion

        public bool UseMeteoStations { set { catchmentMeteoStationSelection1.UseMeteoStations = value; } }
        public IEventedList<string> MeteoStations { set { catchmentMeteoStationSelection1.MeteoStations = value; } }
    }
}