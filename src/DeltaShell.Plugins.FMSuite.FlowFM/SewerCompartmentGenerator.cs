using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerCompartmentGenerator: SewerFeatureFactory, ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerCompartmentGenerator));

        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement == null) return null;
            return CreateCompartment<Compartment>(gwswElement, network);
        }

        public static T FindOrGetNewCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            //We know they are not null, otherwise we would have already returned in the CompartmentFactory method.
            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            var parentManhole = GetParentManholeFromGwswElement(gwswElement, network);

            if (network == null || !parentManhole.ContainsCompartment(compartmentName))
                return new T{Name = compartmentName, ParentManhole = parentManhole};

            var compartmentFound = parentManhole.GetCompartmentByName(compartmentName);
            return (T) (compartmentFound ?? new T { Name = compartmentName, ParentManhole = parentManhole });
        }

        protected static T CreateCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            if (!IsValidGwswCompartment(gwswElement)) return null;

            var compartment = FindOrGetNewCompartment<T>(gwswElement, network);
            SetCompartmentAttributes(compartment, gwswElement, network);
            return compartment;
        }

        private static bool IsValidGwswCompartment(GwswElement gwswElement)
        {
            var manholeName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.ManholeId);
            if (manholeName == null || manholeName.ValueAsString == string.Empty)
            {
                Log.WarnFormat(
                    Resources
                        .SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);
                return false;
            }

            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId);
            if (compartmentName == null || compartmentName.ValueAsString == string.Empty)
            {
                Log.WarnFormat(
                    Resources
                        .SewerFeatureFactory_CreateManHoleCompartment_Manhole_with_manhole_id___0___could_not_be_created__because_one_of_its_compartments_misses_its_unique_id_,
                    manholeName.ValueAsString);
                return false;
            }

            return true;
        }

        private static Manhole GetParentManholeFromGwswElement(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeAttribute = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.ManholeId);
            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            
            //if we could not place an ID then we use the compartment name to create a place holder (if needed)
            var manholeName = manholeAttribute != null ? manholeAttribute.ValueAsString : compartmentName;

            Manhole parentManhole = null;
            if (network != null)
            {
                //Find manhole by its name
                parentManhole = network.Manholes.FirstOrDefault(m => m.Name.Equals(manholeName)) as Manhole;
                //If it was not found, try searching by containing this compartment.
                if (parentManhole == null)
                {
                    parentManhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartment(compartmentName)) as Manhole;
                }
            }

            if (parentManhole == null)
            {
                parentManhole = new Manhole(manholeName);
                if (network != null)
                {
                    Log.WarnFormat("A manhole placeholder {0} has been created for compartment {1}.", manholeName, compartmentName);
                    network.Nodes.Add(parentManhole);
                }
            }
            return parentManhole;
        }

        private static void SetCompartmentAttributes(Compartment compartment, GwswElement gwswElement, IHydroNetwork network)
        {
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            var uniqueId = compartment.Name;
            compartment.ParentManhole = GetParentManholeFromGwswElement(gwswElement, network);
            var manholeId = compartment.ParentManhole.Name;

            // Set the rest of manhole values
            double doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.NodeLength, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.ManholeLength = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.NodeWidth, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.ManholeWidth = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.FloodableArea, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.FloodableArea = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.BottomLevel, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.BottomLevel = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.SurfaceLevel, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.SurfaceLevel = doubleValue;

            double yCoordinate;
            double xCoordinate;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.XCoordinate, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out xCoordinate)
                && TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.YCoordinate, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out yCoordinate))
                compartment.Geometry = new Point(xCoordinate, yCoordinate);

            // Set shape value of the manhole
            string nodeShape;
            if (gwswElementKeyValuePairs.TryGetValue(ManholeMapping.PropertyKeys.NodeShape, out nodeShape))
            {
                try
                {
                    compartment.Shape =
                        (CompartmentShape)EnumDescriptionAttributeTypeConverter.GetEnumValue<CompartmentShape>(
                            nodeShape);
                }
                catch
                {
                    LogManholeWarningMessage(uniqueId, "string", ManholeMapping.PropertyKeys.NodeShape, nodeShape, manholeId);
                }
            }
        }
    }
}