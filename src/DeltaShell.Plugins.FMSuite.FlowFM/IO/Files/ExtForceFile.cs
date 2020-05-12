using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
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
        private const string areaKey = "AREA";
        private const string averagingTypeKey = "AVERAGINGTYPE";
        private const string relSearchCellSizeKey = "RELATIVESEARCHCELLSIZE";

        // general keywords in file
        private const string disabledQuantityKey = "DISABLED_QUANTITY";
        private const string quantityKey = "QUANTITY";
        private const string fileNameKey = "FILENAME";
        private const string fileTypeKey = "FILETYPE";
        private const string methodKey = "METHOD";
        private const string operandKey = "OPERAND";

        // keywords in file used for polygons (*.pol files)
        private const string valueKey = "VALUE";
        private const string factorKey = "FACTOR";
        private const string offsetKey = "OFFSET";
        private const string sedimentConcentrationPostfix = "_SedConc";

        private static readonly string[] unsupportedQuantityKeys =
        {
            "WUANTITY",
            "_UANTITY"
        };

        private readonly ILog log = LogManager.GetLogger(typeof(ExtForceFile));

        // items that existed in the file when the file was read
        private readonly IDictionary<ExtForceFileItem, object> existingForceFileItems;
        private readonly HashSet<ExtForceFileItem> supportedExtForceFileItems;
        private readonly IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems;

        private string currentLine;

        public ExtForceFile()
        {
            existingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            supportedExtForceFileItems = new HashSet<ExtForceFileItem>();
            polyLineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>();
            WriteToDisk = true;
        }

        public bool WriteToDisk { get; set; }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions => polyLineForceFileItems.Keys.OfType<IBoundaryCondition>();

        /// <summary>
        /// Get the data files that are references in the extForceFile.
        /// </summary>
        /// <param name="modelDefinition"> </param>
        /// <returns> A list of tuples of name and file path. </returns>
        public IEnumerable<string[]> GetFeatureDataFiles(WaterFlowFMModelDefinition modelDefinition)
        {
            ExtForceFileHelper.StartWritingSubFiles();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bc => bc.Feature.Name != null))
            {
                foreach (FlowBoundaryCondition bc in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    polyLineForceFileItems.TryGetValue(bc, out ExtForceFileItem matchingItem);
                    List<string[]> dataFiles = ExtForceFileHelper.GetBoundaryDataFiles(bc, boundaryConditionSet, matchingItem).ToList();

                    foreach (string[] dataFile in dataFiles)
                    {
                        yield return dataFile;
                    }
                }
            }

            foreach (SourceAndSink sourceAndSink in modelDefinition.SourcesAndSinks)
            {
                polyLineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem matchingItem);
                List<string[]> dataFiles = ExtForceFileHelper.GetSourceAndSinkDataFiles(sourceAndSink, matchingItem).ToList();

                foreach (string[] dataFile in dataFiles)
                {
                    yield return dataFile;
                }
            }
        }

        protected override bool ExcludeEqualsIdentifier => false;

        private string ExtFilePath { get; set; }

        private string ExtSubFilesReferenceFilePath { get; set; }
    }
}