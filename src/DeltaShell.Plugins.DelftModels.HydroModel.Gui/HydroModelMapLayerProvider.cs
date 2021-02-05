using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui
{
    public class HydroModelMapLayerProvider : IMapLayerProvider
    {
        public ILayer CreateLayer(object data, object parentData)
        {
            var model = data as HydroModel;
            if (model != null)
            {
                return new GroupLayer(model.Name)
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                };
            }

            var modelFolder = data as ModelFolder;
            if (modelFolder != null && modelFolder.Model is HydroModel)
            {
                return new GroupLayer("Output")
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                };
            }

            return null;
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is HydroModel ||
                   (data is ModelFolder folder && folder.Model is HydroModel);
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var hydroModel = data as HydroModel;
            if (hydroModel != null)
            {
                yield return hydroModel.Region;

                foreach (var activity in hydroModel.Activities)
                {
                    yield return activity;
                }

                var hydroModelWorkFlows = hydroModel.CurrentWorkflow.GetActivitiesOfType<IHydroModelWorkFlow>();
                foreach (var hydroModelWorkFlow in hydroModelWorkFlows)
                {
                    yield return hydroModelWorkFlow;
                    if (hydroModelWorkFlow.Data.OutputDataItems.Select(di => di.Value).OfType<IFeatureCoverage>().Any())
                        yield return new ModelFolder {Model = hydroModel, Role = DataItemRole.Output};
                }
            }

            var modelFolder = data as ModelFolder;
            if (modelFolder != null && modelFolder.Model is HydroModel)
            {
                var folderHydroModel = (HydroModel) modelFolder.Model;

                var modelWorkFlows = folderHydroModel.CurrentWorkflow.GetActivitiesOfType<IHydroModelWorkFlow>().Where(wf => wf != null && wf.Data != null).ToList();
                foreach (var modelWorkFlow in modelWorkFlows)
                {
                    foreach (var coverage in modelWorkFlow.Data.OutputDataItems.Select(di => di.Value).OfType<IFeatureCoverage>())
                    {
                        yield return coverage;
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