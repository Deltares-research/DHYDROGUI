using System.ComponentModel;
using DelftTools.Hydro;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Providers
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
            get { return Area2D != null ? Area2D.CoordinateSystem : null; }
            set { } 
        }

        private HydroArea Area2D
        {
            get { return area2D; }
            set
            {
                var previousCoordinateSystem = CoordinateSystem;
                if (area2D != null)
                    area2D.PropertyChanged -= HydroAreaOnPropertyChanged;
            
                area2D = value;

                if (area2D != null)
                    area2D.PropertyChanged += HydroAreaOnPropertyChanged;

                if (area2D != null && area2D.CoordinateSystem != previousCoordinateSystem)
                    OnCoordinateSystemChanged();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Area2D = null;
        }

        private void HydroAreaOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CoordinateSystem") return;
            OnCoordinateSystemChanged();
        }
    }
}