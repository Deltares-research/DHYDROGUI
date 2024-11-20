using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public class WaveDataAccessListener : IDataAccessListener
    {
        public void SetProjectRepository(IProjectRepository repository)
        {
        }

        public IDataAccessListener Clone()
        {
            return new WaveDataAccessListener();
        }

        public void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
        }

        public void OnPostLoad(object entity, object[] state, string[] propertyNames)
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
        }

        public bool OnPreUpdate(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public bool OnPreInsert(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public void OnPostUpdate(object entity, object[] state, string[] propertyNames)
        {
        }

        public void OnPostInsert(object entity, object[] state, string[] propertyNames)
        {
        }

        public bool OnPreDelete(object entity, object[] deletedState, string[] propertyNames)
        {
            return false;
        }

        public void OnPostDelete(object entity, object[] deletedState, string[] propertyNames)
        {
        }
    }
}