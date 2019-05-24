using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    public partial class SubstanceProcessLibraryWizardPage : UserControl, IWizardPage
    {
        public SubstanceProcessLibraryWizardPage(string dataDirectory = null)
        {
            InitializeComponent();

            splitContainer1.Panel1MinSize = 100;
            splitContainer1.Panel2MinSize = 250;

            if (dataDirectory == null)
            {
                dataDirectory = DelwaqFileStructureHelper.GetDelwaqDataFolderPath();
            }

            InitializeControls(dataDirectory);

            toolTipProcessFilePath.SetToolTip(pictureBoxProcessFilePath,
                                              Resources.SubstanceProcessLibraryWizardPage_Dll_file_path_tooltip);
            toolTipProcessDefinitionsFilePath.SetToolTip(pictureBoxProcessDefinitionsFilePath,
                                                         Resources
                                                             .SubstanceProcessLibraryWizardPage_Process_definition_files_path_tooltip);
        }

        # region IWizardPage

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            return true;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        # endregion

        # region Obtaining data

        public string SubFilePath => UsingStandardSubFile ? GetStandardSubFile() : GetCustomFile();

        public WaterQualityProcessType SubFileProcessType
        {
            get
            {
                if (tabControl1.SelectedTab == customProcessTypeTab)
                {
                    return WaterQualityProcessType.Custom;
                }

                ListViewItem selectedListViewItem = listView2.SelectedItems
                                                             .OfType<ListViewItem>()
                                                             .FirstOrDefault();

                return selectedListViewItem != null && selectedListViewItem.Group == listView2.Groups[1]
                           ? WaterQualityProcessType.Duflow
                           : WaterQualityProcessType.Sobek;
            }
        }

        public bool UsingCustomProcessFiles => !UsingStandardSubFile && radioButtonCustomProcesses.Checked;

        public string CustomProcessDllFilePath => textBoxProcessFile.Text;

        public string CustomProcessDefinitionFilesPath => textBoxProcessDefinitionsFile.Text;

        private bool UsingStandardSubFile => tabControl1.SelectedTab == standardProcessTypeTab;

        private string GetStandardSubFile()
        {
            FileInfo selectedItem = listView2.SelectedItems
                                             .Cast<ListViewItem>()
                                             .Select(lvi => lvi.Tag)
                                             .OfType<FileInfo>()
                                             .FirstOrDefault();

            return selectedItem != null ? selectedItem.FullName : "";
        }

        private string GetCustomFile()
        {
            return customLibraryPanel.FileName ?? "";
        }

        # endregion

        # region Initialization/updating logic

        private void InitializeControls(string dataDirectory)
        {
            IEnumerable<ListViewItem> sobekListItems =
                CreateListViewItems(dataDirectory + "\\Sobek", listView2.Groups[0]);
            IEnumerable<ListViewItem> duflowListItems =
                CreateListViewItems(dataDirectory + "\\Duflow", listView2.Groups[1]);

            listView2.Items.Clear();
            listView2.Items.AddRange(sobekListItems.Concat(duflowListItems).ToArray());

            UpdateProcessTypeControls();
        }

        private static IEnumerable<ListViewItem> CreateListViewItems(string subFilePath, ListViewGroup group)
        {
            Dictionary<string, FileInfo> subFileDictionary;
            Dictionary<string, string> descriptionDictionary;

            if (Directory.Exists(subFilePath))
            {
                subFileDictionary = new DirectoryInfo(subFilePath)
                                    .GetFiles("*.sub", SearchOption.AllDirectories)
                                    .ToDictionary(f => Path.GetFileNameWithoutExtension(f.FullName), f => f);

                descriptionDictionary = new DirectoryInfo(subFilePath)
                                        .GetFiles("*.des", SearchOption.AllDirectories)
                                        .ToDictionary(f => Path.GetFileNameWithoutExtension(f.FullName),
                                                      ReadDescriptionFileLine);
            }
            else
            {
                subFileDictionary = new Dictionary<string, FileInfo>();
                descriptionDictionary = new Dictionary<string, string>();
            }

            return subFileDictionary.Select(kvp => new ListViewItem
            {
                ImageIndex = 0,
                Text = GetDescriptionText(descriptionDictionary, kvp.Key),
                Tag = kvp.Value,
                Group = @group,
                ToolTipText = GetDescriptionText(descriptionDictionary, kvp.Key)
            });
        }

        private static string ReadDescriptionFileLine(FileInfo f)
        {
            var line = "";

            try
            {
                line = f.OpenText().ReadLine();
            }
            catch {}

            return line;
        }

        private static string GetDescriptionText(IDictionary<string, string> descriptionDictionary, string filename)
        {
            return descriptionDictionary.ContainsKey(filename) && !string.IsNullOrEmpty(descriptionDictionary[filename])
                       ? descriptionDictionary[filename]
                       : filename;
        }

        private void ImportSubFile()
        {
            var substanceProcessLibrary = new SubstanceProcessLibrary();

            try
            {
                string subFilePath = SubFilePath;

                if (!string.IsNullOrEmpty(subFilePath))
                {
                    new SubFileImporter().Import(substanceProcessLibrary, subFilePath);
                }
            }
            finally
            {
                substanceProcessLibraryView.Data = substanceProcessLibrary;
            }
        }

        private void UpdateProcessTypeControls()
        {
            bool customProcessFilesTabSelected = tabControl1.SelectedTab == customProcessTypeTab;

            bool usingCustomProcessFiles = customProcessFilesTabSelected && radioButtonCustomProcesses.Checked;

            processFileLabel.Enabled = usingCustomProcessFiles;
            textBoxProcessFile.Enabled = usingCustomProcessFiles;
            textBoxProcessFile.ReadOnly = !usingCustomProcessFiles;
            textBoxProcessFile.BackColor = usingCustomProcessFiles ? SystemColors.Window : SystemColors.Control;
            buttonSelectProcessFilePath.Enabled = usingCustomProcessFiles;

            processDefinitionFilesLabel.Enabled = usingCustomProcessFiles;
            textBoxProcessDefinitionsFile.Enabled = usingCustomProcessFiles;
            textBoxProcessDefinitionsFile.ReadOnly = !usingCustomProcessFiles;
            textBoxProcessDefinitionsFile.BackColor =
                usingCustomProcessFiles ? SystemColors.Window : SystemColors.Control;
            buttonSelectProcessDefinitionsFilePath.Enabled = usingCustomProcessFiles;
        }

        # endregion

        # region Event handling

        private void SubFileSelectionChanged(object sender, EventArgs e)
        {
            ImportSubFile();

            UpdateProcessTypeControls();
        }

        private void RadioButtonStandardSobekProcessesCheckedChanged(object sender, EventArgs e)
        {
            if (!radioButtonStandardSobekProcesses.Checked)
            {
                return;
            }

            UpdateProcessTypeControls();
        }

        private void RadioButtonCustomProcessesCheckedChanged(object sender, EventArgs e)
        {
            if (!radioButtonCustomProcesses.Checked)
            {
                return;
            }

            UpdateProcessTypeControls();
        }

        private void ButtonSelectProcessFilePathClick(object sender, EventArgs e)
        {
            if (openFileDialogProcessFilePath.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            textBoxProcessFile.Text = openFileDialogProcessFilePath.FileName;
        }

        private void ButtonSelectProcessDefinitionsFilePathClick(object sender, EventArgs e)
        {
            if (openFileDialogProcessDefinitionsFilePath.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            textBoxProcessDefinitionsFile.Text =
                Path.GetDirectoryName(openFileDialogProcessDefinitionsFilePath.FileName) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(openFileDialogProcessDefinitionsFilePath
                                                     .FileName); // Note: strip off the extension
        }

        private void StandardSubFileListViewResize(object sender, EventArgs e)
        {
            listViewHeaderName.Width = listView2.Width;

            Refresh();
        }

        # endregion
    }
}