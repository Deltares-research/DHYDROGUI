using System.Drawing;
using DeltaShell.NGHS.Common.Gui.Properties;

namespace DeltaShell.NGHS.Common.Gui.Restart
{
    /// <summary>
    /// A static class to hold the resources used by all concrete RestartFileNodePresenter classes
    /// </summary>
    internal static class RestartFileNodePresenterResources
    {
        /// <summary>
        /// The icon to use for tree nodes of RestartFile instances representing actual files
        /// </summary>
        public static Image RestartIcon => Resources.restart;
        /// <summary>
        /// The icon to use for the tree node representing an empty RestartFile 
        /// </summary>
        public static Image EmptyRestartIcon => Resources.restart_empty;
    }
}