using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.IO;
using DeltaShell.Dimr.xsd;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace SobekImport.Tests.Helpers
{
    public class ModelImportAndExportChecker: IDisposable
    {
        private bool rr;
        private bool disposed;

        private HydroModel modelToRun;
        public WaterFlowModel1D waterFlowModel1D; 
        public RainfallRunoffModel rainfallRunoffModel;

        public ModelImportAndExportChecker(string pathDirSobekFiles)
        {
            modelToRun = ImportModelToRun(pathDirSobekFiles, b => rr = b);
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            modelToRun.ExplicitWorkingDirectory = tempDirectory;
            
            waterFlowModel1D = modelToRun.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();
            if (waterFlowModel1D != null)
            {
                waterFlowModel1D.ExplicitWorkingDirectory = Path.Combine(tempDirectory, "dflow1d");  // This is the place where the TBL files are copied to. 

                // Explicitly set this since Default has changed from (1 to 2) since tests were written
                var iadvec1D = waterFlowModel1D.ParameterSettings.FirstOrDefault(s => s.Name == "Iadvec1D");
                if (iadvec1D != null)
                {
                    iadvec1D.Value = "1";
                }
            }
            rainfallRunoffModel = modelToRun.Activities.OfType<RainfallRunoffModel>().FirstOrDefault();

            if (modelToRun == null)
            {
                throw new ArgumentException("no model found!");
            }
        }

        private static HydroModel ImportModelToRun(string pathDirSobekFiles, Action<bool> setUseRR = null)
        {
            var settingsDatPath = Path.Combine(pathDirSobekFiles, "Settings.dat");
            var settingsDat = File.ReadAllText(settingsDatPath).ToLower();
            var indexRestart = settingsDat.IndexOf("[restart]");
            settingsDat = settingsDat.Substring(0, indexRestart);

            var useFlow = settingsDat.Contains("channel=-1") || settingsDat.Contains("river=-1");
            var rr = settingsDat.Contains("3b=-1");
            if (setUseRR != null)
            {
                setUseRR(rr);
            }

            var modelImporter = new SobekHydroModelImporter(rr, useFlow, useFlow);

            var model = modelImporter.ImportItem(pathDirSobekFiles + @"\network.tp");

            var hydroModel = model as HydroModel;
            if (hydroModel == null) return null;

            if (useFlow)
            {
                var realTimeControlModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
                if (realTimeControlModel != null && realTimeControlModel.ControlGroups.Count == 0)
                {
                    hydroModel.Activities.Remove(realTimeControlModel);
                }
            }

            return hydroModel;
        }

        public string WorkingDirHisFilesPath { get; set; }

        public HydroModel ModelToRun
        {
            get { return modelToRun; }
        }

        public void RunModels()
        {
            RunModel(modelToRun);

            if (rr)
            {
                WorkingDirHisFilesPath = rainfallRunoffModel.ModelController.WorkingDirectory;
            }
            else //flow+rtc
            {
                WorkingDirHisFilesPath = Path.Combine(modelToRun.ExplicitWorkingDirectory + @"\dflow1d\");
            }
        }

        private void RunModel(IModel model)
        {
            ActivityRunner.RunActivity(model);

            if (model.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed.");
            }
        }

        public void CopyModelData(string exportDir)
        {
            var sourceDir = modelToRun.ExplicitWorkingDirectory;
            FileUtils.DeleteIfExists(exportDir);
            FileUtils.CopyDirectory(sourceDir, exportDir, "");
        }

        public void CheckModelExport(string exportDir, string referenceDir)
        {

            // Check dmr.xml
            var generatedDimrFile = Path.Combine(exportDir, "dimr.xml");
            Assert.IsTrue(File.Exists(generatedDimrFile), "File dimr.xml does not exist");

            var generatedDimrXml = dimrXML.LoadFromFile(generatedDimrFile);

            var referenceDimrFile = Path.Combine(referenceDir, "dimr.xml");
            var referenceDimrXml = dimrXML.LoadFromFile(referenceDimrFile);

            Assert.AreEqual(referenceDimrXml.documentation.fileVersion, generatedDimrXml.documentation.fileVersion);

            CheckDimrComponents(referenceDimrXml, generatedDimrXml);
            
            CheckDimrCouplers(referenceDimrXml, generatedDimrXml);

            CheckDimrControls(referenceDimrXml, generatedDimrXml);


            // Check Model Directories
            CheckModelDir(exportDir, referenceDir, "dflow1d");
            CheckModelDir(exportDir, referenceDir, "rtc");
            CheckModelDir(exportDir, referenceDir, "rr");

            // Remove data when successful
            FileUtils.DeleteIfExists(exportDir);
        }

        private static void CheckDimrControls(dimrXML referenceDimrXml, dimrXML generatedDimrXml)
        {
            for (int i = 0; i < referenceDimrXml.control.Length; i++)
            {
                var referenceParallelControl = referenceDimrXml.control[i] as dimrParallelXML;
                if (referenceParallelControl != null)
                {
                    var generatedParallelControl = generatedDimrXml.control[i] as dimrParallelXML;
                    Assert.IsNotNull(generatedParallelControl);
                    for (int j = 0; j < referenceParallelControl.Items.Length; j++)
                    {
                        var referenceParallelControlStartGroupItem = referenceParallelControl.Items[j] as dimrStartGroupXML;
                        if (referenceParallelControlStartGroupItem != null)
                        {
                            var generatedParallelControlStartGroupItem = generatedParallelControl.Items[j] as dimrStartGroupXML;
                            Assert.IsNotNull(generatedParallelControlStartGroupItem);
                            Assert.AreEqual(referenceParallelControlStartGroupItem.time, generatedParallelControlStartGroupItem.time);
                            for (int k = 0; k < referenceParallelControlStartGroupItem.Items.Length; k++)
                            {
                                Assert.AreEqual(referenceParallelControlStartGroupItem.Items[k].name, generatedParallelControlStartGroupItem.Items[k].name);
                            }
                        }
                        var referenceParallelControlSimpleItem = referenceParallelControl.Items[j] as dimrComponentOrCouplerRefXML;
                        if (referenceParallelControlSimpleItem != null)
                        {
                            var generatedParallelControlSimpleItem = generatedParallelControl.Items[j] as dimrComponentOrCouplerRefXML;
                            Assert.IsNotNull(generatedParallelControlSimpleItem);
                            Assert.AreEqual(referenceParallelControlSimpleItem.name, generatedParallelControlSimpleItem.name);
                        }
                    }
                }
                
                var referenceSimpleControl = referenceDimrXml.control[i] as dimrComponentOrCouplerRefXML;
                
                if (referenceSimpleControl != null)
                {
                    var generatedSimpleControl = generatedDimrXml.control[i] as dimrComponentOrCouplerRefXML;
                    Assert.IsNotNull(generatedSimpleControl);
                    Assert.AreEqual(referenceSimpleControl.name, generatedSimpleControl.name);
                }
            }
        }

        private static void CheckDimrCouplers(dimrXML referenceDimrXml, dimrXML generatedDimrXml)
        {
            if (referenceDimrXml.coupler == null) return;
  
            for (int i = 0; i < referenceDimrXml.coupler.Length; i++)
            {
                Assert.AreEqual(referenceDimrXml.coupler[i].name, generatedDimrXml.coupler[i].name );
                Assert.AreEqual(referenceDimrXml.coupler[i].sourceComponent, generatedDimrXml.coupler[i].sourceComponent);
                Assert.AreEqual(referenceDimrXml.coupler[i].targetComponent, generatedDimrXml.coupler[i].targetComponent);
                for (int j = 0; j < referenceDimrXml.coupler[i].item.Length; j++)
                {
                    Assert.AreEqual(referenceDimrXml.coupler[i].item[j].sourceName, generatedDimrXml.coupler[i].item[j].sourceName);
                    Assert.AreEqual(referenceDimrXml.coupler[i].item[j].targetName, generatedDimrXml.coupler[i].item[j].targetName);
                }
            }
        }

        private static void CheckDimrComponents(dimrXML referenceDimrXml, dimrXML generatedDimrXml)
        {
            for (int i = 0; i < referenceDimrXml.component.Length; i++)
            {
                Assert.AreEqual(referenceDimrXml.component[i].name, generatedDimrXml.component[i].name);
                Assert.AreEqual(referenceDimrXml.component[i].library, generatedDimrXml.component[i].library);
                Assert.AreEqual(referenceDimrXml.component[i].workingDir, generatedDimrXml.component[i].workingDir);
                Assert.AreEqual(referenceDimrXml.component[i].inputFile, generatedDimrXml.component[i].inputFile);
            }
        }

        public void CheckModelDir(string exportDir, string referenceDir, string modelName)
        {
            var expModelDir = Path.Combine(exportDir, modelName);
            var refModelDir = Path.Combine(referenceDir, modelName);

            var expModelDirExists = Directory.Exists(expModelDir);
            var refModelDirExists = Directory.Exists(refModelDir);

            // Check Consistency
            if (refModelDirExists)
            {
                Assert.IsTrue(expModelDirExists, string.Format("Model {0} does not exist", modelName));
            }
            else
            {
                Assert.IsFalse(expModelDirExists,
                    string.Format("Model {0} does exist, but is not in Reference", modelName));
            }

            if (!refModelDirExists) return;  // Nothing to Check Further

            var expFiles = Directory.EnumerateFiles(expModelDir).ToList();
            var refFiles = Directory.EnumerateFiles(refModelDir).ToList();

            //Assert.AreEqual(expFiles.Count, refFiles.Count,
            //    string.Format("Number of Files not the same in Model {0}", modelName));

            var expFileNames = expFiles.Select(Path.GetFileName).ToList();
            var refFileNames = refFiles.Select(Path.GetFileName).ToList();

            foreach (var refFileName in refFileNames)
            {
                var refFile = Path.Combine(refModelDir, refFileName);
                var expFile = Path.Combine(expModelDir, refFileName);

                Assert.IsTrue(File.Exists(expFile),
                    string.Format("Model {0}: File {1} not present in Export", modelName, refFileName));

                if (Path.GetExtension(refFileName) != ".xml")
                {
                    var expInfo = new FileInfo(expFile);
                    var refInfo = new FileInfo(refFile);

                    Assert.AreEqual(refInfo.Length, expInfo.Length,
                        string.Format("Model {0}: File {1} differs in Size", modelName, refFileName));

                    Assert.AreEqual(FileUtils.GetChecksum(refFile), FileUtils.GetChecksum(expFile),
                        string.Format("Model {0}: File {1} not equal to Reference", modelName, refFileName));
                }
                else
                {
                    var refFileLines = File.ReadAllLines(refFile);
                    var expFileLines = File.ReadAllLines(expFile);

                    for (var i = 0; i < refFileLines.Length; i++)
                    {
                        if (i != 1)  // Skip second line which differs always
                        {
                            Assert.AreEqual(refFileLines[i], expFileLines[i], string.Format("Model {0}: File {1} differs in Line {2}", modelName, refFileName, i + 1));                            
                        }
                    }
                }
            }

            foreach (var expFileName in expFileNames)
            {
                var refFile = Path.Combine(refModelDir, expFileName);

                Assert.IsTrue(File.Exists(refFile), string.Format("Model {0}: File {1} present in Export, but not in Reference", modelName, expFileName));
            }
        }

        #region IDispose Members

        /// <summary>
        /// See <see cref="System.IDisposable.Dispose"/> for more information.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Called when the object is being disposed or finalized.
        /// </summary>
        /// <param name="disposing">True when the object is being disposed (and therefore can
        /// access managed members); false when the object is being finalized without first
        /// having been disposed (and therefore can only touch unmanaged members).</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseSources();
            }

            disposed = true;
        }

        private void CloseSources()
        {
            if(waterFlowModel1D != null)
            {
                waterFlowModel1D.Dispose();
            }
        }

        #endregion

        
    }
}
