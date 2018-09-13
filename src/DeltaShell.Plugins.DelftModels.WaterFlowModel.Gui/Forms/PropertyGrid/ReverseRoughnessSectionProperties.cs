using DelftTools.Hydro.Roughness;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ReverseRoughnessSectionProperties_DisplayName")]
    public class ReverseRoughnessSectionProperties : RoughnessSectionPropertiesBase<ReverseRoughnessSection>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "ReverseRoughnessSectionProperties_UseReverseRoughness_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ReverseRoughnessSectionProperties_UseReverseRoughness_Description")]
        public bool UseReverseRoughness
        {
            get { return !data.UseNormalRoughness; }
            set
            {
                data.BeginEdit(GetEditActionName(value));
                data.UseNormalRoughness = !value;
                data.EndEdit();
            }
        }

        private string GetEditActionName(bool newValue)
        {
            return string.Format("Changing '{0}' from {1} to {2} ({3})", 
                                 Resources.ReverseRoughnessSectionProperties_UseReverseRoughness_DisplayName, 
                                 !data.UseNormalRoughness, newValue,
                                 data.Name);
        }
    }
}