using System;
using System.ComponentModel;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    [DisplayName("Connection")]
    public class Connection : Unique<long>, IFeature
    {
        private readonly Input input;
        private readonly Output output;

        public Connection(Input input, Output output)
        {
            this.input = input;
            this.output = output;
            Geometry = new LineString(new[]
            {
                input.Feature.Geometry.Coordinates[0],
                output.Feature.Geometry.Coordinates[0]
            });
        }

        [DisplayName("Input location")]
        [FeatureAttribute(Order = 1)]
        public string InputLocation
        {
            get
            {
                return input.LocationName;
            }
        }

        [DisplayName("Input parameter")]
        [FeatureAttribute(Order = 2)]
        public string InputParameter
        {
            get
            {
                return input.ParameterName;
            }
        }

        [DisplayName("Output location")]
        [FeatureAttribute(Order = 3)]
        public string OutputLocation
        {
            get
            {
                return output.LocationName;
            }
        }

        [DisplayName("Output parameter")]
        [FeatureAttribute(Order = 4)]
        public string OutputParameter
        {
            get
            {
                return output.ParameterName;
            }
        }

        [Browsable(false)]
        public IGeometry Geometry { get; set; }

        [Browsable(false)]
        public IFeatureAttributeCollection Attributes { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}