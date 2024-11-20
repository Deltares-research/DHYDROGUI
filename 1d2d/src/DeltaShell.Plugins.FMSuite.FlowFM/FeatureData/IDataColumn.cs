using System;
using System.Collections;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public interface IDataColumn : INameable
    {
        bool IsActive { get; set; }

        Type DataType { get; }
        
        object DefaultValue { get; set; }

        IList ValueList { get; set; }

        IList CreateDefaultValueList(int length);
    }
}