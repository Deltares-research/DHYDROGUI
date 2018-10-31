using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    /// <summary>
    /// Defines a feature collection (used by layers for rendering) for a Link1D2D
    /// </summary>
    public class WaterFlowFM1D2DLinkFeatureCollection : FeatureCollection
    {
        private WaterFlowFMModel model;
        public WaterFlowFM1D2DLinkFeatureCollection(WaterFlowFMModel fmModel) : base(fmModel.Links.ToList(), typeof(Link1D2D))
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
