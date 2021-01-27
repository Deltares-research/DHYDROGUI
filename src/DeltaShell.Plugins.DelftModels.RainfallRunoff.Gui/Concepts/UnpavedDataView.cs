using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class UnpavedDataView : UserControl, IView, IRRUnitAwareView, IRRMeteoStationAwareView, IRRModelRunModeAwareView
    {
        private readonly AreaDictionaryEditorController<UnpavedEnums.CropType> cropAreaController;

        private readonly
            Dictionary<Type, string> drainageTypes = new Dictionary<Type, string>
                {
                    {typeof (DeZeeuwHellingaDrainageFormula), "De Zeeuw-Hellinga"},
                    {typeof (ErnstDrainageFormula), "Ernst"},
                    {typeof (KrayenhoffVanDeLeurDrainageFormula), "Krayenhoff van de Leur"},
                };

        private RainfallRunoffEnums.AreaUnit areaUnit;
        private UnpavedData data;
        private bool updatingRadioButtons;
        private bool isCapsimUsed;

        public UnpavedDataView()
        {
            InitializeComponent();
            
            RainfallRunoffFormsHelper.ApplyRealNumberFormatToDataBinding(this);

            areaDictionaryEditor.TotalAreaLabel = "Total area crops";
            cropAreaController = new AreaDictionaryEditorController<UnpavedEnums.CropType>(areaDictionaryEditor);
            unpavedTabControl.SelectedIndexChanged += UnpavedTabControlSelectedTabChanged;
        }


        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return areaUnit; }
            set
            {
                cropAreaController.AreaUnit = value;
                ViewModel.AreaUnit = value;
                areaUnit = value;
            }
        }

        /// <summary>
        /// CapSim is used -> specific soiltype enabled
        /// </summary>
        public bool IsCapsimUsed
        {
            get { return isCapsimUsed; }
            set
            {
                isCapsimUsed = value;
                soilTypeComboBox.Enabled = !isCapsimUsed;
                capsimSoilTypeComboBox.Enabled = isCapsimUsed;
            }
        }

        private UnpavedDataViewModel ViewModel { get; set; }

        #region IView<UnpavedData> Members

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged -= UnpavedDataViewPropertyChanged;

                    if (data.Catchment != null) 
                        data.Catchment.Links.CollectionChanged -= LinksCollectionChanged;

                    bindingSourceUnpaved.DataSource = typeof (UnpavedData);
                    bindingSourceUnpavedViewModel.DataSource = typeof (UnpavedDataViewModel);
                    ViewModel = null;
                    cropAreaController.Data = null;
                    rrBoundarySeriesView1.Data = null;
                    catchmentMeteoStationSelection1.CatchmentModelData = null;
                    catchmentMeteoStationSelection1.MeteoStations = null;
                }

                data = (UnpavedData) value;
                
                if (data != null)
                {
                    Text = "Unpaved data: " + data.Name;
                    ViewModel = new UnpavedDataViewModel(data, AreaUnit);

                    ((INotifyPropertyChanged) data).PropertyChanged += UnpavedDataViewPropertyChanged;
                    bindingSourceUnpaved.DataSource = data;
                    bindingSourceUnpavedViewModel.DataSource = ViewModel;

                    rrBoundarySeriesView1.Data = data.BoundaryData;

                    data.Catchment.Links.CollectionChanged += LinksCollectionChanged;

                    Initialize();
                }
            }
        }

        private void LinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateRunMode();
        }

        void UnpavedTabControlSelectedTabChanged(object sender, EventArgs e)
        {
            if (unpavedTabControl.SelectedTab == waterlevelTab)
            {
                UpdateRunMode();
            }
        }

        private void UpdateRunMode()
        {
            if (GetIsModelRunningParallelWithFlowFunc != null)
                ViewModel.ModelRunningParallelWithFlow = GetIsModelRunningParallelWithFlowFunc();
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        #region Manual RadioButton Binding

        private void SeriesRadioCheckedChanged(object sender, EventArgs e)
        {
            if (updatingRadioButtons)
            {
                return;
            }

            //databinding doesn't work well for >2 radio buttons
            var radioButton = (RadioButton) sender;

            if (!radioButton.Checked)
            {
                return;
            }

            bindingSourceUnpavedViewModel.SuspendBinding();
            
            if (sender == seepageConstantRadio)
            {
                ViewModel.SeepageIsConstant = true;
            }
            else if (sender == seepageSeriesRadio)
            {
                ViewModel.SeepageIsSeries = true;
            }
            else if (sender == seepageH0SeriesRadio)
            {
                ViewModel.SeepageIsH0Series = true;
            }

            bindingSourceUnpavedViewModel.ResumeBinding();
        }

        private void GroundwaterCheckedChanged(object sender, EventArgs e)
        {
            if (updatingRadioButtons)
            {
                return;
            }

            //databinding doesn't work well for >2 radio buttons
            var radioButton = (RadioButton) sender;

            if (!radioButton.Checked)
            {
                return;
            }

            bindingSourceUnpavedViewModel.SuspendBinding();

            if (sender == groundwaterLinkedNodeRadio)
            {
                ViewModel.GroundWaterLevelIsFromLinkedNode = true;
            }
            else if (sender == groundwaterSeriesRadio)
            {
                ViewModel.GroundWaterLevelIsSeries = true;
            }
            else if (sender == groundwaterConstantRadio)
            {
                ViewModel.GroundWaterLevelIsConstant = true;
            }

            bindingSourceUnpavedViewModel.ResumeBinding();
        }

        #endregion

        private void UnpavedDataViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(data.SeepageSource) ||
                e.PropertyName == nameof(data.InitialGroundWaterLevelSource))
            {
                UpdateRadioButtons();
                return;
            }

            if (e.PropertyName == nameof(data.DrainageFormula))
            {
                dataChanging = true;
                drainageComboBox.SelectedItem = drainageTypes[data.DrainageFormula.GetType()];
                dataChanging = false;
                InitializeDrainagePanel();
            }

        }

        private void InitializeDrainagePanel()
        {
            drainagePanel.Controls.Clear();
            Control drainageControl = CreateDrainageControl(data.DrainageFormula);
            drainagePanel.Controls.Add(drainageControl);
        }

        private static Control CreateDrainageControl(IDrainageFormula drainageFormula)
        {
            Control drainageControl;

            switch (drainageFormula)
            {
                case ErnstDrainageFormula ernstDrainageFormula:
                    drainageControl = new ErnstZeeuwHellingaDrainageControl
                        {Data = ernstDrainageFormula};
                    break;
                case KrayenhoffVanDeLeurDrainageFormula krayenhoffVanDeLeurDrainageFormula:
                    drainageControl = new KrayenhoffDrainageControl
                        {Data = krayenhoffVanDeLeurDrainageFormula};
                    break;
                case DeZeeuwHellingaDrainageFormula zeeuwHellingaDrainageFormula:
                    drainageControl = new ErnstZeeuwHellingaDrainageControl
                        {Data = zeeuwHellingaDrainageFormula};
                    break;
                default:
                    throw new NotImplementedException("Unknown drainage formula?");
            }

            drainageControl.Dock = DockStyle.Fill;
            return drainageControl;
        }

        private void Initialize()
        {
            cropAreaController.Data = data.AreaPerCrop;

            soilTypeComboBox.DataSource = Enum.GetValues(typeof (UnpavedEnums.SoilType));
            capsimSoilTypeComboBox.DataSource = Enum.GetValues(typeof(UnpavedEnums.SoilTypeCapsim));
            storageUnitComboBox.DataSource = Enum.GetValues(typeof (RainfallRunoffEnums.StorageUnit));
            infiltrationUnitComboBox.DataSource = Enum.GetValues(typeof (RainfallRunoffEnums.RainfallCapacityUnit));

            drainageComboBox.Items.Clear();
            drainageComboBox.Items.AddRange(drainageTypes.Values.ToList().ToArray());
            drainageComboBox.SelectedItem = drainageTypes[data.DrainageFormula.GetType()];

            catchmentMeteoStationSelection1.CatchmentModelData = data;

            UpdateRadioButtons();
            InitializeDrainagePanel();
        }

        private void UpdateRadioButtons()
        {
            //manual synchronization..crappy but true:
            updatingRadioButtons = true;

            seepageConstantRadio.Checked = ViewModel.SeepageIsConstant;
            seepageSeriesRadio.Checked = ViewModel.SeepageIsSeries;
            seepageH0SeriesRadio.Checked = ViewModel.SeepageIsH0Series;
            groundwaterLinkedNodeRadio.Checked = ViewModel.GroundWaterLevelIsFromLinkedNode;
            groundwaterConstantRadio.Checked = ViewModel.GroundWaterLevelIsConstant;
            groundwaterSeriesRadio.Checked = ViewModel.GroundWaterLevelIsSeries;

            updatingRadioButtons = false;
        }

        private void SeepageSeriesButtonClick(object sender, EventArgs e)
        {
            RainfallRunoffFormsHelper.ShowTableEditor(this, data.SeepageSeries);
        }

        private void SeepageH0SeriesButtonClick(object sender, EventArgs e)
        {
            RainfallRunoffFormsHelper.ShowTableEditor(this, data.SeepageH0Series);
        }

        private void GroundwaterSeriesButtonClick(object sender, EventArgs e)
        {
            RainfallRunoffFormsHelper.ShowTableEditor(this, data.InitialGroundWaterLevelSeries);
        }

        private bool dataChanging = false;

        [EditAction]
        private void DrainageComboBoxSelectedValueChanged(object sender, EventArgs e)
        {
            if (dataChanging)
                return;

            if (Data == null)
            {
                return;
            }

            string selectedItem = drainageComboBox.SelectedItem.ToString();
            
            if (selectedItem.Contains("Krayenhoff"))
            {
                data.SwitchDrainageFormula<KrayenhoffVanDeLeurDrainageFormula>();
            }
            else if (selectedItem.Contains("Ernst"))
            {
                data.SwitchDrainageFormula<ErnstDrainageFormula>();
            }
            else
            {
                data.SwitchDrainageFormula<DeZeeuwHellingaDrainageFormula>();
            }
        }
        
        public bool UseMeteoStations { set { catchmentMeteoStationSelection1.UseMeteoStations = value; } }
        public IEventedList<string> MeteoStations { set { catchmentMeteoStationSelection1.MeteoStations = value; } }
        
        public Func<bool> GetIsModelRunningParallelWithFlowFunc { set; private get; }
    }
}