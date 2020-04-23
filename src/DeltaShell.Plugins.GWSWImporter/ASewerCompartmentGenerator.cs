using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public abstract class ASewerCompartmentGenerator : IGwswFeatureGenerator<ISewerFeature>
    {
        private static ILog Log = LogManager.GetLogger(typeof(ASewerCompartmentGenerator));

        public abstract ISewerFeature Generate(GwswElement gwswElement);

        protected ISewerFeature CreateCompartment<T>(GwswElement gwswElement) 
            where T : Compartment, new()
        {
            if (gwswElement == null) return null;

            var compartmentToBeAdded = CreateNewCompartment<T>(gwswElement);
            SetCompartmentProperties(compartmentToBeAdded, gwswElement);

            return compartmentToBeAdded;
        }

        private T CreateNewCompartment<T>(GwswElement gwswElement) where T : Compartment, new()
        {
            var compartmentIdAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);
            var compartmentName = compartmentIdAttribute.GetValidStringValue();
            if (compartmentName == null)
            {
                Log.WarnFormat(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_will_be_added_to_the_network_with_a_unique_name, "Compartment", gwswElement.GetElementLine());
            }

            return new T { Name = compartmentName };
        }

        protected static void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment()) return;

            var manholeIdAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            compartment.ParentManholeName =  manholeIdAttribute.GetValidStringValue();

            double xCoordinate;
            double yCoordinate;
            var xCoordinateAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.XCoordinate);
            var yCoordinateAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.YCoordinate);
            if (xCoordinateAttribute.TryGetValueAsDouble(out xCoordinate) && yCoordinateAttribute.TryGetValueAsDouble(out yCoordinate))
                compartment.Geometry = new Point(xCoordinate, yCoordinate);

            // Set the rest of compartment values
            double auxDouble;
            var nodeLengthAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeLength);
            if (nodeLengthAttribute.TryGetValueAsDouble(out auxDouble))
                compartment.ManholeLength = auxDouble / 1000.0; // Conversion from mm to m
            else
                compartment.ManholeLength = 0.8d;

            var nodeWidthAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeWidth);
            if (nodeWidthAttribute.TryGetValueAsDouble(out auxDouble))
                compartment.ManholeWidth = auxDouble / 1000.0; // Conversion from mm to m
            else
                compartment.ManholeWidth = 0.8d;

            var floodableAreaAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.FloodableArea);
            if (floodableAreaAttribute.TryGetValueAsDouble(out auxDouble))
                compartment.FloodableArea = auxDouble;
            else
                compartment.FloodableArea = 100;

            var bottomLevelAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.BottomLevel);
            if (bottomLevelAttribute.TryGetValueAsDouble(out auxDouble))
                compartment.BottomLevel = auxDouble;

            var surfaceLevelAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.SurfaceLevel);
            if (surfaceLevelAttribute.TryGetValueAsDouble(out auxDouble))
                compartment.SurfaceLevel = auxDouble;

            // Set shape value of the compartment
            var nodeShapeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeShape);
            if (nodeShapeAttribute.IsValidAttribute())
            {
                compartment.Shape = nodeShapeAttribute.GetValueFromDescription<CompartmentShape>();
            }

            var outletCompartment = compartment as OutletCompartment;
            if (outletCompartment != null)
            {
                var surfaceWaterLevelAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel);
                if (surfaceWaterLevelAttribute.TryGetValueAsDouble(out auxDouble))
                    outletCompartment.SurfaceWaterLevel = auxDouble;
            }
        }
    }
}