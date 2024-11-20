using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
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

        public override string Text { get { return "Wind file"; } }

        private void FileOpenButtonClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open file",
                Filter = WindSelectionDialog.MakeFileFilter(windField)
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var spiderWebWindField = windField as SpiderWebWindField;
                if (spiderWebWindField != null)
                {
                    spiderWebWindField.WindFilePath = dialog.FileName;
                }
                var griddedWindField = windField as GriddedWindField;
                if (griddedWindField != null)
                {
                    var filePath = dialog.FileName;
                    if (griddedWindField.SeparateGridFile)
                    {
                        var gridFile = WindSelectionDialog.GetCorrespondingGridFile(filePath);
                        if (gridFile == null)
                        {
                            return;
                        }
                        griddedWindField.GridFilePath = gridFile;
                    }
                    griddedWindField.WindFilePath = filePath;
                }
            }
        }

        private IWindField WindField
        {
            get { return windField; }
            set
            {
                windField = value;
                AfterWindFieldSet();
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

        public object Data
        {
            get { return WindField; }
            set
            {
                if (value is GriddedWindField || value is SpiderWebWindField)
                {
                    WindField = (IWindField) value;
                }
            }
        }

        public Image Image { get; set; }
        
        public void EnsureVisible(object item){}

        public ViewInfo ViewInfo { get; set; }
    }
}
