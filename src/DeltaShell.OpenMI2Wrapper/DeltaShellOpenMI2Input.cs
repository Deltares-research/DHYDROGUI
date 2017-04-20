using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core.Workflow.DataItems;
using Deltares.OpenMI2.Oatc.Sdk.Backbone;
using GeoAPI.Extensions.Feature;
using OpenMI.Standard2;
using OpenMI.Standard2.TimeSpace;

namespace DeltaShell.OpenMI2Wrapper
{
    internal sealed class DeltaShellOpenMI2Input : Input
    {
        private IDataItem dataItem;

        internal DeltaShellOpenMI2Input(IDataItem dataItem, string id,
                                        IQuantity quantity, IElementSet elementSet,
                                        ITimeSpaceComponent component) : base(id)
        {
            this.dataItem = dataItem;
            ValueDefinition = quantity;
            SpatialDefinition = elementSet;
            Component = component;
        }

        public void SetLink()
        {
            DataItem newDataItem = new DataItem { Name = "openmi-input-" + dataItem.Name, ValueType = dataItem.ValueType };
            dataItem.LinkTo(newDataItem);
            dataItem = newDataItem;
        }

        public void RemoveLink()
        {
            dataItem = dataItem.LinkedBy[0];
            dataItem.Unlink();
        }

        /// <summary>
        /// Copy values from exchange item to data item.
        /// </summary>
        public void FeedValuesToDataItem(DateTime time)
        {
            var numValues = Values.GetIndexCount(new[] {0});
            if (numValues == 0)
            {
                return;
            }
            var values = new double[numValues];
            for (int i = 0; i < numValues; i++)
            {
                values[i] = (double) Values.GetValue(new[] {0, i});
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
            SetDataItemValuesAsDoubles(values, dataItem.Value, time);
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