using System;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange = false)]
    public abstract class ConnectionPoint : RtcBaseObject, IFeature
    {
        private IFeature feature;

        private string parameterName;

        protected ConnectionPoint()
        {
            UpdateName();
        }

        [Aggregation]
        public IFeature Feature
        {
            get
            {
                return feature;
            }
            set
            {
                UnsubscribeFeatureEvents();
                feature = value;
                SubscribeFeatureEvents();

                UpdateName();
            }
        }

        [DisplayName("Parameter")]
        [ReadOnly(true)]
        [FeatureAttribute(Order = 2)]
        public string ParameterName
        {
            get
            {
                return parameterName;
            }
            set
            {
                parameterName = value;
                UpdateName();
            }
        }

        public string UnitName { get; set; }

        [DisplayName("Location")]
        [FeatureAttribute(Order = 1)]
        public string LocationName
        {
            get
            {
                return Feature == null ? string.Empty : Feature.ToString();
            }
        }

        public bool IsConnected
        {
            get
            {
                return Feature != null;
            }
        }

        [NoNotifyPropertyChange]
        public double Value { get; set; }

        [DisplayName("Type")]
        [FeatureAttribute(Order = 3)]
        public abstract ConnectionType ConnectionType { get; }

        public IGeometry Geometry
        {
            get
            {
                return Feature?.Geometry;
            }
            set
            {
                if (Feature != null)
                {
                    Feature.Geometry = value;
                }
            }
        }

        public IFeatureAttributeCollection Attributes
        {
            get
            {
                return Feature?.Attributes;
            }
            set
            {
                if (Feature != null)
                {
                    Feature.Attributes = value;
                }
            }
        }

        public void Reset()
        {
            Feature = null;
            ParameterName = "";
            UnitName = "";
        }

        public virtual void CopyFrom(object source)
        {
            var connectionPoint = source as ConnectionPoint;
            if (connectionPoint != null)
            {
                Feature = connectionPoint.Feature;
                ParameterName = connectionPoint.ParameterName;
                UnitName = connectionPoint.UnitName;
                Value = connectionPoint.Value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override object Clone()
        {
            var connectionPoint = (ConnectionPoint) Activator.CreateInstance(GetType());
            connectionPoint.CopyFrom(this);
            return connectionPoint;
        }

        private void UpdateName()
        {
            if (IsConnected)
            {
                Name = LocationName + "_" + ParameterName;
            }
            else
            {
                Name = "[Not Set]";
            }
        }

        private void SubscribeFeatureEvents()
        {
            if (!(feature is INotifyPropertyChange))
            {
                return;
            }

            ((INotifyPropertyChange) feature).PropertyChanged += OnFeaturePropertyChanged;
        }

        private void UnsubscribeFeatureEvents()
        {
            if (!(feature is INotifyPropertyChange))
            {
                return;
            }

            ((INotifyPropertyChange) feature).PropertyChanged -= OnFeaturePropertyChanged;
        }

        private void OnFeaturePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateName();
        }
    }
}