using System;
using DelftTools.Functions;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    public interface IMeteoDataDistributed : ICloneable, IUnique<long>
    {
        IFunction Data { get; set; }

        IFunction GetTimeSeries(object item);
    }
}
