using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D
{
    /// <summary>
    /// <see cref="Input2DGroupLayerSubProvider"/> provides the input group layer
    /// containing the 2D components as well as the child layer objects that should be
    /// part of it.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class Input2DGroupLayerSubProvider : ILayerSubProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Input2DGroupLayerSubProvider));
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="Input2DGroupLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public Input2DGroupLayerSubProvider (IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is InputLayerData inputData &&
            inputData.Dimension == LayerDataDimension.Data2D &&
            inputData.Model.Equals(parentData);

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData) 
                ? instanceCreator.Create2DGroupLayer() 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is InputLayerData layerData &&
                  layerData.Dimension == LayerDataDimension.Data2D))
            {
                yield break;
            }

            IWaterFlowFMModel model = layerData.Model;

            if (ShouldYieldArea(model))
            {
                WarnInvalidEnclosures(model.Area.Enclosures);
                yield return model.Area;
            }

            yield return model.Grid;
            yield return model.Bathymetry;
            yield return model.InitialWaterLevel;
            yield return model.BoundaryConditionSets;
            yield return model.Boundaries;
            yield return model.Roughness;
            yield return model.Viscosity;
            yield return model.Diffusivity;

            if (model.UseInfiltration)
            {
                yield return model.Infiltration;
            }
            
            if (model.HeatFluxModelType != HeatFluxModelType.None)
            {
                yield return model.InitialTemperature;
            }

            if (model.UseSalinity)
            {
                foreach (ICoverage coverage in model.InitialSalinity.Coverages)
                {
                    yield return coverage;
                }
            }

            foreach (UnstructuredGridCellCoverage tracer in model.InitialTracers)
            {
                yield return tracer;
            }

            if (model.UseMorSed)
            {
                foreach (UnstructuredGridCellCoverage fraction in model.InitialFractions)
                {
                    yield return fraction;
                }
            }

            yield return model.Pipes;
            yield return model.Links;

            yield return new EstimatedSnappedFeatureGroupData(model);
        }

        private static bool ShouldYieldArea(IWaterFlowFMModel model)
        {
            IModel rootModel = GetRootModel(model);

            return rootModel == null || 
                   rootModel is WaterFlowFMModel || 
                   model.GetDataItemByValue(model.Area).LinkedTo == null;
        }

        private static void WarnInvalidEnclosures(IEnumerable<GroupableFeature2DPolygon> enclosures)
        {
            foreach (GroupableFeature2DPolygon invalidEnclosure in enclosures.Where(IsInvalid))
            { 
                log.WarnFormat(Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid, 
                               invalidEnclosure.Name);
            }
        }

        private static bool IsInvalid(GroupableFeature2DPolygon enclosure) =>
            !(enclosure.Geometry is Polygon polygon && polygon.IsValid);

        private static IModel GetRootModel(IModel model)
        {
            IModel rootModel = GetRootModelRecursive(model);
            return rootModel == model ? null : rootModel;
        }

        private static IModel GetRootModelRecursive(IModel model)
        {
            while (model.Owner is IModel ownerModel)
            {
                model = ownerModel;
            }

            return model;
        }
    }
}