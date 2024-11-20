using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Core.Services;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.Dimr.Properties;
using log4net;
namespace DeltaShell.Dimr
{
    /// <summary>
    /// Provides a runner for DIMR models.

    /// </summary>
    public class DimrRunner : IDisposable
    {
        private const decimal fileVersion = 1;
        private const string createdBy = "Deltares, Coupling Team";
        private static readonly ILog log = LogManager.GetLogger(typeof(DimrRunner));

        private static readonly dimrDocumentationXML documentation = new dimrDocumentationXML
        {
            createdBy = createdBy,
            fileVersion = fileVersion
        };

        private readonly IDimrModel model;
        private readonly IDimrApiFactory dimrApiFactory;
        private IFileExportService fileExportService;

        protected bool runLocal;
        private bool disposed;
        private string dimrFile;

        private double timeStep;
        private DateTime stopTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DimrRunner"/> class.
        /// </summary>
        /// <param name="model">The DIMR model to run.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public DimrRunner(IDimrModel model)
            : this(model, new DimrApiFactory(), new FileExportService())
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DimrRunner"/> class.
        /// </summary>
        /// <param name="model">The DIMR model to run.</param>
        /// <param name="dimrApiFactory">Factory for creating the DIMR Api instance.</param>
        /// <param name="fileExportService">Service for retrieving the registered DIMR model file exporters.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/>, <paramref name="dimrApiFactory"/> or <paramref name="fileExportService"/> is <c>null</c>.
        /// </exception>
        public DimrRunner(IDimrModel model, IDimrApiFactory dimrApiFactory, IFileExportService fileExportService)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNull(dimrApiFactory, nameof(dimrApiFactory));
            Ensure.NotNull(fileExportService, nameof(fileExportService));
            
            this.model = model;
            this.dimrApiFactory = dimrApiFactory;
            this.fileExportService = fileExportService;
        }

        /// <summary>
        /// Gets the DIMR Api instance.
        /// </summary>
        public IDimrApi Api { get; private set; }

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

        public void OnInitialize()
        {
            model.DataItems.RemoveAllWhere(di => di.Tag == DimrRunHelper.dimrRunLogfileDataItemTag);
            if (model.RunsInIntegratedModel)
            {
                return;
            }

            try
            {
                ValidateExportAndInitialize(true);

                model.CurrentTime = model.StartTime;
                OnProgressChanged();
            }
            catch (Exception e)
            {
                HandleException(e);
                throw;
            }
        }

        public void OnProgressChanged()
        {
            Api?.ProcessMessages();
        }

        public void OnExecute()
        {
            if (model.RunsInIntegratedModel)
            {
                return;
            }

            try
            {
                if (Api == null)
                {
                    return;
                }

                int returnCode = Api.Update(timeStep);

                if (returnCode != 0)
                {
                    throw new DimrErrorCodeException(model.Status, returnCode);
                }

                model.CurrentTime = Api.CurrentTime;
                OnProgressChanged();
                if (stopTime.Subtract(model.CurrentTime).TotalSeconds <= 0)
                {
                    model.Status = ActivityStatus.Done;
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw;
            }
        }

        public void OnFinish()
        {
            if (model.RunsInIntegratedModel || Api == null)
            {
                return;
            }

            try
            {
                int returnCode = Api.Finish();

                if (returnCode != 0)
                {
                    throw new DimrErrorCodeException(model.Status, returnCode);
                }
            }
            catch (Exception e)
            {
                HandleException(e);
                throw;
            }
        }

        public void OnCleanup()
        {
            if (Api != null)
            {
                Api.Dispose();
                Api = null;
            }

            string validPath = model.DimrExportDirectoryPath ?? Path.GetDirectoryName(dimrFile);
            if (!Directory.Exists(validPath))
            {
                return;
            }

            string outputDirectory = Path.Combine(validPath, model.DimrModelRelativeOutputDirectory);
            if (!Directory.Exists(outputDirectory))
            {
                return;
            }

            model.ConnectOutput(outputDirectory);

            DimrRunHelper.ConnectDimrRunLogFile(model, model.DimrExportDirectoryPath);
        }

        public static string GenerateDimrXML(IDimrModel dimrModel, string workDirectory)
        {
            // generate dimr config
            string newDimrFile = Path.Combine(workDirectory, "dimr.xml");
            FileUtils.DeleteIfExists(newDimrFile);
            var dimrConfig = new dimrXML {documentation = documentation};

            // control section
            var element = new dimrComponentOrCouplerRefXML {name = dimrModel.Name};
            dimrConfig.control = new object[]
            {
                element
            };
            FileUtils.CreateDirectoryIfNotExists(Path.Combine(dimrModel.DimrExportDirectoryPath, dimrModel.DimrModelRelativeOutputDirectory));
            // component section
            var component = new dimrComponentXML
            {
                name = dimrModel.Name,
                library = dimrModel.LibraryName,
                workingDir = dimrModel.DirectoryName,
                inputFile = dimrModel.InputFile
            };
            dimrConfig.component = new[]
            {
                component
            };
            new DimrXMLSerializer().SaveToFile(newDimrFile, dimrConfig);
            return newDimrFile;
        }

        public Array GetVar(string key)
        {
            double[] value =
            {
                double.NaN
            };
            if (CanCommunicateWithDimrApi)
            {
                value = (double[]) Api.GetValues(key);
            }

            return value;
        }

        public void SetVar(string key, Array values)
        {
            if (CanCommunicateWithDimrApi)
            {
                Api.SetValues(key, values);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Api?.Dispose();
            }

            disposed = true;
        }

        public bool CanCommunicateWithDimrApi => model != null &&
                                                  (model.Status == ActivityStatus.Initialized ||
                                                   model.Status == ActivityStatus.Executing ||
                                                   model.Status == ActivityStatus.Executed ||
                                                   model.Status == ActivityStatus.Done)
                                                  && Api != null;

        private void HandleException(Exception e)
        {
            log.ErrorFormat(e.Message);
            model.Status = ActivityStatus.Failed;
            if (Api != null)
            {
                Api.ProcessMessages();
                Api.Dispose();
                Api = null;
            }
        }

        private void ValidateExportAndInitialize(bool disconnectOutput)
        {
            // validate the model
            ValidateModel();
            if (model.Status == ActivityStatus.Failed)
            {
                return;
            }

            if (disconnectOutput)
            {
                //disconnect current output from files
                model.DisconnectOutput();
            }

            // export this model
            IDimrModelFileExporter exporter = GetDimrModelFileExporter(model);
            if (exporter == null)
            {
                throw new InvalidOperationException($"No file exporter found for model '{model.Name}'.");
            }

            string exportPath = model.DimrExportDirectoryPath;

            ExportDimrModel(exportPath, model, exporter);

            // generate the dimr config xml
            dimrFile = GenerateDimrXML(model, exportPath);

            // initialize dimr
            Api = dimrApiFactory.CreateNew(!runLocal);
            
            if (Api == null)
            {
                throw new ArgumentNullException(Resources.DimrRunner_Could_not_load_dimr_api);
            }

            Api.DimrRefDate = model.StartTime;
            Api.KernelDirs = model.KernelDirectoryLocation;

            int returnCode = Api.Initialize(dimrFile);

            if (returnCode != 0)
            {
                throw new DimrErrorCodeException(model.Status, returnCode);
            }

            timeStep = Api.TimeStep.TotalSeconds;
            stopTime = Api.StopTime;
        }

        private IDimrModelFileExporter GetDimrModelFileExporter(IDimrModel dimrModel) 
            => FileExportService.GetFileExportersFor(dimrModel).OfType<IDimrModelFileExporter>().FirstOrDefault();

        private void ExportDimrModel(string workDirectory, object modelObject, IFileExporter exporter)
        {
            bool orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;

            FileUtils.CreateDirectoryIfNotExists(workDirectory);
            
            string exportDir = Path.Combine(workDirectory, model.DirectoryName);
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            FileUtils.ClearDirectory(exportDir, model.IgnoredFilePathsWhenCleaningWorkingDirectory);
            
            exporter.Export(modelObject, model.GetExporterPath(exportDir));
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        // Display any warnings or errors.
        [InvokeRequired]
        private void ValidateModel()
        {
            bool orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;

            ValidationReport validationReport = model.Validate();
            if (validationReport != null && validationReport.Severity() == ValidationSeverity.Error)
            {
                string errorMessage = "Validation errors: " +
                                      $"{string.Join("\n", validationReport.GetAllIssuesRecursive().Where(i => i.Severity == ValidationSeverity.Error).Select(i => $"\t{i.Subject}: {i.Message}").ToArray())}";

                model.Status = ActivityStatus.Failed;
                log.Error(model.Name + " model validation failed; please review the validation report.\n\r" + errorMessage);
            }

            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }
    }
}