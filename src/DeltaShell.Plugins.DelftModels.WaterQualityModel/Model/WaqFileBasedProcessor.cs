using System;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Process helper class for running D-Water Quality models.
    /// </summary>
    public class WaqFileBasedProcessor : IWaqProcessor
    {
        private const int NoDataValue = -999;
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqFileBasedProcessor));

        public bool TryToCancel { get; set; }

        /// <summary>
        /// Run waq calculation process.
        /// </summary>
        /// <param name="initializationSettings">Settings needed to make the run.</param>
        /// <param name="setProgress">Method to set the progress.</param>
        public void Process(WaqInitializationSettings initializationSettings, Action<double> setProgress)
        {
            var errorMessages = "";
            DateTime startTime = DateTime.Now;

            string optionalCustomProcessDllSwitch =
                !string.IsNullOrEmpty(initializationSettings.SubstanceProcessLibrary.ProcessDllFilePath)
                    ? "-openpb \"" + initializationSettings.SubstanceProcessLibrary.ProcessDllFilePath + "\""
                    : string.Empty;

            Log.Debug("Started delwaq2.exe.");
            WaterQualityUtils.RunProcess(WaterQualityApiDataSet.DelWaq2ExePath,
                                         string.Format(FileConstants.InputFileName + " " + optionalCustomProcessDllSwitch),
                                         initializationSettings.Settings.WorkingOutputDirectory, () => TryToCancel, false, 3000,
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

        private static double ParseProgressText(string progressString)
        {
            var regex = new Regex(string.Format("(?<percentage>{0})% Completed", RegularExpression.Float));

            Match match = regex.Match(progressString);

            return RegularExpression.ParseDouble(match, "percentage", NoDataValue);
        }
    }
}