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

                UnSubscribeFromNetwork(Network);
                GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag).Value = value;
                SubscribeToNetwork(Network);
                
                // refresh data
                //moet dit ? RefreshNetworkRelatedData();

            }
        }
        private void SubscribeToNetwork(IHydroNetwork network)
        {
            if (network != null)
            {
                ((INotifyCollectionChange) network).CollectionChanged += NetworkCollectionChanged;
                ((INotifyPropertyChanged) network).PropertyChanged += NetworkPropertyChanged;
                
                var hydroNetworkParent = network.Parent;
                fmRegion?.SubRegions?.Add(network);
                network.Parent = hydroNetworkParent;
            }
        }

        public virtual void UnSubscribeFromNetwork(IHydroNetwork network)
        {
            if (network != null)
            {
                ((INotifyCollectionChange) network).CollectionChanged -= NetworkCollectionChanged;
                ((INotifyPropertyChanged) network).PropertyChanged -= NetworkPropertyChanged;

                var hydroNetworkParent = network.Parent;
                fmRegion?.SubRegions?.Remove(network);
                network.Parent = hydroNetworkParent;
            }
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

        public IEventedList<RoughnessSection> RoughnessSections
        {
            get { return ModelDefinition?.RoughnessSections; }
        }
        public bool UseReverseRoughness { get; set; }
        public bool UseReverseRoughnessInCalculation { get; set; }
        
        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes. Since this property
        ///   can be set/reset while the node was not part of the network it is necessary to monitor additions and removals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.GetRemovedOrAddedItem() is IChannel)
            {
                if (Equals(sender, Network.Branches))
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                        {
                            // remove all child data items
                            var dataItemsToRemove = new List<IDataItem>();
                            var networkDataItem = GetDataItemByValue(Network);

                            if (networkDataItem != null)
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
                        }
                    }

                    ClearOutput();
                }
                // check if removed item is used in the child data items
                else if (e.GetRemovedOrAddedItem() is IFeature && e.Action == NotifyCollectionChangedAction.Remove)
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
            ModelDefinition.RefreshNetworkRelatedData();
            
            // update network in output coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is INetworkCoverage)
                .Select(di => di.Value)
                .Cast<INetworkCoverage>()
                .ForEach(c => c.Network = Network);
            ModelDefinition.UpdateRoughnessSections();
        }
        
        /// <summary>
        /// Gets the boundary conditions for this model
        /// </summary>
        public IEnumerable<Model1DBoundaryNodeData> BoundaryConditions1D
        {
            get { return ModelDefinition.BoundaryConditions1D; }
        }

        /// <summary>
        /// Gets the boundary conditions data item set for this model
        /// </summary>
        public virtual IDataItemSet BoundaryConditions1DDataItemSet
        {
            get { return boundaryNodeDataItemSet; }
        }

        /// <summary>
        /// Replaces an existing boundary condition by <paramref name="boundaryNodeData"/>
        /// </summary>
        public virtual void ReplaceBoundaryCondition(Model1DBoundaryNodeData boundaryNodeData)
        {
            ModelDefinition.ReplaceBoundaryCondition(boundaryNodeData);
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
            get { return ModelDefinition.LateralSourcesData; }
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
