using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public class DHydroConfigXmlExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DHydroConfigXmlExporter));

        public IDictionary<IDimrModel, int> CoreCountDictionary { get; set; }

        public string ExportFilePath { get; set; }

        public string Name
        {
            get
            {
                return "DIMR configuration";
            }
        }

        public string Category
        {
            get
            {
                return "DIMR";
            }
        }

        public string Description
        {
            get
            {
                return string.Empty;
            }
        }

        public string FileFilter
        {
            get
            {
                return "xml files|*.xml";
            }
        }

        public Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public bool Export(object item, string path)
        {
            ICompositeActivity workflow = GetWorkflow(item);

            string exportPath = ExportFilePath ?? path;
            if (exportPath == null)
            {
                Log.ErrorFormat("Invalid export file path");
                return false;
            }

            string exportDirectory = Path.GetDirectoryName(Path.GetFullPath(exportPath));
            if (exportDirectory == null)
            {
                Log.ErrorFormat("Invalid export directory");
                return false;
            }

            var errorLog = new StringBuilder();
            XDocument configDocument;
            try
            {
                List<IDimrModel> dimrModels = GetDimrModelsFromItem(item).ToList();
                string validationReportMessages = ValidateDimrModels(dimrModels);

                if (!string.IsNullOrEmpty(validationReportMessages))
                {
                    throw new InvalidOperationException(Name +
                                                        " model validation failed; please review the validation report of the submodels.\n\r" +
                                                        validationReportMessages);
                }

                foreach (IDimrModel dimrModel in dimrModels)
                {
                    string exportSubDirectory = Path.Combine(exportDirectory, dimrModel.DirectoryName);
                    FileUtils.CreateDirectoryIfNotExists(exportSubDirectory);

                    var dimrModelExporter = (IFileExporter) Activator.CreateInstance(dimrModel.ExporterType);
                    if (!dimrModelExporter.Export(dimrModel, dimrModel.GetExporterPath(exportSubDirectory)))
                    {
                        string formattedError = $"Export failed for model {dimrModel.Name}{Environment.NewLine}";
                        errorLog.Append(formattedError);
                        Log.ErrorFormat(formattedError);
                    }
                }

                var writer = new DHydroConfigWriter {CoreCountDictionary = new Dictionary<IDimrModel, int>()};
                configDocument = writer.CreateConfigDocument(workflow);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Export failed: " + e.Message);
                errorLog.Append($"Export failed: {e.Message}{Environment.NewLine}");
                return false;
            }
            finally
            {
                UnInitialize();
                if (!string.IsNullOrEmpty(errorLog.ToString()))
                {
                    string errorFile = exportPath.Replace(".xml", ".err");
                    File.WriteAllText(errorFile, errorLog.ToString());
                    Log.InfoFormat("Export error log written: {0} ", errorFile);
                }
            }

            configDocument.Save(exportPath);
            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(HydroModel);
            yield return typeof(IDimrModel);
        }

        public bool CanExportFor(object item)
        {
            var dHydroActivity = item as IDimrModel;
            return dHydroActivity?.IsMasterTimeStep ?? GetDimrModelsFromItem(item).Any();
        }

        private void UnInitialize()
        {
            CoreCountDictionary = null;
            ExportFilePath = null;
        }

        private static string ValidateDimrModels(List<IDimrModel> dimrModels)
        {
            var validationReportMessages = new StringBuilder();

            foreach (IDimrModel dimrModel in dimrModels)
            {
                ValidationReport validationReport = dimrModel.Validate();
                if (validationReport != null && validationReport.Severity() == ValidationSeverity.Error)
                {
                    string errorMessage = string.Format("Validation errors: {0}",
                                                        string.Join("\n", validationReport.GetAllIssuesRecursive()
                                                                                          .Where(i => i.Severity == ValidationSeverity.Error)
                                                                                          .Select(i => string.Format("\t{0}: {1}", i.Subject, i.Message)).ToArray()));
                    validationReportMessages.Append(errorMessage + Environment.NewLine);
                }
            }

            return validationReportMessages.ToString();
        }

        private ICompositeActivity GetWorkflow(object item)
        {
            ICompositeActivity workflow = null;
            var hydroModel = item as HydroModel;
            if (hydroModel != null)
            {
                return hydroModel.CurrentWorkflow;
            }

            var activity = item as IActivity;
            if (activity != null && item is IDimrModel)
            {
                workflow = new SequentialActivity();
                workflow.Activities.Add(activity);
            }

            if (workflow == null)
            {
                Log.ErrorFormat("Could not create valid DIMR workflow for object of type {0}", item.GetType());
            }

            return workflow;
        }

        private static IEnumerable<IDimrModel> GetDimrModelsFromItem(object item)
        {
            var hydroModel = item as HydroModel;
            if (hydroModel != null)
            {
                if (hydroModel.CurrentWorkflow == null)
                {
                    return Enumerable.Empty<IDimrModel>();
                }

                return hydroModel.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                 .Plus(hydroModel.CurrentWorkflow as IDimrModel)
                                 .Where(dm => dm != null)
                                 .ToList();
            }

            var dimrModel = item as IDimrModel;
            return dimrModel != null ? Enumerable.Repeat(dimrModel, 1) : Enumerable.Empty<IDimrModel>();
        }
    }
}