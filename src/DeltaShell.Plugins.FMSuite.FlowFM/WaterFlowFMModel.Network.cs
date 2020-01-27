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
using DelftTools.Hydro.Structures;
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
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel : IModelWithRoughnessSections
    {
        private IDiscretization networkDiscretization;
        private IFeatureCoverage inflows;

        public const string DiscretizationObjectName = "Computational 1D Grid";

        [NoNotifyPropertyChange]
        public virtual IHydroNetwork Network
        {
            get { return (IHydroNetwork) GetDataItemValueByTag(WaterFlowFMModelDataSet.NetworkTag); }
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
                //moet dit ? RefreshNetworkRelatedData();

            }
        }

        private void SubscribeToNetwork()
        {
            ((INotifyCollectionChanged) Network).CollectionChanged += NetworkCollectionChanged;
            ((INotifyPropertyChanged) Network).PropertyChanged += NetworkPropertyChanged;
            ((INotifyPropertyChanged) Network).PropertyChanged += NetworkCoordinateSystemPropertyChanged;

            var hydroNetworkParent = Network.Parent;
            fmRegion?.SubRegions?.Add(Network);
            Network.Parent = hydroNetworkParent;
            if (NetworkDiscretization != null) NetworkDiscretization.Network = Network;
            RoughnessSections?.ForEach(rs => rs.Network = Network);
        }

        public virtual void UnSubscribeFromNetwork()
        {

            ((INotifyCollectionChanged) Network).CollectionChanged -= NetworkCollectionChanged;
            ((INotifyPropertyChanged) Network).PropertyChanged -= NetworkPropertyChanged;
            ((INotifyPropertyChanged) Network).PropertyChanged -= NetworkCoordinateSystemPropertyChanged;

            var hydroNetworkParent = Network.Parent;
            fmRegion?.SubRegions?.Remove(Network);
            Network.Parent = hydroNetworkParent;
            if (NetworkDiscretization != null) NetworkDiscretization.Network = null;
        }

        [EditAction]
        private void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //is dit nodig?
            if (sender is IDataItem && ((IDataItem)sender).Value is IHydroNetwork)
            {
                if (e.PropertyName == nameof(IDataItem.Value))
                {
                    RefreshNetworkRelatedData();
                }
            }

            if (sender is OutletCompartment && e.PropertyName == "SurfaceWaterLevel")
            {
                var outlet = (OutletCompartment) sender;
                var model1DBoundaryNodeData = BoundaryConditions1D.FirstOrDefault(bc => bc.Node == outlet.ParentManhole);
                SetBoundaryConditionDataForOutlet(model1DBoundaryNodeData);
            }

            if (sender == Network && e.PropertyName == nameof(IEditableObject.IsEditing) && Network.CurrentEditAction is BranchSplitAction &&
                !Network.IsEditing && NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Any())
            {
                OnEndingBranchSplit((BranchSplitAction)Network.CurrentEditAction);
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
        /// <summary>
        /// Called when a network is inserted into or linked to the model
        /// </summary>
        public void RefreshNetworkDataRelatedData()
        {
            if (NetworkDiscretization != null && NetworkDiscretization.Network != Network)
            {
                NetworkDiscretization.Network = Network;
                NetworkDiscretization.Clear();
                if (string.IsNullOrEmpty(NetworkDiscretization.Name))
                    NetworkDiscretization.Name = WaterFlowFMModel.DiscretizationObjectName;
            }

            // update boundary conditions
            if (Network != null)
            {
                foreach (var node in Network.Nodes)
                {
                    AddBoundaryCondition(Helper1D.CreateDefaultBoundaryCondition(node, false, false));
                }
            }
            // update laterals
            if (Network != null)
            {
                foreach (var lateralSource in Network.LateralSources)
                {
                    AddLateralSourceData(new Model1DLateralSourceData { Feature = (LateralSource)lateralSource });
                }
            }

        }
        private void AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(NetworkLocation toLocation)
        {
            var locations = new HashSet<Coordinate>(NetworkDiscretization.Locations.Values.Select(l => l.Geometry?.Coordinate));
            var locationGeometry = toLocation.Geometry;
            if (locationGeometry == null) Log.Warn($"No geometry set for {toLocation.Name}");
            if (!locations.Contains(locationGeometry?.Coordinate))
            {
                NetworkDiscretization.Locations.AddValues(new[] { toLocation });
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
        
        private DataItemSet boundaryNodeDataItemSet;
        private DataItemSet lateralSourceDataItemSet;

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
            else if (e.GetRemovedOrAddedItem() is LateralSource && !(Network.CurrentEditAction is BranchMergeAction))
            {
                UpdateLateralSource(e);
            }
            else if (e.GetRemovedOrAddedItem() is CrossSectionSectionType)
            {
                UpdateCrossSectionSectionType(e);
            }
            else if (removedOrAddedItem is IChannel)
            {
                if (Equals(sender, Network.Branches))
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                        {
                            var channel = (IChannel) e.GetRemovedOrAddedItem();
                            foreach (var lateralSource in channel.BranchSources)
                            {
                                RemoveLateralSourceData(lateralSource);
                            }

                            // remove all child data items
                            var dataItemsToRemove = new List<IDataItem>();
                            var networkDataItem = GetDataItemByValue(Network);

                            if (networkDataItem != null)
                            {
                                foreach (var dataItem in networkDataItem.Children)
                                {
                                    // check if child data item uses WaterFlowModelBranchFeatureValueConverter
                                    var valueConverter = dataItem.ValueConverter as Model1DBranchFeatureValueConverter;
                                    if (valueConverter == null || !(valueConverter.Location is IBranchFeature))
                                    {
                                        continue;
                                    }

                                    // check if data item is related to the removed branch
                                    var branchFeature = (IBranchFeature) valueConverter.Location;
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
                                    networkDataItem.Children.Remove(dataItem);
                                }
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Add:
                        {
                            var channel = (IChannel) e.GetRemovedOrAddedItem();
                            foreach (var lateralSource in channel.BranchSources)
                            {
                                AddLateralSourceData(new Model1DLateralSourceData {Feature = lateralSource});
                            }

                            break;
                        }
                    }
                    ClearOutput();
                }
            }
            else if (Equals(sender, Network.Branches) && removedOrAddedItem is ISewerConnection)
            {
                var sewerConnection = removedOrAddedItem as SewerConnection;
                if (sewerConnection?.Length > 0)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(
                                new NetworkLocation(sewerConnection, 0.0));
                            AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(
                                new NetworkLocation(sewerConnection, sewerConnection.Length));
                            break;
                    }
                }
            }
            else if (removedOrAddedItem is CrossSectionSectionType)
            {
                UpdateRoughnessSectionsEvent(e);
            }

            if (removedOrAddedItem is IPipe || removedOrAddedItem is IManhole)
            {
                AddSewerRoughnessIfNecessary();
            }

            
            // check if removed item is used in the child data items
            if (removedOrAddedItem is IFeature && e.Action == NotifyCollectionChangedAction.Remove)
            {
                var asNetworkFeature = removedOrAddedItem as INetworkFeature;
                if (asNetworkFeature != null && asNetworkFeature.IsBeingMoved())
                {
                    return;
                }

                var childDataItems =
                    AllDataItems.Where(
                        di =>
                            di.Parent != null && di.ValueConverter != null &&
                            di.ValueConverter.OriginalValue == removedOrAddedItem).ToList();

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
            ClearBoundaryConditions();
            ClearLateralSourceData();
            RefreshNetworkDataRelatedData();
            
            // update network in output coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is INetworkCoverage)
                .Select(di => di.Value)
                .Cast<INetworkCoverage>()
                .ForEach(c => c.Network = Network);
            UpdateRoughnessSections();
        }

        private void RefreshBoundaryConditions1DDataItemSet()
        {
            foreach (var model1DBoundaryNodeData in BoundaryConditions1D.Where(bc1d =>
                bc1d.DataType != Model1DBoundaryNodeDataType.None))
            {
                var bcDataItem = boundaryNodeDataItemSet.DataItems.FirstOrDefault(di =>
                {
                    var boundaryNodeData = (di.Value as Model1DBoundaryNodeData);
                    if (boundaryNodeData == null) return false;

                    return model1DBoundaryNodeData.Feature.Equals(boundaryNodeData.Feature);
                });
                if (bcDataItem != null)
                {
                    bcDataItem.Value = model1DBoundaryNodeData;
                    bcDataItem.Hidden = model1DBoundaryNodeData.DataType == Model1DBoundaryNodeDataType.None;
                }
            }
        }

        /// <summary>
        /// Gets the boundary conditions for this model
        /// </summary>
        public IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D
        {
            get { return boundaryConditions1D; }
            private set
            {
                if (boundaryConditions1D != null)
                {
                    BoundaryConditions1D.CollectionChanged -= BoundaryConditions1DOnCollectionChanged;
                    ((INotifyPropertyChanged)(BoundaryConditions1D)).PropertyChanged -= BoundaryConditions1DOnPropertyChanged;
                }
                boundaryConditions1D = value;

                if (boundaryConditions1D != null)
                {
                    BoundaryConditions1D.CollectionChanged += BoundaryConditions1DOnCollectionChanged;
                    ((INotifyPropertyChanged)(BoundaryConditions1D)).PropertyChanged += BoundaryConditions1DOnPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Gets the boundary conditions data item set for this model
        /// </summary>
        public virtual IDataItemSet BoundaryConditions1DDataItemSet
        {
            get { return boundaryNodeDataItemSet; }
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
        public IEventedList<Model1DLateralSourceData> LateralSourcesData
        {
            get { return lateralSourcesData; }
            private set
            {
                if (lateralSourcesData != null)
                {
                    lateralSourcesData.CollectionChanged -= LateralSourceDatasOnCollectionChanged;
                }
                lateralSourcesData = value;
                if (lateralSourcesData != null)
                {
                    lateralSourcesData.CollectionChanged += LateralSourceDatasOnCollectionChanged;
                }
            }
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
        private IEventedList<Model1DBoundaryNodeData> boundaryConditions1D;
        private IEventedList<Model1DLateralSourceData> lateralSourcesData;

        private void AddSewerRoughnessIfNecessary()
        {
            var roughnessSection = RoughnessSections.FirstOrDefault(rs => string.Equals(rs.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase) && ReferenceEquals(rs.Network, Network));
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
            if (lateralSourceData == null) return;
            lateralSourceData.UseSalt = false;
            lateralSourceData.UseTemperature = false;

            LateralSourcesData.Add(lateralSourceData);
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
                    var bc = Helper1D.CreateDefaultBoundaryCondition(node, false, false);
                    SetBoundaryConditionDataForOutlet(bc);
                    AddBoundaryCondition(bc);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveBoundaryCondition(node);
                    break;
            }
        }

        private void SetBoundaryConditionDataForOutlet(Model1DBoundaryNodeData bc)
        {
            var manhole = bc?.Node as Manhole;
            if (manhole != null && manhole.Compartments.OfType<OutletCompartment>().Any())
            {
                var outlet = manhole.Compartments.OfType<OutletCompartment>().First();
                bc.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                bc.WaterLevel = outlet.SurfaceWaterLevel;
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
