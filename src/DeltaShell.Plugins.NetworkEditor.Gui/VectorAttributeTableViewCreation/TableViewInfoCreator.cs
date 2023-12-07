using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation
{
    /// <summary>
    /// Class for creating <see cref="ViewInfo"/> object specifically for the <see cref="VectorLayerAttributeTableView"/>.
    /// </summary>
    public sealed class TableViewInfoCreator
    {
        private readonly GuiContainer guiContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableViewInfoCreator"/>.
        /// </summary>
        /// <param name="guiContainer"> The GUI container which provides an <see cref="DelftTools.Shell.Gui.IGui"/> instance. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="guiContainer"/> is <c>null</c>.
        /// </exception>
        public TableViewInfoCreator(GuiContainer guiContainer)
        {
            Ensure.NotNull(guiContainer, nameof(guiContainer));
            this.guiContainer = guiContainer;
        }

        /// <summary>
        /// Creates a new <see cref="ViewInfo"/> object for the specified creation context.
        /// </summary>
        /// <param name="creationContext"> Creation context that is specific to the underlying data. </param>
        /// <typeparam name="TFeature"> The type of data to create the view info for. </typeparam>
        /// <typeparam name="THydroRegion"> The of hydro region the data is in. </typeparam>
        /// <typeparam name="TFeatureRow"> The type of feature row object. </typeparam>
        /// <returns>
        /// A newly constructed <see cref="ViewInfo{TData,TViewData,TView}"/> object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="creationContext"/> is <c>null</c>.
        /// </exception>
        public ViewInfo<IEnumerable<TFeature>, ILayer, VectorLayerAttributeTableView> Create<TFeature, TFeatureRow, THydroRegion>(
            ITableViewCreationContext<TFeature, TFeatureRow, THydroRegion> creationContext) where TFeature : IFeature
                                                                                            where TFeatureRow : class, IFeatureRowObject
                                                                                            where THydroRegion : IHydroRegion
        {
            Ensure.NotNull(creationContext, nameof(creationContext));

            return new ViewInfo<IEnumerable<TFeature>, ILayer, VectorLayerAttributeTableView>
            {
                Description = creationContext.GetDescription(),
                CompositeViewType = typeof(ProjectItemMapView),
                AdditionalDataCheck = data => GetHydroNetworkDataItem(creationContext, data) != null,
                GetCompositeViewData = data => GetHydroNetworkDataItem(creationContext, data),
                GetViewData = GetDataLayer,
                AfterCreate = (view, data) => ConfigureView(view, data, creationContext)
            };
        }

        private IDataItem GetHydroNetworkDataItem<TFeature, TFeatureRow, THydroRegion>(
            ITableViewCreationContext<TFeature, TFeatureRow, THydroRegion> creationContext, IEnumerable<TFeature> data) where TFeature : IFeature
                                                                                                                        where TFeatureRow : class, IFeatureRowObject
                                                                                                                        where THydroRegion : IHydroRegion
        {
            return GetDataItems().FirstOrDefault(d => d.Value is THydroRegion hydroRegion &&
                                                      creationContext.IsRegionData(hydroRegion, data));
        }

        private IEnumerable<IDataItem> GetDataItems()
        {
            return guiContainer.Gui.Application.Project.GetAllItemsRecursive().OfType<IDataItem>();
        }

        private ILayer GetDataLayer(object data)
        {
            foreach (ProjectItemMapView projectItemMapView in guiContainer.Gui.DocumentViews.OfType<ProjectItemMapView>())
            {
                ILayer layer = projectItemMapView.MapView.GetLayerForData(data);
                if (layer != null)
                {
                    return layer;
                }
            }

            throw new ArgumentException($"Could not find a layer corresponding to object {data}");
        }

        private void ConfigureView<TFeature, TFeatureRow, THydroRegion>(
            VectorLayerAttributeTableView view, IEnumerable<TFeature> data, ITableViewCreationContext<TFeature, TFeatureRow, THydroRegion> creationContext)
            where TFeature : IFeature
            where TFeatureRow : class, IFeatureRowObject
            where THydroRegion : IHydroRegion
        {
            ProjectItemMapView map = GetMapView(data);

            view.DeleteSelectedFeatures = () => map.MapView.MapControl.DeleteTool.DeleteSelection();
            view.OpenViewMethod = o => guiContainer.Gui.CommandHandler.OpenView(o);
            view.ZoomToFeature = feature => map.MapView.EnsureVisible(feature);
            view.CanAddDeleteAttributes = false;
            view.DynamicAttributeVisible = s => false;
            view.SetCreateFeatureRowFunction(f => creationContext.CreateFeatureRowObject((TFeature)f, data));
            creationContext.CustomizeTableView(view, data, guiContainer);
        }

        private ProjectItemMapView GetMapView<TFeature>(IEnumerable<TFeature> data) where TFeature : IFeature
        {
            return guiContainer.Gui.DocumentViews.OfType<ProjectItemMapView>().First(v => v.MapView.GetLayerForData(data) != null);
        }
    }
}