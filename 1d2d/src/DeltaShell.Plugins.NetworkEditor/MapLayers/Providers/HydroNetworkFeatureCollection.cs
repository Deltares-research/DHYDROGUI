using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Providers
{
    public class HydroNetworkFeatureCollection : FeatureCollection, IEnumerableListEditor
    {
        private IList networkFeatures;
        private IHydroNetwork network;
        private bool isInitialized;
        private bool isDirty;

        public virtual IHydroNetwork Network
        {
            get { return network; }
            set
            {
                if (network != null)
                {
                    ((INotifyCollectionChanged)network).CollectionChanged -= HydroNetworkFeatureCollectionCollectionChanged;
                    ((INotifyPropertyChanged)network).PropertyChanged -= OnNetworkPropertyChanged;
                }   

                network = value;

                if (network != null)
                {
                    ((INotifyCollectionChanged)network).CollectionChanged += HydroNetworkFeatureCollectionCollectionChanged;
                    ((INotifyPropertyChanged)network).PropertyChanged += OnNetworkPropertyChanged;
                }

                if (!isInitialized)
                {
                    InitializeFeatures();
                }
            }
        }

        public override Type FeatureType
        {
            get { return base.FeatureType; }
            set
            {
                base.FeatureType = value;
                if (!isInitialized)
                {
                    InitializeFeatures();
                }
            }
        }
        [InvokeRequired]
        private void OnNetworkPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            envelope = null;

            if (propertyChangedEventArgs.PropertyName == "CoordinateSystem")
            {
                CoordinateSystem = network.CoordinateSystem;
                isDirty = true;
            }

			if (network.IsEditing)
                return; // do nothing yet

            if (isDirty) // if dirty and not editing, always fire event
            {
                FireFeaturesChanged();
                isDirty = false;
                return;
            }

            // else, only fire event on EndEdit
            if (sender.Equals(network) && propertyChangedEventArgs.PropertyName == "IsEditing")
                FireFeaturesChanged();
        }

        private void HydroNetworkFeatureCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            envelope = null;

            if (e.GetRemovedOrAddedItem() == null || (e.GetRemovedOrAddedItem().GetType() != FeatureType && (RefreshForChangedItem != null && !RefreshForChangedItem(e.GetRemovedOrAddedItem()))))
            {
                return;
            }
            isDirty = true;
        }

        public virtual Func<object, bool> RefreshForChangedItem { get; set; }

        private void InitializeFeatures()
        {
            ClearNetworkFeatures();

            // create backing list wrapping enumerable
            networkFeatures = GetNetworkFeatures();

            var enumerableListCache = networkFeatures as IEnumerableListCache;
            
            if (enumerableListCache == null)
            {
                isInitialized = false;
                return;
            }
            
            enumerableListCache.CollectionChangeSource = (INotifyCollectionChange)Network;
            enumerableListCache.PropertyChangeSource = (INotifyPropertyChange) Network;

            isInitialized = true;
        }

        private void ClearNetworkFeatures()
        {
            var enumerableListCache = networkFeatures as IEnumerableListCache;
            if (enumerableListCache != null)
            {
                enumerableListCache.CollectionChangeSource = null;
                enumerableListCache.PropertyChangeSource = null;
            }

            networkFeatures = null;
        }

        private IList GetNetworkFeatures()
        {
            if (FeatureType == null || Network == null)
            {
                return null;
            }
            if (FeatureType == typeof(Channel))
            {
                return GetEnumerableList(Network.Channels.OfType<Channel>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(HydroNode))
            {
                return GetEnumerableList(Network.Nodes.OfType<HydroNode>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(Manhole))
            {
                return GetEnumerableList(Network.Nodes.OfType<Manhole>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(OutletCompartment))
            {
                return GetEnumerableList(Network.OutletCompartments, (INotifyCollectionChange) Network);
            }
            if (FeatureType == typeof(Compartment))
            {
                return GetEnumerableList(Network.Compartments, (INotifyCollectionChange) Network);
            }
            if (FeatureType == typeof(IOrifice))
            {
                return GetEnumerableList(Network.Orifices, (INotifyCollectionChange) Network);
            }
            if (FeatureType == typeof(SewerConnection))
            {
                return GetEnumerableList(Network.SewerConnections.OfType<SewerConnection>().Where(sc => !sc.IsPipe()), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(Pipe))
            {
                return GetEnumerableList(Network.Branches.OfType<Pipe>(), (INotifyCollectionChange) Network);
            }
            if (FeatureType == typeof(CompositeBranchStructure))
            {
                return GetEnumerableList(Network.CompositeBranchStructures.OfType<CompositeBranchStructure>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(Pump))
            {
                return GetEnumerableList(Network.Pumps.OfType<Pump>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(Weir))
            {
                return GetEnumerableList(Network.Weirs.OfType<Weir>().Except(Network.Weirs.OfType<Orifice>()), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof (Gate))
            {
                return GetEnumerableList(Network.Gates.OfType<Gate>(), (INotifyCollectionChange) Network);
            }
            if (FeatureType == typeof(Culvert))
            {
                return GetEnumerableList(Network.Culverts.OfType<Culvert>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(Bridge))
            {
                return GetEnumerableList(Network.Bridges.OfType<Bridge>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(CrossSection))
            {
                return GetEnumerableList(Network.CrossSections.OfType<CrossSection>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(LateralSource))
            {
                return GetEnumerableList(Network.LateralSources.OfType<LateralSource>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(Retention))
            {
                return GetEnumerableList(Network.Retentions.OfType<Retention>(), (INotifyCollectionChange)Network);
            }
            if (FeatureType == typeof(ObservationPoint))
            {
                return GetEnumerableList(Network.ObservationPoints.OfType<ObservationPoint>(), (INotifyCollectionChange)Network);
            }
            throw new InvalidOperationException("Should never get here");
        }

        private IList GetEnumerableList<T>(IEnumerable<T> enumerable, INotifyCollectionChange notifyCollectionChange)
        {
            return new EnumerableList<T>
                       {
                           Enumerable = enumerable, 
                           Editor = this,
                           CollectionChangeSource = notifyCollectionChange
                       };
        }

        public override IList Features
        {
            get
            {
                if (!isInitialized)
                {
                    InitializeFeatures();
                }

                return networkFeatures;
            }
        }

        public virtual void OnInsert(int index, object o)
        {
            if (typeof(IBranch).IsAssignableFrom(FeatureType))
            {
                lock (network.Branches)
                {
                    network.Branches.Insert(index, (IBranch) o);
                }
            }
            else if (typeof(INode).IsAssignableFrom(FeatureType))
            {
                lock (network.Nodes)
                {
                    network.Nodes.Insert(index, (INode) o);
                }
            }
            else if (typeof(IBranchFeature).IsAssignableFrom(FeatureType))
            {
                throw new NotSupportedException("add via branch");
            }
            else if (typeof(INodeFeature).IsAssignableFrom(FeatureType))
            {
                throw new NotSupportedException("add via node");
            }
        }

        public virtual void OnAdd(object o)
        {
            if (typeof(IBranch).IsAssignableFrom(FeatureType))
            {
                lock (network.Branches)
                {
                    network.Branches.Add((IBranch) o);
                }
            }
            else if (typeof(INode).IsAssignableFrom(FeatureType))
            {
                lock (network.Nodes)
                {
                    network.Nodes.Add((INode) o);
                }
            }
            else if (typeof(IBranchFeature).IsAssignableFrom(FeatureType))
            {
                var branchFeature = (IBranchFeature) o;
                branchFeature.Branch.BranchFeatures.Add(branchFeature);
            }
            else if (typeof(INodeFeature).IsAssignableFrom(FeatureType))
            {
                throw new NotSupportedException("add via node");
            }
        }

        public virtual void OnRemove(object o)
        {
            if (typeof(IBranch).IsAssignableFrom(FeatureType))
            {
                lock (network.Branches)
                {
                    network.Branches.Remove((IBranch) o);
                }
            }
            else if (typeof(INode).IsAssignableFrom(FeatureType))
            {
                lock (network.Nodes)
                {
                    var node = (INode) o;
                    network.Nodes.Remove(node);
                }
            }
            else if (typeof(IBranchFeature).IsAssignableFrom(FeatureType))
            {
                var branchFeature = (IBranchFeature)o;
                lock (branchFeature.Branch.BranchFeatures)
                {
                    branchFeature.Branch.BranchFeatures.Remove(branchFeature); 
                }
            }
            else if (typeof(INodeFeature).IsAssignableFrom(FeatureType))
            {
                var nodeFeature = (INodeFeature)o;
                lock (nodeFeature.Node.NodeFeatures)
                {
                    nodeFeature.Node.NodeFeatures.Remove(nodeFeature);
                }
            }
        }

        public virtual void OnRemoveAt(int index)
        {
            OnRemove(networkFeatures[index]);
        }

        public virtual void OnReplace(int index, object o)
        {
            if (typeof(IBranch).IsAssignableFrom(FeatureType))
            {
                network.Branches[index] = (IBranch)o;
            }
            else if (typeof(INode).IsAssignableFrom(FeatureType))
            {
                network.Nodes[index] = (INode)o;
            }
            else if (typeof(IBranchFeature).IsAssignableFrom(FeatureType))
            {
                throw new NotSupportedException();
            }
            else if (typeof(INodeFeature).IsAssignableFrom(FeatureType))
            {
                throw new NotSupportedException();
            }
        }

        public virtual void OnClear()
        {
            if (typeof(IBranch).IsAssignableFrom(FeatureType))
            {
                lock (network.Branches)
                {
                    network.Branches.Clear();
                }
            }
            else if (typeof(INode).IsAssignableFrom(FeatureType))
            {
                lock (network.Nodes)
                {
                    network.Nodes.Clear();
                }
            }
            else if (typeof(IBranchFeature).IsAssignableFrom(FeatureType))
            {
                foreach (var branch in network.Branches)
                {
                    lock (branch.BranchFeatures)
                    {
                        branch.BranchFeatures.Clear();
                    }
                }
            }
            else if (typeof(INodeFeature).IsAssignableFrom(FeatureType))
            {
                foreach (var node in network.Nodes)
                {
                    lock (node.NodeFeatures)
                    {
                        node.NodeFeatures.Clear();
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearNetworkFeatures();
                Network = null;
            }
            
            base.Dispose(disposing);
        }
    }
}
