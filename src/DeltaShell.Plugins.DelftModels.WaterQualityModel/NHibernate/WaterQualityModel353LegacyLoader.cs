using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate
{
    /// <summary>
    /// Legacy loader for <see cref="WaterQualityModelApplicationPlugin"/> version 3.5.3.
    /// </summary>
    public class WaterQualityModel353LegacyLoader : LegacyLoader
    {
        /// <summary>
        /// Called after the project migrated.
        /// Removes the former explicit working directory (_output) of the D-Water Quality model.
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

        private static void MigrateModel(WaterQualityModel model)
        {
            string projectDir = Path.GetDirectoryName(model.ModelDataDirectory);
            string explicitWorkingDirName = model.Name.Replace(" ", "_") + "_output";
            FileUtils.DeleteIfExists(Path.Combine(projectDir, explicitWorkingDirName));
        }

        private static IEnumerable<WaterQualityModel> GetModels(Project project) => project.RootFolder.Models.OfType<WaterQualityModel>();
    }
}