using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    [Extension(typeof(IPlugin))]
    public class SobekImportGuiPlugin : GuiPlugin
    {
        public override string Name
        {
            get { return "Sobek import (UI)"; }
        }

        public override string DisplayName
        {
            get { return "SOBEK Import Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Plugin that provides functionality to import from sobek legacy format"; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.0.0"; }
        }
    }
}