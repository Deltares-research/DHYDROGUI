using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views;
using Image = System.Drawing.Image;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Interaction logic for GwswImportDialog.xaml
    /// </summary>
    public partial class GwswImportDialog : IView, IDialog
    {
        public GwswImportDialog()
        {
            InitializeComponent();
            if (ViewModel.CloseAction == null)
                ViewModel.CloseAction = result =>
                {
                    DialogResult = result;
                    Close();
                };
        }

        public object Data
        {
            get { return ViewModel.Importer; }
            set
            {
                ViewModel.Importer = (GwswFileImporter) value;
                ViewModel.MessageAction = ShowMessageDialog;
                ViewModel.GetDelimeter = GetDelimeter;
            }
        }

        public string Text { get; set; }
        public Image Image { get; set; }
        public void EnsureVisible(object item)
        {

        }

        public bool Visible { get { return true; } }
        public ViewInfo ViewInfo { get; set; }
        public void Dispose()
        {
            
        }

        public DelftDialogResult ShowModal()
        {
            return ShowModal(null);
        }

        public DelftDialogResult ShowModal(object owner)
        {
            ShowDialog();
            return DialogResult.HasValue && DialogResult.Value 
                ? DelftDialogResult.OK 
                : DelftDialogResult.Cancel;
        }

        private void Click_LoadDefinitionFile(object sender, RoutedEventArgs e)
        {
            var selectedFile = BrowseFiles();
            if (!String.IsNullOrEmpty(selectedFile)
                && ViewModel.GwswFeatureFiles != null && ViewModel.GwswFeatureFiles.Any())
            {
                var message =
                    "Importing a new Definition File will remove the existent Feature Files from the list. \nDo you wish to continue with this action?";
                var proceed = ShowMessageDialog("Feature Files to be overwritten", message, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                ViewModel.OverwriteGwswFeatureFiles = proceed;
                if (!proceed) return;
            }

            ViewModel.SelectedDefinitionFilePath = selectedFile;
        }

        private void Click_AddFeatureFile(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedFeatureFilePath = BrowseFiles();
        }

        private char GetDelimeter(char delimeter)
        {
            var selector = new DelimeterSelector();
            selector.Data = delimeter;

            var value = selector.ShowModal(this);
            if (value == DelftDialogResult.OK)
                return (char)selector.Data;

            return delimeter;
        }

        private string BrowseFiles()
        {
            var dialog = new OpenFileDialog { Filter = ViewModel.Importer.FileFilter };
            var result = dialog.ShowDialog();
            return result != System.Windows.Forms.DialogResult.OK ? null : dialog.FileName;
        }

        private bool ShowMessageDialog(string title, string message, MessageBoxButtons button, MessageBoxIcon icon)
        {
            var result = MessageBox.Show(message, title, button, icon);
            return result == System.Windows.Forms.DialogResult.OK ||
                   result == System.Windows.Forms.DialogResult.Yes;
        }
    }
}
