using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.xsd;
using log4net;

namespace DeltaShell.Dimr
{
    public class DimrRunner
    {
        private string dimrFile;
        private readonly IDimrModel model;
        private IDimrApi dimrApi;
        protected bool runLocal;
        public const string DimrRunLogfileDataItemTag = "DimrRunLog";

        public DimrRunner(IDimrModel model)
        {
            this.model = model;
        }

        public void OnInitialize()
        {
            model.DataItems.RemoveAllWhere(di => di.Tag == DimrRunLogfileDataItemTag);
            if (model.RunsInIntegratedModel) return;
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

        private void ValidateExportAndInitialize(bool disconnectOutput)
        {
            // validate the model
            ValidateModel();
            if (model.Status == ActivityStatus.Failed) return;

            if (disconnectOutput)
            {
                //disconnect current output from files
                model.DisconnectOutput();
            }
            // export this model
            var exporter = (IFileExporter)Activator.CreateInstance(model.ExporterType);

            string exportPath = string.Empty;
            
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
            log.Info(model.KernelVersions);
            dimrApi = DimrApiFactory.CreateNew(runRemote: !runLocal);

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

        public void OnProgressChanged()
        {
            if (dimrApi != null) dimrApi.ProcessMessages();
            //base.OnProgressChanged();
        }

        public void OnExecute()
        {
            if (model.RunsInIntegratedModel) return;
            try
            {
                if (dimrApi == null) return;

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
            if (model.RunsInIntegratedModel) return;
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
            var validPath = model.DimrExportDirectoryPath ?? Path.GetDirectoryName(dimrFile);
            if (!Directory.Exists(validPath)) return;

            var outputDirectory = Path.Combine(validPath, model.DimrModelRelativeOutputDirectory);
            if (!Directory.Exists(outputDirectory)) return;

            model.ConnectOutput(outputDirectory);
            ConnectDimrRunLogFile(model);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(DimrRunner));
        private const decimal fileVersion = 1;
        private const string createdBy = "Deltares, Coupling Team";
        private const string DIMR_RUN_LOGFILE_NAME = "dimr_redirected.log";

        private static readonly dimrDocumentationXML documentation = new dimrDocumentationXML
        {
            createdBy = createdBy,
            fileVersion = fileVersion
        };

        private double timeStep;
        private DateTime stopTime;

        public bool CanCommunicateWithDimrApi
        {
            get
            {
                return (model != null) &&
                       (model.Status == ActivityStatus.Initialized ||
                        model.Status == ActivityStatus.Executing ||
                        model.Status == ActivityStatus.Executed ||
                        model.Status == ActivityStatus.Done)
                       && (dimrApi != null);
            }
        }

        public IDimrApi Api
        {
            get { return dimrApi; }
            set { dimrApi = value; }
        }

        public static string GenerateDimrXML(IDimrModel dimrModel, string workDirectory)
        {
            // generate dimconfig
            var dimrFile = Path.Combine(workDirectory, "dimr.xml");
            FileUtils.DeleteIfExists(dimrFile);
            var dimrConfig = new dimrXML() { documentation = documentation };

            // control section
            var element = new dimrComponentOrCouplerRefXML() { name = dimrModel.Name };
            dimrConfig.control = new[] { element };
            FileUtils.CreateDirectoryIfNotExists(Path.Combine(dimrModel.DimrExportDirectoryPath, dimrModel.DimrModelRelativeOutputDirectory));
            // component section
            var component = new dimrComponentXML
            {
                name = dimrModel.Name,
                library = dimrModel.LibraryName,
                workingDir = dimrModel.DirectoryName,
                inputFile = dimrModel.InputFile
            };
            dimrConfig.component = new[] { component };
            //XmlValidate(dimrConfig.Serialize());
            dimrConfig.SaveToFile(dimrFile);
            return dimrFile;
        }

        private void ExportDimrModel(string workDirectory, object modelObject, IFileExporter exporter)
        {
            var orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;
           
            FileUtils.CreateDirectoryIfNotExists(workDirectory);
            var exportDir = Path.Combine(workDirectory, model.DirectoryName);
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            ClearFolder(exportDir);
            exporter.Export(modelObject, model.GetExporterPath(exportDir));
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        private void ClearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

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
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += ValidationCallBack;

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(stringReader, settings);

            // Parse the file. 
            while (reader.Read()) ;

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
            var orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;
            //log.Info(KernelVersions);

            var validationReport = model.Validate();
            if (validationReport != null && validationReport.Severity() == ValidationSeverity.Error)
            {
                var errorMessage = String.Format("Validation errors: {0}",
                    String.Join("\n", validationReport.GetAllIssuesRecursive()
                        .Where(i => i.Severity == ValidationSeverity.Error)
                        .Select(i => String.Format("\t{0}: {1}", i.Subject, i.Message)).ToArray()));

                model.Status = ActivityStatus.Failed;
                log.Error(model.Name + " model validation failed; please review the validation report.\n\r" + errorMessage);
            }
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        public Array GetVar(string key)
        {
            double[] value = new[] { Double.NaN };
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

        public static void ConnectDimrRunLogFile(IModel model)
        {
            var dimrModel=model as IDimrModel;
            var dimrLogDirectory = "";

            dimrLogDirectory = dimrModel != null ? dimrModel.DimrExportDirectoryPath : model.ExplicitWorkingDirectory;

            var completeDimrLogFilename = Path.Combine(dimrLogDirectory, DIMR_RUN_LOGFILE_NAME);
            if (!File.Exists(completeDimrLogFilename)) return;
            
            //add an dimr run log output dataitem with the log...
            var logDataItem = model.DataItems.FirstOrDefault(di => di.Tag == DimrRunLogfileDataItemTag);
            if (logDataItem == null)
            {
                var textDocument = new TextDocument(true) {Name = "Dimr Run Log"};

                logDataItem = new DataItem(textDocument, DataItemRole.Output, DimrRunLogfileDataItemTag);
                model.DataItems.Add(logDataItem);
            }

            using (Stream objStream = File.OpenRead(completeDimrLogFilename))
            {
                // Read data from file
                byte[] arrData = {};
                var stringBuilder = new StringBuilder();
                // Read data from file until read position is not equals to length of file
                while (objStream.Position != objStream.Length)
                {
                    // Read number of remaining bytes to read
                    long lRemainingBytes = objStream.Length - objStream.Position;

                    // If bytes to read greater than 2 mega bytes size create array of 2 mega bytes
                    // Else create array of remaining bytes
                    if (lRemainingBytes > 262144)
                    {
                        arrData = new byte[262144];
                    }
                    else
                    {
                        arrData = new byte[lRemainingBytes];
                    }

                    // Read data from file
                    objStream.Read(arrData, 0, arrData.Length);

                    stringBuilder.Append(Encoding.UTF8.GetString(arrData, 0, arrData.Length));
                }
                ((TextDocument) logDataItem.Value).Content = stringBuilder.ToString();
            }
        }
    }
}