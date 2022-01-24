using DelftTools.Controls.Swf.Csv;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class LateralSourceImportDialog : CsvImportWizardDialog
    {
        private LateralSourceMappingPage mappingPage;

        public bool ForBoundaryConditions
        {
            get => mappingPage != null && mappingPage.ForBoundaryConditions;
            set
            {
                if (mappingPage != null)
                    mappingPage.ForBoundaryConditions = value;
            }
        }
        
        public bool BatchMode
        {
            get => mappingPage != null && mappingPage.BatchMode;
            set
            {
                if (mappingPage != null)
                    mappingPage.BatchMode = value;
            }
        }

        protected override ICsvDataSelectionWizardPage CreateCsvMappingPage()
        {
            mappingPage = new LateralSourceMappingPage();
            
            // required fields is handled in mapping page
            mappingPage.ImportDataTypeChanged +=
                () => mappingPage.SetData(csvSeparatorPage.PreviewDataTable, null);
            return mappingPage;
        }

        protected override void OnUserFinishedMapping(string filePath, CsvMappingData mappingData)
        {
            if (!(Data is LateralSourceImporter importer)) return;

            importer.FilePath = filePath;
            importer.CsvMappingData = mappingData;

            importer.FileImporter.CsvImporterMode = 
                BatchMode ? CsvImporterMode.SeveralFunctionsBasedOnDiscriminator : CsvImporterMode.OneFunction;

            if (mappingPage.rbQH.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.Qh;
            }
            else if (mappingPage.rbQT.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.Qt;
            }
            else if (mappingPage.rbQ.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.Q;
            }
            else if (mappingPage.rbHT.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.Ht;
            }
            else if (mappingPage.rbH.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.H;
            }
        }
    }
}
