using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows;
using DelftTools.Controls;

namespace DeltaShell.Plugins.ImportExport.GWSW.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GwswImportDialog : Window, IDialog, IView
    {
        public GwswImportDialog()
        {
            InitializeComponent();

            GwswImportControl.CloseAction = result =>
            {
                DialogResult = result;
                Close();
            };
        }

        public object Data
        {
            get { return GwswImportControl.Importer; }
            set { GwswImportControl.Importer = (GwswFileImporter) value; }
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        [ExcludeFromCodeCoverage]
        public DelftDialogResult ShowModal()
        {
            return ShowModal(null);
        }

        [ExcludeFromCodeCoverage]
        public DelftDialogResult ShowModal(object owner)
        {
            ShowDialog();
            return DialogResult.HasValue && DialogResult.Value
                       ? DelftDialogResult.OK
                       : DelftDialogResult.Cancel;
        }
        
        public void EnsureVisible(object item)
        {
            // no element to focus
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Image?.Dispose();
            }
        }
    }
}
