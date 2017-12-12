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
        public virtual INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswManhole()) return null;
            var manhole = GetNewOrExistingManhole(gwswElement, network);
            SetManholeCoordinateAttributes(manhole, gwswElement);
            return manhole;
        }

        protected virtual string GetManholeName(GwswElement gwswElement, IHydroNetwork network)
        {
            return gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId).GetValidStringValue();
        }

        protected virtual IManhole FindManhole(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeName = GetManholeName(gwswElement, network);
            return network?.Manholes.FirstOrDefault(m => m.Name.Equals(manholeName));
        }

        protected virtual IManhole GetNewOrExistingManhole(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeName = GetManholeName(gwswElement, network);
            return FindManhole(gwswElement, network) ?? new Manhole(manholeName);
        }

        protected static void SetManholeCoordinateAttributes(IManhole manhole, GwswElement gwswElement)
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