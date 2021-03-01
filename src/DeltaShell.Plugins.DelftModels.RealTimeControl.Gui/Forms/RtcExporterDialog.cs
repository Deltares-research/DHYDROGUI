using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public class RtcExporterDialog : Form, IConfigureDialog, IView
    {
        public string Title { get; set; }

        public object Data { get; set; }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

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

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object item)
        {
            var exporter = item as RealTimeControlModelExporter;
            if (exporter != null)
            {
                exporter.Directory = Directory;
            }
        }

        public void EnsureVisible(object item) {}

        private string Directory { get; set; }
    }
}