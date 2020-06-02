using System;
using System.ComponentModel;
using System.Windows.Forms;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    public partial class WaveSpectrumFileSelection : UserControl
    {
        private WaveBoundaryCondition data;

        private int selectedPointIndex;

        public WaveSpectrumFileSelection()
        {
            InitializeComponent();
            selectFileBtn.Click += SelectFileBtnOnClick;
        }

        public Func<string, string> ImportIntoDirectory { private get; set; }

        public WaveBoundaryCondition Data
        {
            get => data;
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChange) data).PropertyChanged -= OnBoundaryConditionPropertyChanged;
                }

                data = value;
                if (data != null)
                {
                    ((INotifyPropertyChange) data).PropertyChanged += OnBoundaryConditionPropertyChanged;
                }

                UpdatePanel();
            }
        }

        public string MdwFilePath { get; set; }

        public int SelectedPointIndex
        {
            get => selectedPointIndex;
            set
            {
                selectedPointIndex = value;
                UpdatePanel();
            }
        }

        private void SelectFileBtnOnClick(object sender, EventArgs eventArgs)
        {
            openFileDialog1.Filter = "Spectrum Files (*.sp1,*.sp2)|*.sp1;*.sp2";
            openFileDialog1.Title = "Select spectrum file ...";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                data.SpectrumFiles[selectedPointIndex] = ImportIntoDirectory(filePath);
                UpdatePanel();
            }
        }

        private void OnBoundaryConditionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var bc = sender as WaveBoundaryCondition;
            if (bc == null)
            {
                return;
            }

            if (bc.IsEditing)
            {
                return;
            }

            UpdatePanel();
        }

        private void UpdatePanel()
        {
            if (data == null)
            {
                return;
            }

            if (data.DataType != BoundaryConditionDataType.SpectrumFromFile)
            {
                return;
            }

            if (data.DataPointIndices.Contains(selectedPointIndex))
            {
                spectrumFileBox.Text = data.SpectrumFiles[selectedPointIndex];
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }
    }
}