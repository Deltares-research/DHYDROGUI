using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class UShapeCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public UShapeCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var UShapeShape = CreateUShapeShapeFromGwsw(gwswElement);
            return UShapeShape;
        }

        private ISewerFeature CreateUShapeShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width) && heightAttribute.TryGetValueAsDouble(logHandler, out height))
            {
                var arcHeight = height / 1000; /*Conversion from millimeters to meters*/
                return new CrossSectionStandardShapeUShape
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = arcHeight,
                    ArcHeight = arcHeight,
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultUShapeShape(shapeName);
        }

        private static ISewerFeature GetDefaultUShapeShape(string name)
        {
            var defaultUShape = CrossSectionStandardShapeUShape.CreateDefault();
            defaultUShape.Name = name;
            defaultUShape.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultUShape;
        }
    }
}