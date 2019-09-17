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
    public partial class LoadsDataWizardPage : UserControl, IWizardPage
    {
        public LoadsDataWizardPage(string dataDirectory = null)
        {
            InitializeComponent();
      
            if (dataDirectory == null)
            {
                dataDirectory = DelwaqFileStructureHelper.GetDelwaqDataFolderPath();
            }
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
            textBox1.Text = text;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (openCsvFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sr = new StreamReader(openCsvFileDialog.FileName);
                    SetText(sr.ReadToEnd());

                    csvLoadsPath = openCsvFileDialog.FileName;
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Gets the selected file path to read from.
        /// </summary>
        public string csvLoadsPath { get; private set; }
        
        private void Label1_Click(object sender, EventArgs e)
        {

        }
    }
}
