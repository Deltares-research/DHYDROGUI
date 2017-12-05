using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    interface SewerProfileDefinitionReader
    {
        ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement);
    }

    class CsdEggDefinitionReader : SewerProfileDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdEggDefinitionReader));
        public ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;

            double width;
            CrossSectionStandardShapeEgg csEggShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csEggShape = new CrossSectionStandardShapeEgg { Width = width / 1000 /*Conversion from millimeters to meters*/};
                Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 1.5, "(2:3)", csEggShape);
            }
            else
            {
                csEggShape = CrossSectionStandardShapeEgg.CreateDefault();
                Log.MessageForMissingValues(gwswElement, "width");
            }
            
            return new CrossSectionDefinitionStandard(csEggShape);
        }
    }

    class CsdArchDefinitionReader : SewerProfileDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdArchDefinitionReader));
        public ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;

            double height;
            double width;
            CrossSectionStandardShapeArch csArchShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                var arcHeight = height / 1000; /*Conversion from millimeters to meters*/
                csArchShape = new CrossSectionStandardShapeArch
                {
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = arcHeight,
                    ArcHeight = arcHeight
                };
            }
            else
            {
                csArchShape = CrossSectionStandardShapeArch.CreateDefault();
                Log.MessageForMissingValues(gwswElement, "width and/or height");
            }
            return new CrossSectionDefinitionStandard(csArchShape);
        }
    }

    class CsdCunetteDefinitionReader : SewerProfileDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdCunetteDefinitionReader));
        public ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;

            double width;
            CrossSectionStandardShapeCunette csCunetteShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csCunetteShape = new CrossSectionStandardShapeCunette { Width = width / 1000 /*Conversion from millimeters to meters*/};
                Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 0.634, "(1:0.634)", csCunetteShape);
            }
            else
            {
                csCunetteShape = CrossSectionStandardShapeCunette.CreateDefault();
                Log.MessageForMissingValues(gwswElement, "width");
            }
            
            return new CrossSectionDefinitionStandard(csCunetteShape);
        }
    }

    class CsdRectangleDefinitionReader : SewerProfileDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdRectangleDefinitionReader));
        public ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            
            double height;
            double width;
            CrossSectionStandardShapeRectangle csRectangleShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                csRectangleShape = new CrossSectionStandardShapeRectangle
                {
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = height / 1000 /*Conversion from millimeters to meters*/
                };
            }
            else
            {
                csRectangleShape = CrossSectionStandardShapeRectangle.CreateDefault();
                Log.MessageForMissingValues(gwswElement, "width and/or height");
            }
            return new CrossSectionDefinitionStandard(csRectangleShape);
        }
    }

    class CsdCircleDefinitionReader : SewerProfileDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdCircleDefinitionReader));
        public ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            
            double width;
            CrossSectionStandardShapeRound csRoundShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csRoundShape = new CrossSectionStandardShapeRound {Diameter = width / 1000 /*Conversion from millimeters to meters*/};
            }
            else
            {
                csRoundShape = CrossSectionStandardShapeRound.CreateDefault();
                Log.MessageForMissingValues(gwswElement, "width");
            }
            return new CrossSectionDefinitionStandard(csRoundShape);
        }
    }

    class CsdTrapezoidDefinitionReader : SewerProfileDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdCircleDefinitionReader));
        public ICrossSectionDefinition ReadSewerProfileDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            double height;
            double slope1;
            double slope2;
            double slope;
            CrossSectionStandardShapeTrapezium csTrapezoidShape;

            var slope1Attribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.Slope1);
            var slope2Attribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.Slope2);
            var slope1PresentAndWellFormatted = slope1Attribute.TryGetValueAsDouble(out slope1);
            var slope2PresentAndWellFormatted = slope2Attribute.TryGetValueAsDouble(out slope2);
            if (slope1PresentAndWellFormatted && !slope2PresentAndWellFormatted)
            {
                slope = slope1;
            }
            else if (!slope1PresentAndWellFormatted && slope2PresentAndWellFormatted)
            {
                slope = slope2;
            }
            else if (slope1PresentAndWellFormatted && slope2PresentAndWellFormatted)
            {
                slope = (slope1 + slope2) / 2;
            }
            else
            {
                Log.MessageForMissingValues(gwswElement, "width, height and/or slope");
                return new CrossSectionDefinitionStandard(CrossSectionStandardShapeTrapezium.CreateDefault());
            }

            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                var trapezoidWidth = width / 1000;
                csTrapezoidShape = new CrossSectionStandardShapeTrapezium
                {
                    BottomWidthB = trapezoidWidth,
                    Slope = slope,
                    MaximumFlowWidth = (width + 2 * height / slope) / 1000
                };
            }
            else
            {
                csTrapezoidShape = CrossSectionStandardShapeTrapezium.CreateDefault();
                Log.MessageForMissingValues(gwswElement, "width, height and/or slope");
            }
            return new CrossSectionDefinitionStandard(csTrapezoidShape);
        }
    }

    static class SewerDictionaryExtensions
    {
        public static void LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion<T>(this ILog logger, GwswElement gwswElement,
            double width, double heightProportion, string proportionString, T csShape)
            where T : CrossSectionStandardShapeWidthHeightBase
        {
            double height;
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (heightAttribute.TryGetValueAsDouble(out height) && Math.Abs(heightProportion * width - height) < 0.0001) return;

            var csHeight = csShape.Height * 1000;
            var idAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);

            var id = idAttribute?.ValueAsString;
            logger.WarnFormat(
                "The width and height of sewer profile '{0}' are not in the right proportion {1}. Width is now {2} mm and height is now {3} mm.",
                id, proportionString, width, csHeight);
        }

        public static void MessageForMissingValues(this ILog logger, GwswElement gwswElement, string missingValuesText)
        {
            var id = gwswElement?.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId)?.ValueAsString;
            logger.WarnFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", id, missingValuesText);
        }
    }
}
