using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public class RtcExporterDialog : Form, IConfigureDialog, IView
    {
        public string Title { get; set; }

        public DelftDialogResult ShowModal()
        {
            var browserDialog = new FolderBrowserDialog {ShowNewFolderButton = true};
            if (browserDialog.ShowDialog() != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }
            Directory = browserDialog.SelectedPath;
            return DelftDialogResult.OK;
        }

        private string Directory { get; set; }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object item)
        {
            if (item is RealTimeControlModelExporter exporter)
            {
                exporter.Directory = Directory;
            }
        }

        public object Data { get; set; }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }
    }
}
