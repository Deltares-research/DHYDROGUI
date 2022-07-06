using System;
using System.ComponentModel;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using log4net;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "NetworkCoverageProperties_DisplayName")]
    public class NetworkCoverageProperties : ObjectProperties<INetworkCoverage>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkCoverageProperties));

        [Category("General")]
        [Description("Name of the network coverage")]
        [DynamicReadOnly]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        [Description("Method used to generate segments from location")]
        public SegmentGenerationMethod SegmentMethod
        {
            get { return data.SegmentGenerationMethod; }
        }

        [Category("General")]
        [Description("Default value when no data is available")]
        [DynamicReadOnly]
        public double DefaultValue
        {
            get { return data.DefaultValue; }
            set { data.DefaultValue = value; }
        }

        [Category("General")]
        [DisplayName("Unit")]
        public string Unit
        {
            get { return data.Components[0].Unit.Symbol; }
        }

        public enum NetworkCoverageInterpolationType
        {
            Constant,
            Linear
        }

        private NetworkCoverageInterpolationType ToNetworkCoverageInterpolationType(InterpolationType type)
        {
            switch (type)
            {
                case DelftTools.Functions.Generic.InterpolationType.Constant:
                    return NetworkCoverageInterpolationType.Constant;
                case DelftTools.Functions.Generic.InterpolationType.Linear:
                    return NetworkCoverageInterpolationType.Linear;
                default:
                    throw new NotSupportedException("Interpolation type: " + type + " is not supported.");
            }
        }

        private InterpolationType FromNetworkCoverageInterpolationType(NetworkCoverageInterpolationType type)
        {
            switch (type)
            {
                case NetworkCoverageInterpolationType.Constant:
                    return DelftTools.Functions.Generic.InterpolationType.Constant;
                case NetworkCoverageInterpolationType.Linear:
                    return DelftTools.Functions.Generic.InterpolationType.Linear;
                default:
                    throw new NotSupportedException("Interpolation type: " + type + " is not supported.");
            }
        }

        [Category("Approximation")]
        public ExtrapolationType ExtrapolationType
        {
            get { return data.Locations.ExtrapolationType; }
        }

        [Category("Approximation")]
        [DynamicReadOnly]
        public NetworkCoverageInterpolationType InterpolationType
        {
            get { return ToNetworkCoverageInterpolationType(data.Locations.InterpolationType); }
            set
            {
                if (data.Locations.AllowSetInterpolationType)
                {
                    data.Locations.InterpolationType = FromNetworkCoverageInterpolationType(value);    
                }
                else
                {
                    Log.WarnFormat("Unable to set interpolation-type for locations, it is not allowed.");
                }
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("Name") || propertyName.Equals("InterpolationType") || propertyName.Equals("DefaultValue"))
            {
                return !data.IsEditable;
            }

            return false;
        }

        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        public ICoordinateSystem CoordinateSystem
        {
            get { return data.CoordinateSystem; }
        }
    }
}