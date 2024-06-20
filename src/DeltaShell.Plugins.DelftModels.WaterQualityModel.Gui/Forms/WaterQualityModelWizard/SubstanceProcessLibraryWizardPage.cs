using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Wpf.Services;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    public partial class SubstanceProcessLibraryWizardPage : UserControl, IWizardPage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SubstanceProcessLibraryWizardPage));

        public SubstanceProcessLibraryWizardPage(string dataDirectory = null)
        {
            InitializeComponent();

            splitContainer1.Panel1MinSize = 100;
            splitContainer1.Panel2MinSize = 250;

            if (dataDirectory == null)
            {
                dataDirectory = WaterQualityApiDataSet.WaqDataDirectory;
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

        public WaterQualityProcessType SubFileProcessType => tabControl1.SelectedTab == customProcessTypeTab ? 
                                                                 WaterQualityProcessType.Custom : 
                                                                 WaterQualityProcessType.Sobek;

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

            listView2.Items.Clear();
            listView2.Items.AddRange(sobekListItems.ToArray());

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
            catch (Exception e) when (e is UnauthorizedAccessException || e is SecurityException || e is IOException)
            {
                Log.WarnFormat(e.Message);
            }

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
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = "Process file|*.dll" };
            
            string filePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            if (filePath == null)
            {
                return;
            }

            textBoxProcessFile.Text = filePath;
        }

        private void ButtonSelectProcessDefinitionsFilePathClick(object sender, EventArgs e)
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = "Process definition files|*.def;*.dat" };
            
            string filePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            if (filePath == null)
            {
                return;
            }

            textBoxProcessDefinitionsFile.Text =
                Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(filePath); // Note: strip off the extension
        }

        private void StandardSubFileListViewResize(object sender, EventArgs e)
        {
            listViewHeaderName.Width = listView2.Width;

            Refresh();
        }

        # endregion
    }
}