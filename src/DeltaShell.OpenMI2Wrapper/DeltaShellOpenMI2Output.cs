using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using Deltares.OpenMI2.Oatc.Sdk.Backbone;
using Deltares.OpenMI2.Oatc.Sdk.Backbone.Generic;
using OpenMI.Standard2;
using OpenMI.Standard2.TimeSpace;

namespace DeltaShell.OpenMI2Wrapper
{
    public sealed class DeltaShellOpenMI2Output : Output
    {
        private readonly IDataItem dataItem;

        internal DeltaShellOpenMI2Output(IDataItem modelDataItem, string id,
                                         IQuantity quantity, IElementSet elementSet,
                                         ITimeSpaceComponent component) : base(id)
        {
            dataItem = modelDataItem;
            ValueDefinition = quantity;
            SpatialDefinition = elementSet;
            Component = component;
            TimeSet=new TimeSet();
        }

        public bool HasSubOutputItems { get; set; }

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

        /// <summary>
        /// Copy values from data item to exchange item.
        /// </summary>
        public void GetValuesFromDataItem(DateTime time)
        {
            TimeSet.SetSingleTime(new Time(time.ToUniversalTime()));
            // TODO: check dateTime with model time
            if (dataItem.Value == null)
            {
                throw new Exception("Value == null for data item " + dataItem.Name);
            }
            Values = new TimeSpaceValueSet<double>(GetDataItemValuesAsDoubles());
        }

        public IList<double> GetDataItemValuesAsDoubles()
        {
            object dataItemValueObject = dataItem.Value;
            if (dataItemValueObject is double)
            {
                return new[] { (double)dataItemValueObject };
            }

            if (dataItemValueObject is double[])
            {
                return (double[])dataItemValueObject;
            }

            throw new Exception("Unknown data item value type: " + dataItemValueObject.GetType());
        }
    }
}