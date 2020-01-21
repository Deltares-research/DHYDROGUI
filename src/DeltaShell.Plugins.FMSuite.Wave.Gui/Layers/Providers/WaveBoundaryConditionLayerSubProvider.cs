using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    // TODO: remove this once old boundaries are retired.
    public class WaveBoundaryConditionLayerSubProvider : ILayerSubProvider
    {
        private static readonly string modelName = typeof(WaveModel).Name;

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEventedList<WaveBoundaryCondition> &&
                   parentData is IWaveModel;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            if (!(sourceData is IEventedList<WaveBoundaryCondition> boundaryConditions &&
                  parentData is IWaveModel model))
            {
                return null;
            }

            return new VectorLayer(WaveLayerNames.BoundaryConditionLayerName)
            {
                DataSource = new Feature2DCollection().Init(boundaryConditions, "BoundaryCondition", modelName,
                                                            model.CoordinateSystem),
                Style = new VectorStyle
                {
                    Symbol = WaveLayerIcons.CoordinateBasedBoundary,
                    GeometryType = typeof(IPoint)
                },
                NameIsReadOnly = true
            };
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}