using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Forms
{
    /// <summary>
    /// Hack..
    /// </summary>
    public class BoundaryConditionImportDialog : IDialog, IView
    {
        private OpenFileDialog openFileDialog;

        public BoundaryConditionImportDialog()
        {
            openFileDialog = new OpenFileDialog();
        }

        public bool Visible { get; private set; }

        public string Title { get; set; }

        public object Data { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public DelftDialogResult ShowModal()
        {
            var importer = (WaveSpectralFileImporter) Data;
            openFileDialog.Filter = importer.FileFilter;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }

            if (Path.GetExtension(openFileDialog.FileName) == ".sp2" &&
                MessageBox.Show("This will delete all currently defined boundary conditions in the model, continue?",
                                "Warning",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
            {
                return DelftDialogResult.Cancel;
            }

            importer.SelectedFilePath = Path.GetFullPath(openFileDialog.FileName);
            return DelftDialogResult.OK;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Dispose() {}
        public void EnsureVisible(object item) {}
    }
}