using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class CircleCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {

        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var roundShape = CreateRoundShapeFromGwsw(gwswElement);
            return roundShape;
        }

        private CrossSectionStandardShapeCircle CreateRoundShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            var materialValue = gwswElement.GetAttributeValueFromList<string>(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width))
            {
                var multiplier = CalculateDiameterMultiplier(materialValue);
                return new CrossSectionStandardShapeCircle
                {
                    Name = shapeName,
                    Diameter = multiplier * width / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = materialValue
                };
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultCircleShape(shapeName);
        }

        private static double CalculateDiameterMultiplier(string materialValue)
        {
            var pvcDescription = SewerProfileMapping.SewerProfileMaterial.Polyvinylchlorid.GetDescription();
            return materialValue == pvcDescription
                ? 16.0 / 17.0 
                : 1.0;
        }

        private static CrossSectionStandardShapeCircle GetDefaultCircleShape(string name)
        {
            var defaultCircle = CrossSectionStandardShapeCircle.CreateDefault();
            defaultCircle.Name = name;
            defaultCircle.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultCircle;
        }

        public CircleCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
    }
}