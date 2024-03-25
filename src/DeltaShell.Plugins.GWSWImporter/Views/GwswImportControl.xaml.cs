using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DeltaShell.Plugins.FMSuite.FlowFM;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.ImportExport.GWSW.Views
{
    /// <summary>
    /// Interaction logic for GwswImportDialog.xaml
    /// </summary>
    public partial class GwswImportControl
    {
        public GwswImportControl()
        {
            InitializeComponent();

            ViewModel.ShowInformationMessage = (title, message) =>
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        public GwswFileImporter Importer
        {
            get { return ViewModel.Importer; }
            set { ViewModel.Importer = value; }
        }

        public IWaterFlowFMModel Model
        {
            get { return ViewModel.Model; }
            set { ViewModel.Model = value; }
        }

        public Action<bool> CloseAction
        {
            get { return ViewModel.CloseAction; }
            set { ViewModel.CloseAction = value; }
        }

        private void Click_SelectDirectory(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedDirectoryPath = GetSelectedDirectory();
        }

        private void Click_AddFeatureFile(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedFeatureFilePath = BrowseFiles();
        }

        [ExcludeFromCodeCoverage]
        private string GetSelectedDirectory()
        {
            var folderDialogService = new FolderDialogService();
            var folderDialogOptions = new FolderDialogOptions
            {
                InitialDirectory = Properties.Settings.Default.Last_GwswImport_FolderPath
            };

            return folderDialogService.ShowSelectFolderDialog(folderDialogOptions);
        }

        [ExcludeFromCodeCoverage]
        private string BrowseFiles()
        {
            var dialog = new OpenFileDialog { Filter = ViewModel.Importer.FileFilter };
            return dialog.ShowDialog() != DialogResult.OK ? null : dialog.FileName;
        }

        private void GwswImportControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.OnDirectorySelected?.Execute(null);
        }
    }
}
