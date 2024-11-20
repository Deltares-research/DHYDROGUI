using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    /// <summary>
    /// A derived RealTimeControlModel for testing purposes, which takes a list of RestartFile to use as RestartOutput
    /// </summary>
    internal class RealTimeControlModelForTestingRestartOutput : RealTimeControlModel
    {
        public RealTimeControlModelForTestingRestartOutput(List<RealTimeControlRestartFile> outputRestartFilesForTesting)
        {
            ListOfOutputRestartFiles = outputRestartFilesForTesting;
        }
    }
        
}