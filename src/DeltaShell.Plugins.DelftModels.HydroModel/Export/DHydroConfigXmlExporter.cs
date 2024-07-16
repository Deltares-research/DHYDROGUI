using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Core.Services;
using DeltaShell.Dimr;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    /// <summary>
    /// Provides an exporter for DIMR configuration files (*.xml).
    /// </summary>
    public class DHydroConfigXmlExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DHydroConfigXmlExporter));

        private IFileExportService fileExportService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DHydroConfigXmlExporter"/> class.
        /// </summary>
        public DHydroConfigXmlExporter()
            : this(new FileExportService())
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DHydroConfigXmlExporter"/> class.
        /// </summary>
        /// <param name="fileExportService">Service for retrieving the registered DIMR model file exporters.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fileExportService"/> is <c>null</c>.</exception>
        public DHydroConfigXmlExporter(IFileExportService fileExportService)
        {
            Ensure.NotNull(fileExportService, nameof(fileExportService));
            
            this.fileExportService = fileExportService;
        }

        /// <summary>
        /// Gets or sets the service for retrieving the registered DIMR model file exporters.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public IFileExportService FileExportService
        {
            get => fileExportService;
            set
            {
                Ensure.NotNull(value, nameof(value));
                fileExportService = value;
            }
        }

        public IDictionary<IDimrModel, int> CoreCountDictionary { get; set; }

        /// <summary>
        /// The directory to which the DIMR model is exported.
        /// </summary>
        public string ExportDirectoryPath { get; set; }

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
            HydroModelFileContext fileContext = (item as HydroModel)?.FileContext;

            string exportPath = GetExportDimrFilePath(fileContext, path);

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

            FileUtils.CreateDirectoryIfNotExists(exportDirectory);

            var errorLog = string.Empty;
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
                    string exportSubDirectory = Path.Combine(exportDirectory, GetRelativeModelDir(item, dimrModel));
                    FileUtils.CreateDirectoryIfNotExists(exportSubDirectory);

                    var dimrModelExporter = GetDimrModelFileExporter(dimrModel);
                    if (dimrModelExporter == null)
                    {
                        throw new InvalidOperationException($"No file exporter found for model '{dimrModel.Name}'.");
                    }
                    
                    if (!dimrModelExporter.Export(dimrModel, dimrModel.GetExporterPath(exportSubDirectory)))
                    {
                        string formattedError = $"Export failed for model {dimrModel.Name}{Environment.NewLine}";
                        errorLog += formattedError;
                        Log.ErrorFormat(formattedError);
                    }
                }

                var writer = new DHydroConfigWriter {CoreCountDictionary = new Dictionary<IDimrModel, int>()};
                configDocument = writer.CreateConfigDocument(workflow, fileContext);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Export failed: " + e.Message);
                errorLog += $"Export failed: {e.Message}{Environment.NewLine}";
                return false;
            }
            finally
            {
                UnInitialize();
                if (!string.IsNullOrEmpty(errorLog))
                {
                    string errorFile = exportPath.Replace(".xml", ".err");
                    File.WriteAllText(errorFile, errorLog);
                    Log.InfoFormat("Export error log written: {0} ", errorFile);
                }
            }

            configDocument.Save(exportPath);
            return true;
        }

        private string GetExportDimrFilePath(HydroModelFileContext fileContext, string path)
        {
            if (string.IsNullOrEmpty(ExportDirectoryPath))
            {
                return path;
            }

            return Path.Combine(ExportDirectoryPath, GetRelativeExportDimrFilePath(fileContext));
        }

        private static string GetRelativeExportDimrFilePath(HydroModelFileContext fileContext)
        {
            if (fileContext != null && fileContext.IsInitialized)
            {
                return fileContext.GetRelativeDimrFilePath();
            }

            return "dimr.xml";
        }

        private static string GetRelativeModelDir(object item, IDimrModel dimrModel)
        {
            if (item is HydroModel hydroModel)
            {
                return hydroModel.FileContext.GetRelativeModelDirectory(dimrModel);
            }

            return dimrModel.DirectoryName;
        }

        private IDimrModelFileExporter GetDimrModelFileExporter(IDimrModel dimrModel) 
            => FileExportService.GetFileExportersFor(dimrModel).OfType<IDimrModelFileExporter>().FirstOrDefault();

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
            ExportDirectoryPath = null;
        }

        private static string ValidateDimrModels(List<IDimrModel> dimrModels)
        {
            var validationReportMessages = string.Empty;

            foreach (IDimrModel dimrModel in dimrModels)
            {
                ValidationReport validationReport = dimrModel.Validate();
                if (validationReport != null && validationReport.Severity() == ValidationSeverity.Error)
                {
                    string errorMessage = $"Validation errors: {string.Join("\n", validationReport.GetAllIssuesRecursive().Where(i => i.Severity == ValidationSeverity.Error).Select(i => $"\t{i.Subject}: {i.Message}").ToArray())}";
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