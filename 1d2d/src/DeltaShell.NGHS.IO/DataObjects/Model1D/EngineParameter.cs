using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.NGHS.IO.DataObjects.Model1D
{
    [Entity(FireOnCollectionChange=false)]
    public class EngineParameter: Unique<long>, ICloneable
    {
        public EngineParameter()
        {
        }

        /// <summary>
        /// EngineParameter is used to communicate parameter settings with ModelApi. 
        /// </summary>
        /// <param name="quantityType"></param>
        /// <param name="elementSet"></param>
        /// <param name="dataItemRole"></param>
        /// <param name="name"></param>
        /// <param name="unit"></param>
        public EngineParameter(QuantityType quantityType, ElementSet elementSet, DataItemRole dataItemRole, string name, Unit unit)
        {
            Name = name;
            QuantityType = quantityType;
            ElementSet = elementSet;
            Role = dataItemRole;
            AggregationOptions = AggregationOptions.None;
            Unit = unit;
        }

        public override string ToString()
        {
            return Name;
        }

        [NoNotifyPropertyChange]
        public virtual string Name { get; protected set; }

        [NoNotifyPropertyChange]
        public virtual QuantityType QuantityType { get; protected set; }

        [NoNotifyPropertyChange]
        public virtual ElementSet ElementSet { get; set; }

        [NoNotifyPropertyChange]
        public virtual DataItemRole Role { get; protected set; }

        [NoNotifyPropertyChange]
        private Unit unit;
        public virtual Unit Unit
        {
            get { return unit; }
            protected set { unit = value; }
        }

        /// <summary>
        /// Unlike the other properties AggregationOptions can be modified by the user
        /// AggregationOptions is used by ModelApi to calculate the values that are retrieved by
        /// ModelApi GetValues. Unlike ModelApi ??? that always retrieves the AggregationOptions.Current
        /// value.
        ///  - ModelApi GetValues is used to fill the output coverages.
        ///  - ModelApi ??? is used to facilitate online communication like RealTimeControlModel.
        /// </summary>
        public virtual AggregationOptions AggregationOptions
        {
            get { return aggregationOptions; }
            set { aggregationOptions = value; }
        }
        
        private AggregationOptions aggregationOptions;

        public virtual object Clone()
        {
            return new EngineParameter(QuantityType, ElementSet, Role, Name, Unit)
                       {
                           AggregationOptions = AggregationOptions
                       };
        }
    }
}