using System;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.ImportExport.GWSW.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GwswImportTemplateView : IProjectTemplateSettingsView
    {
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

        public void Dispose()
        {
            // nothing to dispose
        }

        public void EnsureVisible(object item)
        {
            // no element to focus
        }
    }
}
