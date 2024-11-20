using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public sealed class WaqFileBasedPreProcessor : IWaqPreProcessor
    {
        private const string RestartString = "0\n" + FileConstants.RestartInFileName + "\n";

        // save the work directory, because you cannot know it anymore in Cleanup phase.
        private string outputWorkDirectory;

        /// <summary>
        /// Writes the include files and binary files.
        /// </summary>
        /// <param name="initSettings">The waq initialization settings.</param>
        /// <param name="includeDirectory">The directory where the include files are written to.</param>
        public void WriteIncludeFilesAndBinaryFiles(WaqInitializationSettings initSettings, string includeDirectory)
        {
            if (!Directory.Exists(includeDirectory))
            {
                Directory.CreateDirectory(includeDirectory);
            }

            string volumesFile = FileUtils.ReplaceDirectorySeparator(initSettings.VolumesFile);
            string verticalDiffusionFile = FileUtils.ReplaceDirectorySeparator(initSettings.VerticalDiffusionFile);
            string attributesFile = FileUtils.ReplaceDirectorySeparator(initSettings.AttributesFile);
            string pointersFile = FileUtils.ReplaceDirectorySeparator(initSettings.PointersFile);
            string areasFile = FileUtils.ReplaceDirectorySeparator(initSettings.AreasFile);
            string flowsFile = FileUtils.ReplaceDirectorySeparator(initSettings.FlowsFile);
            string lengthsFile = FileUtils.ReplaceDirectorySeparator(initSettings.LengthsFile);
            string gridFile = FileUtils.ReplaceDirectorySeparator(initSettings.GridFile);

            var filesDictionary = new Dictionary<string, Func<WaqInitializationSettings, string>>
            {
                {"B1_t0.inc", set => IncludeFileFactory.CreateT0Include(set.ReferenceTime)},
                {"B1_sublist.inc", set => IncludeFileFactory.CreateSubstanceListInclude(set.SubstanceProcessLibrary)},
                {"B2_numsettings.inc", set => IncludeFileFactory.CreateNumSettingsInclude(set.Settings)},
                {"B2_simtimers.inc", set => IncludeFileFactory.CreateSimTimersInclude(set)},
                {"B2_outlocs.inc", set => IncludeFileFactory.CreateOutputLocationsInclude(set.OutputLocations)},
                {"B2_outputtimers.inc", set => IncludeFileFactory.CreateOutputTimersInclude(set.Settings)},
                {"B3_ugrid.inc", set => IncludeFileFactory.CreateGridFileInclude(gridFile)},
                {
                    "B3_nrofseg.inc", set => IncludeFileFactory.CreateNumberOfSegmentsInclude(
                        set.SegmentsPerLayer, set.NumberOfLayers)
                },
                {"B3_attributes.inc", set => IncludeFileFactory.CreateAttributesFileInclude(attributesFile)},
                {"B3_volumes.inc", set => IncludeFileFactory.CreateVolumesFileInclude(volumesFile)},
                {
                    "B4_nrofexch.inc", set => IncludeFileFactory.CreateNumberOfExchangesInclude(
                        set.HorizontalExchanges, set.VerticalExchanges)
                },
                {"B4_pointers.inc", set => IncludeFileFactory.CreatePointersFileInclude(pointersFile)},
                {
                    "B4_cdispersion.inc", set => IncludeFileFactory.CreateConstantDispersionInclude(
                        set.VerticalDispersion, set.Dispersion.First())
                },
                {"B4_area.inc", set => IncludeFileFactory.CreateAreasFileInclude(areasFile)},
                {"B4_flows.inc", set => IncludeFileFactory.CreateFlowsFileInclude(flowsFile)},
                {"B4_length.inc", set => IncludeFileFactory.CreateLengthsFileInclude(lengthsFile)},
                {"B5_boundlist.inc", set => IncludeFileFactory.CreateBoundaryListInclude(set.BoundaryNodeIds, set.NumberOfLayers)},
                {"B5_boundaliases.inc", set => IncludeFileFactory.CreateBoundaryAliasesInclude(set.BoundaryAliases)},
                {
                    "B5_bounddata.inc", set => IncludeFileFactory.CreateBoundaryDataInclude(
                        set.BoundaryDataManager, outputWorkDirectory)
                },
                {"B6_loads.inc", set => IncludeFileFactory.CreateDryWasteLoadInclude(set.LoadAndIds)},
                {"B6_loads_aliases.inc", set => IncludeFileFactory.CreateDryWasteLoadAliasesInclude(set.LoadsAliases)},
                {
                    "B6_loads_data.inc", set => IncludeFileFactory.CreateDryWasteLoadDataInclude(
                        set.LoadsDataManager, outputWorkDirectory)
                },
                {"B7_processes.inc", set => IncludeFileFactory.CreateProcessesInclude(set.SubstanceProcessLibrary)},
                {"B7_constants.inc", set => IncludeFileFactory.CreateConstantsInclude(set.ProcessCoefficients)},
                {"B7_functions.inc", set => IncludeFileFactory.CreateFunctionsInclude(set.ProcessCoefficients)},
                {"B7_parameters.inc", set => IncludeFileFactory.CreateParametersInclude(set)},
                {
                    "B7_dispersion.inc", set => IncludeFileFactory.CreateSpatialDispersionInclude(
                        set.Dispersion.First(), set.NumberOfLayers)
                },
                {
                    "B7_vdiffusion.inc", set => IncludeFileFactory.CreateVerticalDiffusionInclude(
                        verticalDiffusionFile, set.UseAdditionalVerticalDiffusion)
                },
                {"B7_segfunctions.inc", set => IncludeFileFactory.CreateSegfunctionsInclude(set)},
                {"B7_numerical_options.inc", set => IncludeFileFactory.CreateNumericalOptionsInclude(set)},
                {
                    "B8_initials.inc", set => initSettings.UseRestart
                                                  ? RestartString
                                                  : IncludeFileFactory.CreateInitialConditionsInclude(set)
                },
                {"B9_Mapvar.inc", set => IncludeFileFactory.CreateMapVarInclude(set.SubstanceProcessLibrary)},
                {"B9_Hisvar.inc", set => IncludeFileFactory.CreateHisVarInclude(set.SubstanceProcessLibrary)}
            };

            foreach (KeyValuePair<string, Func<WaqInitializationSettings, string>> fileKvp in filesDictionary)
            {
                File.WriteAllText(Path.Combine(includeDirectory, fileKvp.Key), fileKvp.Value(initSettings));
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
            return Path.Combine(FileConstants.IncludesDirectoryName, Path.GetFileName(manager.FolderPath));
        }

        public void InitializeWaq(WaqInitializationSettings initSettings)
        {
            CheckInput(initSettings);

            outputWorkDirectory = Path.Combine(initSettings.Settings.WorkDirectory, FileConstants.OutputDirectoryName);
            initSettings.Settings.WorkingOutputDirectory = outputWorkDirectory;
            FileUtils.CreateDirectoryIfNotExists(outputWorkDirectory, true);

            string includeDirectory = Path.Combine(outputWorkDirectory, FileConstants.IncludesDirectoryName);

            if (!Directory.Exists(includeDirectory))
            {
                Directory.CreateDirectory(includeDirectory);
            }

            WaqInitializationDataVerifier.Verify(initSettings);

            WriteIncludeFilesAndBinaryFiles(initSettings, includeDirectory);

            File.WriteAllText(Path.Combine(outputWorkDirectory, FileConstants.InputFileName), GetInputFileContents(initSettings));
        }

        public void Dispose()
        {
            if (outputWorkDirectory == null)
            {
                return;
            }

            // delete all deltashell-*.wrk and delwaq.rtn, deltashell-initials.map
            string[] workFiles = Directory.GetFileSystemEntries(outputWorkDirectory, FileConstants.WorkFilesName + "-*.wrk");

            foreach (string workFile in workFiles)
            {
                FileUtils.DeleteIfExists(workFile);
            }

            FileUtils.DeleteIfExists(Path.Combine(outputWorkDirectory, "delwaq.rtn"));
            FileUtils.DeleteIfExists(Path.Combine(outputWorkDirectory, FileConstants.InitialConditionsFileName));
        }

        private static void CopyDataTableUserfors(string includeDirectory, DataTableManager dataTableManager)
        {
            if (dataTableManager.DataTables.Any(dt => dt.IsEnabled))
            {
                string targetDirectory = Path.Combine(includeDirectory, Path.GetFileName(dataTableManager.FolderPath));
                FileUtils.DeleteIfExists(targetDirectory);
                Directory.CreateDirectory(targetDirectory);

                foreach (DataTable dataTable in dataTableManager.DataTables)
                {
                    if (dataTable.IsEnabled)
                    {
                        string destinationPath = Path.Combine(targetDirectory,
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

            if (waqInitializationSettings.InputFile == null ||
                string.IsNullOrEmpty(waqInitializationSettings.InputFile.Content))
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