using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    //GuiImportHandler treats ITargetItemFileImporter and IFileImporter differently. We need both
    public class SobekNetworkToNetworkImporter : SobekNetworkImporter, IFileImporter
    {
        public string Name
        {
            get { return "SOBEK Network (import to existing network)"; }
        }

        public string Description
        {
            get { return string.Empty; }
        }

        public override bool CanImportOnRootLevel
        {
            get { return false; }
        }
    }
}
