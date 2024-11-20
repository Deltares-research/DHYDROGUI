using System;
using System.Collections;
using System.ComponentModel;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Providers
{
    public class ComplexFeatureCollection : FeatureCollection
    {
        private IComplexFeature complexFeature;

        public ComplexFeatureCollection(IComplexFeature complexFeature, IList features, Type featureType): base(features, featureType)
        {
            if (!(complexFeature is INotifyPropertyChanged))
                throw new Exception("Cannot create complex feature collection if data is not INotifyPropertyChanged");
            ComplexFeature = complexFeature;
        }

        public override ICoordinateSystem CoordinateSystem
        {
            get { return ComplexFeature?.CoordinateSystem; }
            set { }
        }
        private IComplexFeature ComplexFeature
        {
            get { return complexFeature; }
            set
            {
                var previousCoordinateSystem = CoordinateSystem;
                if (complexFeature != null)
                    ((INotifyPropertyChanged)complexFeature).PropertyChanged -= OnPropertyChanged;

                complexFeature = value;

                if (complexFeature != null)
                    ((INotifyPropertyChanged)complexFeature).PropertyChanged += OnPropertyChanged;

                if (complexFeature != null && complexFeature.CoordinateSystem != previousCoordinateSystem)
                    OnCoordinateSystemChanged();
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IComplexFeature.CoordinateSystem)) return;
            OnCoordinateSystemChanged();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ComplexFeature = null;
        }
    }
}