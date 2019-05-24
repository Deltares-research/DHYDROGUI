using System;
using System.IO;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveOverallSpectrumFileSelection : UserControl
    {
        public WaveOverallSpectrumFileSelection()
        {
            InitializeComponent();
            selectFileBtn.Click += SelectFileBtnOnClick;

            openFileDialog1.Filter = "Overall Spectrum File (*.sp2)|*.sp2";
            openFileDialog1.Title = "Select SWAN spectrum file ...";
        }

        private void SelectFileBtnOnClick(object sender, EventArgs eventArgs)
        {
            SelectSp2File();
        }

        public DialogResult SelectSp2File()
        {
            DialogResult dlgResult = openFileDialog1.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                string filePath = Path.GetFullPath(openFileDialog1.FileName);
                data.OverallSpecFile = data.ImportIntoModelDirectory(filePath);
                UpdatePanel();
            }

            return dlgResult;
        }

        private WaveModel data;

        public WaveModel Data
        {
            get => data;
            set
            {
                data = value;
                UpdatePanel();
            }
        }

        private void UpdatePanel()
        {
            spectrumFileBox.Text = data != null ? data.OverallSpecFile : "";
        }
    }
}