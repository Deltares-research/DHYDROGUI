using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using Deltares.OpenMI.Oatc.Sdk.Backbone;
using Deltares.OpenMI.Oatc.Sdk.DevelopmentSupport;
using OpenMI.Standard;

namespace DeltaShell.OpenMIWrapper
{
    public class DeltaShellOpenMIOutput : OutputExchangeItem
    {
        private readonly IDataItem dataItem;

        public DeltaShellOpenMIOutput(IDataItem modelDataItem, IQuantity quantity, IElementSet elementSet)
        {
            dataItem = modelDataItem;
            Quantity = quantity;
            ElementSet = elementSet;
        }

        public void SetLink()
        {
            if (dataItem.Parent != null)
            {
                dataItem.Parent.Children.Add(dataItem);
            }
        }

        public void RemoveLink()
        {
            if (dataItem.Parent != null)
            {
                dataItem.Parent.Children.Remove(dataItem);
            }
        }

        public ScalarSet GetValues(ITime time)
        {
            var timeStamp = time as ITimeStamp;
            if (timeStamp == null)
            {
                throw new Exception("Time spans not yet implmented");
            }
            DateTime dateTime = CalendarConverter.ModifiedJulian2Gregorian(timeStamp.ModifiedJulianDay);
            // TODO: check dateTime with model time, or remove argument

            if (dataItem.Value == null)
            {
                throw new Exception("Value == null for data item " + dataItem.Name);
            }

            double[] doubles = GetDataItemValuesAsDoubles().ToArray();
            return new ScalarSet(doubles);
        }

        public double[] GetDataItemValuesAsDoubles()
        {
            object dataItemValueObject = dataItem.Value;
            if (dataItemValueObject is double)
            {
                return new[] {(double) dataItemValueObject};
            }

            if (dataItemValueObject is double[])
            {
                return (double[]) dataItemValueObject;
            }

            throw new Exception("Unknown data item value type: " + dataItemValueObject.GetType());
        }
    }
}