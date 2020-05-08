using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
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
            get => Model.GetValueFromModelApi(Location, ParameterName);
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

                Model.SetToModelApi(Location, ParameterName, value);
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
    }
}