using System;
using System.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public interface ISedimentProperty
    {
        string Name { get; set; }
        string Unit { get; set; }
        bool MduOnly { get; set; }
        Func<List<ISediment>, bool> Visible { get; set; }
        Func<List<ISediment>, bool> Enabled { get; set; }
        bool IsEnabled { get; set; }
        bool IsVisible { get; set; }
        string Description { get; set; }
        string DataTemplateName { get; }
        void SedimentPropertyLoad(IniSection section);
        void SedimentPropertyWrite(IniSection section);
    }

    public interface ISedimentProperty<T> : ISedimentProperty
    {
        T Value { get; set; }
        T DefaultValue { get; set; }
        T MinValue { get; set; }
        T MaxValue { get; set; }
        bool MinIsOpened { get; set; }
        bool MaxIsOpened { get; set; }
    }

    public interface ISpatiallyVaryingSedimentProperty : ISedimentProperty
    {
        bool IsSpatiallyVarying { get; set; }
        string SpatiallyVaryingName { get; set; }
    }

    public interface ISpatiallyVaryingSedimentProperty<T> : ISpatiallyVaryingSedimentProperty, ISedimentProperty<T> {}
}