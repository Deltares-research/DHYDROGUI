using System;
using System.Collections;
using DelftTools.Utils;

namespace DelftTools.Hydro.Structures
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