using DelftTools.Hydro.SewerFeatures;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerCompartmentGenerator : ASewerCompartmentGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : CreateCompartment<Compartment>(gwswElement);
        }

        protected override void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment()) return;

            var manholeIdAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            compartment.ParentManholeName = manholeIdAttribute.GetValidStringValue();

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
                compartment.Shape = CompartmentShapeConverter.ConvertStringToCompartmentShape(nodeShapeAttribute.GetValidStringValue());
            }
        }
    }
}