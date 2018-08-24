using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class SewerNetworkHelper
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerNetworkHelper));

        /// <summary>
        /// Sets the manhole coordinates based on the average of the compartments.
        /// </summary>
        /// <param name="manhole">The manhole.</param>
        /// <param name="gwswElement">The GWSW element.</param>
        public static void SetManholeCoordinateAttributes(IManhole manhole, GwswElement gwswElement)
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

        public static string GetUniqueManholeIdInNetwork(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            if (manholeAttribute.IsValidAttribute())
                return manholeAttribute.GetValidStringValue();

            var compartmentName = GetCompartmentName(gwswElement, network);
            Log.WarnFormat(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_will_be_added_to_the_network_with_a_unique_name, "Manhole", gwswElement.GetElementLine(), compartmentName);
            return manholeAttribute.IsValidAttribute() ? manholeAttribute.GetValidStringValue() : GetManholeUniqueNameForCompartment(network, compartmentName);
        }

        private static string GetCompartmentName(GwswElement gwswElement, IHydroNetwork network)
        {
            var compartmentAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);

            //if we could not place an ID then we use the compartment name to create a place holder (if needed)
            var compartmentName = compartmentAttribute.IsValidAttribute()
                ? compartmentAttribute.GetValidStringValue()
                : CreateUniqueCompartmentName(network);
            return compartmentName;
        }

        public static string CreateUniqueCompartmentName(IHydroNetwork network)
        {
            var compartmentList = new List<Compartment>();
            if (network != null)
                compartmentList = network.Manholes.SelectMany(m => m.Compartments).ToList();

            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartmentList, "Compartment"); ;
        }

        public static string GetManholeUniqueNameForCompartment(IHydroNetwork network, string compartmentName)
        {
            var manholes = new List<IManhole>();
            if (network != null)
                manholes = network.Manholes.ToList();

            return NetworkHelper.GetUniqueName("Manhole{0:D2}ForCompartment" + compartmentName, manholes, "Manhole");
        }
    }
}
