using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    [Entity(FireOnCollectionChange=false)]
    public class EngineParameter : Unique<long>, ICloneable
    {
        [NoNotifyPropertyChange] private Unit unit;

        public EngineParameter()
        {
        }
        /// <summary>
        /// EngineParameter is used to communicate parameter settings with Model. 
        /// </summary>
        /// <param name="quantityType"></param>
        /// <param name="elementSet"></param>
        /// <param name="role"></param>
        /// <param name="name"></param>
        /// <param name="unit"></param>
        public EngineParameter(QuantityType quantityType, ElementSet elementSet, DataItemRole role, string name,
                               Unit unit)
        {
            Name = name;
            QuantityType = quantityType;
            ElementSet = elementSet;
            Role = role;
            Unit = unit;
            IsEnabled = false;
        }

        [NoNotifyPropertyChange]
        public virtual string Name { get; protected set; }

        [NoNotifyPropertyChange]
        public virtual QuantityType QuantityType { get; protected set; }

        [NoNotifyPropertyChange]
        public virtual ElementSet ElementSet { get; protected set; }

        [NoNotifyPropertyChange]
        public virtual DataItemRole Role { get; protected set; }
        
        public virtual bool IsEnabled { get; set; }

        public virtual Unit Unit
        {
            get { return unit; }
            protected set { unit = value; }
        }
        
        #region ICloneable Members

        public virtual object Clone()
        {
            return new EngineParameter(QuantityType, ElementSet, Role, Name, Unit)
            {
                IsEnabled = IsEnabled
            };
        }

        #endregion
    }
}