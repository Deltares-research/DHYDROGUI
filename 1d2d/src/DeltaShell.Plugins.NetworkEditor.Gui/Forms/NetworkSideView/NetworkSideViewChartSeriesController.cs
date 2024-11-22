using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Drawing;
using DeltaShell.NGHS.Utils;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    internal class NetworkSideViewChartSeriesController : IDisposable
    {
        private readonly ChartView chartView;
        private readonly IList<SideViewChartData> createdChartData = new List<SideViewChartData>();
        private readonly IList<IChartViewTool> addedTools = new List<IChartViewTool>();
        private Dictionary<string, bool> seriesActiveCache = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

        public NetworkSideViewChartSeriesController(ChartView chartView)
        {
            this.chartView = chartView;
        }

        public NetworkSideViewDataController NetworkSideViewDataController { get; set; }

        private IChart Chart
        {
            get { return chartView?.Chart; }
        }

        public Route Route { get; set; }

        internal void CreateChartSeries()
        {
            if (Route == null)
            {
                return;
            }
            
            seriesActiveCache.Clear();
            seriesActiveCache = Chart.Series.ToDictionaryWithDuplicateLogging("Series",s => s.Title, s => s.Visible, Level.Debug);
            
            CleanSeriesAndTools();

            // create chart data
            SideViewChartData[] bedLevelChartDatas = CreateBedLevelChartData().ToArray();
            SideViewChartData waterLevelChartData = CreateWaterLevelChartData();
            IEnumerable<SideViewChartData> renderedCoveragesChartDatas = CreateRenderedCoverageChartData();

            bool allSewerConnections = Route.Segments.Values.All(s => s.Branch is ISewerConnection);
            SideViewChartData[] pipeChartData = allSewerConnections
                                                    ? CreatePipeChartData().ToArray()
                                                    : Array.Empty<SideViewChartData>();

            SideViewChartData waterLevelInPipeChartData = allSewerConnections
                                                              ? CreateWaterLevelInPipeChartData()
                                                              : null;
            SideViewChartData maxWaterLevelChartData = CreateMaxWaterLevelChartData();

            var lookup = new Dictionary<SideViewChartData, IChartSeries>();
            IEnumerable<SideViewChartData> all = bedLevelChartDatas
                                                 .Plus(waterLevelChartData)
                                                 .Plus(maxWaterLevelChartData)
                                                 .Concat(pipeChartData)
                                                 .Plus(waterLevelInPipeChartData)
                                                 .Concat(renderedCoveragesChartDatas);

            foreach (SideViewChartData chartData in all)
            {
                if (chartData == null) continue;

                IChartSeries chartSeries = CreateSeries(chartData);
                
                Chart.Series.Add(chartSeries);
                createdChartData.Add(chartData);
                lookup.Add(chartData, chartSeries);
            }

            CreateTools(lookup, pipeChartData, waterLevelInPipeChartData);

            chartView.Tools.AddRange(addedTools);

            Chart.Legend.ShowCheckBoxes = true;

            // restore visibility of serie
            if (seriesActiveCache.Any())
            {
                Chart.Series.ForEach(s => s.Visible = !seriesActiveCache.ContainsKey(s.Title) || seriesActiveCache[s.Title]);
            }
        }

        private void CreateTools(Dictionary<SideViewChartData, IChartSeries> lookup, SideViewChartData[] pipeChartData, SideViewChartData waterLevelInPipeChartData)
        {
            if (pipeChartData.Length <= 1) 
            {
                return;
            }

            var pipeSeries = pipeChartData.Select(cd => lookup[cd]).ToArray();
            addedTools.Add(chartView.NewSeriesBandTool(pipeSeries[0], pipeSeries[1], NetworkSideViewStyles.PipeColor));

            if (waterLevelInPipeChartData != null && lookup.TryGetValue(waterLevelInPipeChartData, out IChartSeries waterLevelInPipeSeries))
            {
                addedTools.Add(chartView.NewSeriesBandTool(waterLevelInPipeSeries, pipeSeries[1], Color.FromArgb(72, NetworkSideViewStyles.WaterLevelColor)));
            }
        }

        private void CleanSeriesAndTools()
        {
            Chart.Series.Clear();

            //clear the old binding lists
            createdChartData.ForEach(cd => cd.Dispose());
            createdChartData.Clear();

            addedTools.ForEach(t => chartView.Tools.Remove(t));
            addedTools.Clear();
        }

        private SideViewChartData CreateWaterLevelInPipeChartData()
        {
            IFunction waterLevelInSideView = NetworkSideViewDataController.CreateWaterLevelSideViewFunction();

            if (waterLevelInSideView == null) return null;
            IFunction waterLevelInPipeFunction = NetworkSideViewHelper.GetWaterLevelInPipeFunction(Route, waterLevelInSideView);
            
            return CreateLineChartDataForFunction(waterLevelInPipeFunction, NetworkSideViewStyles.WaterLevelColor, (ls) =>
            {
                ls.DashStyle = DashStyle.Dot;
            });
        }

        private IEnumerable<SideViewChartData> CreatePipeChartData()
        {
            IEnumerable<IFunction> functions = NetworkSideViewDataController?.PipeSideViewFunctions;
            if (functions == null)
                yield break;

            foreach (IFunction function in functions)
            {
                yield return CreateLineChartDataForFunction(function, Color.Black, (ls) =>
                {
                    ls.DashStyle = DashStyle.Solid;
                    ls.Width = 2;
                });
            }
        }

        private IEnumerable<SideViewChartData> CreateRenderedCoverageChartData()
        {
            foreach (IFunction sideViewFunction in NetworkSideViewDataController.RenderedNetworkSideViewFunctions)
            {
                yield return CreateLineChartDataForFunction(sideViewFunction, ColorHelper.GetIndexedColor(chartView.Chart.Series.Count), lcs =>
                {
                    lcs.DashStyle = DashStyle.Solid;
                    lcs.Width = 2;
                });
            }
            
            foreach (IFunction coverage in NetworkSideViewDataController.RenderedFeatureViewFunctions)
            {
                yield return CreatePointChartDataForFunction(coverage, ColorHelper.GetIndexedColor(chartView.Chart.Series.Count));
            }
        }

        private SideViewChartData CreateWaterLevelChartData()
        {
            IFunction waterLevelSideViewFunction = NetworkSideViewDataController.CreateWaterLevelSideViewFunction();
            
            return waterLevelSideViewFunction != null
                       ? CreateLineChartDataForFunction(waterLevelSideViewFunction, NetworkSideViewStyles.WaterLevelColor, (lcs) =>
                       {
                           lcs.Color = NetworkSideViewStyles.WaterLevelColor;
                           lcs.DashStyle = DashStyle.Solid;
                           lcs.Width = 2;
                       })
                       : null;
        }

        private SideViewChartData CreateMaxWaterLevelChartData()
        {
            return NetworkSideViewDataController.MaxWaterLevelFunction != null
                       ? CreateLineChartDataForFunction(NetworkSideViewDataController.MaxWaterLevelFunction, NetworkSideViewStyles.MaxWaterLevelColor, (lcs) =>
                       {
                           lcs.Visible = false;
                           lcs.Color = NetworkSideViewStyles.MaxWaterLevelColor;
                           lcs.DashStyle = DashStyle.Solid;
                           lcs.Width = 2;
                       })
                       : null;
        }

        private IEnumerable<SideViewChartData> CreateBedLevelChartData()
        {
            Dictionary<string, IFunction> profileFunctions = 
                NetworkSideViewDataController.ProfileSideViewFunctions
                                             .ToDictionary(f => f.Name, StringComparer.CurrentCultureIgnoreCase);

            if (profileFunctions.TryGetValue(BedLevelNetworkCoverageBuilder.BedLevelCoverageName, out IFunction bottomLevelSideViewFunction))
            {
                yield return CreateAreaChartDataForFunction(bottomLevelSideViewFunction, Color.FromArgb(72, Color.YellowGreen), (acs) => acs.LineVisible = false);
            }

            if (profileFunctions.TryGetValue(BedLevelNetworkCoverageBuilder.LowestEmbankmentCoverageName, out IFunction lowestEmbankmentSideViewFunction))
            {
                yield return CreateLineChartDataForFunction(lowestEmbankmentSideViewFunction, Color.SaddleBrown, (lcs) => { lcs.DashStyle = DashStyle.Dot; lcs.Width = 3; });
            }

            if (profileFunctions.TryGetValue(BedLevelNetworkCoverageBuilder.LeftEmbankmentCoverageName, out IFunction leftEmbankmentSideViewFunction))
            {
                yield return CreateLineChartDataForFunction(leftEmbankmentSideViewFunction, Color.Goldenrod, (lcs) => { lcs.DashStyle = DashStyle.Dot; lcs.Width = 3; });
            }

            if (profileFunctions.TryGetValue(BedLevelNetworkCoverageBuilder.RightEmbankmentCoverageName, out IFunction rightEmbankmentSideViewFunction))
            {
                yield return CreateLineChartDataForFunction(rightEmbankmentSideViewFunction, Color.RosyBrown, (lcs) => { lcs.DashStyle = DashStyle.Dot; lcs.Width = 3; });
            }
        }

        private SideViewChartData CreateLineChartDataForFunction(IFunction sideViewFunction, Color color, Action<ILineChartSeries> lineStyleCustomizer = null)
        {
            var renderedNetworkCoverageChartData = new SideViewChartData(sideViewFunction, color, ChartSeriesType.LineSeries);
            renderedNetworkCoverageChartData.FunctionBindingList.SynchronizeInvoke = chartView;
            renderedNetworkCoverageChartData.LineStyleCustomizer = lineStyleCustomizer;
            return renderedNetworkCoverageChartData;
        }

        private SideViewChartData CreatePointChartDataForFunction(IFunction sideViewFunction, Color color, Action<IPointChartSeries> pointStyleCustomizer = null)
        {
            var renderedNetworkCoverageChartData = new SideViewChartData(sideViewFunction, color, ChartSeriesType.LineSeries);
            renderedNetworkCoverageChartData.FunctionBindingList.SynchronizeInvoke = chartView;
            renderedNetworkCoverageChartData.PointStyleCustomizer = pointStyleCustomizer;
            return renderedNetworkCoverageChartData;
        }

        private SideViewChartData CreateAreaChartDataForFunction(IFunction sideViewFunction, Color color, Action<IAreaChartSeries> areaStyleCustomizer=null)
        {
            var renderedNetworkCoverageChartData = new SideViewChartData(sideViewFunction, color, ChartSeriesType.AreaSeries);
            renderedNetworkCoverageChartData.FunctionBindingList.SynchronizeInvoke = chartView;
            renderedNetworkCoverageChartData.AreaStyleCustomizer = areaStyleCustomizer;
            return renderedNetworkCoverageChartData;
        }

        private static IChartSeries CreateSeries(SideViewChartData sideViewChartData)
        {
            IFunction function = sideViewChartData.Function;
            NetworkSideViewHelper.ValidateFunction(function);
            IVariable xArgument = function.GetFirstArgumentVariableOfType<double>();
            IVariable yComponent = function.GetFirstComponentVariableOfType<double>();

            IChartSeries chartSeries = null;
            switch (sideViewChartData.Style)
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
        
        public void Dispose()
        {
            // dispose function binding lists:
            foreach (SideViewChartData chartData in createdChartData)
            {
                chartData.Dispose();
            }

            Route = null;
            NetworkSideViewDataController = null;
        }
    }
}