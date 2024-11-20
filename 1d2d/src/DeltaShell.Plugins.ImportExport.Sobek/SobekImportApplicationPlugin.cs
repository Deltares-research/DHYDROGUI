using System.Collections.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Plugin that provides functionality to import from sobek legacy format
    /// </summary>
    [Extension(typeof(IPlugin))]
    public class SobekImportApplicationPlugin : ApplicationPlugin
    {
        internal const string Sobek2ImportTemplateId = "Sobek2ImportTemplate";
        private IApplication application;

        static SobekImportApplicationPlugin()
        {
            Sobek2ModelImporters.RegisterSobek2Importer(() => new SobekModelToRainfallRunoffModelImporter());
        }
        public override string Name
        {
            get { return "Sobek Network import"; }
        }

        public override string DisplayName
        {
            get { return "SOBEK Network Import Plugin"; }
        }

        public override string Description
        {
            get { return "Plugin that provides functionality to import from the SOBEK2 Network format."; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion => "3.5.0.0";

        public override IEnumerable<ProjectTemplate> ProjectTemplates()
        {
            yield return new ProjectTemplate
            {
                Name = "Sobek 2 import",
                Category = ProductCategories.ImportTemplateCategory,
                Description = "Generate a model from an existing Sobek 2 model",
                Id = Sobek2ImportTemplateId,
                ExecuteTemplateOpenView = (project, m) =>
                {
                    project.RootFolder.Add(m);
                    return m;
                }
            };
        }

        public override IApplication Application
        {
            get
            {
                return application;
            }
            set
            {
                if (application != null)
                {
                    application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                }

                application = value;
                
                if (application != null)
                {
                    application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
                }
            }
        }

        private static void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            if (e.NewStatus != ActivityStatus.Cleaning 
                || !(sender is FileImportActivity fileImportActivity) 
                || !(fileImportActivity.FileImporter is IPartialSobekImporter partialSobekImporter))
            {
                return;
            }

            // prevent memory leaks - importer is static
            partialSobekImporter.TargetObject = null;
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new SobekHydroModelImporter();
            yield return new SobekModelToWaterFlowFMImporter();
            yield return new SobekNetworkImporter();
            yield return new SobekNetworkToNetworkImporter();
            yield return new SobekModelToRainfallRunoffModelImporter();
        }
    }
}