using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
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

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors
{
    public class HydroNetworkFeatureEditor : FeatureEditor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetworkFeatureEditor));

        public HydroNetworkFeatureEditor(INetwork network)
        {
            Network = network;
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            // exceptional case for nodes
            if (layer.DataSource.FeatureType == typeof(HydroNode))
            {
                var branch = (IChannel) NetworkHelper.GetNearestBranch(Network.Branches, geometry, 0.1);
                return HydroNetworkHelper.SplitChannelAtNode(branch, geometry.Coordinate);
            }

            var hydroNetwork = Network as HydroNetwork;
            if (hydroNetwork != null && layer.DataSource.FeatureType == typeof(Manhole))
            {
                var pipe = (IPipe) NetworkHelper.GetNearestBranch(hydroNetwork.Pipes, geometry, 0.1);
                return HydroNetworkHelper.SplitPipeAtCoordinate(pipe, geometry.Coordinate);
            }

            var newFeature = layer.FeatureEditor.CreateNewFeature != null
                                      ? CreateNewFeature(layer)
                                      : (IFeature)Activator.CreateInstance(layer.DataSource.FeatureType);

            newFeature.Geometry = geometry;

            if (newFeature is INameable)
            {
                (newFeature as INameable).Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion)Network, newFeature);
            }

            var interactor = layer.FeatureEditor.CreateInteractor(layer, newFeature);

            var networkFeatureInteractor = interactor as INetworkFeatureInteractor;
            if (null != networkFeatureInteractor)
            {
                networkFeatureInteractor.Network = Network;
            }
            try
            {
                var editing = Network.IsEditing;

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

        public INetwork Network { get; private set; }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            IFeatureInteractor featureInteractor = null;
            var vectorLayer = layer as VectorLayer;
            var vectorStyle = (vectorLayer != null ? vectorLayer.Style : null);

            if ((layer != null) && (layer.Name == "Cross Sections"))
            {
                // hack used for default geometry before feature cross section has been created.
                featureInteractor = new CrossSectionInteractor(layer, feature, vectorStyle, Network);
            }
            else switch (feature)
            {
                case ICompositeBranchStructure _:
                    featureInteractor = new CompositeStructureInteractor(layer, feature, vectorStyle, Network);
                    break;
                case ICompartment _:
                    featureInteractor = new CompartmentInteractor(layer, feature, vectorStyle, Network);
                    break;
                case IWeir weir:
                    featureInteractor = weir.WeirFormula is GatedWeirFormula 
                        ? (IFeatureInteractor)new StructureInteractor<Orifice>(layer, feature, vectorStyle, Network) 
                        : new StructureInteractor<Weir>(layer, feature, vectorStyle, Network);
                    break;
                case ICulvert _:
                    featureInteractor = new StructureInteractor<Culvert>(layer, feature, vectorStyle, Network);
                    break;
                case IBridge _:
                    featureInteractor = new StructureInteractor<Bridge>(layer, feature, vectorStyle, Network);
                    break;
                case IPump _:
                    featureInteractor = new StructureInteractor<Pump>(layer, feature, vectorStyle, Network);
                    break;
                case ICrossSection _:
                    featureInteractor = new CrossSectionInteractor(layer, feature, vectorStyle, Network)
                    {
                        SnapRules = { }
                    };
                    break;
                case IManhole _:
                    featureInteractor = new ManholeInteractor(layer, feature, vectorStyle, Network);
                    break;
                case INode _:
                    featureInteractor = new HydroNodeInteractor(layer, feature, vectorStyle, Network);
                    break;
                case IChannel _:
                    featureInteractor = new ChannelInteractor(layer, feature, vectorStyle, Network);
                    break;
                case ISewerConnection _:
                    featureInteractor = new SewerConnectionInteractor(layer, feature, vectorStyle, Network);
                    break;
                case INetworkLocation _:
                    featureInteractor = new NetworkLocationFeatureInteractor(layer, feature, vectorStyle, null);
                    break;
                case LateralSource source:
                    featureInteractor = source.IsDiffuse
                        ? (IFeatureInteractor)new DiffuseLateralSourceInteractor(layer, source, vectorStyle, Network)
                        : new LateralSourceInteractor(layer, source, vectorStyle, Network);
                    break;
                case Retention _:
                    featureInteractor = new BranchFeatureInteractor<Retention>(layer, feature, vectorStyle, Network);
                    break;
                case ObservationPoint _:
                    featureInteractor = new BranchFeatureInteractor<ObservationPoint>(layer, feature, vectorStyle, Network);
                    break;
            }

            if (featureInteractor is INetworkFeatureInteractor)
            {
                ((INetworkFeatureInteractor)featureInteractor).Network = Network;
            }

            return featureInteractor;
        }

        static public IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractor(IFeature feature)
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
    }
}