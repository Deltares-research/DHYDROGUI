using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui
{
    public class RainfallRunoffMapLayerProvider : IMapLayerProvider
    {
        public ILayer CreateLayer(object data, object parentData)
        {
            var rainfallRunoffModel = data as RainfallRunoffModel;
            if (rainfallRunoffModel != null)
            {
                return new GroupLayer(rainfallRunoffModel.Name)
                    {
                        LayersReadOnly = true,
                        NameIsReadOnly = true
                    };
            }
            
            var modelfolder = data as ModelFolder;
            if (modelfolder != null && modelfolder.Model is RainfallRunoffModel)
            {
                return new GroupLayer(modelfolder.Role == DataItemRole.Input ? "Input" : "Output")
                    {
                        LayersReadOnly = true,
                        NameIsReadOnly = true
                    };
            }

            var rrFeatureCoverage = data as IFeatureCoverage;
            if (rrFeatureCoverage != null)
            {
                var coverageLayer = (FeatureCoverageLayer) SharpMapLayerFactory.CreateMapLayerForCoverage(rrFeatureCoverage, null);
                coverageLayer.Visible = false;
                coverageLayer.NameIsReadOnly = true;
                coverageLayer.AutoUpdateThemeOnDataSourceChanged = true;
                coverageLayer.Renderer.GeometryForFeatureDelegate = GetCustomRenderGeometryForSubCatchmentsCached;
                return coverageLayer;
            }

            return null;
        }

        private readonly IDictionary<IFeature, IGeometry> cachedGeometries = new Dictionary<IFeature, IGeometry>();

        private IGeometry GetCustomRenderGeometryForSubCatchmentsCached(IFeature feature)
        {
            var catchment = feature as Catchment;
            if (catchment == null)
                return feature.Geometry;

            IGeometry geometry;
            if (!cachedGeometries.TryGetValue(feature, out geometry))
            {
                geometry = GetCustomRenderGeometryForSubCatchments(catchment);
                cachedGeometries.Add(feature, geometry);
            }
            return geometry;
        }

        private static IGeometry GetCustomRenderGeometryForSubCatchments(Catchment catchment)
        {
            var basin = (IDrainageBasin) catchment.Region;
            
            foreach (var parentCatchment in basin.Catchments)
            {
                if (Equals(parentCatchment, catchment) || parentCatchment.SubCatchments.Contains(catchment))
                    return parentCatchment.Geometry;
            }
            throw new InvalidOperationException("Catchment not found");
        }

        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is RainfallRunoffModel ||
                   (data is ModelFolder && ((ModelFolder) data).Model is RainfallRunoffModel) ||
                   (data is IFeatureCoverage && parentObject is ModelFolder && ((ModelFolder) parentObject).Model is RainfallRunoffModel);
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var rainfallRunoffModel = data as RainfallRunoffModel;
            if (rainfallRunoffModel != null)
            {
                var basinDataItem = rainfallRunoffModel.DataItems.FirstOrDefault(di => di.Role == DataItemRole.Input && di.Value is IDrainageBasin);
                if (basinDataItem != null && basinDataItem.LinkedTo == null)
                {
                    yield return new ModelFolder { Model = rainfallRunoffModel, Role = DataItemRole.Input };
                }
                
                yield return new ModelFolder { Model = rainfallRunoffModel, Role = DataItemRole.Output };
            }

            var modelfolder = data as ModelFolder;
            if (modelfolder != null && modelfolder.Model is RainfallRunoffModel)
            {
                var runoffModel = (RainfallRunoffModel)modelfolder.Model;

                if (modelfolder.Role == DataItemRole.Input)
                {
                    yield return runoffModel.Basin;
                }

                if (modelfolder.Role == DataItemRole.Output)
                {
                    foreach (var outputCoverage in runoffModel.OutputCoverages)
                    {
                        yield return outputCoverage;
                    }
                }
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            // no actions needed
        }
    }
}