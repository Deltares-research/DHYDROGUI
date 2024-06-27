using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Legacy loader for <see cref="HydroModelApplicationPlugin"/> version 1.1.1.
    /// </summary>
    public class HydroModel111LegacyLoader : LegacyLoader
    {
        /// <summary>
        /// Called after the project migrated.
        /// Removes the former explicit working directory (_output) of the integrated model.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="project"/> is <c>null</c>.
        /// </exception>
        public override void OnAfterProjectMigrated(Project project)
        {
            Ensure.NotNull(project, nameof(project));

            GetModels(project).ForEach(MigrateModel);

            base.OnAfterProjectMigrated(project);
        }

        private static IEnumerable<HydroModel> GetModels(Project project) => project.RootFolder.Models.OfType<HydroModel>();

        private static void MigrateModel(HydroModel hydroModel)
        {
            string projectDir = Path.GetDirectoryName(((IFileBased) hydroModel).Path);
            string explicitWorkingDirName = hydroModel.Name.Replace(" ", "_") + "_output";
            FileUtils.DeleteIfExists(Path.Combine(projectDir, explicitWorkingDirName));
        }
    }
}