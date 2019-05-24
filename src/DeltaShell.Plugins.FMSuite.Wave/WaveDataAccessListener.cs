using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public class WaveDataAccessListener : DataAccessListenerBase
    {
        public override object Clone()
        {
            return new WaveDataAccessListener();
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var model = entity as WaveModel;
            if (model != null)
            {
                foreach (WaveDomainData domain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
                {
                    IDataItem bathyDataItem = model.DataItems.FirstOrDefault(di => Equals(di.Value, domain.Bathymetry));
                    if (bathyDataItem != null)
                    {
                        // update intermediate results in the spatial operation set:
                        var sovc = bathyDataItem.ValueConverter as CoverageSpatialOperationValueConverter;
                        if (sovc != null)
                        {
                            sovc.SpatialOperationSet.Execute();
                        }
                    }
                }

                foreach (WavmFileFunctionStore wavmFileFunctionStore in model.WavmFunctionStores)
                {
                    wavmFileFunctionStore.Path = Path.Combine(Path.GetDirectoryName(model.MdwFilePath),
                                                              Path.GetFileName(wavmFileFunctionStore.Path));
                }
            }

            base.OnPostLoad(entity, state, propertyNames);
        }
    }
}