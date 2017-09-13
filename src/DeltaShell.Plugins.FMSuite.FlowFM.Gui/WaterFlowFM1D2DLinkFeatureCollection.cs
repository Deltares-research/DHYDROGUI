using System.ComponentModel;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    /// <summary>
    /// Defines a feature collection (used by layers for rendering) for a WaterFlowFM1D2DLink
    /// </summary>
    public class WaterFlowFM1D2DLinkFeatureCollection : FeatureCollection
    {
        private WaterFlowFMModel model;
        public WaterFlowFM1D2DLinkFeatureCollection(WaterFlowFMModel fmModel) : base(fmModel.Links, typeof(WaterFlowFM1D2DLink))
        {
            FmModel = fmModel;
        }

        public WaterFlowFMModel FmModel
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChanged)model).PropertyChanged -= WaterFlowFMModelPropertyChanged;
                }
                model = value;

                if (model != null)
                {
                    CoordinateSystem = model.CoordinateSystem;
                    ((INotifyPropertyChanged)model).PropertyChanged += WaterFlowFMModelPropertyChanged;
                }
            }
        }

        private void WaterFlowFMModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (model == null || e.PropertyName != "CoordinateSystem") return;
            CoordinateSystem = model.CoordinateSystem;
        }

        public override void Dispose()
        {
            model = null;
            base.Dispose();
        }
    }
}
