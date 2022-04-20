using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public interface IRainfallRunoffModel : IHydroModel, IDimrModel, INotifyPropertyChanged
    {
        /// <summary>
        /// Set CapSim calculation on or off
        /// </summary>
        bool CapSim { get; }

        /// <summary>
        /// Data declared on the runoff boundaries
        /// </summary>
        IEventedList<RunoffBoundaryData> BoundaryData { get; }
        
        /// <summary>
        /// Basin (catchments) used by this model
        /// </summary>
        IDrainageBasin Basin { get; }

        /// <summary>
        /// Data defined on the basin catchments
        /// </summary>
        IEnumerable<CatchmentModelData> ModelData { get; }

        event EventHandler<EventArgs<CatchmentModelData>> ModelDataAdded;
        
        event EventHandler<EventArgs<CatchmentModelData>> ModelDataRemoved;
    }
}