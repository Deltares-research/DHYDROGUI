using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Common.FunctionStores
{
    public abstract class FMNetCdfFileFunctionStore : ReadOnlyNetCdfFunctionStoreBase, INameable
    {
        private const string TimeDimensionName = "time";

        //nhib
        protected FMNetCdfFileFunctionStore() {}

        protected FMNetCdfFileFunctionStore(string ncPath) : base(ncPath) {}

        public string Name { get; set; }

        protected override IList<string> TimeDimensionNames => new[]
        {
            TimeDimensionName
        };

        protected override IList<string> TimeVariableNames => new[]
        {
            GetTimeVariableName(TimeDimensionName)
        };

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