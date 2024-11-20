using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui
{
    /// <summary>
    /// <see cref="IRainfallRunoffGuiPlugin"/> describes the specification of the
    /// rainfall runoff plugin.
    /// </summary>
    public interface IRainfallRunoffGuiPlugin : IPlugin
    {
        /// <summary>
        /// Reference to the the gui (set by framework)
        /// </summary>
        IGui Gui { get; }
    }
}