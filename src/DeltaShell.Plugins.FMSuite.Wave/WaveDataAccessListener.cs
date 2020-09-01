
using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
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
                foreach (IWaveDomainData domain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
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
            }

            base.OnPostLoad(entity, state, propertyNames);
        }
    }
}