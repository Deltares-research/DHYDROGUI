using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DelftTools.Utils.IO;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public class WaqFileBasedPreProcessor : IWaqPreProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqFileBasedPreProcessor));

        private static readonly IDictionary<string, string> OutputFiles = new Dictionary<string, string>
        {
            { "List file", "deltashell.lst" },
            { "Process file", "deltashell.lsp" }
        };

        private const string WorkFilesPrefix = "deltashell";
        private const string RestartString = "0\ndeltashell_res_in.map\n";

        // save the work directory, because you cannot know it anymore in Cleanup phase.
        private string workDirectory;

        public bool InitializeWaq(WaqInitializationSettings initSettings, Action<string, string> addTextDocumentAction)
        {
            CheckInput(initSettings);

            var includeDirectory = Path.Combine(initSettings.Settings.WorkDirectory, "includes_deltashell");

            if (!Directory.Exists(includeDirectory))
            {
                Directory.CreateDirectory(includeDirectory);
            }

            WriteIncludeFilesAndBinaryFiles(initSettings, includeDirectory);

            // save the work directory for later use in Cleanup()
            workDirectory = initSettings.Settings.WorkDirectory;

            File.WriteAllText(Path.Combine(workDirectory, WorkFilesPrefix + ".inp"), GetInputFileContents(initSettings));

            Directory.SetCurrentDirectory(workDirectory);

            // create an output directory if it was not specified
            // don't check empty string, because it could be set intentionally to the same folder as the work directory
            if (initSettings.Settings.OutputDirectory == null)
            {
                initSettings.Settings.OutputDirectory = Path.Combine(initSettings.Settings.WorkDirectory, "output");
            }

            // if the string is empty, it is the same as the working directory
            if (initSettings.Settings.OutputDirectory != string.Empty)
            {
                // create the directory
                FileUtils.CreateDirectoryIfNotExists(initSettings.Settings.OutputDirectory);
            }

            foreach (var outputFile in OutputFiles)
            {
                FileUtils.DeleteIfExists(Path.Combine(initSettings.Settings.OutputDirectory, outputFile.Value));
            }

            var parameters = String.Format("{0}.inp {1} \"{2}\" -eco \"{3}\"", WorkFilesPrefix,
                (initSettings.Settings.ProcessesActive ? "-p" : "-np"),
                initSettings.SubstanceProcessLibrary.ProcessDefinitionFilesPath,
                Path.Combine(DelwaqFileStructureHelper.GetDelwaqDataDefaultFolderPath(), "bloom.spe"));

            // additional output directory
            if (!string.IsNullOrEmpty(initSettings.Settings.OutputDirectory))
            {
                parameters += string.Format(" -output \"{0}\"", initSettings.Settings.OutputDirectory);
            }

            var startTime = DateTime.Now;
            Log.Info("Started delwaq1.exe.");
            var processSuccessful = WaterQualityUtils.RunProcess(DelwaqFileStructureHelper.GetDelwaq1ExePath(), parameters, workDirectory, false);
            Log.InfoFormat("Done running delwaq1.exe. (Took {0})", DateTime.Now - startTime);

            // Read the output files
            foreach (var outputFile in OutputFiles)
            {
                addTextDocumentAction(outputFile.Key, Path.Combine(initSettings.Settings.OutputDirectory, outputFile.Value));
            }

            return processSuccessful;
        }

        public void Dispose()
        {
            if (workDirectory == null)
                return;

            // delete all deltashell-*.wrk and delwaq.rtn, deltashell-initials.map
            string[] workFiles = Directory.GetFileSystemEntries(workDirectory, WorkFilesPrefix + "-*.wrk");

            foreach (string workFile in workFiles)
            {
                FileUtils.DeleteIfExists(workFile);
            }

            FileUtils.DeleteIfExists(Path.Combine(workDirectory, "delwaq.rtn"));
            FileUtils.DeleteIfExists(Path.Combine(workDirectory, "deltashell-initials.map"));
        }

        public void WriteIncludeFilesAndBinaryFiles(WaqInitializationSettings initSettings, string includeDirectory)
        {
            if (!Directory.Exists(includeDirectory))
            {
                Directory.CreateDirectory(includeDirectory);
            }

            var filesDictionary = new Dictionary<string, Func<IncludeFileFactory, WaqInitializationSettings, string>>
                                      {
                                          {"B1_t0.inc", (f, set) => f.CreateT0Include(set.ReferenceTime)},
                                          {"B1_sublist.inc", (f, set) => f.CreateSubstanceListInclude(set.SubstanceProcessLibrary)},

                                          {"B2_numsettings.inc", (f, set) => f.CreateNumSettingsInclude(set.Settings)},
                                          {"B2_simtimers.inc", (f, set) => f.CreateSimTimersInclude(set)},
                                          {"B2_outlocs.inc", (f, set) => f.CreateOutputLocationsInclude(set.OutputLocations)},
                                          {"B2_outputtimers.inc", (f, set) => f.CreateOutputTimersInclude(set.Settings)},
                                          
                                          {"B3_nrofseg.inc", (f, set) => f.CreateNumberOfSegmentsInclude(set.SegmentsPerLayer, set.NumberOfLayers)},
                                          {"B3_attributes.inc", (f, set) => f.CreateAttributesFileInclude(set.AttributesFile)},
                                          {"B3_volumes.inc", (f, set) => f.CreateVolumesFileInclude(set.VolumesFile)},

                                          {"B4_nrofexch.inc", (f, set) => f.CreateNumberOfExchangesInclude(set.HorizontalExchanges, set.VerticalExchanges)},
                                          {"B4_pointers.inc", (f, set) => f.CreatePointersFileInclude(set.PointersFile)},
                                          {"B4_cdispersion.inc", (f, set) => f.CreateConstantDispersionInclude(set.VerticalDispersion, set.Dispersion.First())},
                                          {"B4_area.inc", (f, set) => f.CreateAreasFileInclude(set.AreasFile)},
                                          {"B4_flows.inc", (f, set) => f.CreateFlowsFileInclude(set.FlowsFile)},
                                          {"B4_length.inc", (f, set) => f.CreateLengthsFileInclude(set.LengthsFile)},
                                          
                                          {"B5_boundlist.inc", (f, set) => f.CreateBoundaryListInclude(set.BoundaryNodeIds, set.NumberOfLayers)},
                                          {"B5_boundaliases.inc", (f, set) => f.CreateBoundaryAliasesInclude(set.BoundaryAliases)},
                                          {"B5_bounddata.inc", (f, set) => f.CreateBoundaryDataInclude(set.BoundaryDataManager, set.Settings.WorkDirectory)},
                                          
                                          {"B6_loads.inc", (f, set) => f.CreateDryWasteLoadInclude(set.LoadAndIds)},
                                          {"B6_loads_aliases.inc", (f, set) => f.CreateDryWasteLoadAliasesInclude(set.LoadsAliases)},
                                          {"B6_loads_data.inc", (f, set) => f.CreateDryWasteLoadDataInclude(set.LoadsDataManager, set.Settings.WorkDirectory)},
                                          
                                          {"B7_processes.inc", (f, set) => f.CreateProcessesInclude(set.SubstanceProcessLibrary)},
                                          {"B7_constants.inc", (f, set) => f.CreateConstantsInclude(set.ProcessCoefficients)},
                                          {"B7_functions.inc", (f, set) => f.CreateFunctionsInclude(set.ProcessCoefficients)},
                                          {"B7_parameters.inc", (f, set) => f.CreateParametersInclude(set)},
                                          {"B7_dispersion.inc", (f, set) => f.CreateSpatialDispersionInclude(set.Dispersion.First(), set.NumberOfLayers)},
                                          {"B7_vdiffusion.inc", (f,set) => f.CreateVerticalDiffusionInclude(set.VerticalDiffusionFile, set.UseAdditionalVerticalDiffusion)},
                                          {"B7_segfunctions.inc", (f, set) => f.CreateSegfunctionsInclude(set)},
                                          {"B7_numerical_options.inc", (f,set) => f.CreateNumericalOptionsInclude(set)},
                                          
                                          {"B8_initials.inc", (f, set) => initSettings.UseRestart ? RestartString :  f.CreateInitialConditionsInclude(set)},
                                          
                                          {"B9_Mapvar.inc", (f, set) => f.CreateMapVarInclude(set.SubstanceProcessLibrary)},
                                          {"B9_Hisvar.inc", (f, set) => f.CreateHisVarInclude(set.SubstanceProcessLibrary)}
                                      };

            var factory = new IncludeFileFactory();

            foreach (var fileKvp in filesDictionary)
            {
                File.WriteAllText(Path.Combine(includeDirectory, fileKvp.Key), fileKvp.Value(factory, initSettings));
            }

            CopyDataTableUserfors(includeDirectory, initSettings.BoundaryDataManager);
            CopyDataTableUserfors(includeDirectory, initSettings.LoadsDataManager);
        }

        /// <summary>
        /// Gets the relative path from the .inc file where the usefor-files for a given
        /// <see cref="DataTableManager"/> are being stored.
        /// </summary>
        public static string GetDataTableUseforsRelativeFolderPath(DataTableManager manager)
        {
            return Path.Combine("includes_deltashell", Path.GetFileName(manager.FolderPath));
        }

        private static void CopyDataTableUserfors(string includeDirectory, DataTableManager dataTableManager)
        {
            if (dataTableManager.DataTables.Any(dt => dt.IsEnabled))
            {
                var targetDirectory = Path.Combine(includeDirectory, Path.GetFileName(dataTableManager.FolderPath));
                FileUtils.DeleteIfExists(targetDirectory);
                Directory.CreateDirectory(targetDirectory);

                foreach (var dataTable in dataTableManager.DataTables)
                {
                    if (dataTable.IsEnabled)
                    {
                        var destinationPath = Path.Combine(targetDirectory,
                            Path.GetFileName(dataTable.SubstanceUseforFile.Path));
                        FileUtils.DeleteIfExists(destinationPath);
                        dataTable.SubstanceUseforFile.CopyTo(destinationPath);
                    }
                }
            }
        }

        private void CheckInput(WaqInitializationSettings waqInitializationSettings)
        {
            if (waqInitializationSettings == null)
            {
                throw new NullReferenceException("Initialization settings may not be null");
            }

            if (waqInitializationSettings.InputFile == null || string.IsNullOrEmpty(waqInitializationSettings.InputFile.Content))
            {
                throw new NullReferenceException("Input file may not be null");
            }

            if (string.IsNullOrEmpty(waqInitializationSettings.Settings.WorkDirectory))
            {
                throw new NullReferenceException("Work directory must be set");
            }
        }

        private string GetInputFileContents(WaqInitializationSettings initSettings)
        {
            return initSettings.InputFile.Content;
        }
    }
}