using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.Gui.LayerGenerators
{
    internal static class HydroNetworkLayerGeneratorExtensions
    {
        internal static ILayer GenerateHydroNetworkLayer(this IHydroNetwork hydroNetwork, object data)
        {
            switch (data)
            {
                case IEnumerable<IManhole> manholeNodes:
                    return CreateNetworkVectorLayer<Manhole>(manholeNodes, "Manholes", hydroNetwork);
                case IEnumerable<OutletCompartment> outletCompartments:
                    return CreateNetworkVectorLayer<OutletCompartment>(outletCompartments, "Outlets", hydroNetwork);
                case IEnumerable<Compartment> compartments:
                    return CreateNetworkVectorLayer<Compartment>(compartments, "Compartments", hydroNetwork);
                case IEnumerable<IPipe> pipes:
                    return CreateNetworkVectorLayer<Pipe>(pipes, "Pipes", hydroNetwork);
                case IEnumerable<ISewerConnection> sewerConnections:
                    return CreateNetworkVectorLayer<SewerConnection>(sewerConnections, "Sewer Connections",
                                                                     hydroNetwork, refreshForChangedItem: o => !(o is IPipe) && o is SewerConnection sewerConnection && sewerConnection.BranchFeatures.Any());
                case IEnumerable<IHydroNode> hydroNodes:
                    return CreateNetworkVectorLayer<HydroNode>(hydroNodes, "Nodes", hydroNetwork);
                case IEnumerable<IChannel> channels:
                    return CreateNetworkVectorLayer<Channel>(channels, "Branches", hydroNetwork);
                case IEnumerable<IPump> pumps:
                    return CreateNetworkVectorLayer<Pump>(pumps, "Pumps", hydroNetwork,
                                                          refreshForChangedItem: o => o is Channel channel && channel.Pumps.Any());
                case IEnumerable<ILateralSource> lateralSources:
                    return CreateNetworkVectorLayer<LateralSource>(lateralSources, "Lateral Sources", hydroNetwork,
                                                                   refreshForChangedItem: o =>
                                                                       o is Channel channel &&
                                                                       channel.BranchFeatures.OfType<LateralSource>()
                                                                              .Any());
                case IEnumerable<IRetention> retentions:
                    return CreateNetworkVectorLayer<Retention>(retentions, "Retentions", hydroNetwork,
                                                               refreshForChangedItem: o =>
                                                                   o is Channel channel &&
                                                                   channel.BranchFeatures.OfType<Retention>().Any());
                case IEnumerable<IObservationPoint> observationPoints:
                    return CreateNetworkVectorLayer<ObservationPoint>(observationPoints, "Observation Points", hydroNetwork, refreshForChangedItem: o => o is Channel channel && channel.ObservationPoints.Any());
                case IEnumerable<IOrifice> orifices:
                    return CreateNetworkVectorLayer<IOrifice>(orifices, "Orifices", hydroNetwork, o => o is Channel channel && channel.Weirs.OfType<IOrifice>().Any());
                case IEnumerable<IWeir> weirs:
                    return CreateNetworkVectorLayer<Weir>(weirs, "Weirs", hydroNetwork, o => o is Channel channel && channel.Weirs.OfType<Weir>().Any());
                case IEnumerable<IGate> gates:
                    return CreateNetworkVectorLayer<Gate>(gates, "Gates", hydroNetwork, refreshForChangedItem: o => o is Channel channel && channel.Gates.Any());
                case IEnumerable<ICulvert> culverts:
                    return CreateNetworkVectorLayer<Culvert>(culverts, "Culverts", hydroNetwork, refreshForChangedItem: o => o is Channel channel && channel.Culverts.Any());
                case IEnumerable<IBridge> bridges:
                    return CreateNetworkVectorLayer<Bridge>(bridges, "Bridges", hydroNetwork, refreshForChangedItem: o => o is Channel channel && channel.Bridges.Any());
                case IEnumerable<IExtraResistance> extraResistances:
                    return CreateNetworkVectorLayer<ExtraResistance>(extraResistances, "Extra Resistances", hydroNetwork,
                                                                     refreshForChangedItem: o =>
                                                                         o is Channel channel &&
                                                                         channel.BranchFeatures
                                                                                .OfType<ExtraResistance>().Any());
                case IEnumerable<ICompositeBranchStructure> compositeBranchStructures:
                    return CreateNetworkVectorLayer<CompositeBranchStructure>(compositeBranchStructures,
                                                                              "Compound Structures",
                                                                              hydroNetwork,
                                                                              refreshForChangedItem: o =>
                                                                                  o is Channel channel &&
                                                                                  channel.CrossSections.Any());
                case IEnumerable<ICrossSection> crossSections:
                    return CreateNetworkVectorLayer<CrossSection>(crossSections, "Cross Sections", hydroNetwork,
                                                                  refreshForChangedItem: o => o is Channel channel && channel.CrossSections.Any());
                default:
                    return null;
            }
        }

        private static VectorLayer CreateNetworkVectorLayer<TFeature>(IEnumerable<IFeature> networkItems, string name, IHydroNetwork hydroNetwork, Func<object, bool> refreshForChangedItem = null) where TFeature : IFeature
        {
            var layer = new VectorLayer(name)
            {
                Style = NetworkLayerStyleFactory.CreateStyle(networkItems),
                Theme = NetworkLayerStyleFactory.CreateTheme(networkItems),
                NameIsReadOnly = true,
                DataSource = new HydroNetworkFeatureCollection
                {
                    FeatureType = typeof(TFeature),
                    Network = hydroNetwork,
                    RefreshForChangedItem = refreshForChangedItem,
                    CoordinateSystem = hydroNetwork.CoordinateSystem
                },
                CustomRenderers = GetCustomRenderer<TFeature>()
            };

            layer.FeatureEditor = new HydroNetworkFeatureEditor(hydroNetwork)
            {
                CreateNewFeature = NewNetworkFeature<TFeature>(),
                SnapRules = GetSnapRule<TFeature>(layer).ToList()
            };

            return layer;
        }

        private static IList<IFeatureRenderer> GetCustomRenderer<T>()
        {
            var type = typeof(T);

            if (type == typeof(CompositeBranchStructure))
            {
                return new List<IFeatureRenderer> { new CompositeStructureRenderer(new StructureRenderer()) };
            }

            if (type == typeof(Weir) || type == typeof(Pump) || type == typeof(Culvert) ||
                type == typeof(Bridge) || type == typeof(ExtraResistance)
                || type == typeof(OutletCompartment) || type == typeof(IOrifice))
            {
                return new List<IFeatureRenderer> { new StructureRenderer() };
            }

            if (type == typeof(CrossSection))
            {
                return new List<IFeatureRenderer> { new CrossSectionRenderer() };
            }

            if (type == typeof(LateralSource))
            {
                return new List<IFeatureRenderer> { new DiffuseLateralSourceRenderer() };
            }

            if (type.Implements(typeof(IPipe)))
            {
                return new List<IFeatureRenderer> { new ArrowLineStringAdornerRenderer { Orientation = Orientation.Forward, Opacity = 1, Size = 3 } };
            }

            if (type == typeof(Compartment))
            {
                return new List<IFeatureRenderer> { new CompartmentRenderer() };
            }

            return new List<IFeatureRenderer>();
        }

        private static Func<ILayer, IFeature> NewNetworkFeature<T>()
        {
            var type = typeof(T);

            if (type == typeof(Pump))
            {
                return l => new Pump(false);
            }

            if (type == typeof(IOrifice))
            {
                return l => new Orifice(true);
            }

            if (type == typeof(Weir))
            {
                return l => new Weir(true);
            }

            if (type == typeof(Culvert))
            {
                return l => Culvert.CreateDefault();
            }

            if (type == typeof(Bridge))
            {
                return l => Bridge.CreateDefault();
            }

            if (type == typeof(ExtraResistance))
            {
                return l => ExtraResistance.CreateDefault();
            }

            if (type == typeof(CrossSection))
            {
                return l => CrossSection.CreateDefault();
            }

            return null;
        }

        private static IEnumerable<ISnapRule> GetSnapRule<T>(ILayer vectorLayer) where T : IFeature
        {
            var type = typeof(T);
            if (type == typeof(Channel))
            {
                yield return CreateDefaultSnappingRule<BranchSnapRule, IHydroNode>(vectorLayer, SnapRole.AllTrackers, false);
            }

            if (type == typeof(Pipe))
            {
                yield return CreateDefaultSnappingRule<BranchSnapRule, IManhole>(vectorLayer, SnapRole.AllTrackers, false);
                yield return CreateDefaultSnappingRule<BranchSnapRule, IHydroNode>(vectorLayer, SnapRole.AllTrackers, false);
                yield return CreateDefaultSnappingRule<BranchSnapRule, Compartment>(vectorLayer, SnapRole.AllTrackers, false);
            }

            if (type == typeof(SewerConnection) && type != typeof(Pipe))
            {
                yield return CreateDefaultSnappingRule<BranchSnapRule, IManhole>(vectorLayer, SnapRole.AllTrackers, false);
                yield return CreateDefaultSnappingRule<BranchSnapRule, Compartment>(vectorLayer, SnapRole.AllTrackers, false);
            }

            if (type == typeof(CrossSection))
            {
                yield return CreateDefaultSnappingRule<CrossSectionSnapRule, Channel>(vectorLayer);
            }

            if (type == typeof(HydroNode))
            {
                yield return CreateDefaultSnappingRule<HydroNodeSnapRule, Channel>(vectorLayer);
            }

            if (type == typeof(Manhole))
            {
                yield return CreateDefaultSnappingRule<HydroNodeSnapRule, Pipe>(vectorLayer);
            }

            if (type == typeof(LateralSource))
            {
                yield return CreateDefaultSnappingRule<SnapRule, Channel>(vectorLayer);
                yield return CreateDefaultSnappingRule<SnapRule, INode>(vectorLayer);
            }

            if (type == typeof(CompositeBranchStructure) || type == typeof(Retention) || type == typeof(ObservationPoint))
            {
                yield return CreateDefaultSnappingRule<SnapRule, Channel>(vectorLayer);
            }

            if (type == typeof(Culvert) ||
                type == typeof(Bridge) ||
                type == typeof(ExtraResistance))
            {
                yield return new StructureSnapRule
                {
                    // StructureSnapRule needs all structure layer for the custom geometry
                    NewFeatureLayer = vectorLayer,
                    PixelGravity = 40
                };

                yield return CreateDefaultSnappingRule<SnapRule, Channel>(vectorLayer);
            }

            if (type == typeof(Weir) ||
                type == typeof(IOrifice) ||
                type == typeof(Pump))
            {
                yield return new StructureSnapRule
                {
                    // StructureSnapRule needs all structure layer for the custom geometry
                    NewFeatureLayer = vectorLayer,
                    PixelGravity = 40
                };

                yield return CreateDefaultSnappingRule<SnapRule, Channel>(vectorLayer);
                var sewerConnectionRule = CreateDefaultSnappingRule<SnapRule, SewerConnection>(vectorLayer);
                sewerConnectionRule.Criteria = (layer, feature) => feature is SewerConnection && !(feature is IPipe)
                                                                                              && layer.DataSource is HydroNetworkFeatureCollection collection
                                                                                              && collection.Network == ((HydroNetworkFeatureCollection)vectorLayer.DataSource).Network;
                yield return sewerConnectionRule;
            }
        }

        private static T CreateDefaultSnappingRule<T, TFeature>(ILayer featureLayer, SnapRole snapRole = SnapRole.FreeAtObject, bool obligatory = true) where T : SnapRule, new()
        {
            return new T
            {
                Criteria = (layer, feature) => feature is TFeature
                                               && layer.DataSource is HydroNetworkFeatureCollection featureCollection
                                               && featureCollection.Network == ((HydroNetworkFeatureCollection)featureLayer.DataSource).Network,
                SnapRole = snapRole,
                NewFeatureLayer = featureLayer,
                Obligatory = obligatory,
                PixelGravity = 40
            };
        }
    }
}