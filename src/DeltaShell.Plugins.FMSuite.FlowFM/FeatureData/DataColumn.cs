using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class DataColumn<T> : IDataColumn
    {
        public DataColumn()
        {
            ValueList = new List<T>();
            DefaultValue = default(T);
        }

        public bool IsActive { get; set; }

        public Type DataType
        {
            get { return typeof(T); }
        }

        public T DefaultValue { get; set; }

        object IDataColumn.DefaultValue
        {
            get { return DefaultValue; }
            set { DefaultValue = (T) value; }
        }

        public List<T> ValueList { get; private set; }

        public IList CreateDefaultValueList(int length)
        {
            return new List<T>(Enumerable.Repeat(DefaultValue, length));
        }

        IList IDataColumn.ValueList
        {
            get { return ValueList; }
            set { ValueList = (List<T>) value; }
        }
    }
}