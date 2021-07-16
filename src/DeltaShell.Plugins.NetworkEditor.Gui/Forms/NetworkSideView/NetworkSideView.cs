using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Controls.Swf.Charting.Tools;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Drawing;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// </summary>
    public partial class NetworkSideView : UserControl, INetworkSideView, ITimeNavigatable, ICompositeView
    {
        private class SideViewChartData : IDisposable
        {
            public SideViewChartData(IFunction func, Color color, ChartSeriesType style)
            {
                Style = style;
                Color = color;
                Function = func;
            }

            private FunctionBindingList bindingList;
            public FunctionBindingList FunctionBindingList
            {
                get { return bindingList; }
            }

            private IFunction function;
            public IFunction Function
            {
                get { return function; }
                set
                {
                    function = value;
                    if (bindingList != null)
                    {
                        bindingList.Clear();
                        bindingList.Dispose();
                    }

                    bindingList = function != null ? new FunctionBindingList(function) {SynchronizeWaitMethod = Application.DoEvents} : null;
                }
            }

            public Color Color { get; set; }
            public ChartSeriesType Style { get; set; }

            public Action<IPointChartSeries> PointStyleCustomizer { get; set; }
            public Action<ILineChartSeries> LineStyleCustomizer { get; set; }
            public Action<IAreaChartSeries> AreaStyleCustomizer { get; set; }

            public void CustomizeChart(IChartSeries series)
            {
                if (series is IPointChartSeries && PointStyleCustomizer != null)
                {
                    PointStyleCustomizer((IPointChartSeries) series);
                }
                if (series is ILineChartSeries && LineStyleCustomizer != null)
                {
                    LineStyleCustomizer((ILineChartSeries) series);
                }
                if(series is IAreaChartSeries && AreaStyleCustomizer != null)
                {
                    AreaStyleCustomizer((IAreaChartSeries) series);
                }
            }

            public void Dispose()
            {
                if (bindingList != null)
                {
                    bindingList.Clear();
                    bindingList.Dispose();
                }
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkSideView));
        private static readonly Bitmap LateralSourceSmallIcon = Properties.Resources.LateralSourceSmall;
        private static readonly Bitmap RetentionIcon = Properties.Resources.Retention;
        private static readonly Bitmap ObservationIcon = Properties.Resources.Observation;

        private readonly VectorStyle normalCrossSectionStyle = new VectorStyle
        {
            Fill = new SolidBrush(Color.FromArgb(100, Color.LightBlue)),
            Line = new Pen(Color.Black)
        };
        private readonly VectorStyle selectedCrossSectionStyle = new VectorStyle
        {
            Fill = new SolidBrush(Color.LightBlue),
            Line = new Pen(Color.Black)
        };
        private readonly VectorStyle selectStyle = new VectorStyle
        {
            Fill = new SolidBrush(Color.FromArgb(150, Color.Gray)),
            Line = new Pen(Color.Black)
        };
        private readonly VectorStyle defaultStyle = new VectorStyle
        {
            Fill = Brushes.Transparent, 
            Line = new Pen(Color.Black)
        };

        private readonly Color pipeColor = Color.Beige;

        /// <summary>
        /// Holds a list of function pointers (lambda's) with a fixed time parameter to access the coverage (key). 
        /// The time parameter is taken from the time navigator.
        /// </summary>
        private bool contextMenuStripEnabled;
        private SideViewCoveragesContextMenu sideViewCoveragesContextMenu;

        private Route data;
        private NetworkSideViewDataController networkSideViewDataController;
        private readonly IList<SideViewChartData> bottomProfileChartData = new List<SideViewChartData>();
        private readonly IList<SideViewChartData> renderedCoveragesChartData = new List<SideViewChartData>();
        private readonly IDictionary<IFeature, IShapeFeature> FeatureToShape = new Dictionary<IFeature, IShapeFeature>();
        private readonly IDictionary<IShapeFeature, IFeature> ShapesToFeature = new Dictionary<IShapeFeature, IFeature>();

        private bool allowFeatureVisibilityChanges;
        private readonly StructurePresenter structurePresenter;
        private TimeArgumentNavigatable timeNavigator;
        private ShapeModifyTool shapeModifyTool;
        private ISeriesBandTool pipeSeriesBandTool;
        private ISeriesBandTool waterLevelPipesSeriesBandTool;
        private ISeriesBandTool waterLevelChannelsSeriesBandTool;
        private readonly IChart chart;
        private Dictionary<string, bool> seriesActiveCache;

        private readonly IEventedList<IView> childViews = new EventedList<IView>();

        public NetworkSideView()
        {
            InitializeComponent();
            chart = chartView.Chart;
            chart.Legend.Alignment = LegendAlignment.Bottom;
            AddToolsToChart();
            structurePresenter = new StructurePresenter(shapeModifyTool);
            ChartHeaderVisible = true;
            ChartLegendVisible = true;
            chartView.Chart.SurroundingBackGroundColor = SystemColors.ButtonFace;
            AllowFeatureVisibilityChanges = true;

            seriesActiveCache = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            childViews.Add(chartView);
        }

        public IFeature SelectedFeature
        {
            get
            {
                return null != shapeModifyTool.SelectedShape ? ShapesToFeature[shapeModifyTool.SelectedShape] : null;
            }
            set
            {
                UpdateShapeSelection(value);
                chartView.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the route network coverage used as a main data for this view.
        /// </summary>
        public object Data
        {
            get { return data; }
            set { data = value as Route; }
        }

        public NetworkSideViewDataController DataController
        {
            get { return networkSideViewDataController; }
            set
            {
                //unsubscribe old data
                UnsubscribeDataController();
                UnsubscribeTimeNavigator();

                networkSideViewDataController = value;
                if (networkSideViewDataController == null)
                {
                    return;
                }

                networkSideViewDataController.OnDataChanged = FullUpdate;

                UpdateChartTitles();

                SubscribeToDataController();

                FullUpdate();

                if (IsHandleCreated)
                {
                    DataController.OnSideViewHandleCreated();
                }
            }
        }

        /// <summary>
        /// Gets or sets the image for this view
        /// </summary>
        public Image Image { get; set; }

        public bool ContextMenuStripEnabled
        {
            get { return contextMenuStripEnabled; }
            set
            {
                if (contextMenuStripEnabled == value)
                {
                    return;
                }
                if (value != contextMenuStripEnabled)
                {
                    if (value)
                    {
                        chartView.Tools.Add(sideViewCoveragesContextMenu);
                    }
                    else
                    {
                        chartView.Tools.Remove(sideViewCoveragesContextMenu);
                    }
                }
                contextMenuStripEnabled = value;
            }
        }

        public bool ChartLegendVisible
        {
            get
            {
                return chartView.Chart.Legend.Visible;
            }
            set
            {
                chartView.Chart.Legend.Visible = value;
            }
        }

        public bool ChartHeaderVisible
        {
            get { return chartView.Chart.TitleVisible; }
            set { chartView.Chart.TitleVisible = value; }
        }

        public bool AllowFeatureVisibilityChanges
        {
            get { return allowFeatureVisibilityChanges; }
            set
            {
                allowFeatureVisibilityChanges = value;
                optionsPanel.Visible = allowFeatureVisibilityChanges;
            }
            
        }

        private Route NetworkRoute
        {
            get { return data; }
        }

        public object CommandReceiver
        {
            get { return structurePresenter; }
        }

        public DateTime? TimeSelectionStart
        {
            get { return timeNavigator.TimeSelectionStart; }
        }

        public DateTime? TimeSelectionEnd
        {
            get { return timeNavigator.TimeSelectionEnd; }
        }

        public TimeNavigatableLabelFormatProvider CustomDateTimeFormatProvider
        {
            get { return null; }
        }

        public IEnumerable<DateTime> Times
        {
            get { return timeNavigator.Times; }
        }

        public TimeSelectionMode SelectionMode
        {
            get { return TimeSelectionMode.Single; }
        }

        public SnappingMode SnappingMode
        {
            get { return SnappingMode.Nearest; }
        }

        public void UpdateStyles(IBranchFeature branchFeature, VectorStyle normalStyle, VectorStyle selectedStyle)
        {
            IShapeFeature shapeFeature;
            FeatureToShape.TryGetValue(branchFeature, out shapeFeature);
            if (shapeFeature != null)
            {
                shapeFeature.NormalStyle = normalStyle;
                shapeFeature.SelectedStyle = selectedStyle;
            }
        }

        public void UpdateFilter(ICoverage coverage)
        {
            if (coverage == null || !coverage.IsTimeDependent)
                return;
            
            var filteredTimeVariable = (IVariable)coverage.Time.Parent;
            var timeFilter = (IVariableValueFilter)coverage.Filters.FirstOrDefault(f => f.Variable == filteredTimeVariable);
            timeFilter.Values = new[] { timeNavigator.TimeSelectionStart };
        }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        [InvokeRequired]
        public void OnViewDataChanged(bool computeMinMax)
        {
            if (null == networkSideViewDataController)
            {
                // do not remove; see comment in Data set
                return;
            }

            if (NetworkRoute != null && NetworkRoute.Network != null && NetworkRoute.Network.IsEditing)
            {
                return;
            }

            if (computeMinMax)
            {
                UpdateMinMaxLeftAxis();
                UpdateMinMaxRightAxis();
            }

            string errorMessage = "No route defined for network side view.";
            if (NetworkRoute == null || !IsRouteValid(out errorMessage))
            {
                chart.Title = errorMessage;
                chart.LeftAxis.Visible = false;
                chart.RightAxis.Visible = false;
                return;
            }

            CreateChartSeries();

            UpdateCrossSectionShapes();
            UpdateStructureShapes();
            UpdateCompartmentShapes();
            UpdateTitle();
            RefreshView();
        }

        private void CreateChartSeries()
        {
            seriesActiveCache.Clear();
            chart.Series.ForEach(s => seriesActiveCache.Add(s.Title, s.Visible));
            chart.Series.Clear();
            shapeModifyTool.Clear();

            //clear the old binding lists
            bottomProfileChartData.ForEach(cd => cd.Dispose());
            bottomProfileChartData.Clear();
            renderedCoveragesChartData.ForEach(cd => cd.Dispose());
            renderedCoveragesChartData.Clear();

            // create chart data
            var bedLevelChartData = CreateBedLevelChartData().ToArray();
            bedLevelChartData.ForEach(d =>
            {
                chart.Series.Add(CreateSeries(d));
                bottomProfileChartData.Add(d);
            });

            chartView.Tools.Remove(waterLevelChannelsSeriesBandTool);

            var waterLevelChartData = CreateWaterLevelChartData();
            if (waterLevelChartData != null)
            {
                var waterLevelChartSeries = CreateSeries(waterLevelChartData);
                chart.Series.Add(waterLevelChartSeries);
                bottomProfileChartData.Add(waterLevelChartData);

                var bedLevelSeries = chart.Series.FirstOrDefault(s => s.Title.StartsWith(BedLevelNetworkCoverageBuilder.BedLevelCoverageName));
                if (bedLevelSeries != null)
                {
                    waterLevelChannelsSeriesBandTool = chartView.NewSeriesBandTool(waterLevelChartSeries, bedLevelSeries,
                                                                                   Color.FromArgb(72, Color.RoyalBlue));

                    chartView.Tools.Add(waterLevelChannelsSeriesBandTool);
                }
            }

            if (NetworkRoute.Segments.Values.All(s => s.Branch is ISewerConnection))
            {
                var pipeSeries = CreatePipeChartData().Select(CreateSeries).ToList();
                chart.Series.AddRange(pipeSeries);

                if (pipeSeries.Count > 1)
                {
                    chartView.Tools.Remove(pipeSeriesBandTool);
                    pipeSeriesBandTool = chartView.NewSeriesBandTool(pipeSeries[0], pipeSeries[1], pipeColor);
                    chartView.Tools.Add(pipeSeriesBandTool);
                }

                chartView.Tools.Remove(waterLevelPipesSeriesBandTool);

                var waterLevelInPipeChartData = CreateWaterLevelInPipeChartData();
                if (waterLevelInPipeChartData != null)
                {
                    var waterLevelInPipeSeries = CreateSeries(waterLevelInPipeChartData);
                    chart.Series.Add(waterLevelInPipeSeries);
                    waterLevelPipesSeriesBandTool = chartView.NewSeriesBandTool(waterLevelInPipeSeries, pipeSeries[1],
                                                                                Color.FromArgb(72, Color.RoyalBlue));
                    chartView.Tools.Add(waterLevelPipesSeriesBandTool);
                }
            }

            CreateRenderedCoverageChartData().ForEach(d =>
            {
                chart.Series.Add(CreateSeries(d));
                renderedCoveragesChartData.Add(d);
            });

            chart.Legend.ShowCheckBoxes = true;

            chart.Series.ForEach(s => s.Visible = !seriesActiveCache.ContainsKey(s.Title) || seriesActiveCache[s.Title]);
        }

        [InvokeRequired]
        private void RefreshView()
        {
            
        }

        private SideViewChartData CreateWaterLevelInPipeChartData()
        {
            var waterLevelInSideView = networkSideViewDataController.WaterLevelSideViewFunction;
            
            if (waterLevelInSideView == null) return null;

            var waterLevelInPipeFunction = NetworkSideViewHelper.GetWaterLevelInPipeFunction(data, waterLevelInSideView);
            return new SideViewChartData(waterLevelInPipeFunction, Color.RoyalBlue, ChartSeriesType.LineSeries)
            {
                FunctionBindingList = { SynchronizeInvoke = chartView },
                LineStyleCustomizer = (ls) =>
                {
                    ls.DashStyle = DashStyle.Dot;
                }
            };
        }

        private IEnumerable<SideViewChartData> CreatePipeChartData()
        {
            var functions = networkSideViewDataController?.PipeSideViewFunctions;
            if (functions == null) 
                    yield break;

            foreach (var function in functions)
            {
                yield return new SideViewChartData(function, Color.Black, ChartSeriesType.LineSeries)
                {
                    FunctionBindingList = {SynchronizeInvoke = chartView},
                    LineStyleCustomizer = (ls)=>
                    {
                        ls.DashStyle = DashStyle.Solid;
                        ls.Width = 2;
                    }
                };
            }
        }

        private IEnumerable<SideViewChartData> CreateRenderedCoverageChartData()
        {
            foreach (var sideViewFunction in networkSideViewDataController.RenderedNetworkSideViewFunctions)
            {
                var renderedNetworkCoverageChartData = new SideViewChartData(sideViewFunction,
                                                              ColorHelper.GetIndexedColor(chartView.Chart.Series.Count),
                                                              ChartSeriesType.LineSeries);
                renderedNetworkCoverageChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                renderedNetworkCoverageChartData.LineStyleCustomizer = (lcs) =>
                                                                           {
                                                                               lcs.DashStyle = DashStyle.Solid;
                                                                               lcs.Width = 2;
                                                                           };
                yield return renderedNetworkCoverageChartData;
            }

            foreach (var coverage in networkSideViewDataController.RenderedFeatureViewFunctions)
            {
                var renderedFeatureCoverageChartData = new SideViewChartData(coverage,
                                                              ColorHelper.GetIndexedColor(chartView.Chart.Series.Count),
                                                              ChartSeriesType.PointSeries);
                renderedFeatureCoverageChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                renderedFeatureCoverageChartData.PointStyleCustomizer = (pcs) => {};
                yield return renderedFeatureCoverageChartData;
            }
        }

        private SideViewChartData CreateWaterLevelChartData()
        {
            var waterLevelSideViewFunction = networkSideViewDataController.WaterLevelSideViewFunction;
            if (waterLevelSideViewFunction != null)
            {
                var waterLevelChartData = new SideViewChartData(waterLevelSideViewFunction,
                    Color.RoyalBlue,
                    ChartSeriesType.LineSeries);
                waterLevelChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                waterLevelChartData.LineStyleCustomizer = (lcs) =>
                {
                    lcs.Color = Color.RoyalBlue;
                    lcs.DashStyle = DashStyle.Solid;
                    lcs.Width = 2;
                };

                return waterLevelChartData;
            }

            return null;
        }

        private IEnumerable<SideViewChartData> CreateBedLevelChartData()
        {
            var profileSideViewFunctions = networkSideViewDataController.ProfileSideViewFunctions.ToList();
            
            var bottomLevelSideViewFunction = profileSideViewFunctions.FirstOrDefault(psvf => string.Equals(psvf.Name,BedLevelNetworkCoverageBuilder.BedLevelCoverageName));
            if (bottomLevelSideViewFunction != null)
            {
                var bottomLevelChartData = new SideViewChartData(bottomLevelSideViewFunction,
                                                                      Color.FromArgb(72, Color.YellowGreen),
                                                                      ChartSeriesType.AreaSeries);
                bottomLevelChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                bottomLevelChartData.AreaStyleCustomizer = (acs) => acs.LineVisible = false;
                yield return bottomLevelChartData;
            }

            var lowestEmbankmentSideViewFunction = profileSideViewFunctions.FirstOrDefault(psvf => string.Equals(psvf.Name, BedLevelNetworkCoverageBuilder.LowestEmbankmentCoverageName));
            if (lowestEmbankmentSideViewFunction != null)
            {
                var lowestEmbankmentChartData = new SideViewChartData(lowestEmbankmentSideViewFunction,
                                                                      Color.SaddleBrown,
                                                                      ChartSeriesType.LineSeries);
                lowestEmbankmentChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                lowestEmbankmentChartData.LineStyleCustomizer = (lcs) => { lcs.DashStyle = DashStyle.Dot; lcs.Width = 3; };
                yield return lowestEmbankmentChartData;
            }

            var leftEmbankmentSideViewFunction = profileSideViewFunctions.FirstOrDefault(psvf => string.Equals(psvf.Name, BedLevelNetworkCoverageBuilder.LeftEmbankmentCoverageName));
            if (leftEmbankmentSideViewFunction != null)
            {
                var leftEmbankmentChartData = new SideViewChartData(leftEmbankmentSideViewFunction,
                                                                      Color.Goldenrod,
                                                                      ChartSeriesType.LineSeries);
                leftEmbankmentChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                leftEmbankmentChartData.LineStyleCustomizer = (lcs) => { lcs.DashStyle = DashStyle.Dot; lcs.Width = 3; };
                yield return leftEmbankmentChartData;
            }

            var rightEmbankmentSideViewFunction = profileSideViewFunctions.FirstOrDefault(psvf => string.Equals(psvf.Name, BedLevelNetworkCoverageBuilder.RightEmbankmentCoverageName));
            if (rightEmbankmentSideViewFunction != null)
            {
                var rightEmbankmentChartData = new SideViewChartData(rightEmbankmentSideViewFunction,
                                                                      Color.RosyBrown,
                                                                      ChartSeriesType.LineSeries);
                rightEmbankmentChartData.FunctionBindingList.SynchronizeInvoke = chartView;
                rightEmbankmentChartData.LineStyleCustomizer = (lcs) => { lcs.DashStyle = DashStyle.Dot; lcs.Width = 3; };
                yield return rightEmbankmentChartData;
            }
        }

        public void SetCurrentTimeSelection(DateTime? start, DateTime? end)
        {
            timeNavigator.SetCurrentTimeSelection(start, end);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (!disposing)
            {
                base.Dispose(false);
                return;
            }

            if ((components != null))
            {
                components.Dispose();
            }
            base.Dispose(true);

            // dispose function binding lists:
            foreach (var chartData in bottomProfileChartData)
            {
                chartData.Dispose();
            }
            foreach (var chartData in renderedCoveragesChartData)
            {
                chartData.Dispose();
            }

            UnsubscribeDataController();
            UnsubscribeTimeNavigator();
        }

        public event EventHandler<SelectedItemChangedEventArgs> SelectionChanged;

        public event Action CurrentTimeSelectionChanged;

        public event Action TimesChanged;

        private void AddToolsToChart()
        {
            shapeModifyTool = new ShapeModifyTool(chart)
                                  {
                                      ShapeEditMode = (ShapeEditMode.ShapeSelect)
                                  };

            shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;

            shapeModifyTool.SelectStyle = selectStyle;
            shapeModifyTool.DefaultStyle = defaultStyle;

            chartView.Tools.Add(shapeModifyTool);
            sideViewCoveragesContextMenu = new SideViewCoveragesContextMenu(chartView)
            {
                NetworkSideView = this,
                Active = true,
                Enabled = true
            };
            ContextMenuStripEnabled = true;
        }

        private void CreateAndSubscribeTimeNavigator()
        {
            IVariableFilter filter;
            if (networkSideViewDataController.WaterLevelNetworkCoverage != null && networkSideViewDataController.WaterLevelNetworkCoverage.Time != null && networkSideViewDataController.WaterLevelNetworkCoverage.Filters.Count > 0)
            {
                filter = networkSideViewDataController.WaterLevelNetworkCoverage.Filters[0];
            }
            else
            { 
                //create empty filter
                var variable = new Variable<DateTime>();
                filter = new VariableValueFilter<DateTime>(variable, DateTime.MinValue);
            }

            timeNavigator = new TimeArgumentNavigatable((VariableValueFilter<DateTime>)filter);
            timeNavigator.TimeSelectionChanged += TimeNavigatorPropertyChanged;
            timeNavigator.TimesChanged += TimeNavigatorTimesChanged;
            TimeNavigatorTimesChanged(); //we're new, so times have changed
        }

        private void UpdateShapeSelection(IFeature feature)
        {
            //TODO: clean up refactor etc..this code is repeated all over.
            if (feature == null)
            {
                shapeModifyTool.SelectionChanged -= ShapeModifyToolSelectionChanged;
                shapeModifyTool.SelectedShape = null;
                shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
            }
            else
            {
                if (FeatureToShape.ContainsKey(feature))
                {
                    shapeModifyTool.SelectionChanged -= ShapeModifyToolSelectionChanged;
                    shapeModifyTool.SelectedShape = FeatureToShape[feature];
                    shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
                }
            }
        }

        private void OnSelectedFeatureChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectedItemChangedEventArgs(SelectedFeature));
            }
        }

        private void ShapeModifyToolSelectionChanged(object sender, ShapeEventArgs e)
        {
            if ((null != e.ShapeFeature) && (ShapesToFeature.ContainsKey(e.ShapeFeature)))
            {
                SelectedFeature = ShapesToFeature[e.ShapeFeature];
                OnSelectedFeatureChanged();
            }
            else
            {
                SelectedFeature = null;
                OnSelectedFeatureChanged();
            }
        }

        private void FullUpdate()
        {
            DataController.ResetMinMaxZ();

            UnsubscribeTimeNavigator();
            CreateAndSubscribeTimeNavigator();

            OnViewDataChanged(true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (DataController != null)
            {
                DataController.OnSideViewHandleCreated();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (DataController != null)
            {
                DataController.OnSideViewHandleDestroyed();
            }
        }

        private void SubscribeToDataController()
        {
            ((INotifyPropertyChanged)networkSideViewDataController).PropertyChanged += OnViewDataPropertyChanged;
            ((INotifyCollectionChanged)networkSideViewDataController.Network).CollectionChanged += NetworkCollectionChanged;
            sideViewCoveragesContextMenu.NetworkSideViewDataController = DataController;
            CreateAndSubscribeTimeNavigator();
        }

        private void UnsubscribeDataController()
        {
            if (networkSideViewDataController != null)
            {
                // Important this unregistering of handlers can occur in response to a collection changed
                // NetworkCollectionChanged may still be called after unregistering because the event was 
                // already fired and there are multiplpe listeners
                ((INotifyPropertyChanged)networkSideViewDataController).PropertyChanged -= OnViewDataPropertyChanged;
                if (networkSideViewDataController.Network != null)
                    ((INotifyCollectionChanged)networkSideViewDataController.Network).CollectionChanged -= NetworkCollectionChanged;
                sideViewCoveragesContextMenu.NetworkSideViewDataController = null;
                networkSideViewDataController.Dispose();//need to deregister events..hence dispose.
            }
        }

        private void UnsubscribeTimeNavigator()
        {
            if (timeNavigator != null)
            {
                timeNavigator.TimeSelectionChanged -= TimeNavigatorPropertyChanged;
                timeNavigator.TimesChanged -= TimeNavigatorTimesChanged;
            }
        }

        private void TimeNavigatorTimesChanged()
        {
            if (TimesChanged != null)
            {
                TimesChanged();
            }
        }

        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Data == null)
                return; //we're in the process of closing, stop doing anything here

            FullUpdate();
        }

        private void UpdateChartTitles()
        {
            chartView.Chart.BottomAxis.Title = string.Format("Chainage [{0}] along route", NetworkRoute.Components[0].Unit.Symbol);
            // todo: add support for other coverages, eg. velocity and 2nd axis.
            var bedLevelCoverage =
                DataController.ProfileNetworkCoverages.FirstOrDefault(nc => nc.Name == "Bed level");
            if (bedLevelCoverage != null)
            {
                chartView.Chart.LeftAxis.Title = string.Format("Level [{0}]",
                                                               bedLevelCoverage.
                                                                   Components[0].Unit.Symbol);
            }
            chartView.Chart.RightAxis.Minimum = 10;
            chartView.Chart.RightAxis.Maximum= 100;

            chartView.Chart.RightAxis.Visible = true;
            chartView.Chart.RightAxis.Title = "Value";
        }

        private void UpdateCompartmentShapes()
        {
            if (networkSideViewDataController == null)
            {
                // do not remove; see comment in Data set
                return;
            }

            var manHolesInRoute = networkSideViewDataController.ActiveManholes;

            foreach (var tuple in manHolesInRoute)
            {
                var manhole = tuple.Item1;
                var offset = tuple.Item2;

                AddManholeShape(manhole, offset);
            }
        }

        private void UpdateCrossSectionShapes()
        {
            if (networkSideViewDataController == null)
            {
                // do not remove; see comment in Data set
                return;
            }

            if (AllowFeatureVisibilityChanges && !showCrossSections.Checked)
            {
                return;
            }

            foreach (var crossSection in networkSideViewDataController.ActiveBranchFeatures.OfType<ICrossSection>())
            {
                AddCrossSectionShape(data, crossSection);
            }
        }

        private void UpdateStructureShapes()
        {
            if (null == networkSideViewDataController)
            {
                // do not remove; see comment in Data set
                return;
            }

            if (AllowFeatureVisibilityChanges && !showStructures.Checked)
            {
                return;
            }

            double minY = chartView.Chart.LeftAxis.Minimum;

            foreach (var branchFeature in networkSideViewDataController.ActiveBranchFeatures)
            {
                if (branchFeature is ICompositeBranchStructure)
                {
                    AddStructuresToChart((ICompositeBranchStructure) branchFeature, true);
                }
                else if (branchFeature is LateralSource)
                {
                    var source = branchFeature as LateralSource;
                    if (source.IsDiffuse)
                    {
                        AddDiffuseLateralSourceShape(branchFeature, minY);
                    }
                    else
                    {
                        AddImageShape(LateralSourceSmallIcon, branchFeature, minY);
                    }
                }
                else if (branchFeature is Retention)
                {
                    AddImageShape(RetentionIcon, branchFeature, minY);
                }
                else if (branchFeature is ObservationPoint)
                {
                    AddImageShape(ObservationIcon, branchFeature, minY);
                }
            }

            foreach (var branchFeature in networkSideViewDataController.InactiveBranchFeatures.OfType<ICompositeBranchStructure>())
            {
                AddStructuresToChart(branchFeature, false);
            }
        }

        private void AddStructuresToChart(ICompositeBranchStructure compositeStructure, bool active)
        {
            IShapeFeature shape = null;
            foreach (var structure in compositeStructure.Structures)
            {
                if (!FeatureToShape.TryGetValue(structure, out shape))
                {
                    if (structure is IWeir)
                    {
                        shape = GetWeirShape((IWeir) structure);
                    }
                    else if (structure is IPump)
                    {
                        shape = GetPumpShape((IPump) structure);
                    }
                    else if (structure is IBridge)
                    {
                        shape = GetBrigdeShape((IBridge) structure);
                    }
                    else if (structure is ICulvert)
                    {
                        shape = GetCulvertShape((ICulvert) structure);
                    }
                    else if (structure is IExtraResistance)
                    {
                        shape = GetExtraResistanceShape((IExtraResistance) structure);
                    }
                }

                shape.Active = active;
                AddStructureAndShape(structure, shape);
            }
        }
        
        private CulvertInSideViewShape GetCulvertShape(ICulvert culvert)
        {
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, culvert);

            return new CulvertInSideViewShape(chart, offset, culvert, NetworkSideViewHelper.GetReversed(NetworkRoute, culvert));
        }

        private BridgeInSideViewShape GetBrigdeShape(IBridge bridge)
        {
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, bridge);
         
            return new BridgeInSideViewShape(chart, offset,bridge);
        }
        
        private WeirInSideViewShape GetWeirShape(IWeir weir)
        {
            double offsetInSideView = RouteHelper.GetRouteChainage(NetworkRoute, weir);
            //pump levels etc.
            var weirInSideViewShape = new WeirInSideViewShape(chart, offsetInSideView, weir);
            return weirInSideViewShape;
        }

        private PumpInSideViewShape GetPumpShape(IPump pump)
        {
            Route route = NetworkRoute;
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, pump);
            // pump is reversed in route if segment of pump has reversed start and endofset in underlying 
            // branch.
            bool reversed = NetworkSideViewHelper.GetReversed(route, pump);
            //pump levels etc.
            return new PumpInSideViewShape(chart, offset, pump, reversed);
        }

        private ExtraResistanceInSideViewShape GetExtraResistanceShape(IExtraResistance extraResistance)
        {
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, extraResistance);
            return new ExtraResistanceInSideViewShape(chart, offset, extraResistance);
        }
        
        private void AddStructureAndShape(IStructure1D structure, IShapeFeature symbolShapeFeature)
        {
            var hoverText = new HoverText(structure.Name, null, symbolShapeFeature, Color.Black, HoverPosition.Top, ArrowHeadPosition.None);
            if (symbolShapeFeature is IHover hoverShape)
            {
                hoverShape.ClearHovers();
                hoverShape.AddHover(new HoverRectangle(symbolShapeFeature, Color.FromArgb(50, Color.DarkTurquoise)));
                hoverShape.AddHover(hoverText);
            }

            if (structure is IWeir)
            {
                hoverText.BackColor = Color.LightPink;
            }
            else if (structure is IPump)
            {
                hoverText.BackColor = Color.LightGoldenrodYellow;
            }
            else if (structure is IBridge)
            {
                hoverText.BackColor = Color.LightGray;
            }
            else if (structure is ICulvert)
            {
                hoverText.BackColor = Color.Thistle;
            }
            if(!shapeModifyTool.ShapeFeatures.Contains(symbolShapeFeature)) 
                shapeModifyTool.AddShape(symbolShapeFeature);
            FeatureToShape[structure] = symbolShapeFeature;
            ShapesToFeature[symbolShapeFeature] = structure;
        }

        private void AddImageShape(Image image, IBranchFeature structure, double minY)
        {
            if (!FeatureToShape.TryGetValue(structure, out var symbolShapeFeature))
            {
                double offset = RouteHelper.GetRouteChainage(NetworkRoute, structure);
                symbolShapeFeature = new SymbolShapeFeature(chart, offset, minY,
                        SymbolShapeFeatureHorizontalAlignment.Center,
                        SymbolShapeFeatureVerticalAlignment.Center)
                    {Image = image};
                shapeModifyTool.AddShape(symbolShapeFeature);
                if (symbolShapeFeature is IHover hoverShape)
                {
                    hoverShape.AddHover(new HoverRectangle(symbolShapeFeature,Color.FromArgb(100, Color.Cyan)));
                    var hoverText = new HoverText(structure.Name, null, symbolShapeFeature, Color.Black, HoverPosition.Bottom, ArrowHeadPosition.None) {BackColor = Color.WhiteSmoke};
                    hoverShape.AddHover(hoverText);
                }
            }
            if (!shapeModifyTool.ShapeFeatures.Contains(symbolShapeFeature))
                shapeModifyTool.AddShape(symbolShapeFeature);
            FeatureToShape[structure] = symbolShapeFeature;
            ShapesToFeature[symbolShapeFeature] = structure;
        }

        private void AddDiffuseLateralSourceShape(IBranchFeature structure, double minY)
        {
            if (!FeatureToShape.TryGetValue(structure, out var symbolShapeFeature))
            {
                double offset = RouteHelper.GetRouteChainage(NetworkRoute, structure);
                double length = structure.Length;

                var renderStyle = new VectorStyle
                {
                    Line = new Pen(Color.MediumVioletRed, 2),
                    Outline = new Pen(Color.MediumVioletRed, 2)
                };

                renderStyle.Outline.DashStyle = DashStyle.Dash;
                renderStyle.Line.DashStyle = DashStyle.Dash;

                var hoverStyle = renderStyle;
                hoverStyle.Outline.Color = Color.LightBlue;

                symbolShapeFeature = new FixedRectangleShapeFeature(chart, offset, minY, length, 6, true, false)
                {
                    NormalStyle = renderStyle,
                };
                if (symbolShapeFeature is IHover hoverShape)
                {
                    hoverShape.AddHover(new HoverRectangle(symbolShapeFeature, hoverStyle));
                    var hoverText = new HoverText(structure.Name, null, symbolShapeFeature, Color.Black,
                        HoverPosition.Bottom, ArrowHeadPosition.None) {BackColor = Color.WhiteSmoke};
                    hoverShape.AddHover(hoverText);
                }
            }
            if (!shapeModifyTool.ShapeFeatures.Contains(symbolShapeFeature))
                shapeModifyTool.AddShape(symbolShapeFeature);

            FeatureToShape[structure] = symbolShapeFeature;
            ShapesToFeature[symbolShapeFeature] = structure;
        }

        private void AddCrossSectionShape(Route route, ICrossSection crossSection)
        {
            double offset = RouteHelper.GetRouteChainage(route, crossSection);

            var crossSectionShape = new CrossSectionInSideViewShape(chart, offset, 6, crossSection.Definition)
                                        {
                                            HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                                            VerticalShapeAlignment = VerticalShapeAlignment.Top,
                                            NormalStyle = normalCrossSectionStyle,
                                            SelectedStyle = selectedCrossSectionStyle
                                        };
            crossSectionShape.AddHover(new HoverRectangle(crossSectionShape, Color.FromArgb(50, Color.Blue)));
            crossSectionShape.AddHover(new HoverText(crossSection.Name, null, crossSectionShape, Color.Black,
                                                     HoverPosition.Top, ArrowHeadPosition.LeftRight) { BackColor = Color.LightCyan });
            shapeModifyTool.AddShape(crossSectionShape);
            FeatureToShape[crossSection] = crossSectionShape;
            ShapesToFeature[crossSectionShape] = crossSection;
        }

        private void AddManholeShape(IManhole manhole, double offset)
        {
            var compartmentShape = new ManHoleSideViewShape(chart, offset, manhole)
            {
                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                VerticalShapeAlignment = VerticalShapeAlignment.Top,
                SelectedStyle = selectedCrossSectionStyle
            };

            compartmentShape.AddHover(new HoverRectangle(compartmentShape, Color.LightGray));
            compartmentShape.AddHover(new HoverText("ManHole:" + manhole.Name,"",
                compartmentShape, Color.Black, HoverPosition.Top, ArrowHeadPosition.None));

            shapeModifyTool.AddShape(compartmentShape);
            FeatureToShape[manhole] = compartmentShape;
            ShapesToFeature[compartmentShape] = manhole;
        }

        private void TimeNavigatorPropertyChanged(object sender, EventArgs e)
        {
            var timeDependentNetworkCoverages = DataController.RenderedNetworkCoverages.Concat(new[] { DataController.WaterLevelNetworkCoverage });

            // HACK: this looks ugly, how can we do it simpler (methods in function)?
            //update time dependent coverages.
            foreach (var coverage in timeDependentNetworkCoverages)
            {
                UpdateFilter(coverage);
            }

            var timeDependentFeatureCoverages = DataController.RenderedFeatureCoverages;
            
            foreach (var coverage in timeDependentFeatureCoverages)
            {
                UpdateFilter(coverage);
            }

            UpdateTitle();
            OnViewDataChanged(false);
        }

        private void UpdateTitle()
        {
            if (null != networkSideViewDataController.WaterLevelNetworkCoverage && null != networkSideViewDataController.WaterLevelNetworkCoverage.Time)
            {
                chartView.Chart.Title = string.Format("{0} at {1}", NetworkRoute,
                                                             timeNavigator.TimeSelectionStart);
            }
            else
            {
                chartView.Chart.Title = string.Format("{0}", NetworkRoute);
            }
        }

        private bool IsRouteValid(out string message)
        {
            message = "";

            if (NetworkRoute.SegmentGenerationMethod != SegmentGenerationMethod.RouteBetweenLocations)
            {
                message = string.Format("{0} can not be used as a route for sideview.", NetworkRoute.Name);
                log.ErrorFormat(message);
                return false;
            }

            if (RouteHelper.RouteContainLoops(NetworkRoute))
            {
                message = string.Format("{0} is not a valid route; it contains loops.", NetworkRoute.Name);
                log.ErrorFormat(message);
                return false;   
            }

            if (NetworkRoute.Locations.Values.Count < 2)
            {
                message = string.Format("{0} is not a valid route; add extra locations first.", NetworkRoute.Name);
                log.ErrorFormat(message);
                return false;
            }

            if (!NetworkRoute.Network.IsEditing && RouteHelper.IsDisconnected(NetworkRoute) && !NetworkRoute.Locations.Values.All(l => l.Branch is ISewerConnection && Equals(l.Branch?.Source, l.Branch?.Target)))
            {
                message = string.Format("{0} is not a valid route; is the route disconnected?", NetworkRoute.Name);
                log.ErrorFormat(message);
                return false;
            }
            return true;
        }

        private void UpdateMinMaxRightAxis()
        {
            double min = networkSideViewDataController.ZMinValueRenderedCoverages;
            double max = networkSideViewDataController.ZMaxValueRenderedCoverages;


            NetworkSideViewHelper.UpdateMinMaxToEnsureVerticalResolution(ref min, ref max);

            // Set an offset of 15 pixels around the chart
            chartView.Chart.RightAxis.MinimumOffset = 15;
            chartView.Chart.RightAxis.MaximumOffset = 15;
            chartView.Chart.RightAxis.Minimum = min;
            chartView.Chart.RightAxis.Maximum = max;
            chartView.Chart.RightAxis.Automatic = false;
            chartView.Chart.RightAxis.Visible = true;
        }

        private void UpdateMinMaxLeftAxis()
        {
            double min = networkSideViewDataController.ZMinValue;
            double max = networkSideViewDataController.ZMaxValue;
            

            NetworkSideViewHelper.UpdateMinMaxToEnsureVerticalResolution(ref min, ref max);

            // Set an offset of 15 pixels around the chart
            chartView.Chart.LeftAxis.MinimumOffset = 15;
            chartView.Chart.LeftAxis.MaximumOffset = 15;
            chartView.Chart.LeftAxis.Minimum = min;
            chartView.Chart.LeftAxis.Maximum = max;
            chartView.Chart.LeftAxis.Automatic = false;
            chartView.Chart.LeftAxis.Visible = true;
        }

        private void OnViewDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IBranchFeature)
            {
                var structure = (IBranchFeature) sender;

                if (FeatureToShape.ContainsKey(structure))
                {
                    if(e.PropertyName=="Name")
                    {
                       FullUpdate();
                    }
                    // shapes are up-to-update by definition :) only a redraw is necessary
                    chartView.Invalidate();
                    if (!structure.Network.IsEditing)
                    {
                        UpdateMinMaxLeftAxis();
                    }
                }
            }

            if (sender is ICrossSectionDefinition)
            {
                foreach(var structure in FeatureToShape.Keys.OfType<ICrossSection>())
                {
                    var crossSectionDefinition = structure.Definition;
                    if(Equals(sender, crossSectionDefinition))
                    {
                        FullUpdate();
                        return;
                    }
                    if(crossSectionDefinition.IsProxy && Equals(sender, ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition))
                    {
                        FullUpdate();
                        return;
                    }
                }
            }

            if(sender is CrossSectionStandardShapeBase)
            {
                foreach (var structure in FeatureToShape.Keys.OfType<ICrossSection>())
                {
                    var crossSectionDefinition = (structure.Definition.IsProxy
                                                     ? ((CrossSectionDefinitionProxy)structure.Definition).
                                                           InnerDefinition
                                                     : structure.Definition) as CrossSectionDefinitionStandard;

                    if (crossSectionDefinition == null || !Equals(crossSectionDefinition.Shape, sender)) continue;
                    FullUpdate();
                    return;
                }
            }

            //TODO: get a list going with types and properties.. try to keep it refactor proof.
            if (sender is NetworkSideViewDataController)
            {
                var data = (NetworkSideViewDataController)sender;

                if (e.PropertyName.Equals(nameof(data.WaterLevelNetworkCoverage)))
                {
                    UnsubscribeTimeNavigator();
                    CreateAndSubscribeTimeNavigator();
                }

                var interestingMembers = new[]
                                      {
                                          nameof(data.WaterLevelNetworkCoverage),
                                          nameof(data.ProfileNetworkCoverages),
                                          nameof(data.RenderedNetworkCoverages),
                                          nameof(data.RenderedFeatureCoverages)
                                      };

                if (interestingMembers.Contains(e.PropertyName))
                {
                    OnViewDataChanged(true);
                }
            }
        }

        private IChartSeries CreateSeries(SideViewChartData sideViewChartData)
        {
            var function = sideViewChartData.Function;
            NetworkSideViewHelper.ValidateFunction(function);
            var xArgument = function.GetFirstArgumentVariableOfType<double>();
            var yComponent = function.GetFirstComponentVariableOfType<double>();

            IChartSeries chartSeries = null;
            switch(sideViewChartData.Style)
            {
                case ChartSeriesType.PointSeries:
                    chartSeries = NetworkSideViewHelper.GetPointSeries(function, xArgument, yComponent,
                                                                       sideViewChartData.FunctionBindingList,
                                                                       sideViewChartData.Color,
                                                                       PointerStyles.Circle, 6);
                    break;
                case ChartSeriesType.LineSeries:
                    chartSeries = NetworkSideViewHelper.GetLineSeries(function, xArgument, yComponent,
                                                                      sideViewChartData.FunctionBindingList,
                                                                      sideViewChartData.Color);
                    ((ILineChartSeries)chartSeries).DashStyle = DashStyle.Dot;
                    break;
                case ChartSeriesType.AreaSeries:
                    chartSeries = NetworkSideViewHelper.GetAreaSeries(function, xArgument, yComponent,
                                                                      sideViewChartData.FunctionBindingList,
                                                                      sideViewChartData.Color);
                    break;
            }

            if (chartSeries != null)
            {
                sideViewChartData.CustomizeChart(chartSeries);
                chartSeries.VertAxis = function.Components[0].Unit.Symbol != "m AD" ? VerticalAxis.Right : VerticalAxis.Left;
            }

            return chartSeries;
        }

        private void ShowCrossSectionsCheckedChanged(object sender, EventArgs e)
        {
            FullUpdate();
        }

        private void ShowStructuresCheckedChanged(object sender, EventArgs e)
        {
            FullUpdate();
        }

        public IEventedList<IView> ChildViews
        {
            get { return childViews; }
        }

        public bool HandlesChildViews { get { return true; } }

        public void ActivateChildView(IView childView) { }
    }
}