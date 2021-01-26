using System.Collections;
using System.ComponentModel;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui
{
    public class Iterative1D2DCouplerLinkFeatureCollection : FeatureCollection
    {
        private Iterative1D2DCoupler coupler;

        public Iterative1D2DCouplerLinkFeatureCollection(IList features): base(features, typeof(Iterative1D2DCouplerLink))
        {
        }

        public Iterative1D2DCoupler Coupler
        {
            get { return coupler; }
            set
            {
                if (coupler != null)
                {
                    if (coupler.HydroModel != null)
                    {
                        ((INotifyPropertyChanged) coupler.HydroModel).PropertyChanged -= CouplerPropertyChanged;
                    }
                }
                
                coupler = value;

                if (coupler != null)
                {
                    CoordinateSystem = coupler.CoordinateSystem;
                    if (coupler.HydroModel != null)
                    {
                        ((INotifyPropertyChanged)coupler.HydroModel).PropertyChanged += CouplerPropertyChanged;
                    }
                }

            }
        }

        private void CouplerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (coupler == null || e.PropertyName != "CoordinateSystem") return;

            CoordinateSystem = coupler.CoordinateSystem;
        }

        protected override void Dispose(bool disposing)
        {
            Coupler = null;
            base.Dispose(disposing);
        }
    }
}