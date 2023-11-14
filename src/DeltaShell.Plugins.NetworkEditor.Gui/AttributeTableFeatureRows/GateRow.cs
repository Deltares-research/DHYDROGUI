using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    public class GateRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        protected IGate gate;

        protected string OpeningWidthTimeSeriesString = "Time series";

        protected string TimeSeriesString = "Time series";

        public GateRow(IGate gate)
        {
            Gate = gate;
        }

        private IGate Gate
        {
            get => gate;
            set
            {
                if (gate != null)
                {
                    ((INotifyPropertyChanged)gate).PropertyChanged -= GatePropertiesRowPropertyChanged;
                }

                gate = value;
                if (gate != null)
                {
                    ((INotifyPropertyChanged)gate).PropertyChanged += GatePropertiesRowPropertyChanged;
                }
            }
        }

        // gate properties
        public virtual string Name
        {
            get => Gate.Name;
            set => Gate.SetNameIfValid(value);
        }

        [DynamicReadOnly]
        [DisplayName("Sill level [m]")]
        [PropertyOrder(2)]
        public string SillLevel
        {
            get
            {
                if (gate.UseSillLevelTimeSeries)
                {
                    return TimeSeriesString;
                }

                return gate.SillLevel.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (gate.UseSillLevelTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent crest width.");
                }

                gate.SillLevel = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DisplayName("Door height [m]")]
        [PropertyOrder(3)]
        public double DoorHeight
        {
            get => Gate.DoorHeight;
            set => Gate.DoorHeight = value;
        }

        [DisplayName("Horizontal opening direction")]
        [PropertyOrder(4)]
        public GateOpeningDirection HorizontalOpeningDirection
        {
            get => Gate.HorizontalOpeningDirection;
            set => Gate.HorizontalOpeningDirection = value;
        }

        [DynamicReadOnly]
        [DisplayName("Lower edge level [m]")]
        [PropertyOrder(5)]
        public string LowerEdgeLevel
        {
            get
            {
                if (gate.UseLowerEdgeLevelTimeSeries)
                {
                    return TimeSeriesString;
                }

                return gate.LowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (gate.UseLowerEdgeLevelTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent crest width.");
                }

                gate.LowerEdgeLevel = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DynamicReadOnly]
        [DisplayName("Opening width [m]")]
        [PropertyOrder(6)]
        public string OpeningWidth
        {
            get
            {
                if (gate.UseOpeningWidthTimeSeries)
                {
                    return OpeningWidthTimeSeriesString;
                }

                return gate.OpeningWidth.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (gate.UseOpeningWidthTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent crest level.");
                }

                gate.OpeningWidth = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DynamicReadOnly]
        [DisplayName("Sill width [m]")]
        [PropertyOrder(7)]
        public string SillWidth
        {
            get => gate.SillWidth.ToString("0.00", CultureInfo.CurrentCulture);
            set => gate.SillWidth = double.Parse(value, CultureInfo.CurrentCulture);
        }

        [DisplayName("Use sill width")]
        [PropertyOrder(8)]
        public bool UseSillWidth
        {
            get => gate.SillWidth > 0;
            set => gate.SillWidth = value ? gate.Geometry.Length : 0.0;
        }

        [DisplayName("Long name")]
        public virtual string LongName
        {
            get => Gate.LongName;
            set => Gate.LongName = value;
        }

        [ReadOnly(true)]
        public virtual IBranch Branch => Gate.Branch;

        [ReadOnly(true)]
        [DisplayName("Chainage [m]")]
        [DisplayFormat("0.00")]
        public virtual double Chainage => Gate.Chainage;

        public virtual void Dispose()
        {
            Gate = null;
            PropertyChanged = null;
            PropertyChanging = null;
        }

        public IFeature GetFeature()
        {
            return gate;
        }

        [Browsable(false)]
        public bool HasParent { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        private void GatePropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (gate == null)
            {
                return false;
            }

            switch (propertyName)
            {
                case nameof(SillLevel):
                    return gate.UseSillLevelTimeSeries;
                case nameof(OpeningWidth):
                    return gate.UseOpeningWidthTimeSeries;
                case nameof(LowerEdgeLevel):
                    return gate.UseLowerEdgeLevelTimeSeries;
                case nameof(SillWidth):
                    return gate.SillWidth <= 0.0;
                default:
                    return false;
            }
        }
    }
}