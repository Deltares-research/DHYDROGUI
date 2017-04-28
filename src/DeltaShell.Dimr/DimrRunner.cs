using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
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

        public DimrRunner(IDimrModel model)
        {
            this.model = model;
        }

        public void OnInitialize()
        {
            if (model.IsRunByDimr) return;
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

            model.ExplicitWorkingDirectory = ExportDimrModel(model.ExplicitWorkingDirectory, model, exporter);

            // generate the dimr config xml
            dimrFile = GenerateDimrXML(model.ExplicitWorkingDirectory);

            // initialize dimr
            log.Info(model.KernelVersions);
            dimrApi = DimrApiFactory.CreateNew(runRemote: !runLocal);
            dimrApi.DimrRefDate = model.StartTime;
            dimrApi.KernelDirs = model.KernelDirectoryLocation;

            if (dimrApi.Initialize(dimrFile) > 0)
            {
                throw new Exception("Couldn't initialize DIMR Api");
            }
        }

        public void OnProgressChanged()
        {
            if (dimrApi != null) dimrApi.ProcessMessages();
            //base.OnProgressChanged();
        }

        public void OnExecute()
        {
            if (model.IsRunByDimr) return;
            try
            {
                if (dimrApi == null) return;
                dimrApi.Update(dimrApi.TimeStep.TotalSeconds);

                model.CurrentTime = dimrApi.CurrentTime;
                OnProgressChanged();
                if (dimrApi.StopTime.Subtract(dimrApi.CurrentTime).TotalSeconds <= 0)
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
            if (model.IsRunByDimr) return;
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
            var validPath = model.ExplicitWorkingDirectory ?? Path.GetDirectoryName(dimrFile);
            if (!Directory.Exists(validPath)) return;
            model.ConnectOutput(validPath);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(DimrRunner));
        private const decimal fileVersion = 1;
        private const string createdBy = "Deltares, Coupling Team";

        private static readonly dimrDocumentationXML documentation = new dimrDocumentationXML
        {
            createdBy = createdBy,
            fileVersion = fileVersion
        };

        public bool CanCommunicateWithDimrApi
        {
            get
            {
                return model!= null && 
                        model.Status == ActivityStatus.Initialized || model.Status == ActivityStatus.Executing ||
                        model.Status == ActivityStatus.Executed || model.Status == ActivityStatus.Done 
                      && dimrApi != null;
            }
        }

        public IDimrApi Api
        {
            get { return dimrApi; }
            set { dimrApi = value; }
        }

        private string GenerateDimrXML(string workDirectory)
        {
            // generate dimconfig
            var dimrFile = Path.Combine(workDirectory, "dimr.xml");
            FileUtils.DeleteIfExists(dimrFile);
            var dimrConfig = new dimrXML() { documentation = documentation };

            // control section
            var element = new dimrComponentOrCouplerRefXML() { name = model.Name };
            dimrConfig.control = new[] { element };

            // component section
            var component = new dimrComponentXML
            {
                name = model.Name,

                library = model.LibraryName,
                workingDir = model.DirectoryName,
                inputFile = model.InputFile
            };
            dimrConfig.component = new[] { component };
            //XmlValidate(dimrConfig.Serialize());
            dimrConfig.SaveToFile(dimrFile);
            return dimrFile;
        }

        private string ExportDimrModel(string workDirectory, object modelObject, IFileExporter exporter)
        {
            var orgSuspendClearOutputOnInputChange = model.SuspendClearOutputOnInputChange;
            model.SuspendClearOutputOnInputChange = true;
            if (workDirectory == null)
            {
                var dirPath = FileUtils.CreateTempDirectory();
                workDirectory = Path.Combine(dirPath, model.Name.Replace(' ', '_') + "dimr_output");
            }

            FileUtils.CreateDirectoryIfNotExists(workDirectory);
            var exportDir = Path.Combine(workDirectory, model.DirectoryName);
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            ClearFolder(exportDir);
            exporter.Export(modelObject, model.GetExporterPath(exportDir));
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
            return workDirectory;
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
                var errorMessage = string.Format("Validation errors: {0}",
                    string.Join("\n", validationReport.GetAllIssuesRecursive()
                        .Where(i => i.Severity == ValidationSeverity.Error)
                        .Select(i => string.Format("\t{0}: {1}", i.Subject, i.Message)).ToArray()));

                model.Status = ActivityStatus.Failed;
                log.Error(model.Name + " model validation failed; please review the validation report.\n\r" + errorMessage);
            }
            model.SuspendClearOutputOnInputChange = orgSuspendClearOutputOnInputChange;
        }

        public Array GetVar(string key)
        {
            double[] value = new[] { double.NaN };
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
    }
}