using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public abstract class ASewerCrossSectionDefinitionGenerator : ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(ASewerCrossSectionDefinitionGenerator));
        public abstract INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null);

        protected static void AddCrossSectionDefinitionToNetwork<TShape>(GwswElement gwswElement, TShape csShape, IHydroNetwork network)
            where TShape : CrossSectionStandardShapeBase
        {
            var csIdAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);
            var materialAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileMaterial);
            var material = materialAttribute.GetValueFromDescription<SewerProfileMapping.SewerProfileMaterial>();
            var csDefinitionName = csIdAttribute.GetValidStringValue();

            var csDefinition = new CrossSectionDefinitionStandard(csShape)
            {
                Name = csDefinitionName
            };

            if (network == null) return;
            network.SharedCrossSectionDefinitions.RemoveAllWhere(csd => csd.Name == csDefinitionName);
            network.SharedCrossSectionDefinitions.Add(csDefinition);
            var pipes = network.Pipes.Where(p => p.SewerProfileDefinition.Name == csDefinitionName);
            foreach (var p in pipes)
            {
                p.SewerProfileDefinition = csDefinition;
                ((Pipe) p).Material = material;
            }
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
}
