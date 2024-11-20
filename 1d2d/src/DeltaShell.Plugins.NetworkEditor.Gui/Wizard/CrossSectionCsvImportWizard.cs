using System;
using System.IO;
using System.Linq;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.Csv;
using DelftTools.Controls.Swf.WizardPages;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public partial class CrossSectionCsvImportWizard : WizardDialog
    {
        private readonly SelectFileWizardPage fileSelectPage;
        private readonly CsvToDataTableWizardPage csvSeparatorPage;
        private readonly CrossSectionImportSettingsWizardPage csImportSettingsPage;

        public CrossSectionCsvImportWizard()
        {
            InitializeComponent();
            Height = 768;

            WelcomeMessage = "This wizard will allow you to import CSV data";
            FinishedPageMessage = "Press Finish to import the data.";

            fileSelectPage = new SelectFileWizardPage { Filter = "CSV files|*.csv" };
            csvSeparatorPage = new CsvToDataTableWizardPage();
            csImportSettingsPage = new CrossSectionImportSettingsWizardPage();
            
            AddPage(fileSelectPage, "Select CSV file", "Select the CSV-file containing the data");
            AddPage(csvSeparatorPage, "CSV parse columns", "");
            AddPage(csImportSettingsPage, "Cross Section Import Settings",
                    "Choose your settings for the cross section import");
        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            if (page == fileSelectPage)
            {
                //read first 30 lines (or less) of file
                csvSeparatorPage.PreviewText = string.Join(Environment.NewLine, File.ReadLines(fileSelectPage.FileName).Take(30).ToArray());
            }
            else if (page == csvSeparatorPage)
            {
                Importer.CsvSettings = csvSeparatorPage.Settings;
                Importer.FilePath = fileSelectPage.FileName;
            }
            else if (page == csImportSettingsPage)
            {
                Importer.CrossSectionImportSettings = csImportSettingsPage.Settings;
            }
        }

        private CrossSectionFromCsvFileImporterBase Importer
        {
            get { return Data as CrossSectionFromCsvFileImporterBase; }
        }
    }
}
