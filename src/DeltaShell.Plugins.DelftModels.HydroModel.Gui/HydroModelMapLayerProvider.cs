using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.Styles;

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

            var coupler = data as Iterative1D2DCoupler;
            if (coupler != null)
            {
                var linkEndCap = new AdjustableArrowCap(4, 4, true) { BaseCap = LineCap.Triangle };

                return new VectorLayer("1D/2D links")
                    {
                        DataSource = new Iterative1D2DCouplerLinkFeatureCollection((IList)coupler.Features)
                            {
                                Coupler = coupler
                            },
                        CanBeRemovedByUser = false,
                        SmoothingMode = SmoothingMode.AntiAlias,
                        Opacity = 0.7f,
                        Style = new VectorStyle
                            {
                                Line = new Pen(Color.DarkViolet, 3)
                                    {
                                        CustomEndCap = linkEndCap,
                                        CustomStartCap = linkEndCap
                                    }
                            },
                        Selectable = true,
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

        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is HydroModel ||
                   data is Iterative1D2DCoupler ||
                   (data is ModelFolder && ((ModelFolder)data).Model is HydroModel);
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
    }
}