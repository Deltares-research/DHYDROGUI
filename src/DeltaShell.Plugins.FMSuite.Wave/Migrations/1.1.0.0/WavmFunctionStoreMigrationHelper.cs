using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

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
        /// Removes the invalid wavm function stores from the provided
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <param name="logHandler">The log handler to log any unlinked function stores with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static void RemoveWavmFunctionStores(WaveModel waveModel, ILogHandler logHandler)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            IEnumerable<IDataItem> wavmDataItems = GetWavmFunctionStoreDataItems(waveModel);
            foreach (IDataItem dataItem in wavmDataItems)
            {
                var wavmFunctionStore = (WavmFileFunctionStore) dataItem.Value;

                logHandler?.ReportWarningFormat(Resources.WavmFunctionStoreMigrationHelper_RemoveWavmFunctionStores_The_link_with__0__has_been_broken_,
                                                wavmFunctionStore.Name);

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