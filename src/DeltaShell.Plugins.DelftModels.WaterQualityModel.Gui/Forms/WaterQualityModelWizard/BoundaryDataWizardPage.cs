using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
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
