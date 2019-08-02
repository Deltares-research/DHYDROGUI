using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
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

        private bool HasOpenFunctionStores =>
            OutputMapFileStore != null || OutputHisFileStore != null || OutputClassMapFileStore != null;

        private static void ClearFunctionStore(ReadOnlyNetCdfFunctionStoreBase functionStore)
        {
            functionStore.Functions.Clear();
            functionStore.Close();
            try
            {
                FileUtils.DeleteIfExists(functionStore.Path);
            }
            catch (IOException e)
            {
                Log.WarnFormat("Unable to remove output file '{0}':{1}{2}", functionStore.Path, Environment.NewLine, e.Message);
            }
        }
    }
}