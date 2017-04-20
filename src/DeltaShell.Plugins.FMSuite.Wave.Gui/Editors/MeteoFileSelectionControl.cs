using System;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class MeteoFileSelectionControl : UserControl
    {
        private string label;

        public MeteoFileSelectionControl()
        {
            InitializeComponent();
        }
        public Func<string, string> ImportIntoDirectory { private get; set; } 

        public string LabelText { set {fileLabel.Text = value;}}
        public string FileFilter { set { openFileDialog1.Filter = value; }}

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                meteoFileBox.Text = fileName;
            }
        }

        private void selMeteoBtn_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = ImportIntoDirectory != null
                    ? ImportIntoDirectory(openFileDialog1.FileName)
                    : openFileDialog1.FileName;
                meteoFileBox.Text = fileName;
            }
        }
    }
}
