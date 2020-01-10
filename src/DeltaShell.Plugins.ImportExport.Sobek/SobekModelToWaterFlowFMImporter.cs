using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    //GuiImportHandler treats ITargetItemFileImporter and IFileImporter differently. We need both
    public class SobekModelToWaterFlowFMImporter : SobekModelToIntegratedModelImporter, IFileImporter
    {
        public virtual string Name
        {
            get { return "SOBEK Model (Import into Existing Model)"; }
        }
        public string Description { get { return Name; } }
        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public virtual bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }
    }
    public class SobekModelToWaterFlowFmImporterToImporterToWaterFlowFmImporterOnRootImporter : SobekModelToWaterFlowFMImporter
    {
        public override string Name
        {
            get { return "SOBEK 2 Model to FM"; }
        }
        
        public override bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public override object TargetItem
        {
            get
            {
                return targetItem ?? (targetItem = new WaterFlowFMModel("FlowFM"));
            }
            set
            {
                targetItem = value;
                targetItemHasBeenSet = true;
            }
        }

    }
}
