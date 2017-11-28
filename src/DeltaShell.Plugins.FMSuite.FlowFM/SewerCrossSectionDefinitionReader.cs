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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);
            
            string csWidth;
            double width;
            if (gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out csWidth) && double.TryParse(csWidth, out width))
            {
                var csEggShape = new CrossSectionStandardShapeEgg { Width = width };
                return new CrossSectionDefinitionStandard(csEggShape);
            }
            return null;
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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            string csHeight;
            double height;
            string csWidth;
            double width;
            if (gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out csWidth) && double.TryParse(csWidth, out width)
                && gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight, out csHeight) && double.TryParse(csHeight, out height))
            {
                var csRectangleShape = new CrossSectionStandardShapeRectangle
                {
                    Width = width,
                    Height = height
                };
                return new CrossSectionDefinitionStandard(csRectangleShape);
            }
            return null;
        }
    }

    class CsdCircleDefinitionReader : SewerCrossSectionDefinitionReader
    {
        public ICrossSectionDefinition ReadCrossSectionDefinition(GwswElement gwswElement)
        {
            if (gwswElement.ElementTypeName != SewerFeatureType.Crosssection.ToString()) return null;
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            string csWidth;
            double doubleValue;
            CrossSectionStandardShapeRound csRoundShape;
            if (gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out csWidth) 
                && double.TryParse(csWidth, out doubleValue))
            {
                csRoundShape = new CrossSectionStandardShapeRound {Diameter = doubleValue / 1000 /* Conversion from millimeters to meters */};
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
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            string csWidth;
            double width;
            if (gwswElementKeyValuePairs.TryGetValue(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, out csWidth) && double.TryParse(csWidth, out width))
            {
                //TODO: investigate trapezoid properties!
                var csTrapezoidShape = CrossSectionStandardShapeTrapezium.CreateDefault();
                return new CrossSectionDefinitionStandard(csTrapezoidShape);
            }
            return null;
        }
    }
}
