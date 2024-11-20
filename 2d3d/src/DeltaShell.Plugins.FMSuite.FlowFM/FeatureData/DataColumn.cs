using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    [Entity]
    public class DataColumn<T> : IDataColumn
    {
        public DataColumn() : this("") {}

        public DataColumn(string name)
        {
            ValueList = new List<T>();
            DefaultValue = default(T);
            Name = name;
            IsActive = true;
        }

        public T DefaultValue { get; set; }

        public List<T> ValueList { get; private set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Type DataType => typeof(T);

        object IDataColumn.DefaultValue
        {
            get => DefaultValue;
            set => DefaultValue = (T) value;
        }

        IList IDataColumn.ValueList
        {
            get => ValueList;
            set => ValueList = (List<T>) value;
        }

        public IList CreateDefaultValueList(int length)
        {
            return new List<T>(Enumerable.Repeat(DefaultValue, length));
        }
    }
}