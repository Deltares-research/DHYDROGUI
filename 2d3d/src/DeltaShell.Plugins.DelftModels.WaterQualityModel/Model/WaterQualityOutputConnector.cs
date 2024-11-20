using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Connects the output in the output folder to the Water Quality model
    /// </summary>
    public static class WaterQualityOutputConnector
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaterQualityOutputConnector));

        /// <summary>
        /// Connects the specified <paramref name="model"/> to the output.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <remarks>If <paramref name="model"/> or its OutputFolder is <c>null</c>, this method will return.</remarks>
        public static void Connect(WaterQualityModel model)
        {
            string outputDirectoryPath = model?.OutputFolder?.Path;
            if (!Directory.Exists(outputDirectoryPath))
            {
                return;
            }

            ConnectMapOutput(model);
            ConnectHistoryOutput(model);
            ConnectTextFiles(model);
        }

        /// <summary>
        /// Connects the output map files to the model.
        /// </summary>
        /// <remarks>
        /// When both the binary and NetCDF file exist in the output directory, then the method attempts to connect to the NetCDF
        /// file.
        /// NetCdf files with an unsupported convention will not be connected and the model remains unconnected.
        /// </remarks>
        private static void ConnectMapOutput(WaterQualityModel model)
        {
            if (model.OutputFolder.ContainsFile(FileConstants.NetCdfMapFileName, out string mapNetCdfFilePath))
            {
                if (!NetCdfFileConventionChecker.HasSupportedConvention(mapNetCdfFilePath))
                {
                    log.WarnFormat(Resources.WaterQualityModel_File_does_not_meet_supported_UGRID_1_0_or_newer_standard,
                                   Path.GetFileName(mapNetCdfFilePath));
                }
                else
                {
                    model.MapFileFunctionStore.Path = mapNetCdfFilePath;
                }
            }

            else if (model.OutputFolder.ContainsFile(FileConstants.BinaryMapFileName, out string mapFilePath))
            {
                model.MapFileFunctionStore.Path = mapFilePath;
            }
        }

        private static void ConnectHistoryOutput(WaterQualityModel model)
        {
            if (model.OutputFolder.ContainsFile(FileConstants.NetCdfHisFileName, out string hisNetCdfFilePath))
            {
                ConnectHistoryOutput(model, hisNetCdfFilePath);
            }

            else if (model.OutputFolder.ContainsFile(FileConstants.BinaryHisFileName, out string hisFilePath))
            {
                ConnectHistoryOutput(model, hisFilePath);
            }
        }

        private static void ConnectHistoryOutput(WaterQualityModel model, string hisFilePath)
        {
            log.Debug("Started parsing history file.");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            WaqHistoryFileParser.Parse(hisFilePath, model.ObservationVariableOutputs,
                                       model.ModelSettings.MonitoringOutputLevel);

            stopWatch.Stop();

            log.DebugFormat("Done parsing history file. (Took {0})", stopWatch.Elapsed);
        }

        private static void ConnectTextFiles(WaterQualityModel model)
        {
            IDictionary<ADataItemMetaData, string> outputTextFiles = new Dictionary<ADataItemMetaData, string>
            {
                {WaterQualityModel.BalanceOutputDataItemMetaData, FileConstants.BalanceOutputFileName},
                {WaterQualityModel.MonitoringFileDataItemMetaData, FileConstants.MonitoringFileName},
                {WaterQualityModel.ListFileDataItemMetaData, FileConstants.ListFileName},
                {WaterQualityModel.ProcessFileDataItemMetaData, FileConstants.ProcessFileName}
            };

            foreach (KeyValuePair<ADataItemMetaData, string> outputFile in outputTextFiles)
            {
                ADataItemMetaData metaData = outputFile.Key;
                string outputFileName = outputFile.Value;

                if (model.OutputFolder.ContainsFile(outputFileName, out string outputFilePath))
                {
                    model.AddTextDocument(metaData, outputFilePath);
                }
            }
        }
    }
}