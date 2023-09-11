using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class EllipticalCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public EllipticalCrossSectionShapeGenerator(ILogHandler logHandler):base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var ellipticalShape = CreateEllipticalShapeFromGwsw(gwswElement);
            return ellipticalShape;
        }

        private ISewerFeature CreateEllipticalShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width) && heightAttribute.TryGetValueAsDouble(logHandler, out height))
            {
                return new CrossSectionStandardShapeElliptical
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = height / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultEllipticalShape(shapeName);
        }

        private static ISewerFeature GetDefaultEllipticalShape(string name)
        {
            var defaultElliptical = CrossSectionStandardShapeElliptical.CreateDefault();
            defaultElliptical.Name = name;
            defaultElliptical.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultElliptical;
        }
    }
    public class RectangleCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public RectangleCrossSectionShapeGenerator(ILogHandler logHandler) : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var rectangleShape = CreateRectangleShapeFromGwsw(gwswElement);
            return rectangleShape;
        }

        private ISewerFeature CreateRectangleShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width) && heightAttribute.TryGetValueAsDouble(logHandler, out height))
            {
                return new CrossSectionStandardShapeRectangle
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = height / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultRectangleShape(shapeName);
        }

        private static ISewerFeature GetDefaultRectangleShape(string name)
        {
            var defaultRectangle = CrossSectionStandardShapeRectangle.CreateDefault();
            defaultRectangle.Name = name;
            defaultRectangle.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultRectangle;
        }
    }
}