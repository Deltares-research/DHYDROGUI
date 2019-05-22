using System.Windows.Forms;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class BoundaryConditionBcFileImportDialog : Form
    { 
        public BoundaryConditionBcFileImportDialog()
        {
            InitializeComponent();
            overwriteCheckBox.Checked = true;
        }

        public void Configure(IFileImporter fileImporter)
        {
            if (fileImporter is BcFileImporter)
            {
                ((BcFileImporter) fileImporter).DeleteDataBeforeImport = deleteDataCheckBox.Checked;
                ((BcFileImporter) fileImporter).OverwriteExistingData = overwriteCheckBox.Checked;
            }
        }
    }
}
