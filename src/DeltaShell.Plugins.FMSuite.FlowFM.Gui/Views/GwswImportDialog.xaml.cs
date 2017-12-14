using System.Windows;
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
                ViewModel.CloseAction = result =>
                {
                    DialogResult = result;
                    Close();
                };
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
    }
}
