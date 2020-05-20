using System.ComponentModel;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public class WaterFlowFmModelFeature2DCollection : Feature2DCollection
    {
        private WaterFlowFMModel fmModel;
        private static readonly string ModelName = typeof(WaterFlowFMModel).Name;
        public WaterFlowFMModel FmModel
        {
            get { return fmModel; }
            private set
            {
                if (fmModel != null)
                {
                    fmModel.PropertyChanged -= FmModelOnPropertyChanged;
                }

                fmModel = value;

                if (fmModel != null)
                {
                    fmModel.PropertyChanged += FmModelOnPropertyChanged;
                }
            }
        }

        public WaterFlowFmModelFeature2DCollection Init<T>(IEventedList<T> features,
            string featureTypeName,
            WaterFlowFMModel model)
        {
            FmModel = model;
            Init(features, featureTypeName, ModelName, FmModel.CoordinateSystem);
            return this;
        }

        public override void Dispose()
        {
            base.Dispose();
            FmModel = null;
        }

        private void FmModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(fmModel.CoordinateSystem)))
                return;

            CoordinateSystem = fmModel.CoordinateSystem;
        }
    }
}