using System;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public abstract class ASewerCrossSectionShapeGenerator : IGwswFeatureGenerator<ISewerFeature>
    {
        private static ILog Log = LogManager.GetLogger(typeof(ASewerCrossSectionShapeGenerator));
        public abstract ISewerFeature Generate(GwswElement gwswElement);

        protected string GetCrossSectionShapeName(GwswElement gwswElement)
        {
            var nameAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);
            return nameAttribute.GetValidStringValue();
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

        protected static string GetMaterialValue(GwswElement gwswElement)
        {
            var materialAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileMaterial);
            var materialValue = materialAttribute == null
                ? SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription()
                : materialAttribute.GetValidStringValue();
            return materialValue;
        }
    }
}
