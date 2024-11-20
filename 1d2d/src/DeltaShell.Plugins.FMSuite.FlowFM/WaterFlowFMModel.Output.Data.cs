using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Functions;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private FMMapFileFunctionStore outputMapFileStore;
        private FM1DFileFunctionStore output1DFileStore;

        public IEnumerable<RestartFile> RestartOutput { get; set; } = Enumerable.Empty<RestartFile>();

        public TimeSpan OutputTimeStep
        {
            get { return (TimeSpan)ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value; }
            set { ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = value; }
        }

        public virtual FMMapFileFunctionStore OutputMapFileStore
        {
            get { return outputMapFileStore; }
            protected set
            {
                outputMapFileStore = value;
            }
        }

        public virtual FM1DFileFunctionStore Output1DFileStore
        {
            get { return output1DFileStore; }
            protected set
            {
                output1DFileStore = value;
            }
        }

        public virtual FMHisFileFunctionStore OutputHisFileStore { get; protected set; }
        
        public virtual FMClassMapFileFunctionStore OutputClassMapFileStore { get; protected set; }

        public virtual FouFileFunctionStore OutputFouFileStore { get; protected set; }
        
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
        
        private void ClearFunctionStore(PropertyInfo property)
        {
            if (!(property.GetValue(this) is IFunctionStore functionStore))
            {
                return;
            }

            // FunctionStores are cleared, but NetworkCoverages still listen to Network changes,
            // so the Network should be set to null.
            foreach (INetworkCoverage function in functionStore.Functions.OfType<INetworkCoverage>())
            {
                function.Network = null;
            }
            functionStore.Functions.Clear();

            if (functionStore is IFileBased fileBasedFunctionStore)
            {
                fileBasedFunctionStore.Close();
            }

            property.SetValue(this, null);
        }
    }
}