using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    public class Base1D2DLinksMapTool : MapTool
    {

        protected NetworkCoverageLocationLayer discretizationLayer;
        private UnstructuredGridLayer gridLayer;

        public Base1D2DLinksMapTool()
        {
            LinkType = GridApiDataSet.LinkType.Embedded;
        }

        public GridApiDataSet.LinkType LinkType { get; set; }

        public override bool Enabled
        {
            get
            {
                // Check discretization exists and has data. 
                discretizationLayer = Map.GetAllLayers(false).OfType<NetworkCoverageLocationLayer>()
                    .FirstOrDefault(l => l.Coverage is IDiscretization);
                var discretization = discretizationLayer?.Coverage as IDiscretization;
                if (discretization == null)
                {
                    return false;
                }

                // Check grid exists and has data. 
                gridLayer = Map.GetAllLayers(false).OfType<UnstructuredGridLayer>()
                    .FirstOrDefault();
                var grid = gridLayer?.Grid;
                if (grid == null)
                {
                    return false;
                }

                return (discretization.Locations.GetValues().Any() && grid.Cells.Any());
            }
        }
    }
}

