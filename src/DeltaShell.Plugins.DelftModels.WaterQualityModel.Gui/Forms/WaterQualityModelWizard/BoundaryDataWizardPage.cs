using System;
using System.IO;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using MessageBox = System.Windows.MessageBox;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    public partial class BoundaryDataWizardPage : UserControl, IWizardPage
    {
        public BoundaryDataWizardPage()
        {
            InitializeComponent();
        }

        public bool CanDoNext()
        {
            return true;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        public bool CanFinish()
        {
            return true;
        }

        private void SetText(string text)
        {
            previewTextBox.Text = text;
        }

        private void OpenCsvButton_Click(object sender, EventArgs e)
        {
            if (openCsvFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var reader = new StreamReader(openCsvFileDialog.FileName))
                    {
                        SetText(reader.ReadToEnd());

                        CsvBoundaryPath = openCsvFileDialog.FileName;
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"IO exception.\n\nError message: {ex.Message}\n\n" +
                                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Gets the selected file path to read from.
        /// </summary>
        public string CsvBoundaryPath { get; private set; }
    }
}
