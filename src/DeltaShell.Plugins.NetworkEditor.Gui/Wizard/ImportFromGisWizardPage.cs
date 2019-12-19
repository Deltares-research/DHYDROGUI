using System;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public partial class ImportFromGisWizardPage : UserControl, IWizardPage
    {
        private HydroRegionFromGisImporter hydroRegionFromGisImporter;

        public ImportFromGisWizardPage()
        {
            InitializeComponent();
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

        public HydroRegionFromGisImporter HydroRegionFromGisImporter
        {
            get
            {
                return hydroRegionFromGisImporter;
            }
            set
            {
                hydroRegionFromGisImporter = value;
                textBoxSnappingPrecision.Text = hydroRegionFromGisImporter.SnappingPrecision.ToString();
            }
        }

        private void textBoxSnappingPrecision_TextChanged(object sender, EventArgs e)
        {
            int snappingValue;
            if (int.TryParse(textBoxSnappingPrecision.Text, out snappingValue))
            {
                hydroRegionFromGisImporter.SnappingPrecision = snappingValue;
            }
            else
            {
                textBoxSnappingPrecision.Text = hydroRegionFromGisImporter.SnappingPrecision.ToString();
            }
        }

        private void buttonSaveMappingFile_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save importers to mapping file";
            saveFileDialog.Filter = "Mapping file (.xml)|*.xml";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                HydroNetworkFromGisImporterXmlSerializer.Serialize(hydroRegionFromGisImporter, saveFileDialog.FileName);
            }
        }
    }
}
