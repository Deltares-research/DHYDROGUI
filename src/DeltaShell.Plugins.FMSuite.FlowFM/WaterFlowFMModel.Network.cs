using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
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
        private IDiscretization networkDiscretization;
        private IFeatureCoverage inflows;
        private ICompartment previousCompartment;

        public const string DiscretizationObjectName = "Computational 1D Grid";

        public virtual bool DisableNetworkSynchronization { get; set; }

        [NoNotifyPropertyChange]
        public virtual IHydroNetwork Network
        {
            get {
                if (networkDataItem == null)
                    networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
                return (IHydroNetwork) GetDataItemValueByTag(WaterFlowFMModelDataSet.NetworkTag); }
            set
            {
                var networkItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);

                if (networkItem.Value != null)
                {
                    UnSubscribeFromNetwork(networkItem.Value as IHydroNetwork);
                }
                networkItem.Value = value;
                SubscribeToNetwork(value);
                
                // refresh data
                RefreshNetworkRelatedData();

            }
        }

        public void SubscribeToNetwork(IHydroNetwork network)
        {
            ((INotifyCollectionChanging) network).CollectionChanging += NetworkCollectionChanging;
            ((INotifyCollectionChanged) network).CollectionChanged += NetworkCollectionChanged;
            ((INotifyPropertyChanging) network).PropertyChanging += NetworkPropertyChanging;
            ((INotifyPropertyChanged) network).PropertyChanged += NetworkPropertyChanged;
            
            var hydroNetworkParent = network.Parent;
            fmRegion?.SubRegions?.Add(network);
            network.Parent = hydroNetworkParent;
            if (NetworkDiscretization != null) NetworkDiscretization.Network = network;
            UpdateRoughnessSections();
        }

        private void NetworkPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (DisableNetworkSynchronization || !(sender is ISewerConnection connection))
            {
                return;
            }

            if (string.Equals(e.PropertyName, nameof(connection.TargetCompartment)))
            {
                previousCompartment = connection.TargetCompartment;
            }

            if (string.Equals(e.PropertyName, nameof(connection.TargetCompartment)))
            {
                previousCompartment = connection.SourceCompartment;
            }
        }

        private void NetworkCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (DisableNetworkSynchronization)
                return;

            if (Equals(sender, Network.Branches) && e.Action == NotifyCollectionChangeAction.Remove && e.Item is IChannel channel)
            {
                NetworkDiscretization.ReplacePointsForRemovedBranch(channel);
            }

            if (SuspendClearOutputOnInputChange ||
                sender is IEventedList<Route>)
                return;

            if (!OutputIsEmpty)
            {
                OnClearOutput();
            }
        }

        public virtual void UnSubscribeFromNetwork(IHydroNetwork network)
        {
            ((INotifyCollectionChanging)network).CollectionChanging -= NetworkCollectionChanging;
            ((INotifyCollectionChanged) network).CollectionChanged -= NetworkCollectionChanged;
            ((INotifyPropertyChanging)network).PropertyChanging -= NetworkPropertyChanging;
            ((INotifyPropertyChanged) network).PropertyChanged -= NetworkPropertyChanged;
            
            var hydroNetworkParent = network.Parent;
            fmRegion?.SubRegions?.Remove(network);
            network.Parent = hydroNetworkParent;
            if (NetworkDiscretization != null) NetworkDiscretization.Network = null;
        }

        [EditAction]
        private void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DisableNetworkSynchronization) return;

            if (sender == Network && e.PropertyName == nameof(CoordinateSystem))
            {
                CoordinateSystem = Network.CoordinateSystem;
                Network.UpdateGeodeticDistancesOfChannels();
            }

            //is dit nodig?
            if (sender is IDataItem item && 
                item.Value is IHydroNetwork && 
                e.PropertyName == nameof(IDataItem.Value))
            {
                RefreshNetworkRelatedData();
            }

            if (sender is OutletCompartment outlet && e.PropertyName == nameof(OutletCompartment.SurfaceWaterLevel))
            {
                var model1DBoundaryNodeData = BoundaryConditions1D.FirstOrDefault(bc => bc.Node == outlet.ParentManhole);
                model1DBoundaryNodeData.SetBoundaryConditionDataForOutlet();
            }

            if (sender is ISewerConnection connection)
            {
                if (string.Equals(e.PropertyName, nameof(connection.TargetCompartment)))
                {
                    NetworkDiscretization.HandleCompartmentSwitch(previousCompartment, connection.TargetCompartment);
                    previousCompartment = null;
                }

                if (string.Equals(e.PropertyName, nameof(connection.SourceCompartment)))
                {
                    NetworkDiscretization.HandleCompartmentSwitch(previousCompartment, connection.SourceCompartment);
                    previousCompartment = null;
                }

                if (e.PropertyName.Equals(nameof(IBranch.Length)))
                {
                    // assuming connection only has 2 locations
                    var endLocation = NetworkDiscretization.GetLocationsForBranch(connection).FirstOrDefault(n => n.Chainage > 0);
                    if (endLocation != null)
                    {
                        endLocation.Chainage = connection.Length;
                    }
                    else
                    {
                        // add if missing
                        NetworkDiscretization.AddMissingLocationsForSewerConnections(connection);
                    }
                }
            }

            if (sender == Network && e.PropertyName == nameof(IEditableObject.IsEditing) && Network.CurrentEditAction is BranchSplitAction &&
                !Network.IsEditing && NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Any())
            {
                OnEndingBranchSplit((BranchSplitAction)Network.CurrentEditAction);
            }

            if (sender is IBranch branch)
            {
                if (string.Equals(e.PropertyName, nameof(IBranch.Source), StringComparison.InvariantCultureIgnoreCase))
                    ClearBoundaryConditionDataIfNodeIsNotAnEndNodeAnymore(branch.Source);
                else if (string.Equals(e.PropertyName, nameof(IBranch.Target), StringComparison.InvariantCultureIgnoreCase))
                    ClearBoundaryConditionDataIfNodeIsNotAnEndNodeAnymore(branch.Target);
            }
        }

        [InvokeRequired]
        private void ClearBoundaryConditionDataIfNodeIsNotAnEndNodeAnymore(INode node)
        {
            if (node == null)
            {
                return;
            }
            
            if (node.IncomingBranches.Count >= 1 
                && node.OutgoingBranches.Count >=1
                && BoundaryConditions1D.Any(bc =>
                    bc.DataType != Model1DBoundaryNodeDataType.None && Equals(bc.Feature, node)))
            {
                BoundaryConditions1D
                    .Where(bc =>
                        bc.DataType != Model1DBoundaryNodeDataType.None
                        && Equals(bc.Feature, node))
                    .ForEach(bc => bc.DataType = Model1DBoundaryNodeDataType.None);
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
                var networkLocations = locations.Except(NetworkDiscretization.Locations.GetValues()).ToList();
                networkLocations.RemoveAll(nwl => NetworkDiscretization.Locations.GetValues().Select(l=>l.Geometry.Coordinate).Contains(nwl.Geometry.Coordinate));
                NetworkDiscretization.Locations.AddValues(networkLocations);
                NetworkDiscretization.EndEdit();
            }
        }
        /// <summary>
        /// Called when a network is inserted into or linked to the model
        /// </summary>
        public void RefreshNetworkDataRelatedData()
        {
            if (NetworkDiscretization != null && NetworkDiscretization.Network != Network)
            {
                NetworkDiscretization.Network = Network;
                NetworkDiscretization.Clear();
                // geen locaties hier toevoegen?
                if (string.IsNullOrEmpty(NetworkDiscretization.Name))
                    NetworkDiscretization.Name = WaterFlowFMModel.DiscretizationObjectName;
            }

            if (Network != null)
            {
                // Update boundary conditions
                foreach (var node in Network.Nodes)
                {
                    AddBoundaryCondition(Helper1D.CreateDefaultBoundaryCondition(node, false, false));
                }

                // Update laterals
                foreach (var lateralSource in Network.LateralSources)
                {
                    AddLateralSourceData(new Model1DLateralSourceData
                    {
                        Feature = (LateralSource)lateralSource
                    });
                }

                // Update channel friction definitions
                ChannelFrictionDefinitions.AddRange(Network.Channels.Select(channel => new ChannelFrictionDefinition(channel)));

                // Update channel initial condition definitions
                ChannelInitialConditionDefinitions.AddRange(Network.Channels.Select(channel => new ChannelInitialConditionDefinition(channel)));
            }
        }
        
        public IDiscretization NetworkDiscretization 
        {
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
            if (sender != networkDiscretization || networkDiscretization.IsEditing || e.PropertyName != nameof(IEditableObject.IsEditing)) return;
            RefreshMappings();
        }
        
        public IEventedList<ChannelFrictionDefinition> ChannelFrictionDefinitions { get; private set; }
        public IEventedList<PipeFrictionDefinition> PipeFrictionDefinitions { get; private set; }

        public IEventedList<ChannelInitialConditionDefinition> ChannelInitialConditionDefinitions { get; private set; }

        public bool UseReverseRoughness { get; set; }
        public bool UseReverseRoughnessInCalculation { get; set; }
        public IEventedList<RoughnessSection> RoughnessSections { get; private set; }

        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes. Since this property
        ///   can be set/reset while the node was not part of the network it is necessary to monitor additions and removals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DisableNetworkSynchronization) return;

            // Manual call of OnInputCollectionChanged if the model is not owner of the network, i.e. the network is wrapped in a linked data item:
            if (GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag).LinkedTo != null)
            {
                OnInputCollectionChanged(sender, e);
            }

            // when node is added or removed - check if boundary conditions are updated
            var removedOrAddedItem = e.GetRemovedOrAddedItem();
            if (removedOrAddedItem is INode)
            {
                UpdateBoundaryCondition(e);
            }
            else if (removedOrAddedItem is LateralSource && !(Network.CurrentEditAction is BranchMergeAction))
            {
                UpdateLateralSource(e);
            }
            else if (removedOrAddedItem is CrossSectionSectionType)
            {
                UpdateCrossSectionSectionType(e);
                UpdateRoughnessSectionsEvent(e);
            }
            else if (Equals(sender, Network.Branches) && removedOrAddedItem is IBranch branch)
            {
                switch (branch)
                {
                    case IChannel channel:
                        HandleChannelsChanged(e.Action, channel);
                        break;
                    case ISewerConnection sewerConnection when !isLoading:
                        HandleSewerConnectionsChanged(e.Action, sewerConnection);
                        break;
                }
            }

            if (removedOrAddedItem is IPipe || removedOrAddedItem is IManhole)
            {
                AddSewerRoughnessIfNecessary();
            }

            if (e.Action == NotifyCollectionChangedAction.Add && 
                removedOrAddedItem is HydroLink hydroLink && 
                hydroLink.Source is Catchment && 
                hydroLink.Target is LateralSource lateralSource)
            {
                UpdateLateralSourceData(lateralSource);
            }

            // check if removed item is used in the child data items
            if (!(removedOrAddedItem is HydroLink) && removedOrAddedItem is IFeature && e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (removedOrAddedItem is INetworkFeature asNetworkFeature && asNetworkFeature.IsBeingMoved())
                {
                    return;
                }

                var childDataItems = AllDataItems
                                     .Where(di => di.Parent != null
                                                  && di.ValueConverter?.OriginalValue == removedOrAddedItem)
                                     .ToList();

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
        
        private void UpdateLateralSourceData(LateralSource lateralSource)
        {
            Model1DLateralSourceData lateralSourceData = LateralSourcesData.First(d => d.Feature == lateralSource);
            lateralSourceData.DataType = Model1DLateralDataType.FlowRealTime;
        }

        private void HandleSewerConnectionsChanged(NotifyCollectionChangedAction action, ISewerConnection sewerConnection)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    NamingHelper.MakeNamesUnique(Network.Branches);
                    NetworkDiscretization.AddMissingLocationsForSewerConnections(sewerConnection);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    NetworkDiscretization.ReplacePointsForRemovedBranch(sewerConnection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void HandleChannelsChanged(NotifyCollectionChangedAction action, IChannel channel)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var lateralSource in channel.BranchSources)
                    {
                        AddLateralSourceData(new Model1DLateralSourceData
                        {
                            Feature = lateralSource
                        });
                    }

                    ChannelFrictionDefinitions.Add(new ChannelFrictionDefinition(channel));
                    ChannelInitialConditionDefinitions.Add(new ChannelInitialConditionDefinition(channel));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var lateralSource in channel.BranchSources)
                    {
                        RemoveLateralSourceData(lateralSource);
                    }

                    ChannelFrictionDefinitions.Remove(ChannelFrictionDefinitions.First(cfd => ReferenceEquals(cfd.Channel, channel)));
                    ChannelInitialConditionDefinitions.Remove(ChannelInitialConditionDefinitions.First(cicd => ReferenceEquals(cicd.Channel, channel)));

                    // remove all child data items
                    var dataItemsToRemove = new List<IDataItem>();
                    var networkDataItemChildren = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag)?.Children;

                    if (networkDataItemChildren == null)
                    {
                        break;
                    }

                    foreach (var dataItem in networkDataItemChildren)
                    {
                        // check if child data item uses WaterFlowModelBranchFeatureValueConverter
                        if (!(dataItem.ValueConverter is Model1DBranchFeatureValueConverter valueConverter)
                            || !(valueConverter.Location is IBranchFeature branchFeature))
                        {
                            continue;
                        }

                        // check if data item is related to the removed branch
                        if (!channel.BranchFeatures.Contains(branchFeature))
                        {
                            continue;
                        }

                        dataItemsToRemove.Add(dataItem);
                    }

                    foreach (var dataItem in dataItemsToRemove)
                    {
                        dataItem.Unlink();
                        dataItem.LinkedBy.ToArray().ForEach(di => di.Unlink());
                        networkDataItemChildren.Remove(dataItem);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        /// <summary>
        /// Called when a network is inserted into or linked to the model
        /// </summary>
        [EditAction]
        private void RefreshNetworkRelatedData()
        {
            ClearOutput();
            ClearChannelFrictionDefinitions();
            ClearChannelInitialConditionDefinitions();
            RefreshNetworkDataRelatedData();
            
            // update network in output coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is INetworkCoverage)
                .Select(di => di.Value)
                .Cast<INetworkCoverage>()
                .ForEach(c => c.Network = Network);
            UpdateRoughnessSections();
        }

        
        /// <summary>
        /// Gets the boundary conditions for this model
        /// </summary>
        public IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D { get; private set; }

        private void ClearChannelFrictionDefinitions()
        {
            ChannelFrictionDefinitions.Clear();
        }

        private void ClearChannelInitialConditionDefinitions()
        {
            ChannelInitialConditionDefinitions.Clear();
        }

        /// <summary>
        /// Gets the lateral source data for this model
        /// </summary>
        public IEventedList<Model1DLateralSourceData> LateralSourcesData { get; private set; }

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
        private void UpdateCrossSectionSectionType(NotifyCollectionChangedEventArgs e)
        {
            var sectionType = (CrossSectionSectionType)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Add:
                    AddRoughnessSections(sectionType);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var roughnessSection = RoughnessSections.FirstOrDefault(rs => rs.CrossSectionSectionType.Name == sectionType.Name);
                    if (roughnessSection != null)
                    {
                        RemoveRoughnessSections(roughnessSection.CrossSectionSectionType);
                    }
                    break;
            }
        }
        private void RemoveRoughnessSections(CrossSectionSectionType crossSectionSectionType)
        {
            var roughnessSections = RoughnessSections.Where(rs => rs.CrossSectionSectionType == crossSectionSectionType).ToList();
            foreach (var section in roughnessSections) //can be multiple: normal and reverse
            {
                RemoveRoughnessSection(section);
            }
        }
        private void AddRoughnessSections(CrossSectionSectionType crossSectionSectionType)
        {
            var roughnessSection = RoughnessSections.FirstOrDefault(rs => string.Equals(rs.CrossSectionSectionType.Name, crossSectionSectionType?.Name, StringComparison.InvariantCultureIgnoreCase));
            if (roughnessSection != null) return;
            roughnessSection = new RoughnessSection(crossSectionSectionType, Network);
            AddRoughnessSection(roughnessSection);

            if (UseReverseRoughness)
            {
                AddRoughnessSection(new ReverseRoughnessSection(roughnessSection)
                {
                    UseNormalRoughness = !UseReverseRoughnessInCalculation
                });
            }
        }
        protected virtual void AddRoughnessSection(RoughnessSection roughnessSection)
        {
            RoughnessSections.Add(roughnessSection);
        }
        protected virtual void RemoveRoughnessSection(RoughnessSection roughnessSection)
        {
            RoughnessSections.Remove(roughnessSection);
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
            if (RoughnessSections.Any(rs => string.Equals(rs.CrossSectionSectionType.Name, crossSectionSectionType.Name, StringComparison.InvariantCultureIgnoreCase) && ReferenceEquals(rs.Network, Network))) return;
            var roughnessSection = new RoughnessSection(crossSectionSectionType, Network);
            RoughnessSections.Add(roughnessSection);
        }
        private void SynchronizeRoughnessSectionsWithNetwork()
        {
            if (Network == null) return;
            RoughnessSections?.ForEach(rs =>
            {
                rs.RoughnessNetworkCoverage.Clear();
                rs.RoughnessNetworkCoverage.Network = null;
                rs.Network = null;
            });
            RoughnessSections = null;
            RoughnessSections = new EventedList<RoughnessSection>();
            foreach (var crossSectionSectionType in Network.CrossSectionSectionTypes)
            {
                if (RoughnessSections.Any(rs => string.Equals(rs.CrossSectionSectionType.Name, crossSectionSectionType.Name, StringComparison.InvariantCultureIgnoreCase) && ReferenceEquals(rs.Network, Network))) continue;
                var roughnessSection = new RoughnessSection(crossSectionSectionType, Network);
                RoughnessSections.Add(roughnessSection);
            }
        }
        private bool settingSewerRoughness;

        private void AddSewerRoughnessIfNecessary()
        {
            var roughnessSection = RoughnessSections.FirstOrDefault(rs => string.Equals(rs.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (roughnessSection != null)
            {
                if (roughnessSection.GetDefaultRoughnessType() != RoughnessType.WhiteColebrook)
                {
                    roughnessSection.SetDefaultRoughnessType(RoughnessType.WhiteColebrook);
                    roughnessSection.SetDefaultRoughnessValue(0.003);
                }
                return;
            }

            if (settingSewerRoughness && !Network.Manholes.Any() && !Network.Pipes.Any()) return;
            settingSewerRoughness = true;

            var csSectionType = Network.CrossSectionSectionTypes.FirstOrDefault(csst => string.Equals(csst.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (csSectionType == null)
            {
                csSectionType = new CrossSectionSectionType { Name = RoughnessDataSet.SewerSectionTypeName };
                Network.CrossSectionSectionTypes.Add(csSectionType);
            }
            roughnessSection = RoughnessSections.FirstOrDefault(rs => string.Equals(rs.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase) && ReferenceEquals(rs.Network, Network));
            if (roughnessSection == null)
            {
                roughnessSection = new RoughnessSection(csSectionType, Network);
                roughnessSection.SetDefaultRoughnessType(RoughnessType.WhiteColebrook);
                roughnessSection.SetDefaultRoughnessValue(0.003);

                RoughnessSections.Insert(0, roughnessSection);
            }
            else if (roughnessSection.GetDefaultRoughnessType() != RoughnessType.WhiteColebrook)
            {
                roughnessSection.SetDefaultRoughnessType(RoughnessType.WhiteColebrook);
                roughnessSection.SetDefaultRoughnessValue(0.003);
            }
            settingSewerRoughness = false;
        }

        public virtual void UpdateRoughnessSections()
        {
            if (RoughnessSections != null
                && RoughnessSections.Any()
                && RoughnessSections.All(rs => ReferenceEquals(rs.Network, Network)))
            {
                return;
            }

            RoughnessSections?.ForEach(rs =>
            {
                rs.RoughnessNetworkCoverage.Clear();
                rs.RoughnessNetworkCoverage.Network = null;
                rs.Network = null;
            });

            if (Network != null)
            {
                SynchronizeRoughnessSectionsWithNetwork();
                AddSewerRoughnessIfNecessary();
            }
        }

        private void AddLateralSourceData(Model1DLateralSourceData lateralSourceData)
        {
            lateralSourceData.UseSalt = false;
            lateralSourceData.UseTemperature = false;

            LateralSourcesData.Add(lateralSourceData);
        }
        private void UpdateLateralSource(NotifyCollectionChangedEventArgs e)
        {
            var lateralSource = (LateralSource)e.GetRemovedOrAddedItem();
            if (lateralSource.IsBeingMoved() || isLoading) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Add:
                    Model1DLateralSourceData model1DLateralSourceData = CreateLateralSourceData(lateralSource);
                    NamingHelper.MakeNamesUnique(Network.LateralSources);
                    AddLateralSourceData(model1DLateralSourceData);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveLateralSourceData(lateralSource);
                    break;
            }
        }

        private static Model1DLateralSourceData CreateLateralSourceData(LateralSource lateralSource)
        {
            var lateralSourceData = new Model1DLateralSourceData {Feature = lateralSource};
            if (lateralSource.Branch is IPipe pipe)
            {
                lateralSourceData.Compartment = GetCompartment(pipe, lateralSource.Chainage);
            }

            return lateralSourceData;
        }

        private static ICompartment GetCompartment(ISewerConnection pipe, double chainage)
        {
            const double t = 0.000001;
            if (Math.Abs(chainage - 0) < t)
            {
                return pipe.SourceCompartment;
            }

            if (Math.Abs(chainage - pipe.Length) < t)
            {
                return pipe.TargetCompartment;
            }

            return null;
        }
        private void RemoveLateralSourceData(LateralSource lateralSource)
        {
            var lateralSourceData = LateralSourcesData?.FirstOrDefault(ls => ls.Feature == lateralSource);
            if (lateralSourceData == null) return;

            LateralSourcesData.Remove(lateralSourceData);
        }

        public virtual void ReplaceBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
            if (boundaryNodeData == null) return;
            var currentBC1DNode = BoundaryConditions1D.FirstOrDefault(bc1d => bc1d.Feature == boundaryNodeData.Feature);
            if (currentBC1DNode != null)
            {
                var currentIndex = BoundaryConditions1D.IndexOf(currentBC1DNode);
                BoundaryConditions1D.RemoveAt(currentIndex);
                BoundaryConditions1D.Insert(currentIndex, boundaryNodeData);
            }
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
                    var bc = Helper1D.CreateDefaultBoundaryCondition(node, UseSalinity, UseTemperature);
                    bc.SetBoundaryConditionDataForOutlet();
                    AddBoundaryCondition(bc);
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
    }
}
