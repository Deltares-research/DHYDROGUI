using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.IO;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FMClassMapFileFunctionStore : FMNetCdfFileFunctionStore
    {
        public FMClassMapFileFunctionStore(string classMapFilePath) : base (classMapFilePath)
        {
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            var functions = new List<IFunction>();
            return functions;
        }
    }
}
