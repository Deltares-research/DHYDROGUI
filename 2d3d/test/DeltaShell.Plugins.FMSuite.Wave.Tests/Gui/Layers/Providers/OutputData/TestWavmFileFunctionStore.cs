using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers.OutputData
{
    public class TestWavmFileFunctionStore : FMNetCdfFileFunctionStore, IWavmFileFunctionStore
    {
        public CurvilinearGrid Grid { get; set; }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            throw new NotImplementedException();
        }
    }
    
    public class TestWavhFileFunctionStore : FMNetCdfFileFunctionStore, IWavhFileFunctionStore
    {
        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            throw new NotImplementedException();
        }
    }
}