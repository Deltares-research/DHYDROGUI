using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffBasinSynchronizer
    {
        private IDrainageBasin subscribedBasin;
        private IDataItem subscribedBasinDataItem;
        private bool linking;
        private IDrainageBasin preLinkBasin;

        public RainfallRunoffBasinSynchronizer(RainfallRunoffModel model)
        {
            Model = model;
            ((INotifyPropertyChanged)Model).PropertyChanged += RainfallRunoffPropertyChanged;
            AfterDataItemsSet();
        }

        private void RainfallRunoffPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // deal with undo/redo of linking..yes..I know
            if (!EditActionSettings.Disabled || 
                (e.PropertyName != "Value" && e.PropertyName != "LinkedTo") ||
                !Equals(sender, subscribedBasinDataItem)) return;

            Subscribe();
        }

        private RainfallRunoffModel Model { get; set; }

        private IDrainageBasin Basin
        {
            get { return Model.Basin; }
        }

        public bool IsDifferentBasin(IDrainageBasin otherBasin)
        {
            return otherBasin != subscribedBasin;
        }

        private void OnCatchmentAdded(Catchment catchment)
        {
            catchment.AddDefaultModelDataForCatchment(Model);
        }

        private void OnCatchmentRemoved(Catchment catchment)
        {
            RemoveModelDataForCatchment(catchment);
        }

        public void BeforeDataItemsSet()
        {
            Unsubscribe();
            UnsubscribeDataItem();
        }

        private void UnsubscribeDataItem()
        {
            if (subscribedBasinDataItem != null)
            {
                ((INotifyPropertyChanged) subscribedBasinDataItem).PropertyChanged -= DataItemBasinPropertyChanged;
                subscribedBasinDataItem.Linking -= BasinDataItemLinking;
                subscribedBasinDataItem.Linked -= BasinDataItemLinked;
            }
            subscribedBasinDataItem = null;
        }
        
        void BasinDataItemLinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            preLinkBasin = subscribedBasin;
            linking = true;
            Unsubscribe();
        }

        void BasinDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            if (e.Relinking && preLinkBasin != null)
            {
                RainfallRunoffModel.RefreshBasinRelatedData(Model, preLinkBasin);
                Subscribe();
            }
            else
            {
                FullRefresh();
            }
            
            linking = false;
        }

        private void Unsubscribe()
        {
            if (subscribedBasin != null)
            {
                ((INotifyCollectionChanged) subscribedBasin).CollectionChanged -= BasinCollectionChanged;
                ((INotifyPropertyChanged) subscribedBasin).PropertyChanged -= BasinPropertyChanged;
            }
            subscribedBasin = null;
        }

        public void AfterDataItemsSet()
        {
            Subscribe();
            SubscribeToDataItem();
        }

        private void SubscribeToDataItem()
        {
            if (subscribedBasinDataItem != null)
            {
                UnsubscribeDataItem();
            }

            // dataitem
            subscribedBasinDataItem = Model.GetDataItemByTag(RainfallRunoffModelDataSet.BasinTag);
            ((INotifyPropertyChanged) subscribedBasinDataItem).PropertyChanged += DataItemBasinPropertyChanged;
            subscribedBasinDataItem.Linking += BasinDataItemLinking;

            subscribedBasinDataItem.Linked += BasinDataItemLinked;
        }

        private void Subscribe()
        {
            if (subscribedBasin != null)
            {
                Unsubscribe();
            }

            if (Basin != null)
            {
                ((INotifyCollectionChanged) Basin).CollectionChanged += BasinCollectionChanged;
                ((INotifyPropertyChanged) Basin).PropertyChanged += BasinPropertyChanged;
            }
            subscribedBasin = Basin;
        }

        private void FullRefresh()
        {
            BeforeDataItemsSet();
            RefreshModelAfterBasinReplace();
            AfterDataItemsSet();
        }

        [EditAction]
        private void DataItemBasinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(Equals(sender, subscribedBasinDataItem))) 
                return;

            if (e.PropertyName == "Value" && !linking)
            {
                FullRefresh(); //should automatically clear output
            }
        }

        [EditAction]
        void BasinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var catchment = sender as Catchment;
            if (catchment != null && e.PropertyName == nameof(catchment.CatchmentType))
            {
                var currentData = Model.GetCatchmentModelData(catchment);
                if (!catchment.IsModelDataCompatible(currentData))
                {
                    RemoveModelDataForCatchment(catchment);
                    catchment.AddDefaultModelDataForCatchment(Model);
                }
            }

            if (sender is IDrainageBasin basin && e.PropertyName == nameof(basin.CoordinateSystem))
            {
                Model.OutputCoverages.ForEach(c => c.CoordinateSystem = basin?.CoordinateSystem);
            }
        }

        [EditAction]
        private void BasinCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is IEventedList<Catchment> catchmentList && 
                !catchmentList.SkipChildItemEventBubbling)
            {
                //not aggregation list
                CatchmentsCollectionChanged(sender, e);
            }

            if (Equals(sender, Basin.Boundaries))
            {
                BoundariesCollectionChanged(e);
            }
        }

        private void CatchmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var catchment = (Catchment) e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCatchmentAdded(catchment);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnCatchmentRemoved(catchment);
                    break;
            }
        }

        private void BoundariesCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var runoffBoundary = (RunoffBoundary)e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnBoundaryAdded(runoffBoundary);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnBoundaryRemoved(runoffBoundary);
                    break;
            }
        }

        private void OnBoundaryAdded(RunoffBoundary runoffBoundary)
        {
            Model.BoundaryData.Add(new RunoffBoundaryData(runoffBoundary));
        }

        private void OnBoundaryRemoved(RunoffBoundary runoffBoundary)
        {
            var boundaryData = Model.BoundaryData.FirstOrDefault(bd => bd.Boundary == runoffBoundary);
            Model.BoundaryData.Remove(boundaryData);
        }

        private void RefreshModelAfterBasinReplace()
        {
            Model.GetAllModelData().ToList().Select(a => a.Catchment).ForEach(OnCatchmentRemoved);
            Model.BoundaryData.ToList().Select(bd => bd.Boundary).ForEach(OnBoundaryRemoved);

            if (Basin != null)
            {
                Basin.Catchments.ForEach(OnCatchmentAdded);
                Basin.Boundaries.ForEach(OnBoundaryAdded);

                Model.OutputCoverages.ForEach(c => c.CoordinateSystem = Basin?.CoordinateSystem);
            }
        }

        private void RemoveModelDataForCatchment(Catchment catchment)
        {
            var modelData = Model.GetCatchmentModelData(catchment);
            if (modelData != null)
            {
                Model.ModelData.Remove(modelData);
                Model.FireModelDataRemoved(modelData);
            }
        }
    }
}