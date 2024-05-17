using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils
{
    public class ControlledTestModelParameterValueConverter : ParameterValueConverter, IExplicitValueConverter
    {
        public ControlledTestModelParameterValueConverter(IFeature feature, string parameterName)
        {
            base.Location = feature;
            base.ParameterName = parameterName;
        }

        public override double ConvertedValue { get; set; }

        public void Update(DateTime time, object value = null)
        {
            ConvertedValue = Convert.ToDouble(value);
        }
    }
}