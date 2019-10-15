using System.Collections.Generic;
using System.IO;
using DelftTools.Functions;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public abstract class FMNetCdfFileFunctionStore : ReadOnlyNetCdfFunctionStoreBase, INameable
    {
        private const string TimeDimensionName = "time";
        
        protected override IList<string> TimeDimensionNames
        {
            get { return new[] {TimeDimensionName}; }
        }

        protected override IList<string> TimeVariableNames
        {
            get { return new[] { GetTimeVariableName(TimeDimensionName) }; }
        }

        public string Name { get; set; }
        
        //nhib
        protected FMNetCdfFileFunctionStore()
        {
        }

        protected FMNetCdfFileFunctionStore(string ncPath) : base(ncPath)
        {
        }

        protected override string GetTimeVariableName(string dimName)
        {
            return "time";
        }

        protected override void UpdateFunctionsAfterPathSet()
        {
            base.UpdateFunctionsAfterPathSet();

            Name = "Output (" + System.IO.Path.GetFileName(Path) + ")";
        }
    }
}