using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerManholeGenerator: ISewerNetworkFeatureGenerator
    {
        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswManhole()) return null;
            var manhole = GetNewOrExistingManhole(gwswElement, network);
            SetManholeAttributes(manhole, gwswElement);
            return manhole;
        }

        private static IManhole GetNewOrExistingManhole(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId).GetValidStringValue();
            if( network == null) return new Manhole(manholeName);

            //Find manhole by its name or get a new one
            var parentManhole = network.Manholes.FirstOrDefault(m => m.Name.Equals(manholeName));
            return parentManhole ?? new Manhole(manholeName);
        }

        private static void SetManholeAttributes(IManhole manhole, GwswElement gwswElement)
        {
            // Set the rest of manhole values
            double yCoordinate;
            double xCoordinate;
            var xCoord = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.XCoordinate);
            var yCoord = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.YCoordinate);
            if (xCoord.TryGetValueAsDouble(out xCoordinate) && yCoord.TryGetValueAsDouble(out yCoordinate))
            {
                manhole.Geometry = new Point(xCoordinate, yCoordinate);
            }
        }
    }
}