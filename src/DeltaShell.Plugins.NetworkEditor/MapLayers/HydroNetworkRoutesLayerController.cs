using System;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Drawing;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers
{
    public class HydroNetworkRoutesLayerController : IDisposable
    {
        private IHydroNetwork network;
        public IHydroNetwork Network
        {
            get { return network; }
            set
            {
                if (network != null)
                {
                    ((INotifyCollectionChanged)network).CollectionChanged -= NetworkRoutesCollectionChanged;
                }

                network = value;

                if (network != null)
                {
                    ((INotifyCollectionChanged)network).CollectionChanged += NetworkRoutesCollectionChanged;
                }
                GenerateLayersForRoutes();
            }
        }

        private GroupLayer routesLayer;
        public GroupLayer RoutesLayer
        {
            get { return routesLayer; }
            set
            {
                routesLayer = value;
                GenerateLayersForRoutes();
            }
        }

        void NetworkRoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (network == null || sender != network.Routes)
            {
                return;
            }

            RoutesLayer.LayersReadOnly = false;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var route = e.GetRemovedOrAddedItem() as Route;
                    if (!RoutesLayer.Layers.OfType<NetworkCoverageGroupLayer>().Any(l => Equals(route, l.NetworkCoverage)))
                    {
                        AddRouteLayer(route, RoutesLayer.Layers.Count, e.GetRemovedOrAddedIndex());
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var routeLayer = RoutesLayer.Layers.OfType<NetworkCoverageGroupLayer>().FirstOrDefault(rl => ReferenceEquals(rl.NetworkCoverage, e.GetRemovedOrAddedItem()));
                    RoutesLayer.Layers.Remove(routeLayer);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    GenerateLayersForRoutes();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RoutesLayer.LayersReadOnly = true;
        }

        private void AddRouteLayer(Route route, int colorIndex, int layerIndex)
        {
            var routeLayer = new NetworkCoverageGroupLayer { NetworkCoverage = route, NameIsReadOnly = true};
            RoutesLayer.Layers.Insert(layerIndex, routeLayer);
            NetworkCoverageGroupLayer.SetupRouteLayerTheme(routeLayer, ColorHelper.GetIndexedColor(100, colorIndex));
        }

        private void GenerateLayersForRoutes()
        {
            if (RoutesLayer == null)
                return;
            if (Network == null)
                return;

            RoutesLayer.LayersReadOnly = false;

            // remove all layers where coverage does not exist in network.Routes
            RoutesLayer.Layers.RemoveAllWhere(l => l is NetworkCoverageGroupLayer && !Network.Routes.Contains((Route) ((NetworkCoverageGroupLayer)l).NetworkCoverage));

            var count = 0;
            foreach (var route in Network.Routes)
            {
                // already added
                if (RoutesLayer.Layers.OfType<NetworkCoverageGroupLayer>().Any(l => Equals(route, l.NetworkCoverage)))
                {
                    continue;
                }

                AddRouteLayer(route, count++, Network.Routes.IndexOf(route));
            }

            RoutesLayer.LayersReadOnly = true;
        }

        public void Dispose()
        {
            Network = null;
            RoutesLayer = null;
        }
    }
}