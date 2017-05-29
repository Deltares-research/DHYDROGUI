using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category("DIMR_Introduction")]
    [Category(TestCategory.WorkInProgress)]
    public class WaterFlowFMImportExportTest
    {

        /*
         * These are non-functional tests. It tests a dflowfm.exe that is not even used in the application. 
         * I think these should either
         * - be removed
         * - adapted: import, run1, export, import, run2. Compare run1 and run2. Run should be done with the regular
         *   runner, not dflowfm.exe. 
         */

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ModelImportExportTestHarlingen()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(localMduFilePath);

            var exportDir = "export";
            ImportExportRun(localMduFilePath, ref exportDir);
            
            var ncHisFile = Path.Combine(localMduDir, "DFM_OUTPUT_har/001_his.nc");
            var ncHisFileExported = Path.Combine(localMduDir, exportDir + "/DFM_OUTPUT_har/001_his.nc");
            AssertTimeseriesAreEqual("waterlevel", ncHisFile, ncHisFileExported, 1e-03);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ModelImportExportTestSquareGridWithWaq()
        {
            var mduPath = TestHelper.GetTestFilePath(@"square_waq\square.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(localMduFilePath);

            var exportDir = "export";
            ImportExportRun(localMduFilePath, ref exportDir);

            var waqFolder = Path.Combine(localMduDir, "DFM_DELWAQ_square");
            var waqFolderExported = Path.Combine(localMduDir, exportDir, "DFM_DELWAQ_square");
            Assert.IsTrue(Directory.Exists(waqFolder));
            Assert.IsTrue(Directory.Exists(waqFolderExported));
            Assert.IsTrue(Directory.EnumerateFiles(waqFolderExported).Any());


            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.are")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.bnd")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.flo")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.hyd")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.len")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.poi")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.srf")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.tau")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square.vol")));
            Assert.IsTrue(File.Exists(Path.Combine(waqFolderExported, "square_waqgeom.nc")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Ignore("outofmemory")]
        public void ModelImportTestDcsm()
        {
            var mduPath = TestHelper.GetTestFilePath(@"dcsm\par16.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            
            var model = new WaterFlowFMModel(localMduFilePath);

            Assert.IsNotNull(model);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ModelImportExportTestIvoorkust()
        {
            var mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(localMduFilePath);

            string exportDir = "export";
            ImportExportRun(localMduFilePath, ref exportDir);
            
            var ncHisFile = Path.Combine(localMduDir, "DFM_OUTPUT_ivk/ivk_his.nc");
            var ncHisFileExported = Path.Combine(localMduDir, exportDir + "/DFM_OUTPUT_ivk/ivk_his.nc");
            AssertTimeseriesAreEqual("waterlevel", ncHisFile, ncHisFileExported, 1e-03);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ModelImportExportTestPensioenModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(localMduFilePath);

            string exportDir = "export";
            ImportExportRun(localMduFilePath, ref exportDir);

            var ncHisFile = Path.Combine(localMduDir, "DFM_OUTPUT_pensioen/pensioen_his.nc");
            var ncHisFileExported = Path.Combine(localMduDir, exportDir + "/DFM_OUTPUT_pensioen/pensioen_his.nc");

            // Problem in dflowfm.exe with OpenMP (i.e. when OMP_NUM_THREADS != 1)
            // causes numerical differences for win32 version. Does not occur for win64
            // or when OMP_NUM_THREADS=1. Currently being worked on, until solved, I've
            // put 1e-03 below:
            AssertTimeseriesAreEqual("waterlevel", ncHisFile, ncHisFileExported, 1e-03);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ExportOutputCoverage()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(localMduFilePath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var exporter = new CoverageFileExporter();

            var exportDir = Path.Combine(localMduDir,"export");
            FileUtils.CreateDirectoryIfNotExists(exportDir, true);

            Assert.IsTrue(exporter.Export(model.OutputWaterLevel, Path.Combine(exportDir,"test.nc")));
        }

        [Test]
        public void ExportImportAssertUseTemperatureIsSetCorrectly()
        {
            var waterFlowFMModel = new WaterFlowFMModel();
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");
            const string dir = "temptest";
            Directory.CreateDirectory(dir);
            const string mduFileName = "excesstemp.mdu";
            var mduPath = Path.Combine(Path.GetFullPath(dir), mduFileName);
            waterFlowFMModel.ExportTo(mduPath);
            var importedModel = new WaterFlowFMModel(mduPath);
            Assert.IsTrue(importedModel.UseTemperature);
        }

        private static void ImportExportRun(string mduFilePath, ref string exportDir)
        {
            Assert.IsTrue(File.Exists(mduFilePath));
            
            var localMduDir = Path.GetDirectoryName(mduFilePath);
            var model = new WaterFlowFMModel(mduFilePath);
            var exportMduPath = Path.Combine(localMduDir, exportDir);

            try
            {
                if (Directory.Exists(exportMduPath))
                    Directory.Delete(exportMduPath, true);
            }
            catch (IOException)
            {
                // failed to delete.. sometimes happens on build server, let's retry once:
                exportMduPath += "2";
                exportDir += "2";
                if (Directory.Exists(exportMduPath))
                    Directory.Delete(exportMduPath, true);
            }

            Directory.CreateDirectory(exportMduPath);
            
            var exportedMduFile = Path.Combine(exportMduPath, Path.GetFileName(mduFilePath));
            model.ExportTo(exportedMduFile, false);

            // run
            RunUnstruc(mduFilePath);
            RunUnstruc(exportedMduFile);
        }

        private void AssertTimeseriesAreEqual(string variableName, string ncFileNameLeft, string ncFileNameRight, double minimumAbsError)
        {
            Assert.IsTrue(File.Exists(ncFileNameLeft),"NetCDF file not found: " + ncFileNameLeft);
            Assert.IsTrue(File.Exists(ncFileNameRight),"NetCDF file not found: " + ncFileNameRight);

            var arrayLeft = GetDataForVariable(variableName, ncFileNameLeft);
            Assert.IsNotNull(arrayLeft, "variable " + variableName + " not found in NetCDF file " + ncFileNameLeft);

            var arrayRight = GetDataForVariable(variableName, ncFileNameRight);
            Assert.IsNotNull(arrayRight, "variable " + variableName + " not found in NetCDF file " + ncFileNameRight);

            var nTimesLeft = arrayLeft.GetLength(0);
            var nValuesLeft = arrayLeft.GetLength(1);
            var nTimesRight = arrayRight.GetLength(0);
            var nValuesRight = arrayRight.GetLength(1);
            Assert.AreEqual(nTimesLeft, nTimesRight, "number of timesteps");
            Assert.AreEqual(nValuesLeft, nValuesRight, "number of values in timeseries");

            double max = 0.0;

            for (int i = 0; i < nTimesLeft; ++i)
            {
                for (int j = 0; j < nValuesLeft; ++j)
                {
                    var left = (double)arrayLeft.GetValue(i, j);
                    var right = (double)arrayRight.GetValue(i, j);

                    Assert.AreEqual(left, right, Math.Max(1.0e-07 * Math.Abs(left), minimumAbsError), string.Format("value with index {0} at time index {1}", j, i));

                    var err = Math.Abs(left - right);
                    if (err > max) max = err;
                }
            }

            Console.WriteLine("Timeseries '{0}' are equivalent, largest difference equal to {1}", variableName, max);
        }

        private Array GetDataForVariable(string varName, string ncFileName)
        {
            Array data = null;
            NetCdfFile ncFile = null;
            try
            {
                ncFile = NetCdfFile.OpenExisting(ncFileName);
                var ncVariable = ncFile.GetVariableByName(varName);

                if (ncVariable != null)
                {
                    data = ncFile.Read(ncVariable);
                }
            }
            finally
            {
                if (ncFile != null)
                {
                    ncFile.Close();
                }
            }
            return data;
        }

        private static void RunUnstruc(string localMduFile)
        {
            var unstrucBatchScript = TestHelper.GetTestFilePath(@"unstruc\dflowfm.bat");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = unstrucBatchScript,
                    Arguments = Path.GetDirectoryName(unstrucBatchScript) + " "
                                + Path.GetDirectoryName(localMduFile) + " "
                                + Path.GetFileName(localMduFile)
                }
            };
            process.Start();
            if (!process.WaitForExit(120000)) // 2 min. tops
            {
                Process.GetProcessesByName("dflowfm").ForEach(p => p.Kill());
                throw new InvalidOperationException("Took longer than 2 minutes!!");
            }
        }
    }
}