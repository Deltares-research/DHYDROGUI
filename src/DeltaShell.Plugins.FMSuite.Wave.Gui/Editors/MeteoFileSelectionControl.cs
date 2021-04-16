using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using DelftTools.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class MeteoFileSelectionControl : UserControl, INotifyPropertyChanged
    {
        private string fileFilter;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public MeteoFileSelectionControl()
        {
            InitializeComponent();
        }

        public Func<string, string> ImportIntoDirectory { private get; set; }

        public string LabelText
        {
            set => fileLabel.Text = value;
        }

        public string FileFilter
        {
            set => fileFilter = value;
        }

        public string FileName
        {
            get => meteoFileBox.Text;
            set
            {
                if (FileName == value)
                {
                    return;
                }

                meteoFileBox.Text = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void selMeteoBtn_Click(object sender, EventArgs e)
        {
            string filePath = new FileDialogService().SelectFile(fileFilter);
            if (filePath != null)
            {
                FileName = ImportIntoDirectory != null
                               ? ImportIntoDirectory(filePath)
                               : filePath;
            }
        }
    }
}