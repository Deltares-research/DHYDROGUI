using System.Collections.Generic;
using System.Diagnostics;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.GWSW.Views;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    [Extension(typeof(IPlugin))]
    public class GWSWImporterGuiPlugin : GuiPlugin
    {
        public override string Name
        {
            get { return "GWSWImporterUI"; }
        }
        public override string DisplayName
        {
            get { return "GWSW Importer (UI)"; }
        }
        public override string Description
        {
            get { return "Gui plugin of the GWSW Importer"; }
        }
        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }
        public override string FileFormatVersion
        {
            get { return FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion; }
        }

        public override IGraphicsProvider GraphicsProvider { get; } = new GwswGraphicsProvider();

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<GwswFileImporter, GwswImportDialog>
            {
                AfterCreate = (v, i) =>
                {
                    v.GwswImportControl.Model = Gui.SelectedModel as IWaterFlowFMModel;
                }
            };

            yield return new ViewInfo<ProjectTemplate, GwswImportTemplateView>
            {
                AdditionalDataCheck = t => t.Id == GWSWImporterApplicationPlugin.GWSWImportTemplateId
            };
        }
    }
}