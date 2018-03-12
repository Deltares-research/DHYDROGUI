using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerManholeGenerator: ISewerNetworkFeatureGenerator
    {
        public virtual INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
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

        /// <summary>
        /// Sets the manhole coordinates based on the average of the compartiments.
        /// </summary>
        /// <param name="manhole">The manhole.</param>
        /// <param name="gwswElement">The GWSW element.</param>
        protected static void SetManholeCoordinateAttributes(IManhole manhole, GwswElement gwswElement)
        {
            // Set the rest of manhole values
            double x;
            double y;
            var xCoord = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.XCoordinate);
            var yCoord = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.YCoordinate);
            if (xCoord.TryGetValueAsDouble(out x) && yCoord.TryGetValueAsDouble(out y))
            {
                var weight = manhole.Compartments.Count;
                var xManhole = 0.0;
                var yManhole = 0.0;
                var point = manhole.Geometry as Point;
                if (point != null)
                {
                    xManhole = point.X;
                    yManhole = point.Y;
                }
                var xNew = (weight * xManhole + x) / (weight + 1.0);
                var yNew = (weight * yManhole + y) / (weight + 1.0);
                manhole.Geometry = new Point(xNew, yNew);
            }
        }
    }
}