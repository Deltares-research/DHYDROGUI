using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Utils.IO;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelExporter: IFileExporter
    {
        private static ILog Log = LogManager.GetLogger(typeof (RealTimeControlModelExporter));

        private const string SettingsString = "{\r\n\r\n\t\"xmlDir\": \".\",\r\n\t\"schemaDir\": \".\"\r\n\r\n}";
        
        public string Name
        {
            get { return "RTC-Tools xml files"; }
        }

        public string Directory { private get; set; }

        public bool Export(object item, string path)
        {
            var realTimeControlModel = item as RealTimeControlModel;
            if (realTimeControlModel != null)
            {
                realTimeControlModel.RefreshInitialState();

                var directory = Directory ?? path;
                try
                {
                    RealTimeControlXmlWriter.CopyXsds(directory);
                    realTimeControlModel.SetTimeLagHydraulicRulesToTimeSteps(
                        realTimeControlModel.ControlGroups, realTimeControlModel.TimeStep);

                    WriteEngineXmlFiles(realTimeControlModel, directory);
                    if (realTimeControlModel.UseRestart)
                    {
                        ZipFileUtils.Extract(realTimeControlModel.RestartInput.Path, directory);
                    }
                    else
                    {
                        RealTimeControlXmlWriter.GetStateVectorXml(directory, realTimeControlModel.ControlGroups)
                            .Save(Path.Combine(path, RealTimeControlXMLFiles.XmlImportState));
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e.Message); // skip model validation exceptions
                }
                var settingPath = Path.Combine(directory, "settings.json");
                File.WriteAllText(settingPath,SettingsString);
                return true;
            }
            return false;
        }

        public string Category
        {
            get { return "Xml files"; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (RealTimeControlModel);
        }

        public string FileFilter
        {
            get { return "xml files|*.xml"; }
        }

        public Bitmap Icon { get { return Properties.Resources.brick_add; } }
        public bool CanExportFor(object item)
        {
            return true;
        }

        public virtual void WriteEngineXmlFiles(RealTimeControlModel model, string path)
        {
            // write xml with reference to xsd
            var xsdPath = RealTimeControlModelHelper.XsdPath;
            RealTimeControlXmlWriter.GetRuntimeXml(xsdPath, model, model.LimitMemory, model.LogLevel)
                .Save(path + RealTimeControlXMLFiles.XmlRuntime);
            RealTimeControlXmlWriter.GetToolsConfigXml(xsdPath, model.ControlGroups, model.WriteRestart || model.UseRestart)
                .Save(path + RealTimeControlXMLFiles.XmlTools);

            if (model.UseRestart)
            {
                // export target directory may be another than the model working directory, so store the latter
                string modelWorkingDirectory;
                try
                {
                    modelWorkingDirectory = model.ModelStateHandler.ModelWorkingDirectory;
                }
                catch
                {
                    // model working directory was not set (yet)
                    modelWorkingDirectory = null;
                }
                model.ModelStateHandler.ModelWorkingDirectory = path;
                model.ModelStateHandler.FeedStateToModel(model.ModelStateHandler.CreateStateFromFile(Name, model.RestartInput.Path));

                if (modelWorkingDirectory != null)
                {
                    // there was a model working directory set, restore it
                    model.ModelStateHandler.ModelWorkingDirectory = modelWorkingDirectory;
                }
            }
            else
            {
                RealTimeControlXmlWriter.GetStateVectorXml(xsdPath, model.ControlGroups).Save(Path.Combine(path, RealTimeControlXMLFiles.XmlImportState));
            }
            var timeSeriesDoc = RealTimeControlXmlWriter.GetTimeSeriesXml(xsdPath, model, model.ControlGroups);
            if (timeSeriesDoc != null)
            {
                timeSeriesDoc.Save(path + RealTimeControlXMLFiles.XmlTimeSeries);
            }

            RealTimeControlXmlWriter.GetDataConfigXml(xsdPath, model, model.ControlGroups, timeSeriesDoc == null ? null : "timeseries_import.xml")
                .Save(path + RealTimeControlXMLFiles.XmlData);
        }

    }
}
