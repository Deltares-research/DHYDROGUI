using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class MeteoFileSelectionControl : UserControl, INotifyPropertyChanged
    {
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
            set => openFileDialog1.Filter = value;
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileName = ImportIntoDirectory != null
                               ? ImportIntoDirectory(openFileDialog1.FileName)
                               : openFileDialog1.FileName;
            }
        }
    }
}