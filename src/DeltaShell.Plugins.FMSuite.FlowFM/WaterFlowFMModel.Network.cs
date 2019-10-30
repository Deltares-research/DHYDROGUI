using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel : IModelWithRoughnessSections
    {
        private const string NetworkObjectName = "Network";
        private IHydroNetwork network;
        private IDiscretization networkDiscretization;
        private IEventedList<Model1DBoundaryNodeData> boundaryConditions1D;
        private IEventedList<Model1DLateralSourceData> lateralSourcesData;

        public const string DiscretizationObjectName = "Computational 1D Grid";

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
            if (sender != networkDiscretization || e.PropertyName != nameof(IEditableObject.IsEditing)) return;
            if(((Discretization)sender).IsEditing) return;
            RefreshMappings();
        }

        private void SubscribeToNetwork()
        {
            if (network != null)
            {
                ((INotifyCollectionChange)network).CollectionChanged += NetworkCollectionChanged;
                ((INotifyPropertyChanged)network).PropertyChanged += NetworkPropertyChanged;
                ((INotifyPropertyChanged)network).PropertyChanged += NetworkCoordinateSystemPropertyChanged;
            }
        }

        private void UnSubscribeFromNetwork()
        {
            if (network != null)
            {
                ((INotifyCollectionChange)network).CollectionChanged -= NetworkCollectionChanged;
                ((INotifyPropertyChanged)network).PropertyChanged -= NetworkPropertyChanged;
                ((INotifyPropertyChanged)network).PropertyChanged -= NetworkCoordinateSystemPropertyChanged;
            }
        }

        public IEventedList<RoughnessSection> RoughnessSections { get; private set; }
        public bool UseReverseRoughness { get; set; }
        public bool UseReverseRoughnessInCalculation { get; set; }
        private void SynchronizeRoughnessSectionsWithNetwork()
        {
            if (Network == null) return;
            RoughnessSections = null;
            RoughnessSections = new EventedList<RoughnessSection>();
            foreach (var crossSectionSectionType in Network.CrossSectionSectionTypes)
            {
                var roughnessSection = new RoughnessSection(crossSectionSectionType, Network);
                RoughnessSections.Add(roughnessSection);
            }
        }
        
        private void AddSewerRoughnessIfNecessary()
        {
            var roughnessSection = RoughnessSections.FirstOrDefault(rs => rs.Name == RoughnessDataSet.SewerSectionTypeName);
            if ( roughnessSection != null )
            {
                roughnessSection.SetDefaultRoughnessType(RoughnessType.WhiteColebrook);
                roughnessSection.SetDefaultRoughnessValue(0.003);
                return;
            }
            
            var csSectionType = Network.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name == RoughnessDataSet.SewerSectionTypeName);
            if (csSectionType == null)
            {
                csSectionType = new CrossSectionSectionType {Name = RoughnessDataSet.SewerSectionTypeName};
                network.CrossSectionSectionTypes.Add(csSectionType);
            }roughnessSection = new RoughnessSection(csSectionType, Network);
            roughnessSection.SetDefaultRoughnessType(RoughnessType.WhiteColebrook);
            roughnessSection.SetDefaultRoughnessValue(0.003);

            RoughnessSections.Insert(0, roughnessSection);
        }

        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes. Since this property
        ///   can be set/reset while the node was not part of the network it is necessary to monitor additions and removals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // when node is added or removed - check if boundary conditions are updated
            if (e.GetRemovedOrAddedItem() is INode)
            {
                UpdateBoundaryCondition(e);
            }
            else if (e.GetRemovedOrAddedItem() is LateralSource && !(Network.CurrentEditAction is BranchMergeAction))
            {
                UpdateLateralSource(e);
            }
            else if (e.GetRemovedOrAddedItem() is IChannel)
            {
                if (Equals(sender, Network.Branches))
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                            // remove all child data items
                            var dataItemsToRemove = new List<IDataItem>();
                            var networkDataItem = GetDataItemByValue(Network);

                            if (networkDataItem != null) // TODO: temporary fix while we do not have a DataItem for Network in FM Model
                            {
                                dataItemsToRemove.AddRange(networkDataItem.Children);

                                foreach (var dataItem in dataItemsToRemove)
                                {
                                    dataItem.Unlink();
                                    dataItem.LinkedBy.ToArray().ForEach(di => di.Unlink());
                                    networkDataItem.Children.Remove(dataItem);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Add:
                        
                            var channel = (IChannel)e.GetRemovedOrAddedItem();
                            foreach (var lateralSource in channel.BranchSources)
                            {
                                AddLateralSourceData(new Model1DLateralSourceData { Feature = lateralSource });
                            }
                            break;
                    }

                    ClearOutput();
                }
            }
            else if (Equals(sender, Network.Branches) && e.GetRemovedOrAddedItem() is ISewerConnection)
            {
                var sewerConnection = e.GetRemovedOrAddedItem() as SewerConnection;
                if (sewerConnection?.Length > 0)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(new NetworkLocation(sewerConnection, 0.0));
                            AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(new NetworkLocation(sewerConnection, sewerConnection.Length));
                            break;
                    }
                }
            }
            else if (e.GetRemovedOrAddedItem() is CrossSectionSectionType)
            {
                UpdateRoughnessSectionsEvent(e);
            }

            // check if removed item is used in the child data items
            if (e.GetRemovedOrAddedItem() is IFeature && e.Action == NotifyCollectionChangedAction.Remove)
            {
                var asNetworkFeature = e.GetRemovedOrAddedItem() as INetworkFeature;
                if (asNetworkFeature != null && asNetworkFeature.IsBeingMoved())
                {
                    return;
                }

                var childDataItems =
                    AllDataItems.Where(
                        di =>
                            di.Parent != null && di.ValueConverter != null &&
                            di.ValueConverter.OriginalValue == e.GetRemovedOrAddedItem()).ToList();

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

        private void AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(NetworkLocation toLocation)
        {
            if (!NetworkDiscretization.Locations.Values.Any(l =>
                l.Geometry.Coordinate.Equals(toLocation.Geometry.Coordinate)))
            {
                NetworkDiscretization.Locations.AddValues(new[] {toLocation});
            }
        }

        private void UpdateRoughnessSectionsEvent(NotifyCollectionChangedEventArgs e)
        {
            var sectionType = (CrossSectionSectionType)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNewRoughnessSection(sectionType);
                    break;
            }
        }

        private void AddNewRoughnessSection(CrossSectionSectionType crossSectionSectionType)
        {
            var roughnessSection = new RoughnessSection(crossSectionSectionType, Network);
            RoughnessSections.Add(roughnessSection);
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
                if (e.PropertyName == nameof(IDataItem.Value))
                {
                    RefreshNetworkRelatedData();
                }
            }

            if (sender == Network && e.PropertyName == nameof(IEditableObject.IsEditing) && Network.CurrentEditAction is BranchSplitAction &&
                !Network.IsEditing && NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Any())
            {
                OnEndingBranchSplit((BranchSplitAction)Network.CurrentEditAction);
            }
        }
        /// <summary>
        /// - Synchronize the coordinate system in the model with the network coordinate system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void NetworkCoordinateSystemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == Network && e.PropertyName == nameof(CoordinateSystem))
            {
                CoordinateSystem = Network.CoordinateSystem;
                Network.UpdateGeodeticDistancesOfChannels();
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
            // update boundary conditions
            ClearBoundaryConditions();
            if (Network != null)
            {
                foreach (var node in Network.Nodes)
                {
                    AddBoundaryCondition(Helper1D.CreateDefaultBoundaryCondition(node, UseSalinity, UseTemperature));
                }
            }

            // update laterals
            ClearLateralSourceData();
            if (Network != null)
            {
                foreach (var lateralSource in Network.LateralSources)
                {
                    AddLateralSourceData(new Model1DLateralSourceData { Feature = (LateralSource)lateralSource });
                }
            }

            // update network in output coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is INetworkCoverage)
                .Select(di => di.Value)
                .Cast<INetworkCoverage>()
                .ForEach(c => c.Network = Network);
            UpdateRoughnessSections();
        }

        public virtual void UpdateRoughnessSections()
        {
            if (RoughnessSections != null) RoughnessSections.ForEach(rs => rs.Network = null);

            if (Network != null)
            {
                SynchronizeRoughnessSectionsWithNetwork();
                AddSewerRoughnessIfNecessary();
            }
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

        /// <summary>
        /// Gets the boundary conditions for this model
        /// </summary>
        public IEnumerable<Model1DBoundaryNodeData> BoundaryConditions1D
        {
            get { return boundaryConditions1D; }
            private set { boundaryConditions1D = value as IEventedList<Model1DBoundaryNodeData>; }
        }

        /// <summary>
        /// Gets the boundary conditions data item set for this model
        /// </summary>
        private void AddBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
            boundaryConditions1D?.Add(boundaryNodeData);
        }
        private void UpdateBoundaryCondition(NotifyCollectionChangedEventArgs e)
        {
            var node = (INode)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Add:
                    AddBoundaryCondition(WaterFlowModel1DHelper.CreateDefaultBoundaryCondition(node, UseSalinity, UseTemperature));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveBoundaryCondition(node);
                    break;
            }
        }
        private void RemoveBoundaryCondition(INode hydroNode)
        {
            var boundaryCondition = boundaryConditions1D?.FirstOrDefault(bc => bc.Feature == hydroNode);
            if (boundaryCondition == null) return;

            RemoveBoundaryCondition(boundaryCondition);
        }
        private void RemoveBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
            boundaryConditions1D?.Remove(boundaryNodeData);
        }

        private void AddLateralSourceData(Model1DLateralSourceData lateralSourceData)
        {
            if (lateralSourceData == null) return;
            lateralSourceData.UseSalt = UseSalinity;
            lateralSourceData.UseTemperature = UseTemperature;
            lateralSourcesData.Add(lateralSourceData);
        }
        private void UpdateLateralSource(NotifyCollectionChangedEventArgs e)
        {
            var lateralSource = (LateralSource)e.GetRemovedOrAddedItem();
            if (lateralSource.IsBeingMoved()) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Add:
                    AddLateralSourceData(new Model1DLateralSourceData { Feature = lateralSource });
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveLateralSourceData(lateralSource);
                    break;
            }
        }
        private void RemoveLateralSourceData(LateralSource lateralSource)
        {
            var lateralSourceData = LateralSourcesData?.FirstOrDefault(ls => ls.Feature == lateralSource);
            if (lateralSourceData == null) return;

            RemoveLateralSourceData(lateralSourceData);
        }
        private void RemoveLateralSourceData(Model1DLateralSourceData lateralSourceData)
        {
            lateralSourcesData?.Remove(lateralSourceData);
        }
        private void ClearLateralSourceData()
        {
            lateralSourcesData?.Clear();
        }
        private void ClearBoundaryConditions()
        {
            boundaryConditions1D?.Clear();
        }
        /// <summary>
        /// Gets the lateral source data for this model
        /// </summary>
        public virtual IEnumerable<Model1DLateralSourceData> LateralSourcesData
        {
            get { return lateralSourcesData; }
            private set { lateralSourcesData = value as IEventedList<Model1DLateralSourceData>; }
        }

    }
}
