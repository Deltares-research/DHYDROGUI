using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
    public class DHydroConfigXmlExporter: IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DHydroConfigXmlExporter));

        public IDictionary<IDimrModel, int> CoreCountDictionary { get; set; }

        public string Name { get { return "DIMR configuration"; } }

        public string ExportFilePath { get; set; }
        

        private void UnInitialize()
        {
            CoreCountDictionary = null;
            ExportFilePath = null;
        }

        public bool Export(object item, string path)
        {
            var workflow = GetWorkflow(item);

            var exportPath = ExportFilePath ?? path;
            if (exportPath == null)
            {
                Log.ErrorFormat("Invalid export file path");
                return false;
            }

            var exportDirectory = Path.GetDirectoryName(Path.GetFullPath(exportPath));
            if (exportDirectory == null)
            {
                Log.ErrorFormat("Invalid export directory");
                return false;
            }
            string errorLog = string.Empty;
            XDocument configDocument; 
            try
            {
                var dimrModels = GetDimrModelsFromItem(item).ToList();
                var validationReportMessages = ValidateDimrModels(dimrModels);

                if (!string.IsNullOrEmpty(validationReportMessages))
                {
                    throw new InvalidOperationException(Name +
                                                    " model validation failed; please review the validation report of the submodels.\n\r" +
                                                    validationReportMessages);
                }

                foreach (var dimrModel in dimrModels)
                {
                    var exportSubDirectory = Path.Combine(exportDirectory, dimrModel.DirectoryName);
                    FileUtils.CreateDirectoryIfNotExists(exportSubDirectory);

                    var dimrModelExporter = (IFileExporter)Activator.CreateInstance(dimrModel.ExporterType);
                    if (!dimrModelExporter.Export(dimrModel, dimrModel.GetExporterPath(exportSubDirectory)))
                    {
                        var formattedError = string.Format("Export failed for model {0}{1}", dimrModel.Name, Environment.NewLine);
                        errorLog += formattedError;
                        Log.ErrorFormat(formattedError);
                    }
                }

                var writer = new DHydroConfigWriter {CoreCountDictionary = new Dictionary<IDimrModel, int>()};
                configDocument = writer.CreateConfigDocument(workflow);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Export failed: " + e.Message);
                errorLog += string.Format("Export failed: {0}{1}", e.Message, Environment.NewLine);
                return false;
            }
            finally
            {
                UnInitialize();
                if (!string.IsNullOrEmpty(errorLog))
                {
                    var errorFile = exportPath.Replace(".xml", ".err"); 
                    File.WriteAllText(errorFile, errorLog);
                    Log.InfoFormat("Export error log written: {0} ", errorFile);
                }
            }

            configDocument.Save(exportPath);
            return true;
        }

        private static string ValidateDimrModels(List<IDimrModel> dimrModels)
        {
            string validationReportMessages = string.Empty;

            foreach (var dimrModel in dimrModels)
            {
                if (dimrModel is Iterative1D2DCoupler) continue;
                var validationReport = dimrModel.Validate();
                if (validationReport != null && validationReport.Severity() == ValidationSeverity.Error)
                {
                    var errorMessage = string.Format("Validation errors: {0}",
                        string.Join("\n", validationReport.GetAllIssuesRecursive()
                            .Where(i => i.Severity == ValidationSeverity.Error)
                            .Select(i => string.Format("\t{0}: {1}", i.Subject, i.Message)).ToArray()));
                    validationReportMessages += errorMessage + Environment.NewLine;
                }
            }
            return validationReportMessages;
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

        public string Category
        {
            get { return "DIMR"; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (HydroModel);
            yield return typeof (IDimrModel);
        }

        public string FileFilter
        {
            get { return "xml files|*.xml"; }
        }

        public Bitmap Icon
        {
            get { return null; }
        }

        public bool CanExportFor(object item)
        {
            var dHydroActivity = item as IDimrModel;
            return dHydroActivity != null ? dHydroActivity.IsMasterTimeStep : GetDimrModelsFromItem(item).Any();
        }
        
        private static IActivity UnwrapActivity(IActivity activity)
        {
            var activityWrapper = activity as ActivityWrapper;
            return activityWrapper == null ? activity : activityWrapper.Activity;
        }

        private static IEnumerable<IDimrModel> GetDimrModelsFromItem(object item)
        {
            var hydroModel = item as HydroModel;
            if (hydroModel != null)
            {
                if (hydroModel.CurrentWorkflow == null)
                {
                    Log.ErrorFormat("Could not get valid DIMR items from object of type {0}", item.GetType());
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
