using System;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.WizardPages;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.Fews.Forms
{
    public partial class ImportPiTimeSeriesDialog : WizardDialog, IConfigureDialog
    {
        private SelectFileWizardPage selectFileWizardPage;
        private SelectTimeSeriesWizardPage selectTimeSeriesWizardPage;

        public bool MultiSelect
        {
            get; set;
        }

        public ImportPiTimeSeriesDialog()
        {
            InitializeComponent();

            WelcomeMessage = "This wizard will allow you to import FEWS-PI data to a flow or waterlevel timeseries.";
            FinishedPageMessage = "Press Finish to add the timeseries to the project.";

            selectFileWizardPage = new SelectFileWizardPage();
            selectTimeSeriesWizardPage = new SelectTimeSeriesWizardPage();

            selectFileWizardPage.Filter = "FEWS-PI xml files |*.xml";

            AddPage(selectFileWizardPage, "Select FEWS-PI xml file", "Select the xml file containing the data");
            AddPage(selectTimeSeriesWizardPage, "Select time series from file", "Select time series from file");
        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            //copy the filename to the next page
            if (page == selectFileWizardPage)
            {
                //configure next page
                ((PiTimeSeriesImporter) Data).FilePath = selectFileWizardPage.FileName;
                selectTimeSeriesWizardPage.PiTimeSeriesImporter = (PiTimeSeriesImporter)Data;
                selectTimeSeriesWizardPage.MultiSelect = MultiSelect;
                selectTimeSeriesWizardPage.InitPage();
            }
            else
            {
                if (page == selectTimeSeriesWizardPage)
                {
                    if (selectTimeSeriesWizardPage.GetSelectedTimeSeries != null)
                        ((PiTimeSeriesImporter) Data).SelectedTimeSeries =
                            selectTimeSeriesWizardPage.GetSelectedTimeSeries;
                }
            }
            base.OnPageCompleted(page);
        }

        #region IConfigureDialog members

        public void Configure(object fileImporter)
        {
            var importer = (fileImporter as PiTimeSeriesImporter);
            if (importer == null)
            {
                throw new InvalidOperationException("Invalid importer for dialog to configure");
            }
        }

        #endregion
    }
}
