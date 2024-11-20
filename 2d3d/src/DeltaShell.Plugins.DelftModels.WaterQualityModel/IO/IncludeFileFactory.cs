using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public static class IncludeFileFactory
    {
        private static string CreateSpatialIncludeContents(ICollection<IFunction> functionCollection,
                                                           string spatialDataGroupName, int numberOfLayers)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (IFunction locationDependentSpatialData in functionCollection.Where(
                    pc => pc.IsUnstructuredGridCellCoverage()))
                {
                    CreateSpatialIncludeData(spatialDataGroupName, numberOfLayers, writer, locationDependentSpatialData,
                                             locationDependentSpatialData.Name);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the spatial include data.
        /// </summary>
        /// <param name="spatialDataGroupName"> Name of the spatial data group, such as "PARAMETERS". </param>
        /// <param name="numberOfLayers"> The number of water quality layers. </param>
        /// <param name="writer"> The writer to add text to. </param>
        /// <param name="spatialData"> The spatial data. </param>
        /// <param name="spatialDataName"> The name to identify the data with. </param>
        private static void CreateSpatialIncludeData(string spatialDataGroupName, int numberOfLayers,
                                                     StringWriter writer,
                                                     IFunction spatialData, string spatialDataName)
        {
            writer.WriteLine(spatialDataGroupName);
            writer.WriteLine("'{0}'", spatialDataName);
            writer.WriteLine("ALL");
            writer.WriteLine("DATA");

            IMultiDimensionalArray<double> cellValues = spatialData.GetValues<double>();

            // set values on the segments uniform over all layers
            for (var i = 0; i < numberOfLayers; i++)
            {
                foreach (double cellValue in cellValues)
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
        ///     <c>
        ///     USEDATA_ITEM 'boei 23' FORITEM
        ///     'sea'
        ///     'laguna'
        ///     'ocean'
        ///     USEDATA_ITEM 'boei 34' FORITEM
        ///     'sea'
        ///     </c>
        /// </example>
        private static string CreateLocationAliases(IDictionary<string, IList<string>> locationAliases)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (KeyValuePair<string, IList<string>> measureLocation in locationAliases)
                {
                    writer.WriteLine("USEDATA_ITEM '{0}' FORITEM", measureLocation.Key);
                    foreach (string boundary in measureLocation.Value)
                    {
                        writer.WriteLine("'{0}'", boundary);
                    }

                    writer.WriteLine();
                }

                return writer.ToString();
            }
        }

        #region Block 1

        public static string CreateT0Include(DateTime referenceTime)
        {
            return string.Format("'T0: {0}  (scu=       1s)'", referenceTime.ToString("yyyy.MM.dd HH:mm:ss",
                                                                                      CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Create the list of substances with active and passive substances.
        /// </summary>
        public static string CreateSubstanceListInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            List<WaterQualitySubstance> activeSubstances = substanceProcessLibrary.ActiveSubstances.ToList();
            List<WaterQualitySubstance> inActiveSubstances = substanceProcessLibrary.InActiveSubstances.ToList();

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("; number of active and inactive substances");
                writer.WriteLine("{0}             {1}",
                                 activeSubstances.Count, inActiveSubstances.Count);

                WriteSubstanceList(writer, activeSubstances, 1, "active substances");
                WriteSubstanceList(writer, inActiveSubstances, activeSubstances.Count + 1, "passive substances");

                return writer.ToString();
            }
        }

        private static void WriteSubstanceList(StringWriter writer, IList<WaterQualitySubstance> substances,
                                               int startingSubstanceCount, string comment)
        {
            writer.WriteLine("        ; {0}", comment);

            for (var index = 0; index < substances.Count; index++)
            {
                WaterQualitySubstance substance = substances[index];

                writer.WriteLine("{0}            '{1}' ;{2}",
                                 index + startingSubstanceCount,
                                 substance.Name,
                                 substance.Description);
            }
        }

        #endregion Block 1

        #region Block 2

        /// <summary>
        /// Write the output locations (monitoring locations)
        /// Monitoring locations are determined in
        /// <see cref="WaqInitializationSettingsBuilder.CreateOutputLocationInformation"/>.
        /// </summary>
        public static string CreateOutputLocationsInclude(IDictionary<string, IList<int>> outputLocations)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("{0} ; nr of monitor locations", outputLocations.Count);

                foreach (KeyValuePair<string, IList<int>> kvp in outputLocations)
                {
                    writer.WriteLine("'{0}' {1}", kvp.Key, kvp.Value.Count);

                    foreach (string segmentId in kvp.Value.Select(segment => segment.ToString()))
                    {
                        writer.WriteLine(segmentId);
                    }
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Write the general waq model settings for the run.
        /// </summary>
        public static string CreateNumSettingsInclude(IWaterQualityModelSettings waqSettings)
        {
            int integrationOptions = waqSettings.NoDispersionIfFlowIsZero ? 1 : 0;
            integrationOptions += waqSettings.NoDispersionOverOpenBoundaries ? 2 : 0;
            integrationOptions += waqSettings.UseFirstOrder ? 0 : 4;

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("{0}.{1}{2} ; integration option",
                                 (int)waqSettings.NumericalScheme,
                                 integrationOptions, waqSettings.Balance ? 3 : 0);

                writer.WriteLine("; detailed balance options");
                if (waqSettings.Balance)
                {
                    if (waqSettings.BalanceUnit != BalanceUnit.Gram)
                    {
                        writer.WriteLine(waqSettings.BalanceUnit == BalanceUnit.GramPerSquareMeter
                                             ? "BAL_UNITAREA"
                                             : "BAL_UNITVOLUME");
                    }

                    writer.WriteLine("{0} {1} {2}",
                                     waqSettings.LumpProcesses ? "BAL_LUMPPROCESSES" : "BAL_NOLUMPPROCESSES",
                                     waqSettings.LumpTransport ? "BAL_LUMPTRANSPORT" : "BAL_NOLUMPTRANSPORT",
                                     waqSettings.LumpLoads ? "BAL_LUMPLOADS" : "BAL_NOLUMPLOADS");

                    writer.WriteLine("{0} {1}",
                                     waqSettings.SuppressSpace ? "BAL_SUPPRESSSPACE" : "BAL_NOSUPPRESSSPACE",
                                     waqSettings.SuppressTime ? "BAL_SUPPRESSTIME" : "BAL_NOSUPPRESSTIME");
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Create the include file with output timers.
        /// </summary>
        public static string CreateOutputTimersInclude(IWaterQualityModelSettings waqSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("; output control (see DELWAQ-manual)");
                writer.WriteLine("; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss  dddhhmmss");
                writer.WriteLine("{0} for balance output",
                                 CreateDelwaqTimeSettingsInputString(waqSettings.BalanceStartTime,
                                                                     waqSettings.BalanceStopTime,
                                                                     waqSettings.BalanceTimeStep));
                writer.WriteLine("{0} for map output",
                                 CreateDelwaqTimeSettingsInputString(waqSettings.MapStartTime,
                                                                     waqSettings.MapStopTime,
                                                                     waqSettings.MapTimeStep));
                writer.WriteLine("{0} for his output",
                                 CreateDelwaqTimeSettingsInputString(waqSettings.HisStartTime,
                                                                     waqSettings.HisStopTime,
                                                                     waqSettings.HisTimeStep));

                return writer.ToString();
            }
        }

        /// <summary>
        /// Create the include file with the simulation time from the model.
        /// Start time, stop time, time step.
        /// </summary>
        public static string CreateSimTimersInclude(WaqInitializationSettings initializationSettings)
        {
            return CreateDelwaqTimeSettingsInputString(initializationSettings.SimulationStartTime,
                                                       initializationSettings.SimulationStopTime,
                                                       initializationSettings.SimulationTimeStep, true);
        }

        /// <summary>
        /// Creates a formatted string based on a <see cref="startTime"/>, a <see cref="stopTime"/> and a <see cref="timeStep"/>
        /// </summary>
        /// <param name="startTime"> The start time </param>
        /// <param name="stopTime"> The stop time </param>
        /// <param name="timeStep"> The time step </param>
        /// <param name="addEndLineCharacters">
        /// Whether or not to add end line characters (\n) after the parameters (also whether
        /// or not to add a timestep constant)
        /// </param>
        private static string CreateDelwaqTimeSettingsInputString(DateTime startTime, DateTime stopTime,
                                                                  TimeSpan timeStep, bool addEndLineCharacters = false)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                if (addEndLineCharacters)
                {
                    writer.WriteLine("  {0} ; start time", DateTimeToString(startTime));
                    writer.WriteLine("  {0} ; stop time", DateTimeToString(stopTime));
                    writer.WriteLine("  0 ; timestep constant");
                    writer.Write("  {0} ; timestep", FormatTimeStep(timeStep));
                }
                else
                {
                    writer.Write("  {0}  {1}  {2} ;  start, stop and step", DateTimeToString(startTime),
                                 DateTimeToString(stopTime), FormatTimeStep(timeStep));
                }

                return writer.ToString();
            }
        }

        private static string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd-HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static string FormatTimeStep(TimeSpan timeStep)
        {
            return timeStep.Days.ToString("000") + timeStep.Hours.ToString("00") +
                   timeStep.Minutes.ToString("00") + timeStep.Seconds.ToString("00");
        }

        #endregion Block 2

        #region Block 3

        /// <summary>
        /// Creates the include file contents for the grid file.
        /// </summary>
        /// <param name="gridFile">The absolute grid file path.</param>
        /// <returns></returns>
        public static string CreateGridFileInclude(string gridFile)
        {
            return $"UGRID '{gridFile}'";
        }

        /// <summary>
        /// Create the include file contents that multiplies the segments per layer with the number of layers.
        /// </summary>
        public static string CreateNumberOfSegmentsInclude(int segmentsPerLayer, int numberOfLayers)
        {
            return string.Format("{0} ; number of segments", segmentsPerLayer * numberOfLayers);
        }

        /// <summary>
        /// Creates the include file contents that write the filepath to the binary file with attribute.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public static string CreateAttributesFileInclude(string attributesFile)
        {
            return string.Format("INCLUDE '{0}' ; attributes file", attributesFile);
        }

        /// <summary>
        /// Creates the include file contents that write the filepath to the binary file with volumes.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public static string CreateVolumesFileInclude(string volumesFile)
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
        /// <returns> horizontal 0 vertical </returns>
        public static string CreateNumberOfExchangesInclude(int horizontalExchanges, int verticalExchanges)
        {
            return string.Format("{0} 0 {1} ; number of exchanges in three directions", horizontalExchanges,
                                 verticalExchanges);
        }

        /// <summary>
        /// Creates the include file contents that writes the filepath to the binary file with pointers.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public static string CreatePointersFileInclude(string pointersFile)
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
        /// <returns> horizontal 0 vertical </returns>
        public static string CreateConstantDispersionInclude(double verticalDispersion,
                                                             IFunction spatialHorizontalDispersion)
        {
            string horizontal = spatialHorizontalDispersion.IsUnstructuredGridCellCoverage()
                                    ? "0.0"
                                    : WaterQualityFunctionFactory
                                      .GetDefaultValue(spatialHorizontalDispersion)
                                      .ToString(CultureInfo.InvariantCulture);

            return string.Format("{0} 0.0 {1} ; constant dispersion", horizontal,
                                 verticalDispersion.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates the include file contents that writes the filepath to the binary file with areas.
        /// This construction is used to insert dynamic names from the hyd file.
        /// </summary>
        public static string CreateAreasFileInclude(string areasFile)
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
        public static string CreateFlowsFileInclude(string flowsFile)
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
        public static string CreateLengthsFileInclude(string lengthsFile)
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
        /// <param name="boundaryNodeIds"> A boundary with the segments on the top level that it touches. </param>
        /// <param name="numberOfLayers"> The number of layers, because the list of segments is expanded by the number of layers. </param>
        /// <returns> A list of all boundary segments times the number of layers. </returns>
        public static string CreateBoundaryListInclude(IDictionary<WaterQualityBoundary, int[]> boundaryNodeIds,
                                                       int numberOfLayers)
        {
            // construct a list of boundary names with indices that will be used while writing
            IDictionary<string, int> layerIndices = new Dictionary<string, int>(boundaryNodeIds.Count);
            WaterQualityBoundary[] boundaries = boundaryNodeIds.Keys.ToArray();

            for (var i = 0; i < boundaryNodeIds.Count; i++)
            {
                string name = boundaries[i].Name;
                layerIndices[name] = i;
            }

            IList<BoundaryList3DInfo> infosToWrite = GetBoundarySegmentsToWrite(boundaryNodeIds, numberOfLayers);

            // writing!
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine(";'NodeID' 'Comment field' 'Boundary name used for data grouping'");

                // sort the list
                var currentLayerNumber = 1;
                foreach (BoundaryList3DInfo info in infosToWrite)
                {
                    int layerNumber = info.LayerIndex + 1;
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
        /// Creates a list of all information that is required to write a sorted list of segment numbers with corresponding
        /// boundary groups.
        /// </summary>
        /// <returns> A list that is sorted by segment id. </returns>
        private static IList<BoundaryList3DInfo> GetBoundarySegmentsToWrite(
            IDictionary<WaterQualityBoundary, int[]> boundaryNodeIds, int numberOfLayers)
        {
            var infosToWrite = new List<BoundaryList3DInfo>();
            // construct a list of Tuple<segmentId, boundaryName, layerNumber>
            foreach (KeyValuePair<WaterQualityBoundary, int[]> boundaryInfo in boundaryNodeIds)
            {
                string boundaryName = boundaryInfo.Key.Name;
                foreach (int segmentId in boundaryInfo.Value)
                {
                    infosToWrite.Add(new BoundaryList3DInfo(segmentId, boundaryName, 0));
                }
            }

            int initialCount = infosToWrite.Count;

            // loop over the number of layers and expand the list with multiple layers
            for (var i = 1; i < numberOfLayers; i++)
            {
                for (var j = 0; j < initialCount; j++)
                {
                    infosToWrite.Add(new BoundaryList3DInfo(
                                         infosToWrite[j].SegmentId + (initialCount * i),
                                         infosToWrite[j].BoundaryName,
                                         i));
                }
            }

            // sort
            infosToWrite.Sort((bi1, bi2) => bi1.SegmentId.CompareTo(bi2.SegmentId));

            return infosToWrite;
        }

        public static string CreateBoundaryDataInclude(DataTableManager manager, string workDirectory)
        {
            return WriteDataTableManager(manager, workDirectory);
        }

        public static string CreateBoundaryAliasesInclude(IDictionary<string, IList<string>> boundaryAliases)
        {
            return CreateLocationAliases(boundaryAliases);
        }

        #endregion Block 5

        #region Block 6

        /// <summary>
        /// Creates the include file contents for the dry waste load block.
        /// </summary>
        /// <param name="loadAndIds"> </param>
        public static string CreateDryWasteLoadInclude(IDictionary<WaterQualityLoad, int> loadAndIds)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("; Number of loads");
                writer.WriteLine("{0}; Number of loads", loadAndIds.Count);
                writer.WriteLine(";SegmentID  Load-name  Comment  Load-type");
                foreach (KeyValuePair<WaterQualityLoad, int> loadAndId in loadAndIds)
                {
                    writer.WriteLine("{0} '{1}' '' '{2}'", loadAndId.Value, loadAndId.Key.Name, loadAndId.Key.LoadType);
                }

                return writer.ToString();
            }
        }

        public static string CreateDryWasteLoadDataInclude(DataTableManager manager, string workDirectory)
        {
            return WriteDataTableManager(manager, workDirectory);
        }

        public static string CreateDryWasteLoadAliasesInclude(IDictionary<string, IList<string>> aliases)
        {
            return CreateLocationAliases(aliases);
        }

        private static string WriteDataTableManager(DataTableManager manager, string workDirectory)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                // Reverse order as first entry is has highest priority -> should be written last.
                foreach (DataTable dataTable in manager.DataTables.Reverse())
                {
                    if (dataTable.IsEnabled)
                    {
                        string relativeFilePath = FileUtils.GetRelativePath(workDirectory, dataTable.DataFile.Path);
                        string convertedFilePath = FileUtils.ReplaceDirectorySeparator(relativeFilePath);
                        writer.WriteLine("INCLUDE '{0}'", convertedFilePath);
                    }
                }

                return writer.ToString();
            }
        }

        #endregion Block 6

        #region Block 7

        /// <summary>
        /// Creates the processes include file contents, stating which processes should be enabled.
        /// </summary>
        public static string CreateProcessesInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (WaterQualityProcess waterQualityProcess in substanceProcessLibrary.Processes)
                {
                    writer.WriteLine("CONSTANTS 'ACTIVE_{0}' DATA 0", waterQualityProcess.Name);
                }

                return writer.ToString();
            }
        }

        private static void WriteConstant(StringWriter writer, IFunction meteoParameter)
        {
            double defaultValue = WaterQualityFunctionFactory.GetDefaultValue(meteoParameter);

            writer.WriteLine("CONSTANTS '{0}' DATA {1}",
                             meteoParameter.Name,
                             defaultValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void WriteTimeSeries(StringWriter writer, IFunction timeDependentFunction)
        {
            IVariable timeVariable = timeDependentFunction.Arguments[0];
            IVariable valueVariable = timeDependentFunction.Components[0];

            writer.WriteLine("FUNCTIONS");
            writer.WriteLine(timeDependentFunction.Name);
            writer.WriteLine(timeVariable.InterpolationType == InterpolationType.Linear
                                 ? "LINEAR DATA"
                                 : "DATA");

            for (var i = 0; i < timeVariable.Values.Count; i++)
            {
                writer.WriteLine("{0} {1}",
                                 ((DateTime)timeVariable.Values[i]).ToString(
                                     "yyyy/MM/dd-HH:mm:ss", CultureInfo.InvariantCulture),
                                 ((double)valueVariable.Values[i]).ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Creates the constants include file contents, stating all constant parameter values.
        /// </summary>
        public static string CreateConstantsInclude(IEnumerable<IFunction> processCoefficients)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (IFunction constantProcessCoefficient in processCoefficients.Where(pc => pc.IsConst()))
                {
                    WriteConstant(writer, constantProcessCoefficient);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the functions include file contents, stating all time-dependent parameter values.
        /// </summary>
        public static string CreateFunctionsInclude(IEnumerable<IFunction> processCoefficients)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (IFunction timeDependentProcessCoefficient in processCoefficients.Where(pc => pc.IsTimeSeries())
                )
                {
                    WriteTimeSeries(writer, timeDependentProcessCoefficient);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the spatial process parameters include file contents.
        /// </summary>
        public static string CreateParametersInclude(WaqInitializationSettings initializationSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                if (initializationSettings.SurfacesFile != null)
                {
                    writer.WriteLine("PARAMETERS");
                    writer.WriteLine("'Surf'");
                    writer.WriteLine("ALL");
                    writer.WriteLine("BINARY_FILE '{0}' ; from horizontal-surfaces-file key in hyd file",
                                     FileUtils.ReplaceDirectorySeparator(initializationSettings.SurfacesFile));
                }

                if (initializationSettings.ProcessCoefficients != null)
                {
                    // write the rest of the contents
                    writer.Write(CreateSpatialIncludeContents(initializationSettings.ProcessCoefficients, "PARAMETERS",
                                                              initializationSettings.NumberOfLayers));
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the spatial dispersion include-file contents.
        /// </summary>
        /// <param name="dispersion"> The horizontal spatial dispersion. </param>
        /// <param name="numberOfLayers"> The number of water quality layers. </param>
        /// <returns> File contents. </returns>
        public static string CreateSpatialDispersionInclude(IFunction dispersion, int numberOfLayers)
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

        public static string CreateVerticalDiffusionInclude(string verticalDiffusionFile,
                                                            bool useAdditionalVerticalDiffusion)
        {
            if (useAdditionalVerticalDiffusion && !string.IsNullOrEmpty(verticalDiffusionFile))
            {
                using (var writer = new StringWriter(new StringBuilder()))
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
        public static string CreateSegfunctionsInclude(WaqInitializationSettings initializationSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (KeyValuePair<string, string> segfunction in GetSegfunctions(initializationSettings))
                {
                    writer.WriteLine("SEG_FUNCTIONS");
                    writer.WriteLine("'{0}'", segfunction.Key);
                    writer.WriteLine("ALL");
                    writer.WriteLine("BINARY_FILE '{0}'", FileUtils.ReplaceDirectorySeparator(segfunction.Value));
                    writer.WriteLine();
                }

                return writer.ToString();
            }
        }

        private static IDictionary<string, string> GetSegfunctions(WaqInitializationSettings initializationSettings)
        {
            Dictionary<string, string> origDict = initializationSettings
                                                  .ProcessCoefficients.OfType<FunctionFromHydroDynamics>()
                                                  .ToDictionary(f => f.Name, f => f.FilePath);
            initializationSettings.ProcessCoefficients.OfType<SegmentFileFunction>()
                                  .ForEach(pc => origDict.Add(pc.Name, pc.UrlPath));
            return origDict;
        }

        /// <summary>
        /// Creates the include file containing various numerical options for delwaq.
        /// </summary>
        public static string CreateNumericalOptionsInclude(WaqInitializationSettings set)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                // Write Delwaq flags (When on, should be written; Data value doesn't matter):
                if (set.Settings.ClosureErrorCorrection)
                {
                    writer.WriteLine(
                        "CONSTANTS 'CLOSE_ERR' DATA 1 ; If defined, allow delwaq to correct water volumes to keep concentrations continuous");
                }

                // Write numerical options:
                writer.WriteLine("CONSTANTS 'NOTHREADS' DATA {0} ; Number of threads used by delwaq",
                                 set.Settings.NrOfThreads);
                writer.WriteLine("CONSTANTS 'DRY_THRESH' DATA {0} ; Dry cell threshold",
                                 set.Settings.DryCellThreshold.ToString(CultureInfo.InvariantCulture));

                // Write numerical scheme related options:
                if (set.Settings.NumericalScheme.IsIterativeCalculationScheme())
                {
                    writer.WriteLine("CONSTANTS 'maxiter' DATA {0} ; Maximum number of iterations",
                                     set.Settings.IterationMaximum);
                    writer.WriteLine("CONSTANTS 'tolerance' DATA {0} ; Convergence tolerance",
                                     set.Settings.Tolerance.ToString(CultureInfo.InvariantCulture));
                    writer.WriteLine(
                        "CONSTANTS 'iteration report' DATA {0} ; Write iteration report (when 1) or not (when 0)",
                        set.Settings.WriteIterationReport ? "1" : "0");
                }

                return writer.ToString();
            }
        }

        #endregion Block 7

        #region Block 8

        /// <summary>
        /// Creates the include file contents for initial conditions (constant and spatial).
        /// </summary>
        public static string CreateInitialConditionsInclude(WaqInitializationSettings initializationSettings)
        {
            if (!HasInitialConditions(initializationSettings))
            {
                return "";
            }

            return CreateConstantInitialConditionsFileContents(initializationSettings) +
                   CreateSpatialInitialConditionsFileContents(initializationSettings);
        }

        private static string CreateConstantInitialConditionsFileContents(
            WaqInitializationSettings initializationSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("MASS/M2");

                IFunction[] constantInitialConditions =
                    initializationSettings.InitialConditions.Where(ic => ic.IsConst()).ToArray();
                if (constantInitialConditions.Length > 0)
                {
                    writer.WriteLine("INITIALS");
                    constantInitialConditions.ForEach(condition => writer.WriteLine("'{0}'", condition.Name));
                    writer.WriteLine("DEFAULTS");
                    constantInitialConditions.ForEach(condition =>
                                                          writer.WriteLine(
                                                              WaterQualityFunctionFactory
                                                                  .GetDefaultValue(condition)
                                                                  .ToString(CultureInfo.InvariantCulture)));
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Determines whether there are initial conditions available or not.
        /// </summary>
        private static bool HasInitialConditions(WaqInitializationSettings initializationSettings)
        {
            return initializationSettings.InitialConditions != null &&
                   initializationSettings.InitialConditions.Count > 0;
        }

        /// <summary>
        /// Creates the initial conditions file contents with spatial components.
        /// </summary>
        private static string CreateSpatialInitialConditionsFileContents(
            WaqInitializationSettings initializationSettings)
        {
            return CreateSpatialIncludeContents(initializationSettings.InitialConditions, "INITIALS",
                                                initializationSettings.NumberOfLayers);
        }

        #endregion Block 8

        #region Block 9

        /// <summary>
        /// Creates the list of his variables to include in the output parameters.
        /// </summary>
        public static string CreateHisVarInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            List<WaterQualityOutputParameter> hisOutputParameters =
                substanceProcessLibrary.OutputParameters.Where(op => op.ShowInHis).ToList();
            return WriteOutputIncludeParameters(hisOutputParameters, true);
        }

        /// <summary>
        /// Creates the list of map variables to include in the output parameters.
        /// </summary>
        public static string CreateMapVarInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            List<WaterQualityOutputParameter> mapOutputParameters =
                substanceProcessLibrary.OutputParameters.Where(op => op.ShowInMap).ToList();
            return WriteOutputIncludeParameters(mapOutputParameters, false);
        }

        /// <summary>
        /// Write an output parameter include.
        /// Starts with a 2, because this are additional parameters to the default parameter.
        /// The second nummer is the number of items listed.
        /// Then a list of parameters is defined between quotes. If <paramref name="addParameterType"/> is true, a second column
        /// is written with 'volume'.
        /// </summary>
        /// <param name="outputParameters"> The output parameters. </param>
        /// <param name="addParameterType"> If the parameter type should be included as a second column. 'volume' or ' ' </param>
        private static string WriteOutputIncludeParameters(ICollection<WaterQualityOutputParameter> outputParameters,
                                                           bool addParameterType)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("2 ; perform default output and extra parameters listed below");
                writer.WriteLine("{0} ; number of parameters listed", outputParameters.Count);

                foreach (WaterQualityOutputParameter parameter in outputParameters)
                {
                    if (addParameterType)
                    {
                        string parameterType = parameter.Name == "Volume" || parameter.Name == "Surf" ? " " : "volume";
                        writer.WriteLine(" '{0}' '{1}'", parameter.Name, parameterType);
                    }
                    else
                    {
                        writer.WriteLine(" '{0}'", parameter.Name);
                    }
                }

                return writer.ToString();
            }
        }

        #endregion Block 9
    }
}