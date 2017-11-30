using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    interface SewerCrossSectionDefinitionReader
    {
        ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement);
    }

    class CsdEggDefinitionReader : SewerCrossSectionDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdEggDefinitionReader));
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            CrossSectionStandardShapeEgg csEggShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width))
            {
                csEggShape = new CrossSectionStandardShapeEgg { Width = width / 1000 /*Conversion from millimeters to meters*/};
                Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElementKeyValuePairs, width, 1.5, "(2:3)", csEggShape);
            }
            else
            {
                csEggShape = CrossSectionStandardShapeEgg.CreateDefault();
            }
            
            return new CrossSectionDefinitionStandard(csEggShape);
        }

        private static void LogMessageInCaseWidthHeightAreNotInCorrectProportion<T>(IReadOnlyDictionary<string, string> gwswElementKeyValuePairs, double width, T csEggShape)
            where T : CrossSectionStandardShapeWidthHeightBase
        {
            double height;
            string id;
            var heightDefined = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight, out height);
            if (!heightDefined || Math.Abs(1.5 * width - height) < 0.0001) return;

            var csHeight = csEggShape.Height * 1000;
            gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionId, out id);
            Log.WarnFormat(
                "The width and height of sewer profile '{0}' are not in the right proportion (2:3). Width is now {1} mm and height is now {2} mm.",
                id, width, csHeight);
        }
    }

    class CsdArchDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            double height;
            double width;
            CrossSectionStandardShapeArch csArchShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width)
                && gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight, out height))
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
            }
            return new CrossSectionDefinitionStandard(csArchShape);
        }
    }

    class CsdCunetteDefinitionReader : SewerCrossSectionDefinitionReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(CsdCunetteDefinitionReader));
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            double width;
            CrossSectionStandardShapeCunette csCunetteShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width))
            {
                csCunetteShape = new CrossSectionStandardShapeCunette { Width = width / 1000 /*Conversion from millimeters to meters*/};
                Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElementKeyValuePairs, width, 0.634, "(1:0.634)", csCunetteShape);
            }
            else
            {
                csCunetteShape = CrossSectionStandardShapeCunette.CreateDefault();
            }
            
            return new CrossSectionDefinitionStandard(csCunetteShape);
        }
    }

    class CsdRectangleDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double height;
            double width;
            CrossSectionStandardShapeRectangle csRectangleShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width)
                && gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight, out height))
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
            }
            return new CrossSectionDefinitionStandard(csRectangleShape);
        }
    }

    class CsdCircleDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            CrossSectionStandardShapeRound csRoundShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width))
            {
                csRoundShape = new CrossSectionStandardShapeRound {Diameter = width / 1000 /*Conversion from millimeters to meters*/};
            }
            else
            {
                csRoundShape = CrossSectionStandardShapeRound.CreateDefault();
            }
            return new CrossSectionDefinitionStandard(csRoundShape);
        }
    }

    class CsdTrapezoidDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            double height;
            double slope1;
            double slope2;
            double slope;
            CrossSectionStandardShapeTrapezium csTrapezoidShape;

            var slope1PresentAndWellFormatted = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.Slope1, out slope1);
            var slope2PresentAndWellFormatted = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.Slope2, out slope2);
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
                return new CrossSectionDefinitionStandard(CrossSectionStandardShapeTrapezium.CreateDefault());
            }

            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width)
                && gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight, out height))
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
            var heightDefined = gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight, out height);
            if (!heightDefined || Math.Abs(heightProportion * width - height) < 0.0001) return;

            var csHeight = csShape.Height * 1000;
            gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionId, out id);
            logger.WarnFormat(
                "The width and height of sewer profile '{0}' are not in the right proportion {1}. Width is now {2} mm and height is now {3} mm.",
                id, proportionString, width, csHeight);
        }
    }
}
