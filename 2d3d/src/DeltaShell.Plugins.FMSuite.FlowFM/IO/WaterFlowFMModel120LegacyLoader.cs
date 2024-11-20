using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Legacy loader for <see cref="FlowFMApplicationPlugin"/> version 1.2.0.
    /// </summary>
    /// <seealso cref="LegacyLoader"/>
    public class WaterFlowFMModel120LegacyLoader : LegacyLoader
    {
        private readonly LegacyLoader nextLegacyLoader = new WaterFlowFMModel130LegacyLoader();

        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            nextLegacyLoader.OnAfterInitialize(entity, dbConnection);
        }

        /// <summary>
        /// Called after the project migrated.
        /// Unpacks the state files, moves them to the correct location and cleans up the project folder.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="project"/> is <c>null</c>.
        /// </exception>
        public override void OnAfterProjectMigrated(Project project)
        {
            Ensure.NotNull(project, nameof(project));

            IEnumerable<WaterFlowFMModel> models = project.RootFolder.GetAllItemsRecursive().OfType<WaterFlowFMModel>();
            foreach (WaterFlowFMModel model in models)
            {
                var paths = new Paths(model);

                OrganizeRestartFiles(paths);
                model.ConnectOutput(paths.OutputDir);
            }

            nextLegacyLoader.OnAfterProjectMigrated(project);
        }

        private static void OrganizeRestartFiles(Paths paths)
        {
            Directory.CreateDirectory(paths.OutputDir);

            foreach (string stateFile in paths.StateFiles)
            {
                ZipFileUtils.Extract(stateFile, paths.OutputDir);
                File.Delete(stateFile);
            }

            File.Delete(paths.OutputMetaDataXml);
            File.Delete(paths.InputMetaDataXml);
            File.Delete(paths.RestartMeta);
            FileUtils.DeleteIfExists(paths.ExplicitWorkingDirectory);
        }

        private sealed class Paths
        {
            private const string metaData = "metadata.xml";
            private const string restartMeta = "restart.meta";

            private readonly Regex stateFileRegex;
            private readonly WaterFlowFMModel model;

            public Paths(WaterFlowFMModel model)
            {
                this.model = model;
                stateFileRegex = new Regex($@"state_{model.Name}_.*.zip$");
            }

            public string ExplicitWorkingDirectory => ModelDir + "_output";

            public string OutputDir => model.GetModelOutputDirectory();

            public IEnumerable<string> StateFiles => SearchStateFiles(ProjectDir);

            public string OutputMetaDataXml => Path.Combine(OutputDir, metaData);

            public string InputMetaDataXml => Path.Combine(InputDir, metaData);

            public string RestartMeta => Path.Combine(InputDir, restartMeta);

            private string InputDir => model.GetMduDirectory();

            private string ProjectDir => Path.GetDirectoryName(ModelDir);

            private string ModelDir => model.GetModelDirectory();

            private IEnumerable<string> SearchStateFiles(string dir)
            {
                return Directory.EnumerateFiles(dir)
                                .Where(f => stateFileRegex.IsMatch(Path.GetFileName(f)));
            }
        }
    }
}