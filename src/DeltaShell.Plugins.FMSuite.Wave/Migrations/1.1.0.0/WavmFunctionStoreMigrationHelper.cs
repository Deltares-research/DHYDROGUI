using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="WaveDirectoryStructureMigrationHelper"/> provides the methods
    /// for the wavm file function store migration associated with file format version
    /// 1.2.0.0.
    /// </summary>
    public static class WavmFunctionStoreMigrationHelper
    {
        /// <summary>
        /// Updates the paths of the <see cref="WavmFileFunctionStore"/> of the
        /// provided <paramref name="model"/> to <paramref name="modelPath"/> output.
        /// </summary>
        /// <param name="modelPath">The model path.</param>
        /// <param name="model">The model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static void UpdateWavmFileFunctionStorePaths(string modelPath, WaveModel model)
        {
            Ensure.NotNull(modelPath, nameof(modelPath));
            Ensure.NotNull(model, nameof(model));

            IEnumerable<WavmFileFunctionStore> functionStores = 
                GetWavmFunctionStoreDataItems(model).Select(x => (WavmFileFunctionStore) x.Value);

            // We assume at this point the wavm file function store exists at this path
            // If it does not exist after migration, we will remove the function store all
            // together.
            foreach (WavmFileFunctionStore functionStore in functionStores)
            {
                string wavmFileName = Path.GetFileName(functionStore.Path);
                string expectedMigratedWavmPath = Path.Combine(modelPath, "output", wavmFileName);
                functionStore.Path = expectedMigratedWavmPath;
            }
        }

        /// <summary>
        /// Removes the invalid wavm function stores from the provided
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static void RemoveInvalidWavmFunctionStores(WaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            IEnumerable<IDataItem> wavmDataItems = GetWavmFunctionStoreDataItems(waveModel);
            foreach (IDataItem dataItem in wavmDataItems)
            {
                var wavmFunctionStore = (WavmFileFunctionStore) dataItem.Value;

                if (File.Exists(wavmFunctionStore.Path))
                {
                    continue;
                }

                wavmFunctionStore?.Close();
                waveModel.DataItems.Remove(dataItem);
            }
        }

        private static IEnumerable<IDataItem> GetWavmFunctionStoreDataItems(WaveModel model) =>
            WaveDomainHelper.GetAllDomains(model.OuterDomain)
                            .Select(domain => model.GetDataItemByTag(WaveModel.WavmStoreDataItemTag + domain.Name))
                            .Where(di => di != null);
    }
}