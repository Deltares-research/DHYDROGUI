using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffBasinSynchronizer
    {
        private readonly RainfallRunoffCatchmentModelDataFactory modelDataFactory = new RainfallRunoffCatchmentModelDataFactory();
        private DrainageBasin subscribedBasin;
        private IDataItem subscribedBasinDataItem;
        private bool linking;
        private DrainageBasin preLinkBasin;

        public RainfallRunoffBasinSynchronizer(RainfallRunoffModel model)
        {
            Model = model;
            ((INotifyPropertyChanged)Model).PropertyChanged += RainfallRunoffPropertyChanged;
            AfterDataItemsSet();
        }

        private void RainfallRunoffPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // deal with undo/redo of linking..yes..I know
            if (EditActionSettings.Disabled) //this makes sure this ONLY works DURING undo/redo
            {
                if ((e.PropertyName == "Value" || e.PropertyName == "LinkedTo") &&
                    Equals(sender, subscribedBasinDataItem))
                {
                    Subscribe();
                }
            }
        }

        private RainfallRunoffModel Model { get; set; }

        private DrainageBasin Basin
        {
            get { return Model.Basin; }
        }

        public bool IsDifferentBasin(DrainageBasin otherBasin)
        {
            return otherBasin != subscribedBasin;
        }

        private void OnCatchmentAdded(Catchment catchment)
        {
            AddDefaultModelDataForCatchment(catchment);
            foreach(var subcatchment in catchment.SubCatchments)
            {
                OnCatchmentAdded(subcatchment);
            }
        }

        private void OnCatchmentRemoved(Catchment catchment)
        {
            RemoveModelDataForCatchment(catchment);
            foreach (var subcatchment in catchment.SubCatchments)
            {
                OnCatchmentRemoved(subcatchment);
            }
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

            if (e.PropertyName == "Value")
            {
                if (!linking)
                    FullRefresh(); //should automatically clear output
            }
        }

        [EditAction]
        void BasinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var catchment = sender as Catchment;
            if (catchment != null && e.PropertyName == TypeUtils.GetMemberName(() => catchment.CatchmentType))
            {
                var currentData = Model.GetCatchmentModelData(catchment);
                if (!modelDataFactory.IsModelDataCompatible(catchment, currentData))
                {
                    RemoveModelDataForCatchment(catchment);
                    AddDefaultModelDataForCatchment(catchment);
                }
            }
        }

        [EditAction]
        private void BasinCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var catchmentList = sender as IEventedList<Catchment>;
            if (catchmentList != null) //basin catchments or subcatchments
            {
                if (!catchmentList.SkipChildItemEventBubbling) //not aggregation list
                {
                    CatchmentsCollectionChanged(e);
                }
            }

            if (Equals(sender, Basin.Boundaries))
            {
                BoundariesCollectionChanged(e);
            }
        }

        private void CatchmentsCollectionChanged(NotifyCollectionChangingEventArgs e)
        {
            var catchment = (Catchment) e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    OnCatchmentAdded(catchment);
                    break;
                case NotifyCollectionChangeAction.Remove:
                    OnCatchmentRemoved(catchment);
                    break;
            }
        }

        private void BoundariesCollectionChanged(NotifyCollectionChangingEventArgs e)
        {
            var runoffBoundary = (RunoffBoundary)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    OnBoundaryAdded(runoffBoundary);
                    break;
                case NotifyCollectionChangeAction.Remove:
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
            }
        }

        private void AddDefaultModelDataForCatchment(Catchment catchment)
        {
            var catchmentModelData = modelDataFactory.CreateDefaultModelData(catchment);

            if (catchmentModelData == null)
                return;

            if (Model.Basin.Catchments.Contains(catchment))
            {
                Model.ModelData.Add(catchmentModelData);
            }
            else
            {
                var parentCatchment = Model.Basin.AllCatchments.First(c => c.SubCatchments.Contains(catchment));
                var parentModelData = Model.GetCatchmentModelData(parentCatchment);
                parentModelData.SubCatchmentModelData.Add(catchmentModelData);
            }

            Model.FireModelDataAdded(catchmentModelData);
        }

        private void RemoveModelDataForCatchment(Catchment catchment)
        {
            var modelData = Model.GetCatchmentModelData(catchment);
            if (modelData != null)
            {
                if (Model.ModelData.Contains(modelData))
                {
                    Model.ModelData.Remove(modelData);
                }
                else
                {
                    var parentModelData = Model.GetAllModelData().First(md => md.SubCatchmentModelData.Contains(modelData));
                    parentModelData.SubCatchmentModelData.Remove(modelData);
                }
                Model.FireModelDataRemoved(modelData);
            }
        }
    }
}