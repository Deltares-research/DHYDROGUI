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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            CrossSectionStandardShapeEgg csEggShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileWidth, out width))
            {
                csEggShape = new CrossSectionStandardShapeEgg { Width = width / 1000 /*Conversion from millimeters to meters*/};
                Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElementKeyValuePairs, width, 1.5, "(2:3)", csEggShape);
            }
            else
            {
                csEggShape = CrossSectionStandardShapeEgg.CreateDefault();
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width");
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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            double height;
            double width;
            CrossSectionStandardShapeArch csArchShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileWidth, out width)
                && gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileHeight, out height))
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
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width and/or height");
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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            double width;
            CrossSectionStandardShapeCunette csCunetteShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileWidth, out width))
            {
                csCunetteShape = new CrossSectionStandardShapeCunette { Width = width / 1000 /*Conversion from millimeters to meters*/};
                Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElementKeyValuePairs, width, 0.634, "(1:0.634)", csCunetteShape);
            }
            else
            {
                csCunetteShape = CrossSectionStandardShapeCunette.CreateDefault();
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width");
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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double height;
            double width;
            CrossSectionStandardShapeRectangle csRectangleShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileWidth, out width)
                && gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileHeight, out height))
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
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width and/or height");
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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            CrossSectionStandardShapeRound csRoundShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileWidth, out width))
            {
                csRoundShape = new CrossSectionStandardShapeRound {Diameter = width / 1000 /*Conversion from millimeters to meters*/};
            }
            else
            {
                csRoundShape = CrossSectionStandardShapeRound.CreateDefault();
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width");
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

            var slope1PresentAndWellFormatted = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.Slope1, out slope1);
            var slope2PresentAndWellFormatted = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.Slope2, out slope2);
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
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width, height and/or slope");
                return new CrossSectionDefinitionStandard(CrossSectionStandardShapeTrapezium.CreateDefault());
            }

            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileWidth, out width)
                && gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileHeight, out height))
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
                Log.MessageForMissingValues(gwswElementKeyValuePairs, "width, height and/or slope");
            }
            return new CrossSectionDefinitionStandard(csTrapezoidShape);
        }
    }

    static class SewerDictionaryExtensions
    {
        public static bool TryGetDoubleValueFromDictionary(this IReadOnlyDictionary<string, string> gwswElementKeyValuePairs, string key, out double doubleValue)
        {
            string stringValue;
            doubleValue = 0;
            return gwswElementKeyValuePairs.TryGetValue(key, out stringValue) && double.TryParse(stringValue.Replace(".", ","), out doubleValue);
        }

        public static void LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion<T>(this ILog logger, IReadOnlyDictionary<string, string> gwswElementKeyValuePairs,
            double width, double heightProportion, string proportionString, T csShape)
            where T : CrossSectionStandardShapeWidthHeightBase
        {
            double height;
            string id;
            var heightDefined = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(SewerProfileMapping.PropertyKeys.SewerProfileHeight, out height);
            if (!heightDefined || Math.Abs(heightProportion * width - height) < 0.0001) return;

            var csHeight = csShape.Height * 1000;
            gwswElementKeyValuePairs.TryGetValue(SewerProfileMapping.PropertyKeys.SewerProfileId, out id);
            logger.WarnFormat(
                "The width and height of sewer profile '{0}' are not in the right proportion {1}. Width is now {2} mm and height is now {3} mm.",
                id, proportionString, width, csHeight);
        }

        public static void MessageForMissingValues(this ILog logger, IReadOnlyDictionary<string, string> gwswElementKeyValuePairs, string missingValuesText)
        {
            string id;
            gwswElementKeyValuePairs.TryGetValue(SewerProfileMapping.PropertyKeys.SewerProfileId, out id);
            logger.WarnFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", id, missingValuesText);
        }
    }
}
