using System;
using System.IO;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Wpf.Services;
using MessageBox = System.Windows.MessageBox;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    /// <summary>
    /// Wizard page for importing data from a csv file.
    /// </summary>
    public abstract partial class CsvDataWizardPage : UserControl, IWizardPage
    {
        /// <summary>
        /// Gets the selected file path to read from.
        /// </summary>
        public string CsvFilePath { get; private set; }

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
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
            
            string filePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            if (filePath == null)
            {
                return;
            }
            
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    SetText(reader.ReadToEnd());

                    CsvFilePath = filePath;
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IO exception.\n\nError message: {ex.Message}\n\n" +
                                $"Details:\n\n{ex.StackTrace}");
            }
        }
    }
}