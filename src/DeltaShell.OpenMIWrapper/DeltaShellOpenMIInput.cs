using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core.Workflow.DataItems;
using Deltares.OpenMI.Oatc.Sdk.Backbone;
using Deltares.OpenMI.Oatc.Sdk.DevelopmentSupport;
using GeoAPI.Extensions.Feature;
using OpenMI.Standard;

namespace DeltaShell.OpenMIWrapper
{
    public class DeltaShellOpenMIInput : InputExchangeItem
    {
        private IDataItem dataItem;
        private readonly double missingValueDefinition;

        public DeltaShellOpenMIInput(IDataItem dataItem, double missingValueDefinition,
                                     IQuantity quantity, IElementSet elementSet)
        {
            this.dataItem = dataItem;
            this.missingValueDefinition = missingValueDefinition;
            Quantity = quantity;
            ElementSet = elementSet;
        }

        public void SetLink()
        {
            DataItem newDataItem = new DataItem {Name = "openmi-input-" + dataItem.Name, ValueType = dataItem.ValueType};
            dataItem.LinkTo(newDataItem);
            dataItem = newDataItem;
        }

        public void RemoveLink()
        {
            dataItem = dataItem.LinkedBy[0];
            dataItem.Unlink();
        }

        public void SetValues(ITime time, IValueSet valueSet)
        {
            var timeStamp = time as ITimeStamp;
            if (timeStamp == null)
            {
                throw new Exception("Time spans not yet implmented");
            }
            DateTime dateTime = CalendarConverter.ModifiedJulian2Gregorian(timeStamp.ModifiedJulianDay);
            var scalarSet = valueSet as IScalarSet;
            if (scalarSet == null)
            {
                throw new Exception("Unexpected value set type: " + valueSet.GetType());
            }

            var values = new double[valueSet.Count];
            for (int i = 0; i < values.Length; i++)
            {
                if (scalarSet.IsValid(i))
                {
                    values[i] = scalarSet.GetScalar(i);
                }
                else
                {
                    values[i] = missingValueDefinition;
                }
            }

            if (dataItem.ValueType == typeof (double))
            {
                if (values.Length != 1)
                {
                    throw new Exception("Only one value expected");
                }
                dataItem.Value = values[0];
                return;
            }
            SetDataItemValuesAsDoubles(values, dataItem.Value, dateTime);
        }

        private static void SetDataItemValuesAsDoubles(IEnumerable<double> values, object dataItemValueObject,
                                                       DateTime dateTime)
        {
            if (dataItemValueObject is IFunction)
            {
                var diFunction = dataItemValueObject as IFunction;
                if (!diFunction.Components.Any())
                    throw new Exception("Function has no components");
                IVariable timeArgument = diFunction.Arguments.FirstOrDefault(a => a.ValueType == typeof (DateTime));
                IVariable valueComponent = diFunction.Components[0];
                if (timeArgument == null)
                {
                    valueComponent.SetValues(values);
                }
                else
                {
                    diFunction.SetValues(values, new VariableValueFilter<DateTime>(timeArgument, dateTime),
                                         new ComponentFilter(valueComponent));
                }
                return;
            }

            var diFeatureData = dataItemValueObject as IFeatureData;
            if (diFeatureData != null)
            {
                SetDataItemValuesAsDoubles(values, diFeatureData.Data, dateTime);
                // Feature Data is always a function, says Tiemen.
                return;
            }
            {
                throw new Exception(
                    String.Format("cannot copy values to data item value of type " + dataItemValueObject.GetType()));
            }
        }
    }
}