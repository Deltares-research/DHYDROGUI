using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
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
            var folderDialogService = new FolderDialogService();
            var folderDialogOptions = new FolderDialogOptions();

            string selectedPath = folderDialogService.ShowSelectFolderDialog(folderDialogOptions);
            
            if (string.IsNullOrEmpty(selectedPath))
            {
                return DelftDialogResult.Cancel;
            }

            Directory = selectedPath;
            return DelftDialogResult.OK;
        }

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

        public void EnsureVisible(object item)
        {
            // Nothing to be done.
        }

        private string Directory { get; set; }
    }
}