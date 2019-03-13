using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.IO
{
    public static class FileBasedUtils
    {
        private const string InputDirectoryName = "input";
        private const string OutputDirectoryName = "output";

        public static void CleanPersistentDirectories(DirectoryInfo persistedDataDirectory, IHydroModel model)
        {
            if (!persistedDataDirectory.Exists) return;

            var compositeActivity = model as ICompositeActivity;
            if (compositeActivity != null)
            {
                var hydroModelDirectory = Path.Combine(persistedDataDirectory.FullName, compositeActivity.Name);
                var childModelNames = compositeActivity.Activities.OfType<IHydroModel>().Select(a => a.Name).ToList();

                CleanPersistentDirectoryForCompositeModel(new DirectoryInfo(hydroModelDirectory), childModelNames);
                
                foreach (var childModelName in childModelNames)
                {
                    var modelDirectoryInfo = new DirectoryInfo(Path.Combine(hydroModelDirectory, childModelName));
                    CleanPersistentDirectoryForStandAloneModel(modelDirectoryInfo);
                }
            }
            else
            {
                var modelDirectoryInfo = new DirectoryInfo(Path.Combine(persistedDataDirectory.FullName, model.Name));
                CleanPersistentDirectoryForStandAloneModel(modelDirectoryInfo);
            }
        }

        private static void CleanPersistentDirectoryForCompositeModel(DirectoryInfo compositeModelDirectoryInfo, IEnumerable<string> childModelNames)
        {
            if (!compositeModelDirectoryInfo.Exists) return;

            compositeModelDirectoryInfo.GetDirectories()
                .Where(d => !childModelNames.Contains(d.Name))
                .ForEach(d => FileUtils.DeleteIfExists(d.FullName));

            compositeModelDirectoryInfo.GetFiles().ForEach(f => FileUtils.DeleteIfExists(f.FullName));
        }

        private static void CleanPersistentDirectoryForStandAloneModel(DirectoryInfo modelDirectoryInfo)
        {
            if (!modelDirectoryInfo.Exists) return;

            modelDirectoryInfo.GetDirectories()
                .Where(d => d.Name != InputDirectoryName && d.Name != OutputDirectoryName)
                .ForEach(d => FileUtils.DeleteIfExists(d.FullName));

            modelDirectoryInfo.GetFiles().ForEach(f => FileUtils.DeleteIfExists(f.FullName));
        }
    }
}
