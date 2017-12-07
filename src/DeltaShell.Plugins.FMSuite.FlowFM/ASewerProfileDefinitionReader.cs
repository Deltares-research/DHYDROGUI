using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    abstract class ASewerCrossSectionDefinitionGenerator : ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(ASewerCrossSectionDefinitionGenerator));
        public abstract INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network);

        protected static void AddCrossSectionDefinitionToNetwork<TShape>(GwswElement gwswElement, TShape csShape, IHydroNetwork network)
            where TShape : CrossSectionStandardShapeBase
        {
            var csIdAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);
            var csDefinitionName = csIdAttribute.GetValidStringValue();

            var csDefinition = new CrossSectionDefinitionStandard(csShape)
            {
                Name = csDefinitionName
            };

            if (network == null) return;
            network.SharedCrossSectionDefinitions.RemoveAllWhere(csd => csd.Name == csDefinitionName);
            network.SharedCrossSectionDefinitions.Add(csDefinition);
            network.Pipes.Where(p => p.SewerProfileDefinition.Name == csDefinitionName).ForEach(p => p.SewerProfileDefinition = csDefinition);
        }

        protected void MessageForMissingValues(GwswElement gwswElement, string missingValuesText)
        {
            var id = gwswElement?.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId)?.ValueAsString;
            Log.WarnFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", id, missingValuesText);
        }

        protected void MessageForDefaultProfile(GwswElement gwswElement)
        {
            var id = gwswElement?.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId)?.ValueAsString;
            Log.WarnFormat(Resources.SewerFeatureFactory_CreateSewerProfile_Shape_was_not_defined_for_sewer_profile___0___in__Profiel_csv___A_default_round_profile_with_diameter_of_400_mm_is_used_for_this_profile_, id);
        }

        protected void LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion<TShape>(GwswElement gwswElement,
            double width, double heightProportion, string proportionString, TShape csShape)
            where TShape : CrossSectionStandardShapeWidthHeightBase
        {
            double height;
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (heightAttribute.TryGetValueAsDouble(out height) && Math.Abs(heightProportion * width - height) < 0.0001) return;

            var csHeight = csShape.Height * 1000;
            var idAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);

            var id = idAttribute?.ValueAsString;
            Log.WarnFormat(
                "The width and height of sewer profile '{0}' are not in the right proportion {1}. Width is now {2} mm and height is now {3} mm.",
                id, proportionString, width, csHeight);
        }
    }

    class DefaultCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            MessageForDefaultProfile(gwswElement);
            var csRoundShape = new CrossSectionStandardShapeRound
            {
                Diameter = 0.4
            };
            AddCrossSectionDefinitionToNetwork(gwswElement, csRoundShape, network);
            return null;
        }
    }

    class EggCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

            double width;
            CrossSectionStandardShapeEgg csEggShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csEggShape = new CrossSectionStandardShapeEgg { Width = width / 1000 /*Conversion from millimeters to meters*/};
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 1.5, "(2:3)", csEggShape);
            }
            else
            {
                csEggShape = CrossSectionStandardShapeEgg.CreateDefault();
                MessageForMissingValues(gwswElement, "width");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csEggShape, network);
            return null;
        }
    }

    class ArchCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

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
                MessageForMissingValues(gwswElement, "width and/or height");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csArchShape, network);
            return null;
        }
    }

    class CunetteCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

            double width;
            CrossSectionStandardShapeCunette csCunetteShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csCunetteShape = new CrossSectionStandardShapeCunette { Width = width / 1000 /*Conversion from millimeters to meters*/};
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 0.634, "(1:0.634)", csCunetteShape);
            }
            else
            {
                csCunetteShape = CrossSectionStandardShapeCunette.CreateDefault();
                MessageForMissingValues(gwswElement, "width");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csCunetteShape, network);
            return null;
        }
    }

    class RectangleCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            
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
                MessageForMissingValues(gwswElement, "width and/or height");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csRectangleShape, network);
            return null;
        }
    }

    class CircleCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            
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
                MessageForMissingValues(gwswElement, "width");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csRoundShape, network);
            return null;
        }
    }

    class TrapezoidCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            
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
            else if (slope1PresentAndWellFormatted)
            {
                slope = (slope1 + slope2) / 2;
            }
            else
            {
                MessageForMissingValues(gwswElement, "width, height and/or slope");
                csTrapezoidShape = CrossSectionStandardShapeTrapezium.CreateDefault();
                AddCrossSectionDefinitionToNetwork(gwswElement, csTrapezoidShape, network);
                return null;
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
                MessageForMissingValues(gwswElement, "width, height and/or slope");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csTrapezoidShape, network);
            return null;
        }
    }
}
