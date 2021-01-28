using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class MeteoDataView : UserControl, ICompositeView, IRRModelTimeAwareView
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MeteoDataView));
        private readonly DelayedEventHandler<EventArgs> delayedEventHandlerCatchmentsChanged;
        private MeteoData meteoData;
        private readonly MultipleFunctionView functionView;
        private IEventedList<string> stations;
        private readonly IEventedList<IView> childViews = new EventedList<IView>();

        public MeteoDataView()
        {
            InitializeComponent();

            functionView = new MultipleFunctionView
                {
                    Dock = DockStyle.Fill
                };
            pnlView.Controls.Add(functionView);
            if (functionView?.TableView != null) functionView.TableView.SelectionChanged += TableViewOnSelectionChanged;
            cmbMeteoDataType.DataSource = Enum.GetValues(typeof (MeteoDataDistributionType));
            cmbMeteoDataType.SelectedValueChanged += CmbMeteoDataTypeSelectedValueChanged;

            delayedEventHandlerCatchmentsChanged =
                new DelayedEventHandler<EventArgs>(MeteoDataDataChanged)
                    {
                        FireLastEventOnly = true,
                        Delay = 500,
                        SynchronizingObject = this
                    };

            if (functionView != null)
            {
                childViews.AddRange(functionView.ChildViews);
            }

            if (stationsListEditor != null)
                stationsListEditor.MeteoStationsSelected += StationsListEditorOnMeteoStationsSelected;
        }

        private void StationsListEditorOnMeteoStationsSelected(object sender, MeteoStationsSelectedEventArgs e)
        {
            if (functionView?.TableView?.Columns == null) return;

            var selectedRow = functionView.TableView.FocusedRowIndex;
            functionView.TableView.ClearSelection();
            e.SelectedMeteoStations.ForEach(meteoStation =>
            {
                var toBeSelectedColumn = functionView.TableView.Columns.IndexOf(functionView.TableView.GetColumnByName(meteoStation));
                functionView.TableView.SelectCells(
                    selectedRow,
                    toBeSelectedColumn,
                    selectedRow,
                    toBeSelectedColumn,
                    false);
            });
        }


        private void TableViewOnSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            stationsListEditor?.SetSelection(e.Cells.Select(c => c.Column).Distinct().Select(c => c.Name));
        }

        #region IView<MeteoData> Members

        public object Data
        {
            get { return meteoData; }
            set
            {
                if (meteoData != null)
                {
                    ((INotifyPropertyChanged) meteoData).PropertyChanged -= MeteoDataPropertyChanged;
                    meteoData.CatchmentsChanged -= delayedEventHandlerCatchmentsChanged;
                }
                meteoData = (MeteoData) value;

                if (meteoData != null)
                {
                    Text = meteoData.Name;
                    ((INotifyPropertyChanged) meteoData).PropertyChanged += MeteoDataPropertyChanged;
                    meteoData.CatchmentsChanged += delayedEventHandlerCatchmentsChanged;
                    SetMeteoDataView();
                    SetMeteoDataTypeComboBox();
                }
                else
                {
                    ClearDataOfExistingViews();
                    Stations = null; //unsubscribe
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void MeteoDataDataChanged(object sender, EventArgs e)
        {
            SetMeteoDataView();
        }

        /// <summary>
        /// Just one control -> no binding source
        /// </summary>
        private void SetMeteoDataTypeComboBox()
        {
            if ((MeteoDataDistributionType) cmbMeteoDataType.SelectedValue !=
                meteoData.DataDistributionType)
            {
                cmbMeteoDataType.SelectedItem = meteoData.DataDistributionType;
            }
        }

        private void MeteoDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(sender, meteoData)) return;

            if (e.PropertyName != "DataDistributionType" &&
                (e.PropertyName != "IsEditing" || meteoData.IsEditing)) return;

            SetMeteoDataView();
            SetMeteoDataTypeComboBox();
        }

        [EditAction]
        private void CmbMeteoDataTypeSelectedValueChanged(object sender, EventArgs e)
        {
            if (Data == null)
            {
                return;
            }

            var cmb = (ComboBox) sender;
            var selectedType = (MeteoDataDistributionType) cmb.SelectedValue;
            if (meteoData.DataDistributionType != selectedType)
            {
                meteoData.DataDistributionType = selectedType;
            }
        }

        private void SetMeteoDataView()
        {
            ClearDataOfExistingViews();
            stationsListEditor.Visible = false;

            var seriesType = meteoData.DataAggregationType == MeteoDataAggregationType.Cumulative
                                 ? ChartSeriesType.BarSeries
                                 : ChartSeriesType.LineSeries;
            functionView.ChartSeriesType = seriesType;
            switch (meteoData.DataDistributionType)
            {
                case MeteoDataDistributionType.Global:

                    functionView.Name = meteoData.Name + "(global)";
                    functionView.Data = new[] {meteoData.Data};
                    functionView.TableView.AllowColumnSorting = true;

                    break;
                case MeteoDataDistributionType.PerFeature:
                    var functions =
                        TimeDependentFunctionSplitter.SplitIntoFunctionsPerArgumentValue(
                            meteoData.Data as IFeatureCoverage);
                    if (functions.Count == 0)
                    {
                        pnlView.Controls.Add(new Label {Text = "No catchments found, nothing to show.", Dock = DockStyle.Top});
                        return;
                    }

                    functionView.Name = meteoData.Name + "(per catchment)";
                    functionView.OnCreateBindingList = CreateBindingList;
                    functionView.ChartViewOption = ChartViewOptions.SelectedColumns;
                    functionView.Functions = functions;
                    functionView.TableView.AllowColumnSorting = false;

                    break;

                case MeteoDataDistributionType.PerStation:
                    
                    stationsListEditor.Visible = true;
                    stationsListEditor.Data = stations;

                    functionView.Name = meteoData.Name + "(per station)";
                    functionView.OnCreateBindingList = CreateBindingList;
                    functionView.ChartViewOption = ChartViewOptions.SelectedColumns;
                    RefreshMeteoStationsData();
                    functionView.TableView.AllowColumnSorting = false;

                    break;
                default:
                    pnlView.Controls.Add(new Label
                        {
                            Text =
                                String.Format("No view available for meteorological data type {0}.",
                                              meteoData.DataDistributionType)
                        });
                    log.ErrorFormat("No view available for meteorological data type {0}.",
                                    meteoData.DataDistributionType);
                    return;
            }
            functionView.TableView.IsEndEditOnEnterKey = true;
        }

        private void RefreshMeteoStationsData()
        {
            IList functionsList = functionView.Functions as IList; 
            if (functionsList != null)
            {
                functionsList.Clear(); 
            }
            functionView.Functions = TimeDependentFunctionSplitter.SplitIntoFunctionsPerArgumentValue(meteoData.Data);
        }

        private void ClearDataOfExistingViews()
        {
            foreach (var view in pnlView.Controls.OfType<IView>())
            {
                view.Data = null; //allows sub-views to cleanup resources
            }
            stationsListEditor.Data = null;
        }

        private static IFunctionBindingList CreateBindingList(IEnumerable<IFunction> functions)
        {
            return new SplitFunctionsBindingList(functions);
        }

        private void GenerateBtnClick(object sender, EventArgs e)
        {
            var timeSeries = GetTimeVariable();

            if (timeSeries == null)
            {
                MessageBox.Show("Unknown error: no time series found");
                return;
            }

            var generateDialog = new TimeSeriesGeneratorDialog();

            var hintTimeStep = meteoData.Name != null && meteoData.Name.Contains("Evap") ? new TimeSpan(1, 0, 0, 0) : TimeStep;
            generateDialog.SetData(timeSeries, StartTime, StopTime, hintTimeStep);

            ClearDataOfExistingViews();

            EditableObjectExtensions.BeginEdit(meteoData, "Generate/modify timeseries");

            generateDialog.ShowDialog();

            meteoData.EndEdit();

            SetMeteoDataView();
        }

        private IVariable<DateTime> GetTimeVariable()
        {
            return Enumerable.OfType<IVariable<DateTime>>(meteoData.Data.Arguments).FirstOrDefault();
        }

        public IEventedList<IView> ChildViews
        {
            get { return childViews; }
        }

        public bool HandlesChildViews { get; private set; }

        public void ActivateChildView(IView childView) { }

        public DateTime StartTime { private get; set; }

        public DateTime StopTime { private get; set; }

        public TimeSpan TimeStep { private get; set; }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (Data != null)
                Data = null;

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected IEventedList<string> Stations
        {
            get { return stations; }
            set
            {
                if (stations != null)
                {
                    stations.CollectionChanged -= StationsCollectionChanged;
                }
                stations = value;
                stationsListEditor.Data = value;
                if (stations != null)
                {
                    stations.CollectionChanged += StationsCollectionChanged;
                }
            }
        }

        void StationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (meteoData.DataDistributionType != MeteoDataDistributionType.PerStation ||
                meteoData.Data.Arguments[1].Values.Count != Stations.Count) return;

            // meteo data and model stations should be in sync
            RefreshMeteoStationsData();
        }
    }

    public class PrecipitationMeteoDataView: MeteoDataView, IRRMeteoStationAwareView
    {
        public bool UseMeteoStations { get; set; }

        public IEventedList<string> MeteoStations
        {
            get { return Stations; }
            set { Stations = value; }
        }
    }

    public class TemperatureMeteoDataView : MeteoDataView, IRRTemperatureStationAwareView
    {
        public bool UseTemperatureStations { get; set; }

        public IEventedList<string> TemperatureStations
        {
            get { return Stations; }
            set { Stations = value; }
        }
    }
}