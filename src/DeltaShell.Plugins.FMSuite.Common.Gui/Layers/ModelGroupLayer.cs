using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Layers
{
    [Entity(FireOnCollectionChange = false)]
    public class ModelGroupLayer : GroupLayer
    {
        public IModel Model { get; set; }

        public void UpdateCoordinateSystem(ICoordinateSystem currentCS, ICoordinateSystem targetCS)
        {
            foreach (ILayer layer in GetAllLayers(false))
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