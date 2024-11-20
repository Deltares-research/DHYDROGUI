using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Common.Layers
{
    [Entity(FireOnCollectionChange = false)]
    public class ModelGroupLayer : GroupLayer
    {
        public IModel Model { get; set; }

        public void UpdateCoordinateSystem(ICoordinateSystem currentCS, ICoordinateSystem targetCS)
        {
            foreach (var layer in GetAllLayers(false))
            {
                if (layer.DataSource == null || layer.DataSource.CoordinateSystem != currentCS)
                {
                    continue;
                }
                layer.DataSource.CoordinateSystem = targetCS;
            }
        }

        public override void Dispose(bool disposeDataSource)
        {
            Model = null;
            base.Dispose(disposeDataSource);
        }
    }
}