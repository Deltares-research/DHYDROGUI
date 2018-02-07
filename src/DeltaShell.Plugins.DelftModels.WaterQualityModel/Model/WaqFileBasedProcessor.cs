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
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public class WaqFileBasedProcessor : IWaqProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqFileBasedProcessor));
        private static readonly IDictionary<ADataItemMetaData, string> OutputFiles = new Dictionary<ADataItemMetaData, string>
        {
            { WaterQualityModel.BalanceOutputDataItemMetaData, "deltashell-bal.prn" },
            { WaterQualityModel.MonitoringFileDataItemMetaData, "deltashell.mon" }
        };

        private const int NoDataValue = -999;

        public void Initialize(WaqInitializationSettings initializationSettings)
        {
            var outputDirectory = initializationSettings.Settings.OutputDirectory;

            FileUtils.DeleteIfExists(Path.Combine(outputDirectory, "deltashell.map"));
            FileUtils.DeleteIfExists(Path.Combine(outputDirectory, "deltashell.his"));

            foreach (var outputFile in OutputFiles)
            {
                FileUtils.DeleteIfExists(Path.Combine(outputDirectory, outputFile.Value));
            }
        }

        public void Process(WaqInitializationSettings initializationSettings, Action<double> setProgress)
        {
            var errorMessages = "";
            var startTime = DateTime.Now;

            var optionalDuflowSwitch = !string.IsNullOrEmpty(initializationSettings.SubstanceProcessLibrary.ProcessDllFilePath)
                            ? "-openpb \"" + initializationSettings.SubstanceProcessLibrary.ProcessDllFilePath + "\""
                            : string.Empty;

            Log.Debug("Started delwaq2.exe.");
            WaterQualityUtils.RunProcess(DelwaqFileStructureHelper.GetDelwaq2ExePath(),
                string.Format("deltashell.inp " + optionalDuflowSwitch),
                initializationSettings.Settings.WorkDirectory, false, 3000,
                (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data) || setProgress == null) return;

                    var progress = ParseProgressText(e.Data);
                    if (progress != NoDataValue)
                    {
                        setProgress(progress);
                    }

                    if (!string.IsNullOrEmpty(errorMessages) || e.Data.TrimStart(' ').ToLower().StartsWith("error"))
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
        
        public void AddOutput(string workDirectory, IList<WaterQualityObservationVariableOutput> observationVariableOutputs, Action<ADataItemMetaData, string> addTextDocument, MonitoringOutputLevel monitoringOutputLevel)
        {
            if (workDirectory == null)
            {
                Log.Error("Could not add output because work directory is empty.");
                return;
            }

            Log.Debug("Started parsing deltashell.his file.");
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            WaqProcessorHelper.ParseHisFileData(Path.Combine(workDirectory, "deltashell.his"), observationVariableOutputs, monitoringOutputLevel);
            stopWatch.Stop();

            Log.DebugFormat("Done parsing deltashell.his file. (Took {0})", stopWatch.Elapsed);

            if (addTextDocument == null)
            {
                Log.ErrorFormat("Could not read output files : {0}", string.Join(", ", OutputFiles.Keys.Select(key => key.Name)));
                return;
            }

            // Read the output files
            foreach (var outputFile in OutputFiles)
            {
                addTextDocument(outputFile.Key, Path.Combine(workDirectory, outputFile.Value));
            }
        }
        
        private static double ParseProgressText(string progressString)
        {
            var regex = new Regex(string.Format("(?<percentage>{0})% Completed", RegularExpression.Float));

            var match = regex.Match(progressString);

            return RegularExpression.ParseDouble(match, "percentage", NoDataValue);
        }
    }
}