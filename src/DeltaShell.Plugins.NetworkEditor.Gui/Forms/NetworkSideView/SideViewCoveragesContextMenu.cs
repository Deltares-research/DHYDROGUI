using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    internal class SideViewCoveragesContextMenu : IChartViewContextMenuTool
    {
        private bool active;

        public event EventHandler<EventArgs> ActiveChanged;

        public SideViewCoveragesContextMenu(IChartView chartView)
        {
            ChartView = chartView;
        }

        public NetworkSideViewDataController NetworkSideViewDataController { get; set; }
        public NetworkSideView NetworkSideView { get; set; }

        public IChartView ChartView { get; set; }

        public bool Enabled { get; set; }

        public new bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                if (ActiveChanged != null)
                {
                    ActiveChanged(this, null);
                }
            }
        }

        public void OnBeforeContextMenu(ContextMenuStrip menu)
        {
            if (menu.Items.Count > 0)
            {
                menu.Items.Add(new ToolStripSeparator());
            }

            // all not rendered coverages can be added
            var dropDownMenuItem = new ToolStripMenuItem
            {
                Text = "Select Spatial Data",
            };
            dropDownMenuItem.DropDown.Closing += DropDown_Closing;

            IEnumerable<ICoverage> allCoverages =
                NetworkSideViewDataController.AllNetworkCoverages.OfType<ICoverage>().Concat(
                    NetworkSideViewDataController.AllFeatureCoverages.OfType<ICoverage>()).Where(c => c.Time == null || c.Time.Values.Count > 0);

            IEnumerable<IGrouping<string, ICoverage>> modelGroupedCoverages = allCoverages.GroupBy(c => NetworkSideViewDataController.GetModelNameForCoverage(c));

            if (modelGroupedCoverages.Count() == 1)
            {
                AddCoveragesToMenuItem(modelGroupedCoverages.First(), dropDownMenuItem);
            }
            else
            {
                foreach (IGrouping<string, ICoverage> coveragesPerModel in modelGroupedCoverages)
                {
                    var modelItem = new ToolStripMenuItem(coveragesPerModel.Key);
                    AddCoveragesToMenuItem(coveragesPerModel, modelItem);
                    dropDownMenuItem.DropDown.Items.Add(modelItem);
                }
            }

            menu.Items.Add(dropDownMenuItem);
        }

        private void AddCoveragesToMenuItem(IEnumerable<ICoverage> coveragesPerModel, ToolStripMenuItem dropDownMenuItem)
        {
            foreach (ICoverage coverage in coveragesPerModel)
            {
                var selectCoverageMenuItem = new ToolStripMenuItem
                {
                    Text = coverage.Name,
                    Tag = coverage,
                    Checked = IsShown(coverage)
                };
                if (coverage is INetworkCoverage)
                {
                    selectCoverageMenuItem.Click += SelectNetworkCoverageMenuItemClick;
                }
                else
                {
                    selectCoverageMenuItem.Click += SelectFeatureCoverageMenuItemClick;
                }

                selectCoverageMenuItem.CheckOnClick = true;
                dropDownMenuItem.DropDown.Items.Add(selectCoverageMenuItem);
            }
        }

        private bool IsShown(ICoverage coverage)
        {
            if (coverage is IFeatureCoverage)
            {
                return NetworkSideViewDataController.RenderedFeatureCoverages.Contains(coverage as IFeatureCoverage);
            }

            return NetworkSideViewDataController.RenderedNetworkCoverages.Contains(coverage as INetworkCoverage);
        }

        private void SelectNetworkCoverageMenuItemClick(object sender, EventArgs e)
        {
            //get the coverage and add it to the rendered coverages
            var coverage = (NetworkCoverage) ((ToolStripMenuItem) sender).Tag;
            if (NetworkSideViewDataController.RenderedNetworkCoverages.Contains(coverage))
            {
                NetworkSideViewDataController.RemoveRenderedCoverage(coverage);
            }
            else
            {
                NetworkSideViewDataController.AddRenderedCoverage(coverage);
                NetworkSideView.UpdateFilter(coverage);
                NetworkSideView.OnViewDataChanged(false);
            }
        }

        private void SelectFeatureCoverageMenuItemClick(object sender, EventArgs e)
        {
            //get the coverage and add it to the rendered coverages
            var coverage = (IFeatureCoverage) ((ToolStripMenuItem) sender).Tag;
            if (NetworkSideViewDataController.RenderedFeatureCoverages.Contains(coverage))
            {
                NetworkSideViewDataController.RemoveRenderedCoverage(coverage);
            }
            else
            {
                NetworkSideViewDataController.AddRenderedCoverage(coverage);
                NetworkSideView.UpdateFilter(coverage);
                NetworkSideView.OnViewDataChanged(false);
            }
        }

        private static void DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            //prevent popup to close when item is clicked
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
                ((ToolStripDropDownMenu) sender).Invalidate();
            }
        }
    }
}