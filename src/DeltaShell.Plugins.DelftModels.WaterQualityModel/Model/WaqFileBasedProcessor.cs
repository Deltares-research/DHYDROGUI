using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.IO;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Initializes the file paths and prepares the directories before and after running the model.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterQualityModel.Model.IWaqProcessor" />
    public class WaqFileBasedProcessor : IWaqProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqFileBasedProcessor));

        private static readonly IDictionary<ADataItemMetaData, string> OutputFiles =
            new Dictionary<ADataItemMetaData, string>
            {
                {WaterQualityModel.BalanceOutputDataItemMetaData, "deltashell-bal.prn"},
                {WaterQualityModel.MonitoringFileDataItemMetaData, "deltashell.mon"}
            };

        private const int NoDataValue = -999;

        public bool TryToCancel { get; set; }

        /// <summary>
        /// Prepares the working directory before initializing.
        /// </summary>
        /// <param name="initializationSettings">Settings used for initialization.</param>
        public void Initialize(WaqInitializationSettings initializationSettings)
        {
            string outputDirectory = initializationSettings.Settings.OutputDirectory;

            FileUtils.DeleteIfExists(Path.Combine(outputDirectory, "deltashell.map"));
            FileUtils.DeleteIfExists(Path.Combine(outputDirectory, "deltashell.his"));

            foreach (KeyValuePair<ADataItemMetaData, string> outputFile in OutputFiles)
            {
                FileUtils.DeleteIfExists(Path.Combine(outputDirectory, outputFile.Value));
            }
        }

        /// <summary>
        /// Run waq calculation process.
        /// </summary>
        /// <param name="initializationSettings">Settings needed to make the run.</param>
        /// <param name="setProgress">Method to set the progress.</param>
        public void Process(WaqInitializationSettings initializationSettings, Action<double> setProgress)
        {
            var errorMessages = "";
            DateTime startTime = DateTime.Now;

            string optionalDuflowSwitch =
                !string.IsNullOrEmpty(initializationSettings.SubstanceProcessLibrary.ProcessDllFilePath)
                    ? "-openpb \"" + initializationSettings.SubstanceProcessLibrary.ProcessDllFilePath + "\""
                    : string.Empty;

            Log.Debug("Started delwaq2.exe.");
            WaterQualityUtils.RunProcess(DelwaqFileStructureHelper.GetDelwaq2ExePath(),
                                         string.Format("deltashell.inp " + optionalDuflowSwitch),
                                         initializationSettings.Settings.WorkDirectory, () => TryToCancel, false, 3000,
                                         (s, e) =>
                                         {
                                             if (string.IsNullOrEmpty(e.Data) || setProgress == null)
                                             {
                                                 return;
                                             }

                                             double progress = ParseProgressText(e.Data);
                                             if (progress != NoDataValue)
                                             {
                                                 setProgress(progress);
                                             }

                                             if (!string.IsNullOrEmpty(errorMessages) ||
                                                 e.Data.TrimStart(' ').ToLower().StartsWith("error"))
                                             {
                                                 errorMessages += e.Data + Environment.NewLine;
                                             }
                                         });
            Log.DebugFormat("Done running delwaq2.exe. (Took {0})", DateTime.Now - startTime);

            if (!string.IsNullOrEmpty(errorMessages))
            {
                Log.Error(errorMessages);
            }
        }

        /// <summary>
        /// Adds the output generated in <see cref="Process" /> function to the output of the waterQualityModel.
        /// </summary>
        /// <param name="outputDirectory">The directory in which the output is generated.</param>
        /// <param name="observationVariableOutputs">The observation variable outputs of the model. </param>
        /// <param name="addTextDocument">Action to add the output text document to the model. </param>
        /// <param name="monitoringOutputLevel">The monitoring output level.</param>
        public void AddOutput(string outputDirectory,
                              IList<WaterQualityObservationVariableOutput> observationVariableOutputs,
                              Action<ADataItemMetaData, string> addTextDocument,
                              MonitoringOutputLevel monitoringOutputLevel)
        {
            if (outputDirectory == null)
            {
                Log.Error(Resources.WaqFileBasedProcessor_AddOutput_work_directory_is_empty);
                return;
            }

            string hisFilePath = GetExistingHistoryFilePath(outputDirectory);
            if (!string.IsNullOrEmpty(hisFilePath))
            {
                Log.Debug("Started parsing history file.");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                WaqHistoryFileParser.Parse(hisFilePath, observationVariableOutputs, monitoringOutputLevel);

                stopWatch.Stop();

                Log.DebugFormat("Done parsing history file. (Took {0})", stopWatch.Elapsed);
            }

            if (addTextDocument == null)
            {
                Log.ErrorFormat(Resources.WaqFileBasedProcessor_AddOutput_Could_not_read_output_files,
                                string.Join(", ", OutputFiles.Keys.Select(key => key.Name)));
                return;
            }

            // Read the output files
            foreach (KeyValuePair<ADataItemMetaData, string> outputFile in OutputFiles)
            {
                addTextDocument(outputFile.Key, Path.Combine(outputDirectory, outputFile.Value));
            }
        }

        private static string GetExistingHistoryFilePath(string workDirectory)
        {
            if (File.Exists(Path.Combine(workDirectory, "deltashell_his.nc")))
            {
                return Path.Combine(workDirectory, "deltashell_his.nc");
            }

            if (File.Exists(Path.Combine(workDirectory, "deltashell.his")))
            {
                return Path.Combine(workDirectory, "deltashell.his");
            }

            return null;
        }

        private static double ParseProgressText(string progressString)
        {
            var regex = new Regex(string.Format("(?<percentage>{0})% Completed", RegularExpression.Float));

            Match match = regex.Match(progressString);

            return RegularExpression.ParseDouble(match, "percentage", NoDataValue);
        }
    }
}