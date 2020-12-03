using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors
{
    public class HydroNetworkFeatureEditor : FeatureEditor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetworkFeatureEditor));

        public HydroNetworkFeatureEditor(INetwork network)
        {
            Network = network;
        }

        public INetwork Network { get; private set; }

        public static IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractor(IFeature feature)
        {
            switch (feature)
            {
                case LateralSource _:
                    yield return new HydroObjectToHydroLinkRelationInteractor();
                    break;
                case INode _:
                    yield return new NodeToBranchRelationInteractor();
                    yield return new HydroObjectToHydroLinkRelationInteractor();
                    break;
                case IChannel _:
                    yield return new BranchToBranchFeatureRelationInteractor<CompositeBranchStructure>();
                    yield return new BranchToBranchFeatureRelationInteractor<NetworkLocation>();
                    yield return new BranchToBranchFeatureRelationInteractor<LateralSource>();
                    yield return new BranchToBranchFeatureRelationInteractor<ObservationPoint>();
                    yield return new BranchToBranchFeatureRelationInteractor<Retention>();
                    break;
                case ICompositeBranchStructure _:
                    yield return new StructureFeatureToStructureRelationInteractor();
                    break;
            }
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            IFeature newFeature = layer.FeatureEditor.CreateNewFeature != null
                                      ? CreateNewFeature(layer)
                                      : (IFeature) Activator.CreateInstance(layer.DataSource.FeatureType);

            newFeature.Geometry = geometry;

            if (newFeature is INameable nameable)
            {
                nameable.Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion) Network, newFeature);
            }

            IFeatureInteractor interactor = layer.FeatureEditor.CreateInteractor(layer, newFeature);

            if (interactor is INetworkFeatureInteractor networkFeatureInteractor)
            {
                networkFeatureInteractor.Network = Network;
            }

            try
            {
                bool editing = Network.IsEditing;

                if (!editing)
                {
                    Network.BeginEdit("Add new " + newFeature.GetType().Name.ToLower());
                }

                interactor.Add(newFeature); // note topology rules may add extra features

                if (!editing)
                {
                    Network.EndEdit();
                }
            }
            catch (Exception exception)
            {
                log.Error($"Unable to add feature: {exception.Message}", exception);
                if (Network.IsEditing)
                {
                    Network.CancelEdit();
                }

                throw;
            }

            return newFeature;
        }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            IFeatureInteractor featureInteractor = null;
            var vectorLayer = layer as VectorLayer;
            VectorStyle vectorStyle = vectorLayer != null ? vectorLayer.Style : null;

            switch (feature)
            {
                case ICompositeBranchStructure _:
                    featureInteractor = new CompositeStructureInteractor(layer, feature, vectorStyle, Network);
                    break;
                case INode _:
                    featureInteractor = new HydroNodeInteractor(layer, feature, vectorStyle, Network);
                    break;
                case IChannel _:
                    featureInteractor = new ChannelInteractor(layer, feature, vectorStyle, Network);
                    break;
                case INetworkLocation _:
                    featureInteractor = new NetworkLocationFeatureInteractor(layer, feature, vectorStyle, null);
                    break;
                case LateralSource source:
                    featureInteractor = source.IsDiffuse
                                            ? (IFeatureInteractor) new DiffuseLateralSourceInteractor(layer, source, vectorStyle, Network)
                                            : new LateralSourceInteractor(layer, source, vectorStyle, Network);
                    break;
                case Retention _:
                    featureInteractor = new BranchFeatureInteractor<Retention>(layer, feature, vectorStyle, Network);
                    break;
                case ObservationPoint _:
                    featureInteractor = new BranchFeatureInteractor<ObservationPoint>(layer, feature, vectorStyle, Network);
                    break;
            }

            if (featureInteractor is INetworkFeatureInteractor interactor)
            {
                interactor.Network = Network;
            }

            return featureInteractor;
        }
    }
}