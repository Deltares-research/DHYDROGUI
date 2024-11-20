using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Coverages;
using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    public class Base1D2DLinksMapTool : MapTool
    {

        protected NetworkCoverageLocationLayer discretizationLayer;
        private UnstructuredGridLayer gridLayer;

        public Base1D2DLinksMapTool()
        {
            LinkType = LinkGeneratingType.EmbeddedOneToOne;
        }

        public LinkGeneratingType LinkType { get; set; }

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

