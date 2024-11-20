using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.Migrations;
using DeltaShell.Plugins.FMSuite.Wave.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Extension(typeof(IPlugin))]
    public class WaveApplicationPlugin : ApplicationPlugin
    {
        public override string Name => "Delft3D Wave";

        public override string DisplayName => "D-Waves Plugin";

        public override string Description => "A 2D/3D Wave module";

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "1.3.0.0";

        public override IApplication Application
        {
            get => base.Application;
            set
            {
                if (Application != null)
                {
                    Application.ProjectService.ProjectOpened -= Application_ProjectOpened;
                    Application.ProjectService.ProjectCreated -= Application_ProjectOpened;
                    Application.ProjectService.ProjectOpening -= Application_ProjectOpening;
                    Application.ProjectService.ProjectClosing -= Application_ProjectClosing;
                }

                base.Application = value;

                if (Application != null)
                {
                    Application.ProjectService.ProjectOpened += Application_ProjectOpened;
                    Application.ProjectService.ProjectCreated += Application_ProjectOpened;
                    Application.ProjectService.ProjectOpening += Application_ProjectOpening;
                    Application.ProjectService.ProjectClosing += Application_ProjectClosing;
                }
            }
        }

        public IEnumerable<WaveModel> GetModels()
        {
            return Application.ProjectService.Project.RootFolder.GetAllItemsRecursive().OfType<WaveModel>();
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
            {
                Name = "Waves Model",
                Category = "1D / 2D / 3D Standalone Models",
                Image = Properties.Resources.wave,
                GetParentProjectItem = owner =>
                {
                    Folder rootFolder = Application?.ProjectService.Project?.RootFolder;
                    return ApplicationPluginHelper.FindParentProjectItemInsideProject(rootFolder, owner) ?? rootFolder;
                },
                AdditionalOwnerCheck = owner =>
                    !(owner is ICompositeActivity) // Allow "standalone" wave models
                    || !((ICompositeActivity) owner).Activities.OfType<WaveModel>().Any() &&
                    owner is IHydroModel, // Don't allow multiple wave models in one composite activity
                CreateModel = t => new WaveModel {WorkingDirectoryPathFunc = () => Application.WorkDirectory}
            };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new WaveModelFileImporter(() => Application.WorkDirectory);
            yield return new WaveGridFileImporter(Name, GetModels);
            yield return new WaveDepthFileImporter(Name, GetModels);
            yield return new WaveBoundaryFileImporter();
            yield return new WavmFileImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaveModelFileExporter();
            yield return new Delft3DDepthFileExporter();
            yield return new Delft3DGridFileExporter();
        }

        private void Application_ProjectOpening(object sender, EventArgs<string> e)
        {
            string projectFilePath = e.Value;

            Version persistedPluginVersion = GetPersistedPluginVersion(projectFilePath);
            WavesMigrator.Migrate(projectFilePath, persistedPluginVersion, System.Version.Parse(FileFormatVersion));
        }

        // TODO this is not a nice solution.
        private Version GetPersistedPluginVersion(string projectFilePath)
        {
            IDictionary<string, Version> pluginFileFormatVersions = Application.ProjectService.GetProjectFileInfo(projectFilePath).PluginFileFormatVersions;
            return pluginFileFormatVersions.TryGetValue(Name, out Version version)
                       ? version
                       : pluginFileFormatVersions["D-Waves domain persistence plugin"];
        }

        private void Application_ProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaveModel>().ForEach(m =>
            {
                m.WorkingDirectoryPathFunc = () => Application.WorkDirectory;
                m.DimrRunner.FileExportService = Application.FileExportService;
            });
            
            project.CollectionChanging += OnProjectCollectionChanging;
        }
        
        private void Application_ProjectClosing(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            project.CollectionChanging -= OnProjectCollectionChanging;
        }
        
        private void OnProjectCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add && e.Item is WaveModel model)
            {
                model.DimrRunner.FileExportService = Application.FileExportService;
            }
        }

        /// <inheritdoc/>
        public override void AddRegistrations(IDependencyInjectionContainer container)
        {
            container.Register<IDataAccessListenersProvider, WaveDataAccessListenersProvider>(LifeCycle.Transient);
        }
    }
}