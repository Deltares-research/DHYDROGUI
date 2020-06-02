using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Property grid for <see cref="WasteWaterTreatmentPlant"/>.
    /// </summary>
    [ResourcesDisplayName(typeof(Resources), "WasteWaterTreatmentPlantProperties_DisplayName")]
    public class WasteWaterTreatmentPlantProperties : FeatureWithAttributeProperties<WasteWaterTreatmentPlant> {}
}