using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.Gui;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [Category(NghsTestCategory.AcceptanceTests)]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.Wpf)]
    [TestFixture]
    public class AcceptanceModelsTest
    {
        [Test]
        [TestCaseSource(typeof(AcceptanceModelsTestData), nameof(AcceptanceModelsTestData.GetAcceptanceModelTestCases))]
        public void Delft3DFM_AcceptanceModelTest(string mduFilePath)
        {
            // Step 1: using(running GUI) add correct plugins for Delft3DFM
            using (var tempDir = new TemporaryDirectory())
            using (IGui gui = CreateGui())
            {
                gui.Run();

                void MainWindowShown()
                {
                    IApplication app = gui.Application;
                    app.CreateNewProject();

                    // Step 2: Import MDU
                    Assert.True(TryPerformAction(() => ImportFlowFMModelAndAddToProject(app, mduFilePath)), $"Failed to import model: {mduFilePath}");

                    // Step 3: Save Project As
                    string projectPath = Path.Combine(tempDir.Path, "ProjectSave", "TestProject.dsproj");
                    Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPath)), $"Failed to save project before running the model: {projectPath}");

                    // Step 4: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()), $"Failed to close project after import: {projectPath}");

                    // Step 5: Re-Open Project
                    Assert.True(TryPerformAction(() => app.OpenProject(projectPath)), $"Failed to reopen project: {projectPath}");

                    // Step 6: Validation of the model
                    ITimeDependentModel rootModel = null;
                    Assert.True(TryPerformAction(() => GetRootModelAndValidate(app, out rootModel)), $"Failed to validate model: {rootModel.Name}");

                    // Step 7: Dimr Export of FM model
                    string dimrExportPath = Path.Combine(tempDir.Path, "Dimr_Export", "dimr.xml");
                    Assert.True(TryPerformAction(() => ExportDimrConfiguration(dimrExportPath, app, rootModel)), $"Failed to export dimr configuration for model: {rootModel.Name}");

                    // Step 8: Adjust Time Settings (10 time steps)
                    Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModel)), $"Failed to adjust time settings for model: {rootModel.Name}");

                    // Step 9: Run model
                    Assert.True(TryPerformAction(() => RunModel(rootModel)), $"Failed to run model: {rootModel.Name}");

                    // Step 10: Save Project
                    Assert.True(TryPerformAction(() => app.SaveProject()), $"Failed to save project after running the model: {projectPath}");

                    // Step 1: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()), $"Failed to close project after running the model: {projectPath}");
                }

                WpfTestHelper.ShowModal((Control)gui.MainWindow, MainWindowShown);
            }
        }

        [Test]
        [TestCaseSource(typeof(AcceptanceModelsTestData), nameof(AcceptanceModelsTestData.GetDimrAcceptanceModelTestCases))]
        public void Delft3DFM_AcceptanceModelTestUsingDimrConfigs(string dimrXmlPath)
        {
            // Step 1: using(running GUI) add correct plugins for Delft3DFM
            using (var tempDir = new TemporaryDirectory())
            using (IGui gui = CreateGui())
            {
                gui.Run();

                void MainWindowShown()
                {
                    IApplication app = gui.Application;
                    app.CreateNewProject();

                    // Step 2: Read Dimr config
                    var dimrXmlConfig = new DimrXmlConfig(dimrXmlPath);

                    // Step 3: Import Dimr xml
                    Assert.True(TryPerformAction(() => ImportDimrXmlAndAddToProject(app, dimrXmlPath)), $"Failed to import the original model: {dimrXmlPath}");

                    // Step 4: Validation of the model
                    HydroModel rootModelOriginalDimrXml = null;
                    Assert.True(TryPerformAction(() => GetHydroModelAndValidate(app, dimrXmlConfig, out rootModelOriginalDimrXml)), $"Failed to validate the original model: {rootModelOriginalDimrXml.Name}");

                    // Step 5: Adjust Time Settings (10 time steps)
                    Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModelOriginalDimrXml)), $"Failed to adjust time settings for the original model: {rootModelOriginalDimrXml.Name}");

                    // Step 6: Run model
                    Assert.True(TryPerformAction(() => RunIntegratedModel(rootModelOriginalDimrXml, 6)), $"Failed to run the original model: {rootModelOriginalDimrXml.Name}");

                    CheckIfOutputHasBeenCreated(dimrXmlConfig, app, rootModelOriginalDimrXml.Name);

                    // Step 7: Save Project As
                    string saveFolderPathWithOutput = Path.Combine(tempDir.Path, @"SaveTestProjectFolder");
                    string projectPathWithOutput = Path.Combine(saveFolderPathWithOutput, "TestProject.dsproj");

                    Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPathWithOutput)), $"Failed to save as original project after running the model: {projectPathWithOutput}");

                    CheckPersistentFolderStructure(saveFolderPathWithOutput, projectPathWithOutput, rootModelOriginalDimrXml, dimrXmlConfig, 7, out Dictionary<string, DateTime> timeStampsAfterFirstSave);

                    // Step 8: Run model Again
                    Assert.True(TryPerformAction(() => RunIntegratedModel(rootModelOriginalDimrXml, 8)), $"Failed to run model: {rootModelOriginalDimrXml.Name}");

                    // Step 9: Save Project 
                    Assert.True(TryPerformAction(() => app.SaveProject()), $"Failed to save the new output files of the second run: {projectPathWithOutput}");

                    CheckPersistentFolderStructure(saveFolderPathWithOutput, projectPathWithOutput, rootModelOriginalDimrXml, dimrXmlConfig, 9, out Dictionary<string, DateTime> timeStampsAfterSecondSave);
                    CheckUpToDateFiles(timeStampsAfterFirstSave, timeStampsAfterSecondSave, 9);

                    // Step 10: Dimr Export of FM model
                    string dimrExportPath = Path.Combine(tempDir.Path, "Dimr_Export", "dimr.xml");
                    Assert.True(TryPerformAction(() => ExportDimrConfiguration(dimrExportPath, app, rootModelOriginalDimrXml)), $"Failed to export dimr configuration for model: {rootModelOriginalDimrXml.Name}");

                    // Step 11: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()), $"Failed to close project after dimr import: {projectPathWithOutput}");

                    app.CreateNewProject();

                    // Step 12: Import Dimr xml
                    Assert.True(TryPerformAction(() => ImportDimrXmlAndAddToProject(app, dimrExportPath)), $"Failed to import exported dimr xml file: {dimrExportPath}");

                    // Step 13: Validation of the model
                    HydroModel rootModelExportedDimrXml = null;
                    Assert.True(TryPerformAction(() => GetHydroModelAndValidate(app, dimrXmlConfig, out rootModelExportedDimrXml)), $"Failed to validate the imported model after creating a new dimr xml: {rootModelExportedDimrXml.Name}");

                    // Step 14: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()), $"Failed to close project after second import: {projectPathWithOutput}");

                    // Step 15: Re-Open Project
                    Assert.True(TryPerformAction(() => app.OpenProject(projectPathWithOutput)), $"Failed to reopen project: {projectPathWithOutput}");

                    // Step 16: Validation of the model
                    HydroModel rootModelOpenedProject = null;
                    Assert.True(TryPerformAction(() => GetHydroModelAndValidate(app, dimrXmlConfig, out rootModelOpenedProject)), $"Failed to validate model after opening the created project: {rootModelOpenedProject.Name}");

                    // Step 17: Adjust Time Settings (10 time steps)
                    Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModelOpenedProject)), $"Failed to adjust time settings for model after opening the created project: {rootModelOpenedProject.Name}");

                    // Step 18: Run model
                    Assert.True(TryPerformAction(() => RunIntegratedModel(rootModelOpenedProject, 18)), $"Failed to run model after opening the created project: {rootModelOpenedProject.Name}");

                    //// Step 19: Save Project
                    Assert.True(TryPerformAction(() => app.SaveProject()), $"Failed to save project after opening and running the model: {projectPathWithOutput}");

                    CheckPersistentFolderStructure(saveFolderPathWithOutput, projectPathWithOutput, rootModelOpenedProject, dimrXmlConfig, 19, out Dictionary<string, DateTime> timeStampsOutputAfterThirdSave);
                    CheckUpToDateFiles(timeStampsAfterSecondSave, timeStampsOutputAfterThirdSave, 19);

                    // Step 20: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()), $"Failed to close project after opening and running the model: {projectPathWithOutput}");
                }

                WpfTestHelper.ShowModal((Control)gui.MainWindow, (Action)MainWindowShown);
            }
        }

        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new DimrGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new FlowFMGuiPlugin(),
                new WaveGuiPlugin(),
                new HydroModelGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
                new RealTimeControlGuiPlugin(),
                new ScriptingGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new ToolboxGuiPlugin(),
                new WaterQualityModelGuiPlugin(),
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new WaveApplicationPlugin(),
                new HydroModelApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new ToolboxApplicationPlugin(),
                new WaterQualityModelApplicationPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }

        private static void CheckIfOutputHasBeenCreated(DimrXmlConfig dimrXmlConfig, IApplication app, string rootModelOriginalDimrXmlName)
        {
            string integratedModelWorkingDirectoryPath = Path.Combine(app.WorkDirectory, rootModelOriginalDimrXmlName);
            var integratedModelWorkingDirectory = new DirectoryInfo(integratedModelWorkingDirectoryPath);
            Assert.IsTrue(integratedModelWorkingDirectory.GetFiles().Any(f => f.Name == "dimr.xml"));
            DirectoryInfo[] subModelsWorkingDirectories = integratedModelWorkingDirectory.GetDirectories();

            if (dimrXmlConfig.FMModelIncluded)
            {
                DirectoryInfo fmWorkingDirectory = subModelsWorkingDirectories.FirstOrDefault(d => d.Name == "dflowfm");
                Assert.IsNotNull(fmWorkingDirectory);
                Assert.IsTrue(fmWorkingDirectory.GetDirectories().Any(d => d.Name == "output"), "Output of FM has not been created in the working directory");
            }

            if (dimrXmlConfig.RtcModelIncluded)
            {
                DirectoryInfo rtcWorkingDirectory = subModelsWorkingDirectories.FirstOrDefault(d => d.Name == "rtc");
                Assert.IsNotNull(rtcWorkingDirectory);
                Assert.IsTrue(rtcWorkingDirectory.GetDirectories().Any(d => d.Name == "output"), "Output of RTC has not been created in the working directory");
            }

            if (dimrXmlConfig.WavesModelIncluded)
            {
                DirectoryInfo wavesWorkingDirectory = subModelsWorkingDirectories.FirstOrDefault(d => d.Name == "wave");
                Assert.IsNotNull(wavesWorkingDirectory);
                Assert.IsTrue(wavesWorkingDirectory.GetDirectories().Any(d => d.Name == "output"), "Output of Waves has not been created in the working directory");
            }
        }

        private static void CheckUpToDateFiles(IDictionary<string, DateTime> timeStampsFirstSet, IDictionary<string, DateTime> timeStampsSecondSet, int testStepNr)
        {
            foreach (KeyValuePair<string, DateTime> kvp in timeStampsFirstSet)
            {
                string key = kvp.Key;
                Assert.AreNotEqual(timeStampsSecondSet[key], kvp.Value, $"After saving in step {testStepNr} file {key} was not updated");
            }
        }

        private static void CheckPersistentFolderStructure(string saveFolderPath, string projectPath,
                                                           ITimeDependentModel rootModel, DimrXmlConfig dimrXmlConfig, int testStepNr,
                                                           out Dictionary<string, DateTime> timeStampsFiles)
        {
            string dataFolder = Path.Combine(saveFolderPath, "TestProject.dsproj_data");
            timeStampsFiles = new Dictionary<string, DateTime>();

            CheckMainPersistentFolders(saveFolderPath, projectPath, dataFolder, out DateTime timeStampProject);
            timeStampsFiles.Add(Path.GetFileName(projectPath), timeStampProject);

            var integratedModel = (ICompositeActivity)rootModel;

            CheckIntegratedModelFileStructure(dataFolder, integratedModel.Name, testStepNr);

            if (dimrXmlConfig.FMModelIncluded)
            {
                WaterFlowFMModel[] fmModels = integratedModel.Activities.OfType<WaterFlowFMModel>().ToArray();
                CheckFileStructureFM(dataFolder, fmModels[0].Name, testStepNr, timeStampsFiles);
            }

            if (dimrXmlConfig.RtcModelIncluded)
            {
                RealTimeControlModel[] rtcModels = integratedModel.Activities.OfType<RealTimeControlModel>().ToArray();
                CheckFileStructureRtc(dataFolder, rtcModels[0].Name, testStepNr, timeStampsFiles);
            }

            if (dimrXmlConfig.WavesModelIncluded)
            {
                WaveModel[] wavesModels = integratedModel.Activities.OfType<WaveModel>().ToArray();
                CheckFileStructureWaves(dataFolder, wavesModels[0].Name, testStepNr, timeStampsFiles);
            }
        }

        private static void CheckMainPersistentFolders(string saveFolderPath, string projectPath, string dataFolder, out DateTime timeStampProject)
        {
            Assert.True(Directory.Exists(saveFolderPath));
            Assert.True(File.Exists(projectPath));
            timeStampProject = File.GetLastWriteTime(projectPath);
            Assert.True(Directory.Exists(dataFolder));
        }

        private static void CheckIntegratedModelFileStructure(string dataFolder, string integratedModelName, int testStepNr)
        {
            string dataFolderIntegratedModel = Path.Combine(dataFolder, integratedModelName);
            Assert.True(Directory.Exists(dataFolderIntegratedModel), $"After step {testStepNr} the model directory of the integrated model can not be found in the persistent folder");
            string jsonFile = Path.Combine(dataFolderIntegratedModel, integratedModelName + ".json");
            Assert.True(File.Exists(jsonFile), $"After step {testStepNr} the coupling json file of the integrated model can not be found in the persistent folder");
        }

        private static void CheckFileStructureFM(string dataFolder, string fmModelName, int testStepNr, IDictionary<string, DateTime> timeStamps)
        {
            string dataFolderFmModel = Path.Combine(dataFolder, fmModelName);
            Assert.True(Directory.Exists(dataFolderFmModel), $"After step {testStepNr} the model directory of FM can not be found in the persistent folder.");
            string inputFmModel = Path.Combine(dataFolderFmModel, DirectoryNameConstants.InputDirectoryName);
            string outputFmModel = Path.Combine(dataFolderFmModel, DirectoryNameConstants.OutputDirectoryName);

            Assert.True(Directory.Exists(inputFmModel), $"After step {testStepNr} the input directory of FM can not be found in the persistent folder.");
            Assert.True(Directory.Exists(outputFmModel), $"After step {testStepNr} the output directory of FM can not be found in the persistent folder.");

            string mduPath = Path.Combine(inputFmModel, fmModelName + ".mdu");
            Assert.True(File.Exists(mduPath), $"After step {testStepNr} the mdu file of FM can not be found in the persistent folder.");

            string diaPath = Path.Combine(outputFmModel, fmModelName + ".dia");
            Assert.True(File.Exists(diaPath), $"After step {testStepNr} the diagnostic file of FM can not be found in the persistent folder.");
            timeStamps.Add(Path.GetFileName(diaPath), File.GetLastWriteTime(diaPath));
        }

        private static void CheckFileStructureRtc(string dataFolder, string rtcModelName, int testStepNr, IDictionary<string, DateTime> timeStamps)
        {
            string dataFolderRtcModel = Path.Combine(dataFolder, rtcModelName);
            Assert.True(Directory.Exists(dataFolderRtcModel), $"After step {testStepNr} the model directory of RTC can not be found in the persistent folder.");

            string outputRtcModel = Path.Combine(dataFolderRtcModel, DirectoryNameConstants.OutputDirectoryName);
            Assert.True(Directory.Exists(outputRtcModel), $"After step {testStepNr} the output directory of RTC can not be found in the persistent folder.");

            string diaPath = Path.Combine(outputRtcModel, "diag.xml");
            Assert.True(File.Exists(diaPath), $"After step {testStepNr} the diagnostic file of RTC can not be found in the persistent folder.");
            timeStamps.Add(Path.GetFileName(diaPath), File.GetLastWriteTime(diaPath));
        }

        private static void CheckFileStructureWaves(string dataFolder, string wavesModelName, int testStepNr, IDictionary<string, DateTime> timeStamps)
        {
            string dataFolderWavesModel = Path.Combine(dataFolder, wavesModelName);
            Assert.True(Directory.Exists(dataFolderWavesModel), $"After step {testStepNr} the model directory of Waves can not be found in the persistent folder.");

            string inputWavesModel = Path.Combine(dataFolderWavesModel, DirectoryNameConstants.InputDirectoryName);
            Assert.True(Directory.Exists(inputWavesModel), $"After step {testStepNr} the input directory of Waves can not be found in the persistent folder.");

            var outputWavesModelDirectoryInfo = new DirectoryInfo(Path.Combine(dataFolderWavesModel, DirectoryNameConstants.OutputDirectoryName));
            Assert.True(outputWavesModelDirectoryInfo.Exists, $"After step {testStepNr} the output directory of Waves can not be found in the persistent folder.");

            FileInfo diaFile = outputWavesModelDirectoryInfo.GetFiles().First(f => f.Name.StartsWith("swn-diag."));
            Assert.True(diaFile.Exists, $"After step {testStepNr} the  diagnostic file of Waves can not be found in the persistent folder.");
            timeStamps.Add(diaFile.Name, diaFile.LastWriteTime);
        }

        /// <summary>
        /// Wraps Action in a Try-Catch, returns false if Action results in an exception
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Success or Failure of Action</returns>
        private static bool TryPerformAction(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    Console.Error.WriteLine(exception.Message);
                }

                return false;
            }
        }
    
        private static void ImportFlowFMModelAndAddToProject(IApplication app, string mduPath)
        {
            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduPath);

            app.Project.RootFolder.Items.Add(fmModel);
        }

        private static void ImportDimrXmlAndAddToProject(IApplication app, string dimrXmlPath)
        {
            var integratedModel = new HydroModel();
            app.Project.RootFolder.Items.Add(integratedModel);
            DHydroConfigXmlImporter importer = app.FileImporters.OfType<DHydroConfigXmlImporter>().First();
            importer.ImportItem(dimrXmlPath, integratedModel);
        }

        private static void GetHydroModelAndValidate(IApplication app, DimrXmlConfig dimrXmlConfig, out HydroModel hydroModel)
        {
            hydroModel = GetRootModel<HydroModel>(app);

            if (dimrXmlConfig.FMModelIncluded)
            {
                Assert.AreEqual(hydroModel.Activities.OfType<WaterFlowFMModel>().Count(), 1, "The integrated model is missing the FM model after the dimr import.");
            }

            if (dimrXmlConfig.RtcModelIncluded)
            {
                Assert.AreEqual(hydroModel.Activities.OfType<RealTimeControlModel>().Count(), 1, "The integrated model is missing the RTC model after the dimr import.");
            }

            if (dimrXmlConfig.WavesModelIncluded)
            {
                Assert.AreEqual(hydroModel.Activities.OfType<WaveModel>().Count(), 1, "The integrated model is missing the Waves model after the dimr import.");
            }

            ValidationReport report = hydroModel.Validate();

            if (report == null)
            {
                throw new NotSupportedException($"Unable to Validate Root Model: {hydroModel.Name}, did you forget to add support for this model type?");
            }

            CheckValidationErrors(report);
        }

        private static void GetRootModelAndValidate(IApplication app, out ITimeDependentModel timeDependentModel)
        {
            timeDependentModel = GetRootModel<ITimeDependentModel>(app);

            // Unfortunately there is no IValidateable, so instead we handle this for each individual model type...
            ValidationReport report = null;

            var hydroModel = timeDependentModel as HydroModel;
            if (hydroModel != null)
            {
                report = hydroModel.Validate();
            }

            var fmModel = timeDependentModel as WaterFlowFMModel;
            if (fmModel != null)
            {
                report = fmModel.Validate();
            }

            if (report == null)
            {
                throw new NotImplementedException($"Unable to Validate Root Model: {timeDependentModel.Name}, did you forget to add support for this model type?");
            }

            CheckValidationErrors(report);
        }

        private static void CheckValidationErrors(ValidationReport report)
        {
            if (report.ErrorCount > 0)
            {
                string debugInfo = string.Join(Environment.NewLine, report.AllErrors.Select(er => er.Message));
                throw new Exception(debugInfo);
            }
        }

        private static T GetRootModel<T>(IApplication app)
        {
            T model = app.Project.RootFolder.Models.OfType<T>().FirstOrDefault();
            if (model == null)
            {
                throw new NullReferenceException("Unable to get Model from Project Root Folder");
            }

            return model;
        }

        private static void AdjustTimeSettings(ITimeDependentModel timeDependentModel)
        {
            timeDependentModel.StopTime = timeDependentModel.StartTime.AddHours(timeDependentModel.TimeStep.TotalHours * 10);
        }

        private void RunModel(IActivity activity)
        {
            var hydroModel = activity as HydroModel;
            if (hydroModel != null)
            {
                // We get the workflow that runs all models in parallel
                ICompositeActivity workflow = hydroModel.Workflows.FirstOrDefault(w => w.Activities.Count == hydroModel.Activities.Count);
                if (workflow == null)
                {
                    throw new NullReferenceException("Unable to get Workflow (run all models in parallel)");
                }

                hydroModel.CurrentWorkflow = workflow;
            }

            // Run model
            ActivityRunner.RunActivity(activity);

            if (activity.Status != ActivityStatus.Cleaned)
            {
                throw new AssertionException(
                    string.Format("Unable to complete Model run{0}Expected status: Cleaned{0}Actual status: {1}",
                                  Environment.NewLine, activity.Status.ToString()));
            }
        }

        private void RunIntegratedModel(HydroModel hydroModel, int testStepNr)
        {
            // We get the workflow that runs all models in parallel
            ICompositeActivity workflow = hydroModel.Workflows.FirstOrDefault(w => w.Activities.Count == hydroModel.Activities.Count);
            if (workflow == null)
            {
                throw new NullReferenceException($"Unable to get Workflow (run all models in parallel) in step {testStepNr}");
            }

            hydroModel.CurrentWorkflow = workflow;

            // Run model
            ActivityRunner.RunActivity(hydroModel);

            if (hydroModel.Status != ActivityStatus.Cleaned)
            {
                throw new AssertionException($"Unable to complete Model run in step {testStepNr}. " +
                                             $"{Environment.NewLine} Expected status: Cleaned {Environment.NewLine} Actual status: {hydroModel.Status}");
            }
        }

        private static void ExportDimrConfiguration(string dimrExportPath, IApplication app, IModel model)
        {
            var exporter = new DHydroConfigXmlExporter(app.FileExportService);
            if (!exporter.Export(model, dimrExportPath))
            {
                throw new AssertionException($"Dimr export failed for model: '{model.Name}'.");
            }
        }
        
        private struct DimrXmlConfig
        {
            public DimrXmlConfig(string dimrXmlPath)
            {
                string xml = File.ReadAllText(dimrXmlPath);
                
                FMModelIncluded = xml.ContainsCaseInsensitive("dflowfm");
                RtcModelIncluded = xml.ContainsCaseInsensitive("FBCTools_BMI");
                WavesModelIncluded = xml.ContainsCaseInsensitive("wave");
            }

            public bool FMModelIncluded { get; }
            public bool RtcModelIncluded { get; }
            public bool WavesModelIncluded { get; }
        }
    }
}