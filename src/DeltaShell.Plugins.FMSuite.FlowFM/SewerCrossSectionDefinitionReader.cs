using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    interface SewerCrossSectionDefinitionReader
    {
        ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement);
    }

    class CsdEggDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            double width;
            CrossSectionStandardShapeEgg csEggShape;
            if (gwswElementKeyValuePairs.TryGetDoubleValueFromDictionary(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out width))
            {
                csEggShape = new CrossSectionStandardShapeEgg { Width = width / 1000 /*Conversion from millimeters to meters*/};
            }
            else
            {
                csEggShape = CrossSectionStandardShapeEgg.CreateDefault();
            }
            return new CrossSectionDefinitionStandard(csEggShape);
        }
    }

    class CsdHeulDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            throw new System.NotImplementedException();
        }
    }

    class CsdMuilDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            throw new System.NotImplementedException();
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
            return gwswElementKeyValuePairs.TryGetValue(key, out stringValue) && double.TryParse(stringValue, out doubleValue);
        }
    }
}
