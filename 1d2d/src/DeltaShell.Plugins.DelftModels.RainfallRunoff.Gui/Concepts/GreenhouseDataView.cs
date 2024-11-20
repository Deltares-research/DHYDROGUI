using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class GreenhouseDataView : UserControl, IView, IRRUnitAwareView, IRRMeteoStationAwareView
    {
        private readonly AreaDictionaryEditorController<GreenhouseEnums.AreaPerGreenhouseType> greenhouseAreaController;

        private RainfallRunoffEnums.AreaUnit areaUnit;
        private GreenhouseData data;

        public GreenhouseDataView()
        {
            InitializeComponent();
            RainfallRunoffFormsHelper.ApplyRealNumberFormatToDataBinding(this);

            areaDictionaryEditor.TotalAreaLabel = "Total area greenhouses";

            greenhouseAreaController =
                new AreaDictionaryEditorController<GreenhouseEnums.AreaPerGreenhouseType>(areaDictionaryEditor);
        }

        #region IView<T> implementation

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
            throw new NotImplementedException();
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private GreenhouseDataViewModel ViewModel { get; set; }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return areaUnit; }
            set
            {
                ViewModel.AreaUnit = value;
                greenhouseAreaController.AreaUnit = value;
                areaUnit = value;
            }
        }

        #region IView<GreenhouseData> Members

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    bindingSourceGreenhouse.DataSource = typeof (GreenhouseData);
                    bindingSourceGreenhouseViewModel.DataSource = typeof (GreenhouseDataViewModel);
                    greenhouseAreaController.Data = null;
                    catchmentMeteoStationSelection1.CatchmentModelData = null;
                    catchmentMeteoStationSelection1.MeteoStations = null;
                }

                data = (GreenhouseData) value;

                if (data != null)
                {
                    Text = "Greenhouse data: " + data.Name;
                    ViewModel = new GreenhouseDataViewModel(data, AreaUnit);

                    bindingSourceGreenhouse.DataSource = data;
                    bindingSourceGreenhouseViewModel.DataSource = ViewModel;

                    Initialize();
                }
            }
        }

        #endregion

        private void Initialize()
        {
            greenhouseAreaController.Data = data.AreaPerGreenhouse;
            storageUnitComboBox.DataSource = Enum.GetValues(typeof (RainfallRunoffEnums.StorageUnit));
            catchmentMeteoStationSelection1.CatchmentModelData = data;
        }

        public bool UseMeteoStations { set { catchmentMeteoStationSelection1.UseMeteoStations = value; } }
        public IEventedList<string> MeteoStations { set { catchmentMeteoStationSelection1.MeteoStations = value; } }
    }
}