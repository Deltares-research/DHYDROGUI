using System;
using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class CatchmentModelDataSynchronizer<TConceptFilter> : ICatchmentModelDataSynchronizer where TConceptFilter : CatchmentModelData
    {
        private IRainfallRunoffModel model;

        public CatchmentModelDataSynchronizer(IRainfallRunoffModel model)
        {
            Model = model;
        }

        private IRainfallRunoffModel Model
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    model.ModelPropertyChanged -= ModelPropertyChanged;
                    model.ModelDataAdded -= ModelDataAdded;
                    model.ModelDataRemoved -= ModelAreaRemoved;
                }
                model = value;
                if (model != null)
                {
                    model.ModelPropertyChanged += ModelPropertyChanged;
                    model.ModelDataAdded += ModelDataAdded;
                    model.ModelDataRemoved += ModelAreaRemoved;
                }
            }
        }
        
        public void Disconnect()
        {
            Model = null; //unsubscribes
            OnAreaAddedOrModified = null;
            OnAreaRemoved = null;
        }

        public Action<CatchmentModelData> OnAreaAddedOrModified { get; set; }
        public Action<CatchmentModelData> OnAreaRemoved { get; set; }

        private bool ShouldBeInIncluded(CatchmentModelData area)
        {
            return area is TConceptFilter;
        }

        private void ModelAreaRemoved(object sender, EventArgs e)
        {
            var area = sender as CatchmentModelData;
            if (OnAreaRemoved != null)
            {
                OnAreaRemoved(area);
            }
        }

        private void ModelDataAdded(object sender, EventArgs e)
        {
            var area = sender as CatchmentModelData;
            if (ShouldBeInIncluded(area))
            {
                if (OnAreaAddedOrModified != null)
                {
                    OnAreaAddedOrModified(area);
                }
            }
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CatchmentModelData modelData;

            if (sender is CatchmentModelData catchmentModelData)
            {
                modelData = catchmentModelData;
            }
            else
            {
                return;
            }

            if (modelData == null)
            {
                throw new ArgumentException("Property changed for unknown concept!");
            }

            bool shouldBeIncluded = ShouldBeInIncluded(modelData);

            if (shouldBeIncluded)
            {
                OnAreaAddedOrModified(modelData);
            }
            else
            {
                OnAreaRemoved(modelData);
            }
        }
    }
}