using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using Mono.Addins;

namespace DeltaShell.Plugins.DeveloperTools
{
    [Extension(typeof(IPlugin))]
    public class DeveloperToolsGuiPlugin : GuiPlugin
    {
        private static DeveloperToolsGuiPlugin instance;

        public DeveloperToolsGuiPlugin()
        {
            instance = this;
        }

        public override string Name
        {
            get { return "Developer Tools"; }
        }

        public override string DisplayName
        {
            get { return "Delta Shell Developer Tools Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Provides a set of tools useful during development of plugins"; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public static DeveloperToolsGuiPlugin Instance
        {
            get { return instance; }
        }

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get { return new Ribbon(); }
        }
    }
}