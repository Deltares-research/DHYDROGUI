using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.IO;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{

    public class FMClassMapFileFunctionStore : FMNetCdfFileFunctionStore
    {
        /// <summary>Initializes a new instance of the <see cref="FMClassMapFileFunctionStore"/> class.</summary>
        /// <param name="classMapFilePath">The class map file path.</param>
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
