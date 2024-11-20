using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class OpenWaterDataView : UserControl, IView, IRRMeteoStationAwareView, IRRUnitAwareView
    {
        private OpenWaterData data;
        private OpenWaterDataViewModel viewModel;
        private RainfallRunoffEnums.AreaUnit areaUnit;

        public OpenWaterDataView()
        {
            InitializeComponent();
            RainfallRunoffFormsHelper.ApplyRealNumberFormatToDataBinding(this);
        }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get => areaUnit;
            set
            {
                viewModel.AreaUnit = value;
                areaUnit = value;
            }
        }

        public bool UseMeteoStations
        {
            set => catchmentMeteoStationSelection1.UseMeteoStations = value;
        }

        public IEventedList<string> MeteoStations
        {
            set => catchmentMeteoStationSelection1.MeteoStations = value;
        }

        public object Data
        {
            get => data;
            set
            {
                if (data != null)
                {
                    openWaterDataViewModelBindingSource.DataSource = typeof(OpenWaterDataViewModel);

                    catchmentMeteoStationSelection1.CatchmentModelData = null;
                    catchmentMeteoStationSelection1.MeteoStations = null;
                }

                data = (OpenWaterData) value;
                if (data != null)
                {
                    Text = "Open water data: " + data.Name;
                    catchmentMeteoStationSelection1.CatchmentModelData = data;
                    viewModel = new OpenWaterDataViewModel(data, AreaUnit);
                    openWaterDataViewModelBindingSource.DataSource = viewModel;
                }
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item) {}
    }
}