using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        public const string DiscretizationObjectName = "Computational 1D Grid";
        public const string NetworkObjectName = "Network";
        private IHydroNetwork network;

        private IDiscretization networkDiscretization;

        public IHydroNetwork Network
        {
            get { return network; }
            set
            {
                if (value == Network)
                {
                    return;
                }

                UnSubscribeFromNetwork();

                network = value;
                network.Name = NetworkObjectName;

                SubscribeToNetwork();

                // refresh data
                RefreshNetworkRelatedData();
            }
        }

        public IDiscretization NetworkDiscretization {
            get { return networkDiscretization;}
            set
            {
                if (networkDiscretization != null)
                {
                    ((INotifyPropertyChanged) networkDiscretization).PropertyChanged -= OnNetworkDiscretisationChanged;
                }
                networkDiscretization = value;
                networkDiscretization.Name = DiscretizationObjectName;
                if (networkDiscretization != null)
                {
                    ((INotifyPropertyChanged)networkDiscretization).PropertyChanged += OnNetworkDiscretisationChanged;
                }
            }
        }

        private void OnNetworkDiscretisationChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender != networkDiscretization || e.PropertyName != "IsEditing") return;
            if(((Discretization)sender).IsEditing) return;
            RefreshMappings();
        }

        private void SubscribeToNetwork()
        {
            if (network != null)
            {
                ((INotifyCollectionChange)network).CollectionChanged += NetworkCollectionChanged;
                ((INotifyPropertyChanged)network).PropertyChanged += NetworkPropertyChanged;
            }
        }

        private void UnSubscribeFromNetwork()
        {
            if (network != null)
            {
                ((INotifyCollectionChange)network).CollectionChanged -= NetworkCollectionChanged;
                ((INotifyPropertyChanged)network).PropertyChanged -= NetworkPropertyChanged;
            }
        }

        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes. Since this property
        ///   can be set/reset while the node was not part of the network it is necessary to monitor additions and removals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void NetworkCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            // when node is added or removed
            if (e.Item is IHydroNode)
            {

            }
            else if (e.Item is IChannel)
            {
                if (Equals(sender, Network.Branches))
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangeAction.Remove:
                            {
                                // remove all child data items
                                var dataItemsToRemove = new List<IDataItem>();
                                var networkDataItem = GetDataItemByValue(Network);

                                if (networkDataItem != null) // TODO: temporary fix while we do not have a DataItem for Network in FM Model
                                {
                                    foreach (var dataItem in networkDataItem.Children)
                                    {
                                        dataItemsToRemove.Add(dataItem);
                                    }

                                    foreach (var dataItem in dataItemsToRemove)
                                    {
                                        dataItem.Unlink();
                                        dataItem.LinkedBy.ToArray().ForEach(di => di.Unlink());
                                        networkDataItem.Children.Remove(dataItem);
                                    }
                                }
                            }
                            break;
                        case NotifyCollectionChangeAction.Add:
                            {

                            }
                            break;
                    }

                    ClearOutput();
                }
            }

            // check if removed item is used in the child data items
            if (e.Item is IFeature && e.Action == NotifyCollectionChangeAction.Remove)
            {
                var asNetworkFeature = e.Item as INetworkFeature;
                if (asNetworkFeature != null && asNetworkFeature.IsBeingMoved())
                {
                    return;
                }

                var childDataItems =
                    AllDataItems.Where(
                        di =>
                            di.Parent != null && di.ValueConverter != null &&
                            di.ValueConverter.OriginalValue == e.Item).ToList();

                foreach (var childDataItem in childDataItems)
                {
                    // unlink all consumers
                    foreach (var targetDataItem in childDataItem.LinkedBy.ToArray())
                    {
                        targetDataItem.Unlink();
                    }

                    // remove item from parent
                    childDataItem.Parent.Children.Remove(childDataItem);
                }
            }
        }

        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IDataItem && ((IDataItem)sender).Value is IHydroNetwork)
            {
                if (e.PropertyName == "Value")
                {
                    RefreshNetworkRelatedData();
                }
            }

            if (sender == Network && e.PropertyName == "IsEditing" && Network.CurrentEditAction is BranchSplitAction &&
                !Network.IsEditing && NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Any())
            {
                OnEndingBranchSplit((BranchSplitAction)Network.CurrentEditAction);
            }
        }

        /// <summary>
        /// Called when a network is inserted into or linked to the model
        /// </summary>
        [EditAction]
        private void RefreshNetworkRelatedData()
        {
            ClearOutput();
            if (NetworkDiscretization != null && NetworkDiscretization.Network != Network)
            {
                NetworkDiscretization.Network = Network;
                NetworkDiscretization.Clear();
                if (string.IsNullOrEmpty(NetworkDiscretization.Name))
                    NetworkDiscretization.Name = DiscretizationObjectName;
            }

            // update network in output coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is INetworkCoverage)
                .Select(di => di.Value)
                .Cast<INetworkCoverage>()
                .ForEach(c => c.Network = Network);
        }

        private void OnEndingBranchSplit(BranchSplitAction splitAction)
        {
            var locations = (splitAction.NewBranch.Source == splitAction.SplittedBranch.Target
                ? new[]
                {
                    new NetworkLocation(splitAction.SplittedBranch, splitAction.SplittedBranch.Length)
                    {
                        // reset chainage to realy put the chainage to the end of the branch (this is not done via the contructor)
                        Chainage = splitAction.SplittedBranch.Length
                    },
                    new NetworkLocation(splitAction.NewBranch, 0)
                }
                : null);

            if (locations != null)
            {
                NetworkDiscretization.BeginEdit(new DefaultEditAction("Adding point at begin and end of branch"));
                NetworkDiscretization.Locations.AddValues(locations.Except(NetworkDiscretization.Locations.GetValues()));
                NetworkDiscretization.EndEdit();
            }
        }

        private void LoadNetwork()
        {
            if (!File.Exists(NetFilePath)) return;
            var loadedNetwork = NetworkDiscretisationFactory.CreateHydroNetwork(UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(NetFilePath));
            if (loadedNetwork == null) return;
            Network = loadedNetwork;
        }

        private void LoadNetworkAndDiscretisation()
        {
            if (!File.Exists(NetFilePath)) return;
            var loadedNetworkDiscretisation = UGridToNetworkAdapter.LoadNetworkAndDiscretisation(NetFilePath);
            if (loadedNetworkDiscretisation != null)
            {
                NetworkDiscretization = loadedNetworkDiscretisation;
                Network = (IHydroNetwork)loadedNetworkDiscretisation.Network;
                return;
            }

            LoadNetwork();

        }

        private void SaveNetwork()
        {
            UGridGlobalMetaData metaData = new UGridGlobalMetaData(Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion);

            UGridToNetworkAdapter.SaveNetwork(network, NetFilePath, metaData);
        }

        private void SaveNetworkDiscretisation()
        {
            UGridToNetworkAdapter.SaveNetworkDiscretisation(NetworkDiscretization, NetFilePath);
        }
    }
}
