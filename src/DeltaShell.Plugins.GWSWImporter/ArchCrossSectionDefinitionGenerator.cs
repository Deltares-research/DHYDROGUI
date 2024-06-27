using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class ArchCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public ArchCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var archShape = CreateArchShapeFromGwsw(gwswElement);
            return archShape;
        }

        private ISewerFeature CreateArchShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width) && heightAttribute.TryGetValueAsDouble(logHandler, out height))
            {
                var arcHeight = height / 1000; /*Conversion from millimeters to meters*/
                return new CrossSectionStandardShapeArch
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = arcHeight,
                    ArcHeight = arcHeight,
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultArchShape(shapeName);
        }

        private static ISewerFeature GetDefaultArchShape(string name)
        {
            var defaultArch = CrossSectionStandardShapeArch.CreateDefault();
            defaultArch.Name = name;
            defaultArch.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultArch;
        }

        
    }
}