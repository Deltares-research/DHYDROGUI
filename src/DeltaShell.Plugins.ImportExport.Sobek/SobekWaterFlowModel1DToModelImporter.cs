using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    //GuiImportHandler treats ITargetItemFileImporter and IFileImporter differently. We need both
    public class SobekWaterFlowModel1DToModelImporter : SobekWaterFlowModel1DImporter, IFileImporter
    {
        public string Name
        {
            get { return "SOBEK Model (Import into Existing Model)"; }
        }
        public string Description { get { return Name; } }
        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }
    }
}
