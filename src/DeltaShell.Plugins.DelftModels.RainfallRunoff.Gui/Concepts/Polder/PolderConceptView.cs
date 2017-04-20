using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Polder
{
    public partial class PolderConceptView : UserControl, IView, IRRUnitAwareView, IRRMeteoStationAwareView
    {
        private const string pavedName = "PavedArea";
        private const string unpavedName = "UnpavedArea";
        private const string greenhouseName = "GreenhouseArea";
        private const string openWaterName = "OpenWaterArea";
        private static readonly ILog log = LogManager.GetLogger(typeof (PolderConceptView));
        private RainfallRunoffEnums.AreaUnit areaUnit;
        private GreenhouseDataView greenhouseDataView;
        private OpenWaterDataView openWaterDataView;
        private PavedDataView pavedDataView;
        private PolderConcept polderConcept;
        private PolderConceptViewData polderConceptViewData;

        private UnpavedDataView unpavedDataView;

        public PolderConceptView()
        {
            InitializeComponent();
            InitializeTabImages();
        }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            set
            {
                areaUnit = value;
                UpdateAreaUnit();
            }
        }
        
        #region IView<PolderConcept> Members

        public object Data
        {
            get { return polderConcept; }
            set
            {
                if (Data != null)
                {
                    ((INotifyPropertyChanged) polderConcept).PropertyChanged -= PolderConceptPropertyChanged;
                    if (polderConceptViewData != null)
                    {
                        polderConceptViewData.Dispose();
                    }
                }

                polderConcept = (PolderConcept) value;

                PolderPieChart.Data = polderConcept;

                if (polderConcept != null)
                {
                    Text = "Polder concept: " + polderConcept.Name;
                    ((INotifyPropertyChanged) polderConcept).PropertyChanged += PolderConceptPropertyChanged;
                    polderConceptViewData = new PolderConceptViewData(polderConcept, areaUnit);
                    bindingSourcePolderViewData.DataSource = polderConceptViewData;
                }
                else
                {
                    polderConceptViewData = null;
                    bindingSourcePolderViewData.Clear();
                    ClearSubViews();
                }

                SetTabsBasedOnProperties();
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void InitializeTabImages()
        {
            polderImageList.Images.Add(Resources.paved);
            polderImageList.Images.Add(Resources.unpaved);
            polderImageList.Images.Add(Resources.greenhouse);
            polderImageList.Images.Add(Resources.openwater);
            tabPagePaved.ImageIndex = 0;
            tabPageUnpaved.ImageIndex = 1;
            tabPageGreenhouse.ImageIndex = 2;
            tabPageOpenWater.ImageIndex = 3;
        }

        private void ClearSubViews()
        {
            if (pavedDataView != null)
            {
                pavedDataView.Data = null;
            }
            if (unpavedDataView != null)
            {
                unpavedDataView.Data = null;
            }
            if (greenhouseDataView != null)
            {
                greenhouseDataView.Data = null;
            }
            if (openWaterDataView != null)
            {
                openWaterDataView.Data = null;
            }
        }

        private void SetTabsBasedOnProperties()
        {
            pavedDataView = null;
            greenhouseDataView = null;
            unpavedDataView = null;
            openWaterDataView = null;

            tabPageUnpaved.Controls.Clear();
            tabPagePaved.Controls.Clear();
            tabPageGreenhouse.Controls.Clear();
            tabPageOpenWater.Controls.Clear();

            if (polderConcept == null)
            {
                return;
            }

            UpdateTabVisibility();

            if (polderConcept.OpenWater != null)
            {
                SetTabBasedOnProperties(openWaterName);
            }

            if (polderConcept.Greenhouse != null)
            {
                SetTabBasedOnProperties(greenhouseName);
            }

            if (polderConcept.Unpaved != null)
            {
                SetTabBasedOnProperties(unpavedName);
            }

            if (polderConcept.Paved != null)
            {
                SetTabBasedOnProperties(pavedName);
            }
        }

        private void UpdateTabVisibility()
        {
            tabControl.SuspendLayout();

            tabControl.TabPages.Clear();

            if (polderConcept.PavedArea > 0)
                tabControl.TabPages.Add(tabPagePaved);

            if (polderConcept.UnpavedArea > 0)
                tabControl.TabPages.Add(tabPageUnpaved);

            if (polderConcept.GreenhouseArea > 0)
                tabControl.TabPages.Add(tabPageGreenhouse);

            if (polderConcept.OpenWaterArea > 0)
                tabControl.TabPages.Add(tabPageOpenWater);

            tabControl.ResumeLayout();
        }

        private void SetTabBasedOnProperties(string name)
        {
            TabPage tabToSelect = null;

            switch (name)
            {
                case pavedName:
                    if (pavedDataView == null)
                    {
                        pavedDataView = new PavedDataView
                            {Data = polderConcept.Paved, AreaUnit = areaUnit, Dock = DockStyle.Fill};
                        tabPagePaved.Controls.Add(pavedDataView);
                        tabToSelect = tabPagePaved;
                    }
                    break;
                case unpavedName:
                    if (unpavedDataView == null)
                    {
                        unpavedDataView = new UnpavedDataView
                            {Data = polderConcept.Unpaved, AreaUnit = areaUnit, Dock = DockStyle.Fill};
                        tabPageUnpaved.Controls.Add(unpavedDataView);
                        tabToSelect = tabPageUnpaved;
                    }
                    break;
                case greenhouseName:
                    if (greenhouseDataView == null)
                    {
                        greenhouseDataView = new GreenhouseDataView
                            {Data = polderConcept.Greenhouse, AreaUnit = areaUnit, Dock = DockStyle.Fill};
                        tabPageGreenhouse.Controls.Add(greenhouseDataView);
                        tabToSelect = tabPageGreenhouse;
                    }
                    break;
                case openWaterName:
                    if (openWaterDataView == null)
                    {
                        openWaterDataView = new OpenWaterDataView
                            {Data = polderConcept.OpenWater, Dock = DockStyle.Fill};
                        tabPageOpenWater.Controls.Add(openWaterDataView);
                        tabToSelect = tabPageOpenWater;
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format("Tab for property '{0}' is not supported", name));
            }

            if (tabToSelect != null)
            {
                //Switching tabs here can cause hangs (see TOOLS-6224). Possibly when another tabcontrol is switching tabs at the same time?
                //Solution for now is to simply delay the tab switch a while...
                DelayedExecute(150, () =>
                    {
                        var focusedControl = ActiveControl;
                        tabControl.SelectedTab = tabToSelect;
                        if (focusedControl != null)
                            focusedControl.Focus();
                    });
            }
        }

        //ugly, I know
        private static void DelayedExecute(int millisecondsToDelay, MethodInvoker methodToExecute)
        {
            var timer = new Timer {Interval = millisecondsToDelay};
            EventHandler handler = null;
            handler = delegate
                {
                    if (timer.Enabled)
                    {
                        timer.Stop();
                        methodToExecute.Invoke();
                        timer.Tick -= handler;
                        timer.Dispose();
                    }
                };
            timer.Tick += handler;
            timer.Start();
        }

        private void PolderConceptPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bindingSourcePolderViewData.ResetBindings(false);

            if (e.PropertyName == pavedName || e.PropertyName == unpavedName || e.PropertyName == greenhouseName ||
                e.PropertyName == openWaterName)
            {
                UpdateTabVisibility();
                SetTabBasedOnProperties(e.PropertyName);
            }
        }

        private void UpdateAreaUnit()
        {
            SetUnitLabels();

            if (polderConceptViewData != null)
            {
                polderConceptViewData.AreaUnit = areaUnit;
            }
            if (pavedDataView != null)
            {
                pavedDataView.AreaUnit = areaUnit;
            }
            if (greenhouseDataView != null)
            {
                greenhouseDataView.AreaUnit = areaUnit;
            }
            if (unpavedDataView != null)
            {
                unpavedDataView.AreaUnit = areaUnit;
            }
        }

        private void SetUnitLabels()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof (RainfallRunoffEnums.AreaUnit));
            string unitLabelText = converter.ConvertToString(areaUnit);
            lblUnit1.Text = unitLabelText;
            lblUnit2.Text = unitLabelText;
            lblUnit3.Text = unitLabelText;
            lblUnit4.Text = unitLabelText;
            lblUnit5.Text = unitLabelText;
        }

        private void LblTotalAreaPercentageTextChanged(object sender, EventArgs e)
        {
            double value;

            if (Double.TryParse(lblTotalAreaPercentage.Text, out value))
            {
                lblTotalAreaPercentage.ForeColor = value > 100.0 ? Color.Red : Color.Black;
            }
        }

        public bool UseMeteoStations {
            set
            {
                if (pavedDataView != null)
                {
                    pavedDataView.UseMeteoStations = value;
                }
                if (greenhouseDataView != null)
                {
                    greenhouseDataView.UseMeteoStations = value;
                }
                if (unpavedDataView != null)
                {
                    unpavedDataView.UseMeteoStations = value;
                }
                if (openWaterDataView != null)
                {
                    openWaterDataView.UseMeteoStations = value;
                }
            }
        }
        public IEventedList<string> MeteoStations {
            set
            {
                if (pavedDataView != null)
                {
                    pavedDataView.MeteoStations = value;
                }
                if (greenhouseDataView != null)
                {
                    greenhouseDataView.MeteoStations = value;
                }
                if (unpavedDataView != null)
                {
                    unpavedDataView.MeteoStations = value;
                }
                if (openWaterDataView != null)
                {
                    openWaterDataView.MeteoStations = value;
                }  
            }
        }

        private void BtnAddPavedClick(object sender, EventArgs e)
        {
            polderConcept.Catchment.AddSubCatchment(CatchmentType.Paved);
        }

        private void BtnAddUnpavedClick(object sender, EventArgs e)
        {
            polderConcept.Catchment.AddSubCatchment(CatchmentType.Unpaved);
        }

        private void BtnAddGreenhouseClick(object sender, EventArgs e)
        {
            polderConcept.Catchment.AddSubCatchment(CatchmentType.GreenHouse);
        }

        private void BtnAddOpenWaterClick(object sender, EventArgs e)
        {
            polderConcept.Catchment.AddSubCatchment(CatchmentType.OpenWater);
        }
    }
}