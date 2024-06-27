using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Gui;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;

namespace DeltaShell.NGHS.Common.Gui.Validation
{
    /// <summary>
    /// View info for <see cref="ValidatedFeatures"/>>.
    /// When clicking on a <see cref="ValidatedFeatures"/> in the GUI,
    /// the map will be opened and will be zoomed in to the corresponding feature.
    /// </summary>
    public sealed class ValidatedFeaturesViewInfo : ViewInfo<ValidatedFeatures, IMap, MapView>
    {
        private readonly GuiContainer guiContainer;
        private IGui Gui => guiContainer.Gui;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedFeatures"/> class.
        /// </summary>
        /// <param name="guiContainer">The gui container that contains the instance of the <see cref="IGui"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="guiContainer"/> is <c>null</c>.
        /// </exception>
        public ValidatedFeaturesViewInfo(GuiContainer guiContainer)
        {
            Ensure.NotNull(guiContainer, nameof(guiContainer));
            this.guiContainer = guiContainer;

            OnActivateView = Zoom;
            GetViewData = GetMap;
        }

        private static void Zoom(MapView mapView, object viewData)
        {
            var validatedFeatures = (ValidatedFeatures)viewData;

            Envelope envelope = validatedFeatures.GetEnvelope();

            mapView.Map.ZoomToFit(envelope, true);
        }

        private IMap GetMap(ValidatedFeatures validatedFeatures)
        {
            ProjectItemMapView mapView = GetMapView(validatedFeatures);
            return mapView.MapView.Map;
        }

        private ProjectItemMapView GetMapView(ValidatedFeatures validatedFeatures)
        {
            IHydroModel model = GetModel(validatedFeatures);

            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ProjectItemMapView));
            ProjectItemMapView mapView = Gui.DocumentViewsResolver.GetViewsForData(model).OfType<ProjectItemMapView>()
                                            .First(c => Equals(c.Data, model));
            return mapView;
        }

        private IHydroModel GetModel(ValidatedFeatures validatedFeatures)
        {
            IEnumerable<IHydroModel> models = Gui.Application.GetAllModelsInProject().OfType<IHydroModel>();
            return models.First(m =>
                                    m.Region.Equals(validatedFeatures.FeatureRegion) ||
                                    Enumerable.Contains<IComplexFeature>(m.Region.AllRegions, validatedFeatures.FeatureRegion));
        }
    }
}