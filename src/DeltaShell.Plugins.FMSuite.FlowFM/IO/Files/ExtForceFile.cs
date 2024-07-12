using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.ExtForce;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class ExtForceFile : NGHSFileBase
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ExtForceFile));
        
        private readonly HashSet<ExtForceData> supportedExtForceFileItems;
        private readonly IFileSystem fileSystem;

        private string extFilePath;
        private string extSubFilesReferenceFilePath;
        private WaterFlowFMModelDefinition modelDefinition;

        public ExtForceFile()
        {
            ExistingForceFileItems = new Dictionary<ExtForceData, object>();
            supportedExtForceFileItems = new HashSet<ExtForceData>();
            PolyLineForceFileItems = new Dictionary<IFeatureData, ExtForceData>();
            fileSystem = new FileSystem();
        }

        // items that existed in the file when the file was read
        public IDictionary<ExtForceData, object> ExistingForceFileItems { get; }
        public IDictionary<IFeatureData, ExtForceData> PolyLineForceFileItems { get; }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions =>
            PolyLineForceFileItems.Keys.OfType<IBoundaryCondition>();

        protected override bool ExcludeEqualsIdentifier => false;
    }
}