using System;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        private IFMMapFileFunctionStore outputMapFileStore;

        public virtual IFMMapFileFunctionStore OutputMapFileStore
        {
            get => outputMapFileStore;
            protected set => outputMapFileStore = value;
        }

        public virtual IFMClassMapFileFunctionStore OutputClassMapFileStore { get; protected set; }

        public virtual IFMHisFileFunctionStore OutputHisFileStore { get; protected set; }

        public TimeSpan OutputTimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = value;
        }

        public UnstructuredGridCellCoverage OutputWaterLevel
        {
            get
            {
                if (OutputMapFileStore != null)
                {
                    return OutputMapFileStore.Functions.OfType<UnstructuredGridCellCoverage>()
                                             .FirstOrDefault(f => f.Components[0].Name.EndsWith("s1"));
                }

                return null;
            }
        }

        private bool HasOpenFunctionStores =>
            OutputMapFileStore != null || OutputHisFileStore != null || OutputClassMapFileStore != null;

        private static void ClearFunctionStore(IReadOnlyNetCdfFunctionStoreBase functionStore)
        {
            functionStore.Functions.Clear();
            functionStore.Close();
        }
    }
}