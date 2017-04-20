using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class IncludeFileFactory : IncludeFileFactoryBase
    {
        #region Block 2

        /// <summary>
        /// Write the output locations (monitoring locations)
        /// Monitoring locations are determined in
        /// <see cref="WaqInitializationSettingsBuilder.CreateOutputLocationInformation"/>.
        /// </summary>
        public string CreateOutputLocationsInclude(IDictionary<string, IList<int>> outputLocations)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("{0} ; nr of monitor locations", outputLocations.Count);

                foreach (var kvp in outputLocations)
                {
                    writer.WriteLine("'{0}' {1}", kvp.Key, kvp.Value.Count);

                    foreach (var segmentId in kvp.Value.Select(segment => segment.ToString()))
                    {
                        writer.WriteLine(segmentId);
                    }
                }

                return writer.ToString();
            }
        }

        #endregion Block 2
        #region Block 3

        /// <summary>
        /// Create the include file contents that multiplies the segments per layer with the number of layers.
        /// </summary>
        public string CreateNumberOfSegmentsInclude(int segmentsPerLayer, int numberOfLayers)
        {
            return string.Format("{0} ; number of segments", segmentsPerLayer * numberOfLayers);
        }

        /// <summary>
        /// Creates the include file contents that write the filepath to the binary file with attribute.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public string CreateAttributesFileInclude(string attributesFile)
        {
            return string.Format("INCLUDE '{0}' ; attributes file", attributesFile);
        }

        /// <summary>
        /// Creates the include file contents that write the filepath to the binary file with volumes.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public string CreateVolumesFileInclude(string volumesFile)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("-2 ; volumes will be interpolated from a binary file");
                writer.WriteLine("'{0}' ; volumes file from hyd file", volumesFile);

                return writer.ToString();
            }
        }

        #endregion Block 3
        #region Block 4

        /// <summary>
        /// Creates the include file contents that writes the number of horizontal exchanges and vertical exchanges.
        /// </summary>
        /// <returns>horizontal 0 vertical</returns>
        public string CreateNumberOfExchangesInclude(int horizontalExchanges, int verticalExchanges)
        {
            return string.Format("{0} 0 {1} ; number of exchanges in three directions", horizontalExchanges, verticalExchanges);
        }

        /// <summary>
        /// Creates the include file contents that writes the filepath to the binary file with pointers.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public string CreatePointersFileInclude(string pointersFile)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("0 ; pointers from binary file.");
                writer.WriteLine("'{0}' ; pointers file", pointersFile);

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the include file contents that writes the horizontal and vertical dispersion.
        /// </summary>
        /// <returns>horizontal 0 vertical</returns>
        public string CreateConstantDispersionInclude(double verticalDispersion, IFunction spatialHorizontalDispersion)
        {
            string horizontal = spatialHorizontalDispersion.IsUnstructuredGridCellCoverage()
                ? "0.0" : WaterQualityFunctionFactory.GetDefaultValue(spatialHorizontalDispersion).ToString(CultureInfo.InvariantCulture);

            return string.Format("{0} 0.0 {1} ; constant dispersion", horizontal, verticalDispersion.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates the include file contents that writes the filepath to the binary file with areas.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public string CreateAreasFileInclude(string areasFile)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("-2 ; areas will be interpolated from a binary file");
                writer.WriteLine("'{0}' ; areas file", areasFile);

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the include file contents that writes the filepath to the binary file with flows.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public string CreateFlowsFileInclude(string flowsFile)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("-2 ; flows from binary file");
                writer.WriteLine("'{0}' ; flows file", flowsFile);

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the include file contents that writes the filepath to the binary file with lengths.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public string CreateLengthsFileInclude(string lengthsFile)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("0 ; Lengths from binary file");
                writer.WriteLine("'{0}' ; lengths file", lengthsFile);

                return writer.ToString();
            }
        }

        #endregion Block 4
        #region Block 5

        private struct BoundaryList3DInfo
        {
            public readonly int SegmentId;
            public readonly string BoundaryName;
            public readonly int LayerIndex;

            public BoundaryList3DInfo(int segmentId, string boundaryName, int layerIndex)
            {
                SegmentId = segmentId;
                BoundaryName = boundaryName;
                LayerIndex = layerIndex;
            }
        }

        /// <summary>
        /// Create the list of boundaries that is defined by segment ids.
        /// This list is obliged to be sorted ascending. This saves delwaq time during reading.
        /// </summary>
        /// <param name="boundaryNodeIds">A boundary with the segments on the top level that it touches.</param>
        /// <param name="numberOfLayers">The number of layers, because the list of segments is expanded by the number of layers.</param>
        /// <returns>A list of all boundary segments times the number of layers.</returns>
        public string CreateBoundaryListInclude(IDictionary<WaterQualityBoundary, int[]> boundaryNodeIds, int numberOfLayers)
        {
            // construct a list of boundary names with indices that will be used while writing
            IDictionary<string, int> layerIndices = new Dictionary<string, int>(boundaryNodeIds.Count);
            var boundaries = boundaryNodeIds.Keys.ToArray();
            
            for (int i = 0; i < boundaryNodeIds.Count; i++)
            {
                string name = boundaries[i].Name;
                layerIndices[name] = i;
            }

            IList<BoundaryList3DInfo> infosToWrite = GetBoundarySegmentsToWrite(boundaryNodeIds, numberOfLayers);

            // writing!
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine(";'NodeID' 'Comment field' 'Boundary name used for data grouping'");

                // sort the list
                int currentLayerNumber = 1;
                foreach (var info in infosToWrite)
                {
                    var layerNumber = info.LayerIndex+1;
                    // Write comment for starting with a new layer:
                    if (layerNumber == currentLayerNumber)
                    {
                        writer.WriteLine("; Boundaries for layer {0}", layerNumber);
                        currentLayerNumber++;
                    }

                    // write for each tuple: '[-segmentId]' '' '[boundaryName]'
                    writer.WriteLine("'{0}' '' '{1}'", 
                        info.SegmentId, info.BoundaryName);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates a list of all information that is required to write a sorted list of segment numbers with corresponding boundary groups.
        /// </summary>
        /// <returns>A list that is sorted by segment id.</returns>
        private static IList<BoundaryList3DInfo> GetBoundarySegmentsToWrite(IDictionary<WaterQualityBoundary, int[]> boundaryNodeIds, int numberOfLayers)
        {
            List<BoundaryList3DInfo> infosToWrite = new List<BoundaryList3DInfo>();
            // construct a list of Tuple<segmentId, boundaryName, layerNumber>
            foreach (var boundaryInfo in boundaryNodeIds)
            {
                string boundaryName = boundaryInfo.Key.Name;
                foreach (var segmentId in boundaryInfo.Value)
                {
                    infosToWrite.Add(new BoundaryList3DInfo(segmentId, boundaryName, 0));
                }
            }

            int initialCount = infosToWrite.Count;

            // loop over the number of layers and expand the list with multiple layers
            for (int i = 1; i < numberOfLayers; i++)
            {
                for (int j = 0; j < initialCount; j++)
                {
                    infosToWrite.Add(new BoundaryList3DInfo(
                        infosToWrite[j].SegmentId + initialCount*i,
                        infosToWrite[j].BoundaryName,
                        i));
                }
            }

            // sort
            infosToWrite.Sort((bi1, bi2) => bi1.SegmentId.CompareTo(bi2.SegmentId));

            return infosToWrite;
        }

        public string CreateBoundaryDataInclude(DataTableManager manager, string workDirectory)
        {
            return WriteDataTableManager(manager, workDirectory);
        }

        
        public string CreateBoundaryAliasesInclude(IDictionary<string, IList<string>> boundaryAliases)
        {
            return CreateLocationAliases(boundaryAliases);
        }

        #endregion Block 5
        #region Block 6

        /// <summary>
        /// Creates the include file contents for the dry waste load block.
        /// </summary>
        /// <param name="loadAndIds"></param>
        public string CreateDryWasteLoadInclude(IDictionary<WaterQualityLoad, int> loadAndIds)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("; Number of loads");
                writer.WriteLine("{0}; Number of loads", loadAndIds.Count);
                writer.WriteLine(";SegmentID  Load-name  Comment  Load-type");
                foreach (var loadAndId in loadAndIds)
                {
                    writer.WriteLine("{0} '{1}' '' '{2}'", loadAndId.Value, loadAndId.Key.Name, loadAndId.Key.LoadType);
                }

                return writer.ToString();
            }
        }

        public string CreateDryWasteLoadDataInclude(DataTableManager manager, string workDirectory)
        {
            return WriteDataTableManager(manager, workDirectory);
        }

        public string CreateDryWasteLoadAliasesInclude(IDictionary<string, IList<string>> aliases)
        {
            return CreateLocationAliases(aliases);
        }

        private static string WriteDataTableManager(DataTableManager manager, string workDirectory)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                // Reverse order as first entry is has highest priority -> should be written last.
                foreach (var dataTable in manager.DataTables.Reverse())
                {
                    if(dataTable.IsEnabled)
                    {
                        var relativeFilePath = FileUtils.GetRelativePath(workDirectory, dataTable.DataFile.Path);
                        writer.WriteLine("INCLUDE '{0}'", relativeFilePath);
                    }
                }
                return writer.ToString();
            }
        }

        #endregion Block 6
        #region Block 7

        public override string CreateParametersInclude(WaqInitializationSettings initializationSettings)
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                if (initializationSettings.SurfacesFile != null)
                {
                    writer.WriteLine("PARAMETERS");
                    writer.WriteLine("'Surf'");
                    writer.WriteLine("ALL");
                    writer.WriteLine("BINARY_FILE '{0}' ; from horizontal-surfaces-file key in hyd file", initializationSettings.SurfacesFile);    
                }

                if (initializationSettings.ProcessCoefficients != null)
                {
                    // write the rest of the contents
                    writer.Write(CreateSpatialIncludeContents(initializationSettings.ProcessCoefficients, "PARAMETERS", initializationSettings.NumberOfLayers));
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the spatial dispersion include-file contents.
        /// </summary>
        /// <param name="dispersion">The horizontal spatial dispersion.</param>
        /// <param name="numberOfLayers">The number of water quality layers.</param>
        /// <returns>File contents.</returns>
        public string CreateSpatialDispersionInclude(IFunction dispersion, int numberOfLayers)
        {
            if (dispersion.IsUnstructuredGridCellCoverage())
            {
                using (var writer = new StringWriter(new StringBuilder()))
                {
                    writer.WriteLine("CONSTANTS 'ACTIVE_HDisperAdd' DATA 1.0");
                    CreateSpatialIncludeData("PARAMETERS", numberOfLayers, writer, dispersion, "AddDispH");

                    return writer.ToString();
                }
            }

            return string.Empty;
        }

        public string CreateVerticalDiffusionInclude(string verticalDiffusionFile, bool useAdditionalVerticalDiffusion)
        {
            if (useAdditionalVerticalDiffusion && !string.IsNullOrEmpty(verticalDiffusionFile))
            {
                using (StringWriter writer = new StringWriter(new StringBuilder()))
                {
                    writer.WriteLine("CONSTANTS 'ACTIVE_VertDisp' DATA 1.0");
                    writer.WriteLine("SEG_FUNCTIONS");
                    writer.WriteLine("'VertDisper'");
                    writer.WriteLine("ALL");
                    writer.WriteLine("BINARY_FILE '{0}'", verticalDiffusionFile);

                    return writer.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates the SEG_FUNCTION include file contents, stating data on segments for
        /// items in the process library.
        /// </summary>
        public string CreateSegfunctionsInclude(WaqInitializationSettings initializationSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (var segfunction in GetSegfunctions(initializationSettings))
                {
                    writer.WriteLine("SEG_FUNCTIONS");
                    writer.WriteLine("'{0}'", segfunction.Key);
                    writer.WriteLine("ALL");
                    writer.WriteLine("BINARY_FILE '{0}'", segfunction.Value);
                    writer.WriteLine();
                }
                return writer.ToString();
            }
        }

        private IDictionary<string, string> GetSegfunctions(WaqInitializationSettings initializationSettings)
        {
            // TODO SOBEK: Check for data available on 'Chezy', 'Velocity' and 'Width'
            var origDict = initializationSettings.ProcessCoefficients.OfType<FunctionFromHydroDynamics>().ToDictionary(f => f.Name, f => f.FilePath);
            initializationSettings.ProcessCoefficients.OfType<SegmentFileFunction>().ForEach(pc => origDict.Add(pc.Name, pc.UrlPath));
            // TODO TOOLS-21968: 'Surf'
            return origDict;
        }

        /// <summary>
        /// Creates the include file containing various numerical options for delwaq.
        /// </summary>
        public string CreateNumericalOptionsInclude(WaqInitializationSettings set)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                // WRite Delwaq flags (When on, should be written; Data value doesn't matter):
                if (set.Settings.ClosureErrorCorrection)
                {
                    writer.WriteLine("CONSTANTS 'CLOSE_ERR' DATA 1 ; If defined, allow delwaq to correct water volumes to keep concentrations continuous");
                }

                // Write numerical options:
                writer.WriteLine("CONSTANTS 'NOTHREADS' DATA {0} ; Number of threads used by delwaq", set.Settings.NrOfThreads);
                writer.WriteLine("CONSTANTS 'DRY_THRESH' DATA {0} ; Dry cell threshold", set.Settings.DryCellThreshold.ToString(CultureInfo.InvariantCulture));

                // Write numerical scheme related options:
                if (set.Settings.NumericalScheme.IsIterativeCalculationScheme())
                {
                    writer.WriteLine("CONSTANTS 'maxiter' DATA {0} ; Maximum number of iterations", set.Settings.IterationMaximum);
                    writer.WriteLine("CONSTANTS 'tolerance' DATA {0} ; Convergence tolerance", set.Settings.Tolerance.ToString(CultureInfo.InvariantCulture));
                    writer.WriteLine("CONSTANTS 'iteration report' DATA {0} ; Write iteration report (when 1) or not (when 0)", set.Settings.WriteIterationReport ? "1" : "0");
                }

                return writer.ToString();
            }
        }

        #endregion Block 7
        #region Block 8

        protected override string CreateSpatialInitialConditionsFileContents(WaqInitializationSettings initializationSettings)
        {
            return CreateSpatialIncludeContents(initializationSettings.InitialConditions, "INITIALS", initializationSettings.NumberOfLayers);
        }

        #endregion Block 8

        private string CreateSpatialIncludeContents(ICollection<IFunction> functionCollection, string spatialDataGroupName, int numberOfLayers)
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                foreach (var locationDependentSpatialData in functionCollection.Where(pc => pc.IsUnstructuredGridCellCoverage()))
                {
                    CreateSpatialIncludeData(spatialDataGroupName, numberOfLayers, writer, locationDependentSpatialData, locationDependentSpatialData.Name);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the spatial include data.
        /// </summary>
        /// <param name="spatialDataGroupName">Name of the spatial data group, such as "PARAMETERS".</param>
        /// <param name="numberOfLayers">The number of water quality layers.</param>
        /// <param name="writer">The writer to add text to.</param>
        /// <param name="spatialData">The spatial data.</param>
        /// <param name="spatialDataName">The name to identify the data with.</param>
        private void CreateSpatialIncludeData(string spatialDataGroupName, int numberOfLayers, StringWriter writer,
            IFunction spatialData, string spatialDataName)
        {
            writer.WriteLine(spatialDataGroupName);
            writer.WriteLine("'{0}'", spatialDataName);
            writer.WriteLine("ALL");
            writer.WriteLine("DATA");

            var cellValues = spatialData.GetValues<double>();

            // set values on the segments uniform over all layers
            for (int i = 0; i < numberOfLayers; i++)
            {
                foreach (var cellValue in cellValues)
                {
                    writer.WriteLine(cellValue.ToString(CultureInfo.InvariantCulture));
                }
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Write the location aliases as required for boundaries or loads.
        /// </summary>
        /// <example>
        /// <c>
        /// USEDATA_ITEM 'boei 23' FORITEM
        /// 'sea'
        /// 'laguna'
        /// 'ocean'
        /// 
        /// USEDATA_ITEM 'boei 34' FORITEM
        /// 'sea'
        /// 
        /// </c>
        /// </example>
        private static string CreateLocationAliases(IDictionary<string, IList<string>> locationAliases)
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                foreach (var measureLocation in locationAliases)
                {
                    writer.WriteLine("USEDATA_ITEM '{0}' FORITEM", measureLocation.Key);
                    foreach (var boundary in measureLocation.Value)
                    {
                        writer.WriteLine("'{0}'", boundary);
                    }
                    writer.WriteLine();
                }
                return writer.ToString();
            }
        }
    }
}