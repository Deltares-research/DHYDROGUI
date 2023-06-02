using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public interface ICatchmentModelDataSynchronizer
    {
        Action<CatchmentModelData> OnAreaAddedOrModified { get; set; }
        Action<CatchmentModelData> OnAreaRemoved { get; set; }
        void Disconnect();
    }
}