using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    [Extension(typeof(IPlugin))]
    public class SobekImportGuiPlugin : GuiPlugin
    {
        public override string Name
        {
            get { return "Sobek Network import (UI)"; }
        }

        public override string DisplayName
        {
            get { return "SOBEK Network Import Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Plugin that provides functionality to import from sobek network legacy format"; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.0.0"; }
        }

        public override IGraphicsProvider GraphicsProvider { get; } = new SobekImportGraphicsProvider();

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<ProjectTemplate, SobekImportWizardControl>
            {
                AdditionalDataCheck = t => t.Id == SobekImportApplicationPlugin.Sobek2ImportTemplateId,
                AfterCreate = (v, t) =>
                {
                    v.Application = Gui?.Application;
                }
            };

            yield return new ViewInfo<SobekHydroModelImporter, ImportSobekHydroModelWizardDialog>
            {
                GetViewName = (v, o) => v.Title,
                AfterCreate = (dialog, importer) =>
                {
                    importer.TargetObject = Gui.Selection as HydroModel;
                    importer.Application = Gui.Application;
                }
            };
            yield return new ViewInfo<SobekModelToWaterFlowFMImporter, ImportSobekWaterFlowFMWizardDialog>
            {
                GetViewName = (v, o) => v.Title,
                AfterCreate = (dialog, importer) => importer.TargetObject = Gui.Selection as WaterFlowFMModel
            };
            yield return new ViewInfo<SobekNetworkImporter, ImportPartialSobekWizardDialog>
            {
                GetViewName = (v, o) => v.Title,
                AfterCreate = (dialog, importer) => importer.TargetObject = Gui.Selection as HydroNetwork
            };
        }
    }
}