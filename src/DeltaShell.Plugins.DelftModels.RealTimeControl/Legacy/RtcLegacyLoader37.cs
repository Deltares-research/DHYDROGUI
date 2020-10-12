using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy
{
    /// <summary>
    /// Legacy loader for <see cref="RealTimeControlApplicationPlugin"/> version 3.7.0.
    /// </summary>
    public class RtcLegacyLoader37 : LegacyLoader
    {
        /// <summary>
        /// Called after the project migrated.
        /// Set the Path property of <see cref="RealTimeControlModel"/> and switched 
        /// to this path, since it was missing in database.
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

        private static void MigrateModel(RealTimeControlModel model)
        {
            string rootPath = Path.GetDirectoryName(((IFileBased) model.Owner).Path);

            Ensure.NotNull(rootPath, nameof(rootPath));

            string className = Path.GetFileName(model.GetType().Name);
            string newPath = Path.Combine(rootPath, className + "-" + Guid.NewGuid());

            ((IFileBased) model).Path = newPath;
            ((IFileBased)model).SwitchTo(newPath);
        }

        private static IEnumerable<RealTimeControlModel> GetModels(Project project) => project.RootFolder.GetAllItemsRecursive().OfType<RealTimeControlModel>();
    }
}