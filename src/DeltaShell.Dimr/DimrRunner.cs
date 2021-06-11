using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.Dimr.Properties;
using DeltaShell.NGHS.Common.IO;
using log4net;

namespace DeltaShell.Dimr
{
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
        protected bool runLocal;
        private bool disposed;
        private string dimrFile;

        private double timeStep;
        private DateTime stopTime;

        public DimrRunner(IDimrModel model, IDimrApiFactory dimrApiFactory)
        {
            this.model = model;
            this.dimrApiFactory = dimrApiFactory;
        }

        public IDimrApi Api { get; private set; }

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
                if (!HandleException(e))
                {
                    throw;
                }
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
                if (!HandleException(e))
                {
                    throw;
                }
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
                if (!HandleException(e))
                {
                    throw;
                }
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

        private bool HandleException(Exception e)
        {
            // suppress messages about crashed remote process (log as debug for developers)
            bool remoteProcessCrash = e is InvalidOperationException ex 
                                      && ex.Message.Contains("Remote process");

            if (remoteProcessCrash)
            {
                log.Debug(e.Message);
            }

            var errorMessage = remoteProcessCrash
                                   ? $"{model.Name} crashed during {model.Status}, please look the validation report and diagnostic/log file."
                                   : e.Message;

            log.Error(errorMessage);

            model.Status = ActivityStatus.Failed;
            
            Api?.ProcessMessages();
            Api?.Dispose();
            Api = null;

            return remoteProcessCrash;
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
            var exporter = (IFileExporter) Activator.CreateInstance(model.ExporterType);

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

        private void ExportDimrModel(string workDirectory, object modelObject, IFileExporter exporter)
        {
            bool orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;

            FileUtils.CreateDirectoryIfNotExists(workDirectory);
            string exportDir = Path.Combine(workDirectory, model.DirectoryName);
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            CommonFileSystemActions.ClearFolder(exportDir, 
                                                new HashSet<string>(model.IgnoredFilePathsWhenCleaningWorkingDirectory));
            exporter.Export(modelObject, model.GetExporterPath(exportDir));
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        // Display any warnings or errors.
        [InvokeRequired]
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                log.Info(args.Message);
            }
            else
            {
                log.Error(args.Message);
            }
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