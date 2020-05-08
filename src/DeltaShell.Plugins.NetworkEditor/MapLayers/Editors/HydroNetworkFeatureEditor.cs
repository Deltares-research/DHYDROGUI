using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
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
using NetTopologySuite.Extensions.Networks;
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
            if (feature is LateralSource)
            {
                yield return new HydroObjectToHydroLinkRelationInteractor();
            }
            else if (feature is INode)
            {
                yield return new NodeToBranchRelationInteractor();
                yield return new HydroObjectToHydroLinkRelationInteractor();
            }
            else if (feature is IChannel)
            {
                yield return new BranchToCrossSectionRelationInteractor();
                yield return new BranchToBranchFeatureRelationInteractor<CompositeBranchStructure>();
                yield return new BranchToBranchFeatureRelationInteractor<NetworkLocation>();
                yield return new BranchToBranchFeatureRelationInteractor<LateralSource>();
                yield return new BranchToBranchFeatureRelationInteractor<ObservationPoint>();
                yield return new BranchToBranchFeatureRelationInteractor<Retention>();
            }
            else if (feature is ICompositeBranchStructure)
            {
                yield return new StructureFeatureToStructureRelationInteractor();
            }
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            // exceptional case for nodes
            if (layer.DataSource.FeatureType == typeof(HydroNode))
            {
                var branch = (IChannel) NetworkHelper.GetNearestBranch(Network.Branches, geometry, 0.1);
                return HydroNetworkHelper.SplitChannelAtNode(branch, geometry.Coordinate);
            }

            IFeature newFeature = layer.FeatureEditor.CreateNewFeature != null
                                      ? CreateNewFeature(layer)
                                      : (IFeature) Activator.CreateInstance(layer.DataSource.FeatureType);

            newFeature.Geometry = geometry;

            if (newFeature is INameable)
            {
                (newFeature as INameable).Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion) Network, newFeature);
            }

            IFeatureInteractor interactor = layer.FeatureEditor.CreateInteractor(layer, newFeature);

            var networkFeatureInteractor = interactor as INetworkFeatureInteractor;
            if (null != networkFeatureInteractor)
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
                log.Error(string.Format("Unable to add feature: {0}", exception.Message), exception);
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

            if (layer != null && layer.Name == "Cross Sections")
            {
                // hack used for default geometry before feature cross section has been created.
                featureInteractor = new CrossSectionInteractor(layer, feature, vectorStyle, Network);
            }
            else if (feature is ICompositeBranchStructure)
            {
                featureInteractor = new CompositeStructureInteractor(layer, feature, vectorStyle, Network);
            }
            else if (feature is IWeir)
            {
                featureInteractor = new StructureInteractor<Weir>(layer, feature, vectorStyle, Network);
            }
            else if (feature is ICulvert)
            {
                featureInteractor = new StructureInteractor<Culvert>(layer, feature, vectorStyle, Network);
            }
            else if (feature is IBridge)
            {
                featureInteractor = new StructureInteractor<Bridge>(layer, feature, vectorStyle, Network);
            }
            else if (feature is IPump)
            {
                featureInteractor = new StructureInteractor<Pump>(layer, feature, vectorStyle, Network);
            }
            else if (feature is ICrossSection)
            {
                featureInteractor = new CrossSectionInteractor(layer, feature, vectorStyle, Network) {SnapRules = {}};
            }
            else if (feature is INode)
            {
                featureInteractor = new HydroNodeInteractor(layer, feature, vectorStyle, Network);
            }
            else if (feature is IChannel)
            {
                featureInteractor = new ChannelInteractor(layer, feature, vectorStyle, Network);
            }
            else if (feature is INetworkLocation)
            {
                featureInteractor = new NetworkLocationFeatureInteractor(layer, feature, vectorStyle, null);
            }
            else if (feature is LateralSource)
            {
                featureInteractor = ((LateralSource) feature).IsDiffuse
                                        ? (IFeatureInteractor) new DiffuseLateralSourceInteractor(layer, feature, vectorStyle, Network)
                                        : new LateralSourceInteractor(layer, feature, vectorStyle, Network);
            }
            else if (feature is Retention)
            {
                featureInteractor = new BranchFeatureInteractor<Retention>(layer, feature, vectorStyle, Network);
            }
            else if (feature is ObservationPoint)
            {
                featureInteractor = new BranchFeatureInteractor<ObservationPoint>(layer, feature, vectorStyle, Network);
            }
            else if (feature is IExtraResistance)
            {
                featureInteractor = new StructureInteractor<ExtraResistance>(layer, feature, vectorStyle, Network);
            }

            if (featureInteractor is INetworkFeatureInteractor)
            {
                ((INetworkFeatureInteractor) featureInteractor).Network = Network;
            }

            return featureInteractor;
        }
    }
}