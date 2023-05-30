using System;
using System.ComponentModel;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public interface IRRUnitAwareView
    {
        RainfallRunoffEnums.AreaUnit AreaUnit { set; }
    }

    public interface IRRModelTimeAwareView
    {
        DateTime StartTime { set; }
        DateTime StopTime { set; }
        TimeSpan TimeStep { set; }
    }
    
    public interface IRRMeteoStationAwareView
    {
        bool UseMeteoStations { set; }
        IEventedList<string> MeteoStations { set; }
    }

    public interface IRRTemperatureStationAwareView
    {
        bool UseTemperatureStations { set; }
        IEventedList<string> TemperatureStations { set; } 
    }
    
    public interface IRRModelRunModeAwareView : IComponent
    {
        void SetInitialWorkFlowState(bool isRunningParallel);
        void WorkflowChanged(object sender, bool isRunningParallel);
    }
}