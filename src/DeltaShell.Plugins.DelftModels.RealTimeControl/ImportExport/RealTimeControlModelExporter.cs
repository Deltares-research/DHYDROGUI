using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelExporter : IFileExporter
    {
        private const string settingsString = "{\r\n\r\n\t\"xmlDir\": \".\",\r\n\t\"schemaDir\": \".\"\r\n\r\n}";

        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlModelExporter));

        public string Directory { private get; set; }

        public string Name => "RTC-Tools xml files";

        public string Category => "Xml files";

        public string Description => string.Empty;

        public string FileFilter => "xml files|*.xml";

        public Bitmap Icon => Resources.brick_add;

        public bool Export(object item, string path)
        {
            var realTimeControlModel = item as RealTimeControlModel;
            if (realTimeControlModel == null)
            {
                return false;
            }

            realTimeControlModel.RefreshInitialState();

            string directory = Directory ?? path;

            try
            {
                RealTimeControlXmlWriter.CopyXsds(directory);

                realTimeControlModel.SetTimeLagHydraulicRulesToTimeSteps(realTimeControlModel.ControlGroups,
                                                                         realTimeControlModel.TimeStep);

                WriteEngineXmlFiles(realTimeControlModel, directory);

                WriteRestartFiles(path, realTimeControlModel, directory);
            }
            catch (InvalidOperationException e) when (e.Message == Resources.RealTimeControlModelIntervalRule_Import_time_series_for_signals_are_not_existing_export_failed)
            {
                log.Error(e.Message);
            }
            catch (Exception e)

            {
                log.Warn(e.Message); // skip model validation exceptions
            }

            File.WriteAllText(Path.Combine(directory, "settings.json"), settingsString);

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield break;
        }

        public bool CanExportFor(object item)
        {
            return false;
        }

        private static void WriteEngineXmlFiles(RealTimeControlModel model, string path)
        {
            RealTimeControlXmlWriter
                .GetRuntimeConfigXml(path, model, model.LimitMemory, model.LogLevel)
                .Save(Path.Combine(path, RealTimeControlXMLFiles.XmlRuntime));

            RealTimeControlXmlWriter
                .GetToolsConfigXml(path, model.ControlGroups, model.WriteRestart || model.UseRestart)
                .Save(Path.Combine(path, RealTimeControlXMLFiles.XmlTools));

            XDocument timeSeriesDoc = RealTimeControlXmlWriter.GetTimeSeriesXml(path, model, model.ControlGroups);
            timeSeriesDoc?.Save(Path.Combine(path, RealTimeControlXMLFiles.XmlTimeSeries));

            string timeSeriesPathFileName = timeSeriesDoc == null ? null : RealTimeControlXMLFiles.XmlTimeSeries;
            RealTimeControlXmlWriter
                .GetDataConfigXml(path, model, model.ControlGroups, timeSeriesPathFileName)
                .Save(Path.Combine(path, RealTimeControlXMLFiles.XmlData));
        }

        private static void WriteRestartFiles(string path, RealTimeControlModel realTimeControlModel, string directory)
        {
            if (realTimeControlModel.UseRestart)
            {
                using(StreamWriter stream = File.CreateText(Path.Combine(directory, RealTimeControlXMLFiles.XmlImportState)))
                {
                    stream.Write(realTimeControlModel.RestartInput.Content);
                }
            }
            else
            {
                RealTimeControlXmlWriter.GetStateVectorXml(directory, realTimeControlModel.ControlGroups)
                                        .Save(Path.Combine(path, RealTimeControlXMLFiles.XmlImportState));
            }
        }
    }
}