using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
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
        private IDiscretization networkDiscretization;
        private IFeatureCoverage inflows;

        public const string DiscretizationObjectName = "Computational 1D Grid";

        [NoNotifyPropertyChange]
        public virtual IHydroNetwork Network
        {
            get { return (IHydroNetwork)GetDataItemValueByTag(WaterFlowFMModelDataSet.NetworkTag); }
            set
            {
                if (value == Network)
                {
                    return;
                }

                UnSubscribeFromNetwork();

                GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag).Value = value;

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
            var networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            var networkValue = networkDataItem.Value;
            if (networkValue != null)
            {
                ((INotifyCollectionChange)networkValue).CollectionChanged += NetworkCollectionChanged;
                ((INotifyPropertyChanged)networkValue).PropertyChanged += NetworkPropertyChanged;
                ((INotifyPropertyChanged)networkValue).PropertyChanged += NetworkCoordinateSystemPropertyChanged;
            }
            observedNetwork = (IHydroNetwork)networkValue;
        }

        private IHydroNetwork observedNetwork;
        private DataItemSet boundaryNodeDataItemSet;
        private DataItemSet lateralSourceDataItemSet;

        public virtual void UnSubscribeFromNetwork()
        {
            if (observedNetwork != null)
            {
                ((INotifyCollectionChange)observedNetwork).CollectionChanged -= NetworkCollectionChanged;
                ((INotifyPropertyChanged)observedNetwork).PropertyChanged -= NetworkPropertyChanged;
                ((INotifyPropertyChanged)observedNetwork).PropertyChanged -= NetworkCoordinateSystemPropertyChanged;
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
                Network.CrossSectionSectionTypes.Add(csSectionType);
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

        private void LoadNetworkAndDiscretisation()
        {
            if (!File.Exists(NetFilePath)) return;

            var nodeData = File.Exists(StorageNodeFilePath) 
                ? NodeFile.Read(StorageNodeFilePath) 
                : null;

            var loadedNetworkDiscretisation = UGridToNetworkAdapter.LoadNetworkAndDiscretisation(NetFilePath, nodeData);

            NetworkDiscretization = loadedNetworkDiscretisation;
            Network = (IHydroNetwork)loadedNetworkDiscretisation.Network;
        }

        /// <summary>
        /// Gets the boundary conditions for this model
        /// </summary>
        public IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D
        {
            get { return modelDefinition.BoundaryConditions1D; }
        }

        /// <summary>
        /// Gets the boundary conditions data item set for this model
        /// </summary>
        public virtual IDataItemSet BoundaryConditions1DDataItemSet
        {
            get { return boundaryNodeDataItemSet; }
        }

        /// <summary>
        /// Gets the boundary conditions data item set for this model
        /// </summary>
        private void AddBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
            BoundaryConditions1D.Add(boundaryNodeData);
        }
        private void UpdateBoundaryCondition(NotifyCollectionChangedEventArgs e)
        {
            var node = (INode)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Add:
                    AddBoundaryCondition(Helper1D.CreateDefaultBoundaryCondition(node, UseSalinity, UseTemperature));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveBoundaryCondition(node);
                    break;
            }
        }
        private void RemoveBoundaryCondition(INode hydroNode)
        {
            var boundaryCondition = BoundaryConditions1D.FirstOrDefault(bc => bc.Feature == hydroNode);
            if (boundaryCondition == null) return;

            RemoveBoundaryCondition(boundaryCondition);
        }
        private void RemoveBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
           BoundaryConditions1D.Remove(boundaryNodeData);
        }

        /// <summary>
        /// Replaces an existing boundary condition by <paramref name="boundaryNodeData"/>
        /// </summary>
        public virtual void ReplaceBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
            if (boundaryNodeData == null) return;
            var currentBC1DNode = BoundaryConditions1D.FirstOrDefault(bc1d => bc1d.Feature == boundaryNodeData.Feature);
            if (currentBC1DNode != null)
            {
                var currentIndex = BoundaryConditions1D.IndexOf(boundaryNodeData);
                BoundaryConditions1D.RemoveAt(currentIndex);
                BoundaryConditions1D.Insert(currentIndex, boundaryNodeData);
            }
        }

        private void AddLateralSourceData(Model1DLateralSourceData lateralSourceData)
        {
            if (lateralSourceData == null) return;
            lateralSourceData.UseSalt = UseSalinity;
            lateralSourceData.UseTemperature = UseTemperature;

            lateralSourceDataItemSet.DataItems.Add(new DataItem(lateralSourceData));
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
            var dataItemSet = lateralSourceDataItemSet;
            var dataItem = dataItemSet.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, lateralSourceData));

            if (dataItem == null) return;

            dataItemSet.DataItems.Remove(dataItem);
        }
        private void ClearLateralSourceData()
        {
            lateralSourceDataItemSet.DataItems.Clear();
        }
        private void ClearBoundaryConditions()
        {
            boundaryNodeDataItemSet.DataItems.Clear();
        }
        /// <summary>
        /// Gets the lateral source data for this model
        /// </summary>
        public virtual IEventedList<Model1DLateralSourceData> LateralSourcesData
        {
            get { return lateralSourceDataItemSet.AsEventedList<Model1DLateralSourceData>(); }
            private set { lateralSourceDataItemSet.Value = value; ; }
        }

        /// <summary>
        /// Gets the lateral sources data item set for this model
        /// </summary>
        public virtual IDataItemSet LateralSourcesDataItemSet
        {
            get { return lateralSourceDataItemSet; }
        }

        [NoNotifyPropertyChange]
        public virtual IFeatureCoverage Inflows
        {
            get { return inflows; }
            private set { inflows = value; }
        }

        private void AddInflowsDataItem()
        {
            inflows = new FeatureCoverage("Inflows");
            inflows.Arguments.Add(new Variable<DateTime>()); //time variable
            inflows.Arguments.Add(new Variable<IFeature> { IsAutoSorted = false }); //feature variable
            inflows.Components.Add(new Variable<double>("Inflows", new Unit("Discharge", "m³/s"))); //component
        }
        public virtual INetworkCoverage OutputWaterLevel1D
        {
            get { return (INetworkCoverage)RetrieveOutputFunctionByDataItemTag(Model1DParameterNames.LocationWaterLevel); }
        }
        private IFunction RetrieveOutputFunctionByDataItemTag(string tag)
        {
            var dataItem = DataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is IFunction).FirstOrDefault(di => di.Tag == tag);
            if (dataItem == null)
            {
                return null;
            }
            return (IFunction)dataItem.Value;
        }

    }
}
