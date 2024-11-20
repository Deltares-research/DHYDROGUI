using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class CatchmentModelDataSynchronizer<TConceptFilter> : ICatchmentModelDataSynchronizer where TConceptFilter : CatchmentModelData
    {
        private readonly IRainfallRunoffModel model;

        public CatchmentModelDataSynchronizer(IRainfallRunoffModel model)
        {
            this.model = model;
            SubscribeToModel();
        }

        private void SubscribeToModel()
        {
            if (model == null)
            {
                return;
            }

            model.PropertyChanged += ModelPropertyChanged;
            model.ModelDataAdded += ModelDataAdded;
            model.ModelDataRemoved += ModelAreaRemoved;
        }
        
        private void UnsubscribeFromModel()
        {
            if (model == null)
            {
                return; 
            }

            model.PropertyChanged -= ModelPropertyChanged;
            model.ModelDataAdded -= ModelDataAdded;
            model.ModelDataRemoved -= ModelAreaRemoved;
        }
        
        public void Disconnect()
        {
            UnsubscribeFromModel();
            OnAreaAddedOrModified = null;
            OnAreaRemoved = null;
        }

        public Action<CatchmentModelData> OnAreaAddedOrModified { get; set; }
        
        public Action<CatchmentModelData> OnAreaRemoved { get; set; }

        private bool ShouldBeInIncluded(CatchmentModelData area)
        {
            return area is TConceptFilter;
        }

        private void ModelAreaRemoved(object sender, EventArgs<CatchmentModelData> eventArgs)
        {
            if (!ShouldBeInIncluded(eventArgs.Value)) return;

            OnAreaRemoved?.Invoke(eventArgs.Value);
        }

        private void ModelDataAdded(object sender, EventArgs<CatchmentModelData> eventArgs)
        {
            if (!ShouldBeInIncluded(eventArgs.Value)) return;

            OnAreaAddedOrModified?.Invoke(eventArgs.Value);
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is CatchmentModelData catchmentModelData))
            {
                return;
            }

            if (ShouldBeInIncluded(catchmentModelData))
            {
                OnAreaAddedOrModified(catchmentModelData);
            }
            else
            {
                OnAreaRemoved(catchmentModelData);
            }
        }
    }
}