using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using GeoAPI.Extensions.Coverages;
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
                return coverageLayer;
            }

            return null;
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is RainfallRunoffModel ||
                   (data is ModelFolder folder && folder.Model is RainfallRunoffModel) ||
                   (data is IFeatureCoverage && parentData is ModelFolder modelFolder && modelFolder.Model is RainfallRunoffModel);
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var rainfallRunoffModel = data as RainfallRunoffModel;
            if (rainfallRunoffModel != null)
            {
                yield return rainfallRunoffModel.Basin;
                yield return new ModelFolder { Model = rainfallRunoffModel, Role = DataItemRole.Output };
            }

            var modelfolder = data as ModelFolder;
            if (modelfolder != null && modelfolder.Model is RainfallRunoffModel)
            {
                var runoffModel = (RainfallRunoffModel)modelfolder.Model;

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