using System;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class FeatureCollectionViewInfoHelper
    {
        public static ViewInfo<IEventedList<TFeature>, ILayer, VectorLayerAttributeTableView> CreateViewInfo<TFeature>(string name, Func<HydroArea, IEventedList<TFeature>> getCollection, Func<IGui> getGui)
        {
            return new ViewInfo<IEventedList<TFeature>, ILayer, VectorLayerAttributeTableView>
            {
                Description = name,
                GetViewName = (v, o) => o.Name,
                AdditionalDataCheck = o =>
                {
                    var lst = getGui().Application.Project.RootFolder.GetAllItemsRecursive().OfType<HydroArea>().
                        FirstOrDefault(hr => ReferenceEquals(getCollection(hr), o));
                    return lst != null;
                },
                GetViewData = o =>
                {
                    var centralMap =
                        getGui()
                            .DocumentViews.OfType<ProjectItemMapView>()
                            .FirstOrDefault(v => v.MapView.GetLayerForData(o) != null);
                    return centralMap == null ? null : centralMap.MapView.GetLayerForData(o);
                },
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData =
                    o => getGui().Application.Project.RootFolder.GetAllItemsRecursive().OfType<DataItem>().
                        FirstOrDefault(di => di.ValueType == typeof(HydroArea) && ReferenceEquals(getCollection((HydroArea) di.Value), o)),
                AfterCreate = (v, o) =>
                {
                    var centralMap =
                        getGui()
                            .DocumentViews.OfType<ProjectItemMapView>()
                            .FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                    if (centralMap == null) return;

                    v.DeleteSelectedFeatures = () => centralMap.MapView.MapControl.DeleteTool.DeleteSelection();
                    v.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(feature);
                    v.OpenViewMethod = f => getGui().CommandHandler.OpenView(f); 
                    v.DynamicAttributeVisible = s => s == Feature2D.LocationKey;
                    v.CanAddDeleteAttributes = false;
                }
            };
        }

    }
}