using DelftTools.Shell.Gui;

namespace DeltaShell.NGHS.Common.Gui
{
    /// <summary>
    /// Simple container with an <see cref="IGui"/>.
    /// </summary>
    /// <remarks>
    /// This class was introduced to circumvent the use of a Func to retrieve the <see cref="IGui"/>.
    /// See for example the <see cref="GuiPlugin"/> of the Network Editor and D-Flow FM.
    /// The <see cref="GuiPlugin.Gui"/> is mutable and is <c>null</c> when the  <see cref="GuiPlugin.GetViewInfoObjects"/> is called.
    /// </remarks>
    public class GuiContainer
    {
        /// <summary>
        /// Gets or sets the gui.
        /// </summary>
        public IGui Gui { get; set; }
    }
}