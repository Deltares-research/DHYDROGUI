using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class WaterFlowFMFeatureValueConverter : ParameterValueConverter, IExplicitValueConverter
    {
        [Aggregation]
        public WaterFlowFMModel Model { get; set; }

        public WaterFlowFMFeatureValueConverter(WaterFlowFMModel model, IFeature feature, string parameterName, string unit)
        {
            Location = feature;
            Model = model;
            ParameterName = parameterName;
            Unit = unit;
        }

        public override object DeepClone()
        {
            var clone = (WaterFlowFMFeatureValueConverter)base.DeepClone();

            clone.Model = Model;

            return clone;
        }

        [NoNotifyPropertyChange]
        public override double ConvertedValue
        {
            get
            {
                return GetValueFromModelApi(Location, ParameterName);
            }
            set
            {
                if (Model == null)
                {
                    return;
                }

                if (Model.BMIEngine == null)
                {
                    return;
                }

                if (double.IsNaN(value))
                    return;

                SetToModelApi(Location, ParameterName, value);
            }
        }

        public virtual void Update(DateTime time, object value = null)
        {
            ConvertedValue = Convert.ToDouble(value);
        }
        
        private double GetValueFromModelApi(IFeature feature, string parameterName)
        {
            string featureCategory = Model.GetFeatureCategory(feature);
            if (featureCategory == null)
            {
                return Double.NaN;
            }

            // temporary fix for DELFT3DFM-1302 (this should be done in Dimr)
            if (featureCategory == "weirs" && parameterName == "crest_level")
            {
                var weir = (Weir)feature;
                if (!weir.UseCrestLevelTimeSeries)
                {
                    return weir.CrestLevel;
                }

                if (weir.CrestLevelTimeSeries.GetValues<double>().Any())
                {
                    return weir.CrestLevelTimeSeries.GetValues<double>().FirstOrDefault();
                }
            }

            if (Model.DimrRunner.Api == null)
            {
                return Double.NaN;
            }

            var nameable = feature as INameable;
            if (nameable == null)
                return Double.NaN;

            return ((double[])Model.GetVar(featureCategory, nameable.Name, parameterName))[0];
        }

        private void SetToModelApi(IFeature feature, string parameterName, double value)
        {
            string featureCategory = Model.GetFeatureCategory(feature);
            if (featureCategory == null)
            {
                return;
            }

            var nameable = feature as INameable;
            if (nameable == null)
                return;

            Model.SetVar(new [] { value }, featureCategory, nameable.Name, parameterName);
        }
    }
}