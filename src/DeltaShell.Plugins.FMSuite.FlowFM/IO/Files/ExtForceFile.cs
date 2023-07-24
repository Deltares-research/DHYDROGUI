using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public partial class ExtForceFile : NGHSFileBase
    {
        private const string extForcesFileQuantityBlockStarter = "QUANTITY=";

        // Known file extensions

        // keywords in file used for modelDefinition specific data
        private const string frictionTypeKey = "IFRCTYP";

        // general keywords in file
        private const string disabledQuantityKey = "DISABLED_QUANTITY";

        // keywords in file used for polygons (*.pol files)
        private const string valueKey = "VALUE";
        private const string factorKey = "FACTOR";
        private const string offsetKey = "OFFSET";
        private const string extrapoltolKey = "EXTRAPOLTOL";

        private static readonly string[] unsupportedQuantityKeys =
        {
            "WUANTITY",
            "_UANTITY"
        };

        private readonly ILog log = LogManager.GetLogger(typeof(ExtForceFile));
        private readonly HashSet<ExtForceFileItem> supportedExtForceFileItems;

        private string currentLine;

        private string extFilePath;

        public ExtForceFile()
        {
            ExistingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            supportedExtForceFileItems = new HashSet<ExtForceFileItem>();
            PolyLineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>();
        }

        // items that existed in the file when the file was read
        public IDictionary<ExtForceFileItem, object> ExistingForceFileItems { get; }
        public IDictionary<IFeatureData, ExtForceFileItem> PolyLineForceFileItems { get; }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions =>
            PolyLineForceFileItems.Keys.OfType<IBoundaryCondition>();

        protected override bool ExcludeEqualsIdentifier => false;
    }
}