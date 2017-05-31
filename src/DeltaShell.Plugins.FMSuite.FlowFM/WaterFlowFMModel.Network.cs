using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
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

                SubscribeToNetwork();

                // refresh data
                RefreshNetworkRelatedData();
            }
        }

        public IDiscretization NetworkDiscretization
        {
            get { return networkDiscretization; }
            set { networkDiscretization = value; }
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
            if (sender is IDataItem && ((IDataItem) sender).Value is IHydroNetwork)
            {
                if (e.PropertyName == "Value")
                {
                    RefreshNetworkRelatedData();
                }
            }

            if (sender == Network && e.PropertyName == "IsEditing" && Network.CurrentEditAction is BranchSplitAction &&
                !Network.IsEditing && NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Any() )
            {
                OnEndingBranchSplit((BranchSplitAction) Network.CurrentEditAction);
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
                if(string.IsNullOrEmpty(NetworkDiscretization.Name))
                    NetworkDiscretization.Name = "Computational 1D Grid";
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

        private void SaveNetwork()
        {
            try
            {
                using (var uGrid1D = new UGrid1D(NetFilePath))
                {
                    var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);

                    uGrid1D.Create1DGridInFile(
                        network.Name,
                        network.Nodes.Count,
                        network.Branches.Count,
                        totalNumberOfGeometryPoints);

                    uGrid1D.Write1DNetworkNodes(
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                        network.Nodes.Select(n => n.Name).ToArray(),
                        network.Nodes.Select(n => n.Description).ToArray()
                    );

                    uGrid1D.Write1DNetworkBranches(
                        network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Length).ToArray(),
                        network.Branches.Select(b =>
                            {
                                if (b.Geometry != null && b.Geometry.Coordinates != null)
                                {
                                    return b.Geometry.Coordinates.Length;
                                }
                                return 0;
                            }
                        ).ToArray(),
                        network.Branches.Select(b => b.Name).ToArray(),
                        network.Branches.Select(b => b.Description).ToArray()
                    );
                    uGrid1D.Write1DNetworkGeometry(
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray()                       
                        );
                }
            }
            catch(Exception ex)
            {
                throw ex; // TODO: Rethrow the exception?
            }
            
        }

        private void SaveNetworkDiscretization()
        {
            try
            {
                using (UGrid1D uGrid1D = new UGrid1D(NetFilePath))
                {
                    // get the discretisation points from the network discretisation
                    var discretisationPoints = networkDiscretization.Locations.Values.ToArray();

                    // calculate the number of mesh edges -> #meshEdges = #discretisationPoints - #connectionNodes + #branches
                    var numberOfMeshEdges = discretisationPoints.Length - network.Nodes.Count + network.Branches.Count;

                    // create mesh
                    uGrid1D.Create1DMeshInFile(
                        networkDiscretization.Name,
                        discretisationPoints.Length,   
                        numberOfMeshEdges               
                        );

                    // Write discretisation points
                    int[] branchIdx = discretisationPoints.Select(l => l.Branch)
                        .ToArray()
                        .Select(b => networkDiscretization.Network.Branches.IndexOf(b))
                        .ToArray(); 

                    double[] offset = discretisationPoints.Select(l => l.Chainage).ToArray(); 

                    uGrid1D.Write1DMeshDiscretizationPoints(
                        branchIdx,
                        offset
                        );
                }
            }
            catch (Exception ex)
            {
                throw ex; // TODO: Rethrow the exception?
            }
         
        }
    }
}
