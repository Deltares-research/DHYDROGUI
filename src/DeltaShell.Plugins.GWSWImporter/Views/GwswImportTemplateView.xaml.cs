using System;
using System.Drawing;
using DelftTools.Controls;

namespace DeltaShell.Plugins.ImportExport.GWSW.Views
{
    /// <summary>
    /// Interaction logic for GwswImportTemplateView.xaml
    /// </summary>
    public sealed partial class GwswImportTemplateView : IProjectTemplateSettingsView
    {
        private bool disposed;

        public GwswImportTemplateView()
        {
            InitializeComponent();

            GwswImportControl.Importer = new GwswFileImporter(new DefinitionsProvider());
            GwswImportControl.CloseAction = run =>
            {
                if (!run)
                {
                    Cancel();
                    return;
                }

                ExecuteProjectTemplate(GwswImportControl.Importer);
            };
        }

        public object Data { get; set; }

        public string Text { get; set; }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public Action<object> ExecuteProjectTemplate { get; set; }

        public Action Cancel { get; set; }


        public void EnsureVisible(object item)
        {
            // no element to focus
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Image?.Dispose();
            }

            disposed = true;
        }
    }
}
