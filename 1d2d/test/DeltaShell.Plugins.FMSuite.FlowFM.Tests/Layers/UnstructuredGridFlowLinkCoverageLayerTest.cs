using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools.Decorations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Layers
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class UnstructuredGridFlowLinkCoverageLayerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowFlowLinkCoverageLayer()
        {
            var r = new Random();

            var mapPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof_map.nc");
            var grid = NetFileImporter.ImportModelGrid(mapPath);

            // build coverage and show on map
            var coverage = new UnstructuredGridFlowLinkCoverage(grid, false);
            coverage.Components.Add(new Variable<double>());
            coverage.Components[0].SetValues(Enumerable.Range(0, grid.FlowLinks.Count).Select(i => r.NextDouble()*20.0));

            var coverageLayer = new UnstructuredGridFlowLinkCoverageLayer { Coverage = coverage };

            var map = new Map { Layers = { coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}