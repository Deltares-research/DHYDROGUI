using System.ComponentModel;
using DelftTools.Hydro;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    public class HydroAreaFeature2DCollection : Feature2DCollection
    {
        private HydroArea area2D;

        public HydroAreaFeature2DCollection(HydroArea area2D)
        {
            Area2D = area2D;
        }

        public override ICoordinateSystem CoordinateSystem
        {
            get => Area2D != null ? Area2D.CoordinateSystem : null;
            set { }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Area2D = null;
            }
        }

        private HydroArea Area2D
        {
            get => area2D;
            set
            {
                ICoordinateSystem previousCoordinateSystem = CoordinateSystem;
                if (area2D != null)
                {
                    area2D.PropertyChanged -= HydroAreaOnPropertyChanged;
                }

                area2D = value;

                if (area2D != null)
                {
                    area2D.PropertyChanged += HydroAreaOnPropertyChanged;
                }

                if (area2D != null && area2D.CoordinateSystem != previousCoordinateSystem)
                {
                    OnCoordinateSystemChanged();
                }
            }
        }

        private void HydroAreaOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CoordinateSystem")
            {
                return;
            }

            OnCoordinateSystemChanged();
        }
    }
}