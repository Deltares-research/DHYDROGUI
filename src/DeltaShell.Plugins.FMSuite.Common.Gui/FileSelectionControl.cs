using System;
using System.IO;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public partial class FileSelectionControl : UserControl
    {
        private string filePath;

        public FileSelectionControl()
        {
            InitializeComponent();
        }

        public bool ShowFileNameOnly { get; set; }

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                textBox1.Text = ShowFileNameOnly ? Path.GetFileName(filePath) : filePath;
            }
        }

        public string FileFilter { get; set; }

        public string LabelText
        {
            get
            {
                return label.Text;
            }
            set
            {
                label.Text = value;
            }
        }

        public Action<string, int> AfterFileSelected { get; set; }

        private void FileOpenButtonClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                AddExtension = true,
                Filter = FileFilter,
                Multiselect = false
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                FilePath = dialog.FileName;
            }

            if (AfterFileSelected != null)
            {
                AfterFileSelected(FilePath, dialog.FilterIndex - 1);
            }
        }
    }
}