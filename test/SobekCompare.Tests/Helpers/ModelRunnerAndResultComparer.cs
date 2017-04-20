using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.HisData;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace SobekCompare.Tests.Helpers
{
    public class ModelRunnerAndResultComparer: IDisposable
    {
        private bool rr;
        private string pathDirSobek;
        private bool disposed;
        private bool IsHisfileConnected;
        private bool flowTableFilesHaveBeenCopiedOnInitializing;

        private HydroModel modelToRun;
        public WaterFlowModel1D waterFlowModel1D; 
        public RainfallRunoffModel rainfallRunoffModel;

        private INetworkCoverage calcpntNetworkCoverage;
        private INetworkCoverage reachsegNetworkCoverage;

        private const string gridTblPath = @"Grid.tbl";
        private const string branchTblPath = @"branch.tbl";
        private const string nodeTblPath = @"node.tbl";
        private const string profileTblPath = @"profile.tbl";
        private const string qlatTblPath = @"qlat.tbl";
        private const string reachdownTblPath = @"reachdwn.tbl";
        private const string reachupTblPath = @"reachup.tbl";
        private const string structTblPath = @"struct.tbl";
        private const string structdwnTblPath = @"structdwn.tbl";
        private const string structupTblPath = @"structup.tbl";

        public ModelRunnerAndResultComparer(string pathDirSobekFiles)
        {
            ToleranceWaterLevel = 0.01; // 99 - 101 %
            ToleranceWaterDepth = 0.01; // 99 - 101 %
            ToleranceWaterFlow = 0.01; // 99 - 101 %
            ToleranceWaterVelocity = 0.01; // 99 - 101 %

            ToleranceWaterLevelErrorMargin = 0.01; //1 cm
            ToleranceWaterDepthErrorMargin = 0.01; //1 cm
            ToleranceWaterFlowErrorMargin = 0.01; // 10 l /s
            ToleranceWaterVelocityErrorMargin = 0.01; //1 cm / s

            SetPaths(pathDirSobekFiles);

            modelToRun = ImportModelToRun(pathDirSobekFiles, b => rr = b);
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            modelToRun.ExplicitWorkingDirectory = tempDirectory;
            
            waterFlowModel1D = modelToRun.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();
            if (waterFlowModel1D != null)
            {
                waterFlowModel1D.ExplicitWorkingDirectory = Path.Combine(tempDirectory, "dflow1d");  // This is the place where the TBL files are copied to. 
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

        public void RunModels()
        {
            modelToRun.StatusChanged += HydroModelStatusChanged;

            RunModel(modelToRun);

            if (rr)
            {
                WorkingDirHisFilesPath = rainfallRunoffModel.ModelController.WorkingDirectory;
            }
            else //flow+rtc
            {
                WorkingDirHisFilesPath = Path.Combine(modelToRun.ExplicitWorkingDirectory + @"\dflow1d\output\");
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

        private static IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowModel1DExporter();
            yield return new RealTimeControlModelExporter();
            yield return new RainfallRunoffModelExporter(); 
        }

        private void HydroModelStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            // Copy the TBL files into the dflow1d dir AFTER the HydroModel has initialized 
            // (hence the construction with the eventing). 
            if (e.NewStatus == ActivityStatus.Initializing && waterFlowModel1D != null)
            {
                string sourceDir = Path.Combine(pathDirSobek, @"..\CMTWORK");
                string targetDir = waterFlowModel1D.ExplicitWorkingDirectory;
                FileUtils.CreateDirectoryIfNotExists(targetDir, true);
                modelToRun.Sobek2CompareTest = true;
                if (targetDir != null)
                {
                    CopyTableFilesOnInitializing(sourceDir, targetDir);
                    flowTableFilesHaveBeenCopiedOnInitializing = true;
                }
            }
        }

        private void CopyTableFilesOnInitializing(string sourceDir, string targetDir)
        {
            CopyTbl(sourceDir, targetDir, gridTblPath);
            CopyTbl(sourceDir, targetDir, branchTblPath);
            CopyTbl(sourceDir, targetDir, nodeTblPath);
            CopyTbl(sourceDir, targetDir, profileTblPath);
            CopyTbl(sourceDir, targetDir, qlatTblPath);
            CopyTbl(sourceDir, targetDir, reachdownTblPath);
            CopyTbl(sourceDir, targetDir, reachupTblPath);
            CopyTbl(sourceDir, targetDir, structTblPath);
            CopyTbl(sourceDir, targetDir, structdwnTblPath);
            CopyTbl(sourceDir, targetDir, structupTblPath);

            var tblFiles = Directory.GetFiles(sourceDir, "*.tbl");
            if (tblFiles.Length == 0)
            {
                //ERROR: No *.his or *.map file comparison, check testcase cnf file to check data and location names

                //if you get this error (combined with the log statement below), add tbl files to the CMTWORK directory 
                //(they are created by SOBEK 2 during a run, copy them before closing the GUI)
                Console.WriteLine("No tbl files have been found in the CMTWORK directory. Id mappings will most likely fail.");
            }
        }

        private void CopyTbl(string sourceDirectory, string targetDirectory, string TblPath)
        {
            var tblSourcePath = Path.Combine(sourceDirectory, TblPath);
            var tblTargetPath = Path.Combine(targetDirectory, TblPath);

            if(File.Exists(tblSourcePath))
            {
                File.Copy(tblSourcePath, tblTargetPath, true);
            }
        }
        
        public string CompareDeltaShellHisWithSobek212His(string testName, string testCaseDirectory, string testDirectory, string refFilesPath)
        {
            var DebugMode = false;

            //retrieve the path where the his files are stored
            string hisFilesPath = WorkingDirHisFilesPath;

            //Create a new process info structure.
            ProcessStartInfo pInfo = new ProcessStartInfo();
            
            //Set the file name member of the process info structure (e.g. word)
	        if (File.Exists(@"c:\TCL\bin\tclsh86.exe"))
	        {
	            pInfo.FileName = @"c:\TCL\bin\tclsh86.exe";
	        }
	        else
            {
                pInfo.FileName = @"c:\TCL\bin\tclsh84.exe";
            }
            pInfo.Arguments = testCaseDirectory + @"\..\testscripts\CompareHisFiles.tcl" + @" " + testCaseDirectory + @"\..\testscripts" + @" " + testDirectory + @" " + hisFilesPath + @" " + refFilesPath;
            pInfo.UseShellExecute = DebugMode;
            pInfo.RedirectStandardError = !DebugMode;
            //Start the process.
            Process p = Process.Start(pInfo); 
            
            //Wait for the process to end, and read standard error for feedback of test.
            //The only useful information is stored in the first line, so just read that line.
            
            string error = DebugMode ? "<unknown: we're in debug mode>" : p.StandardError.ReadToEnd();
            p.WaitForExit();
            int retVal = p.ExitCode;
            if (retVal != 0)
            {
                //Obtain the error message from the log file written by TCL here
                var failMessage = string.Format("{0} -> {1}", testName, error);
                return failMessage;
            }
            return null;
        }
        
        /// <summary>
        /// tolerance of 0.01 means a range between 99% and 101%
        /// </summary>
        public double ToleranceWaterLevel { get; set; }

        /// <summary>
        /// absolute value of an error margin
        /// if the difference is lower than this value it is acceptable anyway
        /// </summary>
        public double ToleranceWaterLevelErrorMargin { get; set; }

        /// <summary>
        /// tolerance of 0.01 means a range between 99% and 101%
        /// </summary>
        public double ToleranceWaterDepth { get; set; }

        /// <summary>
        /// absolute value of an error margin
        /// if the difference is lower than this value it is acceptable anyway
        /// </summary>
        public double ToleranceWaterDepthErrorMargin { get; set; }

        /// <summary>
        /// tolerance of 0.01 means a range between 99% and 101%
        /// </summary>
        public double ToleranceWaterFlow { get; set; }

        /// <summary>
        /// absolute value of an error margin
        /// if the difference is lower than this value it is acceptable anyway
        /// </summary>
        public double ToleranceWaterFlowErrorMargin { get; set; }

        /// <summary>
        /// tolerance of 0.01 means a range between 99% and 101%
        /// </summary>
        public double ToleranceWaterVelocity { get; set; }

        /// <summary>
        /// absolute value of an error margin
        /// if the difference is lower than this value it is acceptable anyway
        /// </summary>
        public double ToleranceWaterVelocityErrorMargin { get; set; }


        public string WaterLevelReport { get; private set; }
        public string WaterDepthReport { get; private set; }
        public string WaterFlowReport { get; private set; }
        public string WaterVelocityReport { get; private set; }

        private void SetPaths(string pathDirSobekFiles)
        {
            pathDirSobek = pathDirSobekFiles;
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
            if(calcpntNetworkCoverage != null && calcpntNetworkCoverage.Store is IFileBased)
            {
                ((IFileBased)calcpntNetworkCoverage.Store).Close();
            }
            if (reachsegNetworkCoverage != null && reachsegNetworkCoverage.Store is IFileBased)
            {
                ((IFileBased)reachsegNetworkCoverage.Store).Close();
            }
            //if (strucNetworkCoverage != null && strucNetworkCoverage.Store is IFileBased)
            //{
            //    ((IFileBased)strucNetworkCoverage.Store).Close();
            //}
        }

        #endregion

        
    }
}
