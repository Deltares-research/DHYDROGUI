using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public interface IRainfallRunoffAreaUnitManager
    {
        RainfallRunoffEnums.AreaUnit AreaUnit { get; set; }
        event EventHandler AreaUnitChanged;
    }
}