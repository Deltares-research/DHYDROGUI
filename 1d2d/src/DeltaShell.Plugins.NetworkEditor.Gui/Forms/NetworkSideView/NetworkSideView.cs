using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
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
    public partial class NetworkSideView : UserControl, INetworkSideView, ITimeNavigatable, ICompositeView, ISuspendibleView
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkSideView));

        /// <summary>
        /// Holds a list of function pointers (lambda's) with a fixed time parameter to access the coverage (key). 
        /// The time parameter is taken from the time navigator.
        /// </summary>
        private bool contextMenuStripEnabled;
        private readonly SideViewCoveragesContextMenu sideViewCoveragesContextMenu;

        private Route route;
        private NetworkSideViewDataController networkSideViewDataController;
        private readonly NetworkSideViewShapeHandler shapeHandler;
        private readonly NetworkSideViewChartSeriesController chartController;
        
        private TimeArgumentNavigatable timeNavigator;

        private readonly IChart chart;
        private readonly IEventedList<IView> childViews = new EventedList<IView>();
        private bool routeChecked = false;

        public NetworkSideView()
        {
            InitializeComponent();
            chart = chartView.Chart;
            chart.Legend.Alignment = LegendAlignment.Bottom;

            shapeHandler = new NetworkSideViewShapeHandler(chart)
            {
                OnSelectedFeatureChanged = OnSelectedFeatureChanged
            };

            chartView.Tools.Add(shapeHandler.ModifyTool);

            chartController = new NetworkSideViewChartSeriesController(chartView);
            
            sideViewCoveragesContextMenu = new SideViewCoveragesContextMenu(chartView)
            {
                NetworkSideView = this,
                Active = true,
                Enabled = true
            };
            ContextMenuStripEnabled = true;

            ChartHeaderVisible = true;
            ChartLegendVisible = true;
            chartView.Chart.SurroundingBackGroundColor = SystemColors.ButtonFace;
            AllowFeatureVisibilityChanges = true;

            childViews.Add(chartView);
        }

        public IFeature SelectedFeature
        {
            get
            {
                return shapeHandler.SelectedFeature;
            }
            set
            {
                shapeHandler.SelectedFeature = value;
                chartView.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the route network coverage used as a main data for this view.
        /// </summary>
        public object Data
        {
            get => route;
            set
            {
                UnsubscribeFromRoute();

                route = value as Route;

                InitializeForRoute(route);
                SubscribeToRoute();
            }
        }

        private void InitializeForRoute(Route value)
        {
            chartController.Route = value;
            shapeHandler.NetworkRoute = value;

            routeChecked = false;
        }

        private void SubscribeToRoute()
        {
            if (route == null)
            {
                return;
            }

            route.ValuesChanged += RouteValuesChanged;
        }

        private void UnsubscribeFromRoute()
        {
            if (route == null)
            {
                return;
            }

            route.ValuesChanged -= RouteValuesChanged;
        }

        private void RouteValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            routeChecked = false;
        }

        public NetworkSideViewDataController DataController
        {
            get { return networkSideViewDataController; }
            set
            {
                //unsubscribe old data
                UnsubscribeAndDisposeDataController();
                UnsubscribeTimeNavigator();

                networkSideViewDataController = value;
                chartController.NetworkSideViewDataController = value;
                shapeHandler.NetworkSideViewDataController = value;
                if (networkSideViewDataController == null)
                {
                    return;
                }

                networkSideViewDataController.OnDataChanged = FullUpdate;

                UpdateChartTitles();

                SubscribeToAndSetDataController();

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
            get { return chartView.Chart.Legend.Visible; }
            set { chartView.Chart.Legend.Visible = value; }
        }

        public bool ChartHeaderVisible
        {
            get { return chartView.Chart.TitleVisible; }
            set { chartView.Chart.TitleVisible = value; }
        }

        public bool AllowFeatureVisibilityChanges
        {
            get { return shapeHandler.AllowFeatureVisibilityChanges; }
            set
            {
                shapeHandler.AllowFeatureVisibilityChanges = value;
                optionsPanel.Visible = shapeHandler.AllowFeatureVisibilityChanges;
            }
        }

        private Route NetworkRoute
        {
            get { return route; }
        }

        public object CommandReceiver
        {
            get { return shapeHandler.StructurePresenter; }
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

        public ViewInfo ViewInfo { get; set; }

        public IEventedList<IView> ChildViews
        {
            get { return childViews; }
        }

        public bool HandlesChildViews { get { return true; } }

        public void UpdateStyles(IBranchFeature branchFeature, VectorStyle normalStyle, VectorStyle selectedStyle)
        {
            shapeHandler.UpdateStyles(branchFeature, normalStyle, selectedStyle);
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

        [InvokeRequired]
        public void OnViewDataChanged(bool computeMinMax)
        {
            if (null == networkSideViewDataController?.NetworkRoute 
                || null == chartController?.Route)
            {
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

            shapeHandler.ModifyTool.Clear();
            shapeHandler.UpdateAllShapes();

            chartController.CreateChartSeries();
            UpdateTitle();
        }

        public void SetCurrentTimeSelection(DateTime? start, DateTime? end)
        {
            timeNavigator.SetCurrentTimeSelection(start, end);
        }

        public void ActivateChildView(IView childView) { }
        
        public event EventHandler<SelectedItemChangedEventArgs> SelectionChanged;

        public event Action CurrentTimeSelectionChanged;

        public event Action TimesChanged;

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

            chartController.Dispose();
            UnsubscribeAndDisposeDataController();
            UnsubscribeTimeNavigator();

            Data = null;
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
            SubscribeTimeNavigator();
            TimeNavigatorTimesChanged(); //we're new, so times have changed
        }

        private void SubscribeTimeNavigator()
        {
            timeNavigator.TimeSelectionChanged += TimeNavigatorPropertyChanged;
            timeNavigator.TimesChanged += TimeNavigatorTimesChanged;
        }

        private void OnSelectedFeatureChanged()
        {
            SelectionChanged?.Invoke(this, new SelectedItemChangedEventArgs(SelectedFeature));
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

        private void SubscribeToAndSetDataController()
        {
            SubscribeToDataController();
            sideViewCoveragesContextMenu.NetworkSideViewDataController = DataController;
            CreateAndSubscribeTimeNavigator();
        }

        private void SubscribeToDataController()
        {
            ((INotifyPropertyChanged)networkSideViewDataController).PropertyChanged += OnViewDataPropertyChanged;
            ((INotifyCollectionChanged)networkSideViewDataController.Network).CollectionChanged += NetworkCollectionChanged;
        }

        private void UnsubscribeAndDisposeDataController()
        {
            if (networkSideViewDataController == null)
            {
                return;
            }

            // Important this unregistering of handlers can occur in response to a collection changed
            // NetworkCollectionChanged may still be called after unregistering because the event was 
            // already fired and there are multiplpe listeners
            UnsubscribeFromDataController();
                    
            sideViewCoveragesContextMenu.NetworkSideViewDataController = null;
            networkSideViewDataController.Dispose(); //need to deregister events..hence dispose.
        }

        private void UnsubscribeFromDataController()
        {
            if (networkSideViewDataController == null)
            {
                return;
            }

            ((INotifyPropertyChanged)networkSideViewDataController).PropertyChanged -= OnViewDataPropertyChanged;

            if (networkSideViewDataController.Network == null)
            {
                return;
            }

            ((INotifyCollectionChanged)networkSideViewDataController.Network).CollectionChanged -= NetworkCollectionChanged;
        }

        private void UnsubscribeTimeNavigator()
        {
            if (timeNavigator == null)
            {
                return;
            }

            timeNavigator.TimeSelectionChanged -= TimeNavigatorPropertyChanged;
            timeNavigator.TimesChanged -= TimeNavigatorTimesChanged;
        }

        private void TimeNavigatorTimesChanged()
        {
            TimesChanged?.Invoke();
        }

        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Data == null)
            {
                return; //we're in the process of closing, stop doing anything here
            }

            FullUpdate();
        }

        private void UpdateChartTitles()
        {
            chartView.Chart.BottomAxis.Title = string.Format("Chainage [{0}] along route", NetworkRoute.Components[0].Unit.Symbol);

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
            chartView.Chart.Title = networkSideViewDataController.WaterLevelNetworkCoverage?.Time != null 
                                        ? $"{NetworkRoute} at {timeNavigator.TimeSelectionStart}" 
                                        : $"{NetworkRoute}";
        }

        private bool IsRouteValid(out string message)
        {
            message = "";

            if (routeChecked) return true;

            if (NetworkRoute.SegmentGenerationMethod != SegmentGenerationMethod.RouteBetweenLocations)
            {
                message = $"{NetworkRoute.Name} can not be used as a route for sideview.";
                log.ErrorFormat(message);
                return false;
            }

            if (RouteHelper.RouteContainLoops(NetworkRoute))
            {
                message = $"{NetworkRoute.Name} is not a valid route; it contains loops.";
                log.ErrorFormat(message);
                return false;   
            }

            if (NetworkRoute.Locations.Values.Count < 2)
            {
                message = $"{NetworkRoute.Name} is not a valid route; add extra locations first.";
                log.ErrorFormat(message);
                return false;
            }

            if (!NetworkRoute.Network.IsEditing && RouteHelper.IsDisconnected(NetworkRoute) && !NetworkRoute.Locations.Values.All(l => l.Branch is ISewerConnection && Equals(l.Branch?.Source, l.Branch?.Target)))
            {
                message = $"{NetworkRoute.Name} is not a valid route; is the route disconnected?";
                log.ErrorFormat(message);
                return false;
            }

            routeChecked = true;
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
            if (sender is IBranchFeature branchFeature)
            {
                if (shapeHandler.FeatureToShape.ContainsKey(branchFeature))
                {
                    if(e.PropertyName=="Name")
                    {
                       FullUpdate();
                    }
                    // shapes are up-to-update by definition :) only a redraw is necessary
                    chartView.Invalidate();
                    if (!branchFeature.Network.IsEditing)
                    {
                        UpdateMinMaxLeftAxis();
                    }
                }
            }

            if (sender is ICrossSectionDefinition)
            {
                foreach(var structure in shapeHandler.FeatureToShape.Keys.OfType<ICrossSection>())
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
                foreach (var structure in shapeHandler.FeatureToShape.Keys.OfType<ICrossSection>())
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
            
            if (sender is NetworkSideViewDataController)
            {
                if (e.PropertyName.Equals(nameof(NetworkSideViewDataController.WaterLevelNetworkCoverage)))
                {
                    UnsubscribeTimeNavigator();
                    CreateAndSubscribeTimeNavigator();
                }

                var interestingMembers = new[]
                                      {
                                          nameof(NetworkSideViewDataController.WaterLevelNetworkCoverage),
                                          nameof(NetworkSideViewDataController.ProfileNetworkCoverages),
                                          nameof(NetworkSideViewDataController.RenderedNetworkCoverages),
                                          nameof(NetworkSideViewDataController.RenderedFeatureCoverages)
                                      };

                if (interestingMembers.Contains(e.PropertyName))
                {
                    OnViewDataChanged(true);
                }
            }
        }

        private void ShowCrossSectionsCheckedChanged(object sender, EventArgs e)
        {
            shapeHandler.ShowCrossSections = showCrossSections.Checked;
            FullUpdate();
        }

        private void ShowStructuresCheckedChanged(object sender, EventArgs e)
        {
            shapeHandler.ShowStructures = showStructures.Checked;
            FullUpdate();
        }

        public void SuspendUpdates()
        {
            UnsubscribeFromRoute();
            UnsubscribeFromDataController();
            UnsubscribeTimeNavigator();
            InitializeForRoute(null);
        }

        public void ResumeUpdates()
        {
            InitializeForRoute(route);
            SubscribeToRoute();
            SubscribeToDataController();
            SubscribeTimeNavigator();
            FullUpdate();
        }
    }
}