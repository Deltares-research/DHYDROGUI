using System.Collections.Generic;
using DelftTools.Controls.Swf.Csv;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms
{
    public class FlowTimeSeriesCsvImportDialog : CsvImportWizardDialog
    {
        private FlowSeriesCsvMappingPage mappingPage;

        public bool ForBoundaryConditions
        {
            get { return mappingPage != null && mappingPage.ForBoundaryConditions; }
            set
            {
                if (mappingPage != null)
                    mappingPage.ForBoundaryConditions = value;
            }
        }
        
        public bool BatchMode
        {
            get { return mappingPage != null && mappingPage.BatchMode; }
            set
            {
                if (mappingPage != null)
                    mappingPage.BatchMode = value;
            }
        }

        protected override ICsvDataSelectionWizardPage CreateCsvMappingPage()
        {
            mappingPage = new FlowSeriesCsvMappingPage();
            mappingPage.ImportDataTypeChanged +=
                () => mappingPage.SetData(csvSeparatorPage.PreviewDataTable, GetRequiredFields());
            return mappingPage;
        }

        protected override void OnUserFinishedMapping(string filePath, CsvMappingData mappingData)
        {
            // set data to importer
            var importer = Data as FlowDataCsvImporter;
            if (importer == null) return;

            importer.FilePath = filePath;
            importer.CsvMappingData = mappingData;

            if (BatchMode)
            {
                importer.FileImporter.CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnDiscriminator;
            }
            else
            {
                importer.FileImporter.CsvImporterMode = CsvImporterMode.OneFunction;
            }

            if (mappingPage.rbQH.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.QH;
            }
            else if (mappingPage.rbQT.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.QT;
            }
            else if (mappingPage.rbQ.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.Q;
            }
            else if (mappingPage.rbHT.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.HT;
            }
            else if (mappingPage.rbH.Checked)
            {
                importer.BoundaryRelationType = BoundaryRelationType.H;
            }
        }

        protected override IEnumerable<CsvRequiredField> GetRequiredFields()
        {
            yield break; // handled in dedicated mapping page
        }
    }
}
