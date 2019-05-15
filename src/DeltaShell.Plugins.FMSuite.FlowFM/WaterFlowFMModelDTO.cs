using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM {
    public class WaterFlowFMModelDTO
    {
        public ICoordinateSystem CoordinateSystem { get; set; }

        public IEnumerable<Feature2D> ObservationPoints { get; set; }

        public IEnumerable<Feature2D> ObservationCrossSections { get; set; }

        public IEnumerable<Weir2D> SimpleWeirs { get; set; }

        public IEnumerable<Weir2D> GeneralStructureWeirs { get; set; }

        public IEnumerable<Weir2D> GatedWeirs { get; set; }

        public IEnumerable<Pump2D> Pumps { get; set; }
    }
}