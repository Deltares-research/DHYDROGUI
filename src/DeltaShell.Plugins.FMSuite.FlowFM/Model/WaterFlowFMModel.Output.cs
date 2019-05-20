using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        private FMMapFileFunctionStore outputMapFileStore;

        public virtual FMMapFileFunctionStore OutputMapFileStore
        {
            get => outputMapFileStore;
            protected set => outputMapFileStore = value;
        }

        public virtual FMClassMapFileFunctionStore OutputClassMapFileStore { get; protected set; }

        public virtual FMHisFileFunctionStore OutputHisFileStore { get; protected set; }

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
    }
}