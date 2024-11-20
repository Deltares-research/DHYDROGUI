using System;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class WaterFlowFMFeatureValueConverter : ParameterValueConverter, IExplicitValueConverter
    {
        public WaterFlowFMFeatureValueConverter(WaterFlowFMModel model, IFeature feature, string parameterName,
                                                string unit)
        {
            Location = feature;
            Model = model;
            ParameterName = parameterName;
            Unit = unit;
        }

        [NoNotifyPropertyChange]
        public override double ConvertedValue
        {
            get => GetValueFromModelApi(Location, ParameterName);
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
                {
                    return;
                }

                SetValueToModelApi(Location, ParameterName, value);
            }
        }

        [Aggregation]
        public WaterFlowFMModel Model { get; set; }

        public override object DeepClone()
        {
            var clone = (WaterFlowFMFeatureValueConverter) base.DeepClone();

            clone.Model = Model;

            return clone;
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
                return double.NaN;
            }

            // temporary fix for DELFT3DFM-1302 (this should be done in Dimr)
            if (featureCategory == "weirs" && parameterName == "crest_level")
            {
                var weir = (Structure) feature;
                if (!weir.UseCrestLevelTimeSeries)
                {
                    return weir.CrestLevel;
                }

                if (weir.CrestLevelTimeSeries.GetValues<double>().Any())
                {
                    return weir.CrestLevelTimeSeries.GetValues<double>().FirstOrDefault();
                }
            }

            if (!(feature is INameable nameable))
            {
                return double.NaN;
            }

            return ((double[]) Model.GetVar(featureCategory, nameable.Name, parameterName))[0];
        }

        private void SetValueToModelApi(IFeature feature, string parameterName, double value)
        {
            string featureCategory = Model.GetFeatureCategory(feature);
            if (featureCategory == null)
            {
                return;
            }

            if (!(feature is INameable nameable))
            {
                return;
            }

            Model.SetVar(new[]
            {
                value
            }, featureCategory, nameable.Name, parameterName);
        }
    }
}