using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="LegacyLoader"/> for the <see cref="WaveModel"/> to migrate
    /// to the directory structure associated with file format version 1.2.0.0.
    /// </summary>
    /// <seealso cref="LegacyLoader" />
    public class WaveModel110LegacyLoader : LegacyLoader
    {
        public override void OnAfterProjectMigrated(Project project)
        {
            base.OnAfterProjectMigrated(project);

            IEnumerable<WaveModel> waveModels =
                project.RootFolder.GetAllItemsRecursive().OfType<WaveModel>();

            foreach (WaveModel waveModel in waveModels)
            {
                WaveDirectoryStructureMigrationHelper.Migrate(waveModel);
            }
        }
    }
}