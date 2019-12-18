using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Property grid for <see cref="RunoffBoundary"/>.
    /// </summary>
    [ResourcesDisplayName(typeof(Resources), "RunoffBoundaryProperties_DisplayName")]
    public class RunoffBoundaryProperties : FeatureWithAttributeProperties<RunoffBoundary> {}
}