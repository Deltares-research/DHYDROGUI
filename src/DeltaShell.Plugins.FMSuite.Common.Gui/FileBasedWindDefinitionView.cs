using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.Wind;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public partial class FileBasedWindDefinitionView : UserControl, IView
    {
        private FileBasedWindDefinition windDefinition;

        public FileBasedWindDefinitionView()
        {
            InitializeComponent();

            TypeComboBox.Items.AddRange(
                Enum.GetValues(typeof (WindDefinitionType)).Cast<object>().ToArray());
            TypeComboBox.Format += TypeComboBoxFormat;
            TypeComboBox.SelectedIndexChanged += SelectedWindTypeChanged;
            TypeComboBox.SelectedIndex = 0;
            Load += FileBasedWindDefinitionView_Load;
        }

        void FileBasedWindDefinitionView_Load(object sender, EventArgs e)
        {
            RefreshFlowLayoutPanel();
        }

        private void SelectedWindTypeChanged(object sender, EventArgs e)
        {
            if (windDefinition == null ||
                windDefinition.Type == (WindDefinitionType) TypeComboBox.SelectedItem)
            {
                return;
            }
            if (TypeComboBox.SelectedIndex != -1)
            {
                var selectedType = (WindDefinitionType) TypeComboBox.SelectedItem;
                windDefinition.Type = selectedType;
            }

            RefreshFlowLayoutPanel();
        }

        private void RefreshFlowLayoutPanel()
        {
            if (windDefinition == null)
            {
                flowLayoutPanel1.Controls.Clear();
                groupBox1.Visible = false;
                TypeComboBox.Enabled = false;
                addSpiderWebButton.Enabled = false;
            }
            else
            {
                groupBox1.Visible = true;
                TypeComboBox.Enabled = true;
                flowLayoutPanel1.SuspendLayout();
                flowLayoutPanel1.Controls.Clear();
                if (windDefinition != null)
                {
                    foreach (var quantity in windDefinition.WindFiles.Keys)
                    {
                        AddOpenFileControl(quantity, windDefinition.WindFiles[quantity].FilePathHandler.FilePath);
                    }

                    RefreshSpiderWebButton();
                }
                flowLayoutPanel1.ResumeLayout();
                groupBox1.PerformLayout();
            }
        }

        private void RefreshSpiderWebButton()
        {
            if (windDefinition.CanAddSpiderWeb)
            {
                addSpiderWebButton.Text = "Add spider web";
                addSpiderWebButton.Enabled = true;
            }
            else if (windDefinition.CanRemoveSpiderWeb)
            {
                addSpiderWebButton.Text = "Remove spider web";
                addSpiderWebButton.Enabled = true;
            }
            else
            {
                addSpiderWebButton.Text = "Add spider web";
                addSpiderWebButton.Enabled = false;                
            }
        }

        private void AddOpenFileControl(FileBasedWindDefinition.FileBasedWindQuantity quantity, string filePath)
        {
            var fileSelectionControl = new FileSelectionControl
                {
                    ShowFileNameOnly = false,
                    LabelText = quantity.GetDescription(),
                    FilePath = filePath,
                    FileFilter = string.Join("|", FileBasedWindDefinition.WindQuantityFileExtensions[quantity]),
                    AfterFileSelected =
                        (s, i) =>
                        SetFilePath(quantity, s, FileBasedWindDefinition.WindQuantityFileExtensions[quantity][i])
                };
            flowLayoutPanel1.Controls.Add(fileSelectionControl);
        }

        private void SetFilePath(FileBasedWindDefinition.FileBasedWindQuantity quantity, string filePath, string fileFilter)
        {
            if (windDefinition != null && File.Exists(filePath))
            {
                windDefinition.AddQuantityKey(quantity, filePath, fileFilter);
            }
        }

        static void TypeComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            e.Value =
                ((WindDefinitionType) e.ListItem).GetDescription();
        }

        #region IView

        public object Data
        {
            get { return windDefinition; }
            set
            {
                windDefinition = value as FileBasedWindDefinition;

                if (windDefinition != null)
                {
                    TypeComboBox.SelectedItem = windDefinition.Type;
                }

                RefreshFlowLayoutPanel();
            }
        }

        public Image Image { get; set; }
        
        public void EnsureVisible(object item){}

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void SpiderWebButtonClick(object sender, EventArgs e)
        {
            if (windDefinition != null)
            {
                if (windDefinition.CanAddSpiderWeb)
                {
                    windDefinition.AddSpiderWeb();
                }
                else if (windDefinition.CanRemoveSpiderWeb)
                {
                    windDefinition.RemoveSpiderWeb();
                }
            }
            RefreshFlowLayoutPanel();
        }
    }
}
