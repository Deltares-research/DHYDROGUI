using System;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public partial class CrossSectionImportSettingsWizardPage : UserControl, IWizardPage
    {
        public CrossSectionImportSettingsWizardPage()
        {
            InitializeComponent();

            cbImportChainages.Checked = true;
            cbCreateIfNotFound.Checked = true;
        }

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            return true;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        public event EventHandler PageUpdated;

        public CrossSectionImportSettings Settings
        {
            get
            {
                return new CrossSectionImportSettings
                    {
                        ImportChainages = cbImportChainages.Checked,
                        CreateIfNotFound = cbCreateIfNotFound.Checked
                    };
            }
        }
    }
}
