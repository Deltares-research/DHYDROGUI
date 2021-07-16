using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Controls.Swf.Charting.Tools;
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
        private readonly IList<SideViewChartData> bottomProfileChartData = new List<SideViewChartData>();
        private readonly IList<SideViewChartData> renderedCoveragesChartData = new List<SideViewChartData>();
        private Dictionary<string, bool> seriesActiveCache = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

        private ISeriesBandTool pipeSeriesBandTool;
        private ISeriesBandTool waterLevelPipesSeriesBandTool;
        private ISeriesBandTool waterLevelChannelsSeriesBandTool;
        
        private NetworkSideViewDataController networkSideViewDataController;

        public NetworkSideViewChartSeriesController(ChartView chartView)
        {
            this.chartView = chartView;
        }

        public NetworkSideViewDataController NetworkSideViewDataController
        {
            get { return networkSideViewDataController;}
            set { networkSideViewDataController = value; }
        }

        private IChart Chart
        {
            get { return chartView?.Chart; }
        }

        public Route Route { get; set; }

        internal void CreateChartSeries()
        {
            seriesActiveCache.Clear();
            seriesActiveCache = Chart.Series.ToDictionaryWithDuplicateLogging("Series",s => s.Title, s => s.Visible, Level.Debug);
            Chart.Series.Clear();

            //clear the old binding lists
            bottomProfileChartData.ForEach(cd => cd.Dispose());
            bottomProfileChartData.Clear();
            renderedCoveragesChartData.ForEach(cd => cd.Dispose());
            renderedCoveragesChartData.Clear();

            // create chart data
            var bedLevelChartData = CreateBedLevelChartData().ToArray();
            bedLevelChartData.ForEach(d =>
            {
                Chart.Series.Add(CreateSeries(d));
                bottomProfileChartData.Add(d);
            });

            chartView.Tools.Remove(waterLevelChannelsSeriesBandTool);

            var waterLevelChartData = CreateWaterLevelChartData();
            if (waterLevelChartData != null)
            {
                var waterLevelChartSeries = CreateSeries(waterLevelChartData);
                Chart.Series.Add(waterLevelChartSeries);
                bottomProfileChartData.Add(waterLevelChartData);

                var bedLevelSeries = Chart.Series.FirstOrDefault(s => s.Title.StartsWith(BedLevelNetworkCoverageBuilder.BedLevelCoverageName));
                if (bedLevelSeries != null)
                {
                    waterLevelChannelsSeriesBandTool = chartView.NewSeriesBandTool(waterLevelChartSeries, bedLevelSeries,
                                                                                   Color.FromArgb(72, Color.RoyalBlue));

                    chartView.Tools.Add(waterLevelChannelsSeriesBandTool);
                }
            }

            if (Route.Segments.Values.All(s => s.Branch is ISewerConnection))
            {
                var pipeSeries = CreatePipeChartData().Select(CreateSeries).ToList();
                Chart.Series.AddRange(pipeSeries);

                if (pipeSeries.Count > 1)
                {
                    chartView.Tools.Remove(pipeSeriesBandTool);
                    pipeSeriesBandTool = chartView.NewSeriesBandTool(pipeSeries[0], pipeSeries[1], NetworkSideViewStyles.PipeColor);
                    chartView.Tools.Add(pipeSeriesBandTool);
                }

                chartView.Tools.Remove(waterLevelPipesSeriesBandTool);

                var waterLevelInPipeChartData = CreateWaterLevelInPipeChartData();
                if (waterLevelInPipeChartData != null)
                {
                    var waterLevelInPipeSeries = CreateSeries(waterLevelInPipeChartData);
                    Chart.Series.Add(waterLevelInPipeSeries);
                    waterLevelPipesSeriesBandTool = chartView.NewSeriesBandTool(waterLevelInPipeSeries, pipeSeries[1],
                                                                                Color.FromArgb(72, Color.RoyalBlue));
                    chartView.Tools.Add(waterLevelPipesSeriesBandTool);
                }
            }

            CreateRenderedCoverageChartData().ForEach(d =>
            {
                Chart.Series.Add(CreateSeries(d));
                renderedCoveragesChartData.Add(d);
            });

            Chart.Legend.ShowCheckBoxes = true;

            Chart.Series.ForEach(s => s.Visible = !seriesActiveCache.ContainsKey(s.Title) || seriesActiveCache[s.Title]);
        }
        
        private SideViewChartData CreateWaterLevelInPipeChartData()
        {
            var waterLevelInSideView = networkSideViewDataController.WaterLevelSideViewFunction;

            if (waterLevelInSideView == null) return null;

            var waterLevelInPipeFunction = NetworkSideViewHelper.GetWaterLevelInPipeFunction(Route, waterLevelInSideView);
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
                    FunctionBindingList = { SynchronizeInvoke = chartView },
                    LineStyleCustomizer = (ls) =>
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
                renderedFeatureCoverageChartData.PointStyleCustomizer = (pcs) => { };
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

            var bottomLevelSideViewFunction = profileSideViewFunctions.FirstOrDefault(psvf => string.Equals(psvf.Name, BedLevelNetworkCoverageBuilder.BedLevelCoverageName));
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

        private IChartSeries CreateSeries(SideViewChartData sideViewChartData)
        {
            var function = sideViewChartData.Function;
            NetworkSideViewHelper.ValidateFunction(function);
            var xArgument = function.GetFirstArgumentVariableOfType<double>();
            var yComponent = function.GetFirstComponentVariableOfType<double>();

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
            foreach (var chartData in bottomProfileChartData)
            {
                chartData.Dispose();
            }
            foreach (var chartData in renderedCoveragesChartData)
            {
                chartData.Dispose();
            }

            Route = null;
            NetworkSideViewDataController = null;
        }
    }
}