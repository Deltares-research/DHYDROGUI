using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Controls;
using Image = System.Drawing.Image;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Interaction logic for GwswImportDialog.xaml
    /// </summary>
    public partial class GwswImportDialog : Window, IView, IDialog
    {
        public GwswImportDialog()
        {
            InitializeComponent();
            if (ViewModel.CloseAction == null)
                ViewModel.CloseAction = new Action(() => this.Close());
        }

        public object Data
        {
            get { return ViewModel.Importer; }
            set { ViewModel.Importer = (GwswFileImporter) value; }
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

        private void FeatureList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedItems = FeatureList.SelectedItems
                .Cast<KeyValuePair<string, string>>()
                .ToList();
        }

        private void CancelImport_OnClick(object sender, RoutedEventArgs e)
        {
            if(DialogResult != null)
                DialogResult = false;
            Close();
        }

        private void OkImport_OnClick(object sender, RoutedEventArgs e)
        {
            if (DialogResult != null)
                DialogResult = true;
            Close();
        }
    }
}
