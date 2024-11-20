using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class GriddedWindView : UserControl, IView
    {
        private IWindField windField;

        public GriddedWindView()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get
            {
                return "Wind file";
            }
        }

        public object Data
        {
            get
            {
                return WindField;
            }
            set
            {
                if (value is GriddedWindField || value is SpiderWebWindField)
                {
                    WindField = (IWindField) value;
                }
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        private IWindField WindField
        {
            get
            {
                return windField;
            }
            set
            {
                windField = value;
                AfterWindFieldSet();
            }
        }

        private void FileOpenButtonClick(object sender, EventArgs e)
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = WindSelectionDialog.MakeFileFilter(windField) };
            
            string selectedFilePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);

            if (selectedFilePath != null)
            {
                var spiderWebWindField = windField as SpiderWebWindField;
                if (spiderWebWindField != null)
                {
                    spiderWebWindField.WindFilePath = selectedFilePath;
                }

                var griddedWindField = windField as GriddedWindField;
                if (griddedWindField != null)
                {
                    if (griddedWindField.SeparateGridFile)
                    {
                        string gridFile = WindSelectionDialog.GetCorrespondingGridFile(selectedFilePath);
                        if (gridFile == null)
                        {
                            return;
                        }

                        griddedWindField.GridFilePath = gridFile;
                    }

                    griddedWindField.WindFilePath = selectedFilePath;
                }
            }
        }

        private void AfterWindFieldSet()
        {
            gridFilePathLabel.Visible = false;
            WindGridTextBox.Visible = false;

            fileOpenButton.Enabled = windField != null;

            var griddedWindField = windField as GriddedWindField;
            if (griddedWindField != null && griddedWindField.SeparateGridFile)
            {
                gridFilePathLabel.Visible = true;
                WindGridTextBox.Visible = true;
                WindGridTextBox.Text = griddedWindField.GridFilePath;
                WindDataTextBox.Text = griddedWindField.WindFilePath;
            }

            var spiderWebWindField = windField as SpiderWebWindField;
            if (spiderWebWindField != null)
            {
                WindDataTextBox.Text = spiderWebWindField.WindFilePath;
            }
        }
    }
}