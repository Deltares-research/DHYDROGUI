using System;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public static class FeatureCollectionViewInfoHelper
    {
        public static ViewInfo<IEventedList<TFeature>, ILayer, VectorLayerAttributeTableView> CreateViewInfo<TFeature, TModel>(string name, Func<TModel, IEventedList<TFeature>> getCollection, GuiContainer guiContainer)
        {
            return new ViewInfo<IEventedList<TFeature>, ILayer, VectorLayerAttributeTableView>
                {
                    Description = name,
                    GetViewName = (v, o) => o.Name,
                    AdditionalDataCheck = o =>
                        {
                            var model = guiContainer.Gui.Application.GetAllModelsInProject().OfType<TModel>().FirstOrDefault(m => Equals(o, getCollection(m)));
                            return model != null;
                        },
                    GetViewData = o =>
                        {
                            var centralMap =
                                guiContainer.Gui
                                      .DocumentViews.OfType<ProjectItemMapView>()
                                      .FirstOrDefault(v => v.MapView.GetLayerForData(o) != null);
                            return centralMap == null ? null : centralMap.MapView.GetLayerForData(o);
                        },
                    CompositeViewType = typeof (ProjectItemMapView),
                    GetCompositeViewData = o => guiContainer.Gui.Application.GetAllModelsInProject().OfType<TModel>().FirstOrDefault(m => Equals(o, getCollection(m))),
                    AfterCreate = (v, o) =>
                        {
                            var centralMap =
                                guiContainer.Gui
                                      .DocumentViews.OfType<ProjectItemMapView>()
                                      .FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                            if (centralMap == null) return;

                            v.DeleteSelectedFeatures = () => centralMap.MapView.MapControl.DeleteTool.DeleteSelection();
                            v.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(feature);
                            v.DynamicAttributeVisible = s => s != Feature2D.LocationKey;
                            v.CanAddDeleteAttributes = false;
                            v.OpenViewMethod = ob => guiContainer.Gui.CommandHandler.OpenView(ob);
                        }
                };
        }

    }
}