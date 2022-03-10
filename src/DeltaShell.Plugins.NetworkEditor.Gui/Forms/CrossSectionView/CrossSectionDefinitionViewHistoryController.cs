using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Controls.Swf.Charting.Tools;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public class CrossSectionDefinitionViewHistoryController
    {
        private IList<ICrossSectionDefinition> crossSectionHistory = new List<ICrossSectionDefinition>();
        private readonly IHistoryTool historyTool;

        public CrossSectionDefinitionViewHistoryController(IHistoryTool historyTool)
        {
            this.historyTool = historyTool;
            historyTool.ShowToolTip = true;

            if (historyTool.ChartView == null)
            {
                throw new ArgumentException("HistoryTool not attached to ChartView");
            }
        }

        public void RefreshHistoryInChart(double currentThalweg)
        {
            historyTool.ClearHistory();

            if (!historyTool.Active)
            {
                return;
            }

            foreach (var oldCrossSection in crossSectionHistory)
            {
                //Align by thalweg
                var deltaThalweg = currentThalweg - oldCrossSection.Thalweg;
                var shiftedProfile = oldCrossSection.GetProfile().Select(c => new Coordinate(c.X + deltaThalweg, c.Y)).ToList();
                var series = CreateHistoryLineSeries(oldCrossSection.Name, shiftedProfile);
                series.ShowInLegend = false;
                historyTool.Add(series);
            }
        }

        public void AddCrossSectionToHistory(ICrossSectionDefinition crossSectionDefinition)
        {
            if (!crossSectionHistory.Contains(crossSectionDefinition))
            {
                crossSectionHistory.Add(crossSectionDefinition);
            }
        }

        public void RemoveCrossSectionFromHistory(ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionHistory.Contains(crossSectionDefinition))
            {
                crossSectionHistory.Remove(crossSectionDefinition);
            }
        }

        public void ClearHistory()
        {
            historyTool.ClearHistory();
            crossSectionHistory.Clear();
        }

        private static IChartSeries CreateHistoryLineSeries(string title, List<Coordinate> profile)
        {
            var profileSeries = (LineChartSeries)ChartSeriesFactory.CreateLineSeries();
            profileSeries.Title = title;
            profileSeries.XValuesDataMember = "X";
            profileSeries.YValuesDataMember = "Y";
            profileSeries.DataSource = profile;
            return profileSeries;
        }
    }
}
