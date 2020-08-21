using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.xsd;
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
        protected bool runLocal;
        private string dimrFile;
        private IDimrApi dimrApi;

        private double timeStep;
        private DateTime stopTime;

        public DimrRunner(IDimrModel model)
        {
            this.model = model;
        }

        public bool CanCommunicateWithDimrApi
        {
            get
            {
                return model != null &&
                       (model.Status == ActivityStatus.Initialized ||
                        model.Status == ActivityStatus.Executing ||
                        model.Status == ActivityStatus.Executed ||
                        model.Status == ActivityStatus.Done)
                       && dimrApi != null;
            }
        }

        public IDimrApi Api
        {
            get
            {
                return dimrApi;
            }
            set
            {
                dimrApi = value;
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
                Console.WriteLine(e.Message);
                log.ErrorFormat(e.Message);
                model.Status = ActivityStatus.Failed;
                if (dimrApi != null)
                {
                    dimrApi.ProcessMessages();
                    dimrApi.Dispose();
                    dimrApi = null;
                }
            }
        }

        public void OnProgressChanged()
        {
            if (dimrApi != null)
            {
                dimrApi.ProcessMessages();
            }

            //base.OnProgressChanged();
        }

        public void OnExecute()
        {
            if (model.RunsInIntegratedModel)
            {
                return;
            }

            try
            {
                if (dimrApi == null)
                {
                    return;
                }
                
                int returnCode = dimrApi.Update(timeStep);

                if (returnCode != 0)
                {
                    throw new DimrErrorCodeException(model.Status, returnCode);
                }

                model.CurrentTime = dimrApi.CurrentTime;
                OnProgressChanged();
                if (stopTime.Subtract(model.CurrentTime).TotalSeconds <= 0)
                {
                    model.Status = ActivityStatus.Done;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                log.ErrorFormat(e.Message);
                model.Status = ActivityStatus.Failed;
                if (dimrApi != null)
                {
                    dimrApi.ProcessMessages();
                    dimrApi.Dispose();
                    dimrApi = null;
                }
            }
        }

        public void OnFinish()
        {
            if (model.RunsInIntegratedModel)
            {
                return;
            }

            if (dimrApi != null)
            {
                dimrApi.Finish();
            }
        }

        public void OnCleanup()
        {
            if (dimrApi != null)
            {
                dimrApi.Dispose();
                dimrApi = null;
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

            string dimrLogDirectory = model is IDimrModel dimrModel ? 
                                          dimrModel.DimrExportDirectoryPath : model.ExplicitWorkingDirectory;
            DimrRunHelper.ConnectDimrRunLogFile(model, dimrLogDirectory);
        }
        
        public static string GenerateDimrXML(IDimrModel dimrModel, string workDirectory)
        {
            // generate dimconfig
            string dimrFile = Path.Combine(workDirectory, "dimr.xml");
            FileUtils.DeleteIfExists(dimrFile);
            var dimrConfig = new dimrXML() {documentation = documentation};

            // control section
            var element = new dimrComponentOrCouplerRefXML() {name = dimrModel.Name};
            dimrConfig.control = new[]
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
            //XmlValidate(dimrConfig.Serialize());
            dimrConfig.SaveToFile(dimrFile);
            return dimrFile;
        }

        public Array GetVar(string key)
        {
            double[] value = new[]
            {
                double.NaN
            };
            if (CanCommunicateWithDimrApi)
            {
                value = (double[]) dimrApi.GetValues(key);
            }

            return value;
        }

        public void SetVar(string key, Array values)
        {
            if (CanCommunicateWithDimrApi)
            {
                dimrApi.SetValues(key, values);
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
            var exporter = (IFileExporter) Activator.CreateInstance(model.ExporterType);

            var exportPath = string.Empty;

            if (model.DimrExportDirectoryPath == null)
            {
                exportPath = FileUtils.CreateTempDirectory();
                model.DimrExportDirectoryPath = exportPath;
            }
            else
            {
                exportPath = model.DimrExportDirectoryPath;
            }

            ExportDimrModel(exportPath, model, exporter);

            // generate the dimr config xml
            dimrFile = GenerateDimrXML(model, exportPath);

            // initialize dimr
            dimrApi = DimrApiFactory.CreateNew(!runLocal);

            if (dimrApi == null)
            {
                throw new ArgumentNullException("Could not load the Dimr api.");
            }

            dimrApi.DimrRefDate = model.StartTime;
            dimrApi.KernelDirs = model.KernelDirectoryLocation;

            int returnCode = dimrApi.Initialize(dimrFile);

            if (returnCode != 0)
            {
                throw new DimrErrorCodeException(model.Status, returnCode);
            }

            timeStep = dimrApi.TimeStep.TotalSeconds;
            stopTime = dimrApi.StopTime;
        }

        private void ExportDimrModel(string workDirectory, object modelObject, IFileExporter exporter)
        {
            bool orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;

            FileUtils.CreateDirectoryIfNotExists(workDirectory);
            string exportDir = Path.Combine(workDirectory, model.DirectoryName);
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            ClearFolder(exportDir);
            exporter.Export(modelObject, model.GetExporterPath(exportDir));
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        private void ClearFolder(string FolderName)
        {
            var dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                FileUtils.DeleteIfExists(fi.FullName);
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                FileUtils.DeleteIfExists(di.FullName);
            }
        }

        private static void XmlValidate(string xmlString)
        {
            var stringReader = new StringReader(xmlString);
            // Set the validation settings.
            var settings = new XmlReaderSettings {ValidationType = ValidationType.Schema};
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += ValidationCallBack;

            // Create the XmlReader object.
            var reader = XmlReader.Create(stringReader, settings);

            // Parse the file. 
            while (reader.Read())
            {
                ;
            }
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
            //log.Info(KernelVersions);

            ValidationReport validationReport = model.Validate();
            if (validationReport != null && validationReport.Severity() == ValidationSeverity.Error)
            {
                string errorMessage = string.Format("Validation errors: {0}",
                                                    string.Join("\n", validationReport.GetAllIssuesRecursive()
                                                                                      .Where(i => i.Severity == ValidationSeverity.Error)
                                                                                      .Select(i => string.Format("\t{0}: {1}", i.Subject, i.Message)).ToArray()));

                model.Status = ActivityStatus.Failed;
                log.Error(model.Name + " model validation failed; please review the validation report.\n\r" + errorMessage);
            }

            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        public void Dispose()
        {
            dimrApi?.Dispose();
        }
    }
}