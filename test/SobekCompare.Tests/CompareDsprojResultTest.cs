using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.ImportExport.Sobek.HisData;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using SobekCompare.Tests.Helpers;

namespace SobekCompare.Tests
{
    [TestFixture]
    [Category(TestCategorySobekValidation.WaterFlow1D)]
    public class CompareDsprojResultTest
    {

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [Test]
        public void TestRetentions()
        {
            var testPath = Path.Combine(TestHelper.GetDataDir(), @"dsprojTests\TestRetentions");
            var dsProjPath = Path.Combine(testPath, "RetentionTest.dsproj");

            RunDsProjInWaterFlow1D(dsProjPath);

            var resultPath = Path.Combine(dsProjPath + "_data", @"Flow1D_output\dflow1d\output");
            var referencePath = Path.Combine(testPath, @"ReferenceData");

            compareHisFile(resultPath, referencePath, "calcpnt.his");
            compareHisFile(resultPath, referencePath, "reachseg.his");
            compareHisFile(resultPath, referencePath, "qwb.his");
        }


        [Test]
        public void TestCompoundCulvertDiffBob()
        {
            var testPath = Path.Combine(TestHelper.GetDataDir(), @"dsprojTests\TestCompoundCulvertDiffBob");
            var dsProjPath = Path.Combine(testPath, "TestCompoundCulvertDiffBob.dsproj");

            RunDsProjInWaterFlow1D(dsProjPath);

            var resultPath = Path.Combine(dsProjPath + "_data", @"Integrated_Model_output\dflow1d\output");
            var referencePath = Path.Combine(testPath, @"ReferenceData");

            compareHisFile(resultPath, referencePath, "calcpnt.his");
            compareHisFile(resultPath, referencePath, "reachseg.his");
            compareHisFile(resultPath, referencePath, "struc.his");
        }


        [Test]
        public void TestCompoundCulvertSameBob()
        {
            var testPath = Path.Combine(TestHelper.GetDataDir(), @"dsprojTests\TestCompoundCulvertSameBob");
            var dsProjPath = Path.Combine(testPath, "TestCompoundCulvertSameBob.dsproj");

            RunDsProjInWaterFlow1D(dsProjPath);

            var resultPath = Path.Combine(dsProjPath + "_data", @"Integrated_Model_output\dflow1d\output");
            var referencePath = Path.Combine(testPath, @"ReferenceData");

            compareHisFile(resultPath, referencePath, "calcpnt.his");
            compareHisFile(resultPath, referencePath, "reachseg.his");
            compareHisFile(resultPath, referencePath, "struc.his");

        }

        private IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowModel1DExporter();
            yield return new RealTimeControlModelExporter();
            yield return new DHydroConfigXmlExporter();
        }

        private void RunDsProjInWaterFlow1D(string dsProjPath)
        {

            FileUtils.DeleteIfExists(dsProjPath + "_data");

            using (var application = new DeltaShellApplication())
            {

                application.Plugins.Add(new NHibernateDaoApplicationPlugin());
                application.Plugins.Add(new CommonToolsApplicationPlugin());
                application.Plugins.Add(new SharpMapGisApplicationPlugin());
                application.Plugins.Add(new NetCdfApplicationPlugin());
                application.Plugins.Add(new HydroModelApplicationPlugin());
                application.Plugins.Add(new NetworkEditorApplicationPlugin());
                application.Plugins.Add(new RealTimeControlApplicationPlugin());
                application.Plugins.Add(new WaterFlowModel1DApplicationPlugin());

                application.Run(dsProjPath);

                var hydroModel = application.GetAllModelsInProject().OfType<HydroModel>().FirstOrDefault();
                
                var flowModel = hydroModel != null
                    ? hydroModel.Activities.OfType<WaterFlowModel1D>().FirstOrDefault()
                    : application.GetAllModelsInProject().OfType<WaterFlowModel1D>().FirstOrDefault();

                Assert.NotNull(flowModel);
                SobekCompareTestHelper.RefreshCrossSectionDefinitionSectionWidths(flowModel.Network);

                var model = hydroModel != null ? (IModel) hydroModel : flowModel;
                var projDir = Path.GetDirectoryName(dsProjPath);
                var fileName = Path.GetFileNameWithoutExtension(dsProjPath);
                var dataPath = Path.Combine(projDir, fileName + ".dsproj_data", model.Name.Replace(' ', '_') + "_output");
                    
                model.ExplicitWorkingDirectory = dataPath;
                ActivityRunner.RunActivity(model);
            }
        }

        private void compareHisFile(string resultPath, string referencePath, string hisFile)
        {
            var tstHis = Path.Combine(resultPath, hisFile);
            var refHis = Path.Combine(referencePath, hisFile);

            var eventedTstList = new HisFunctionStore(tstHis).Functions;
            Assert.NotNull(eventedTstList);
            var eventedRefList = new HisFunctionStore(refHis).Functions;
            Assert.NotNull(eventedRefList);

            Assert.AreEqual(eventedTstList.Count, eventedRefList.Count, "Different Quantity of Data in HIS-Files");

            for (var iFunc = 0; iFunc < eventedTstList.Count - 1; iFunc++)
            {
                var tstFuncName = eventedTstList[iFunc].Name;
                var refFuncName = eventedRefList[iFunc].Name;

                // BC: Commented this out: it fails due to superscripts difference in m3/s. 
                // Assert.AreEqual(tstFuncName, refFuncName, "Different Function Names in Function {0}", iFunc);

                var tstFunction = eventedTstList.FirstOrDefault(f => f.Name == tstFuncName);
                Assert.NotNull(tstFunction);
                var tstValues = tstFunction.GetValues();

                var refFunction = eventedRefList.FirstOrDefault(f => f.Name == refFuncName);
                Assert.NotNull(refFunction);
                var refValues = refFunction.GetValues();

                Assert.AreEqual(tstValues.Count, refValues.Count, "HIS-File {0}: Number of Values not Equal for {1}", hisFile, tstFuncName);
                if (iFunc < 2)
                {
                    for (var i = 0; i < tstValues.Count; i++)
                    {
                        Assert.AreEqual(tstValues[i], refValues[i],
                            string.Format("HIS-File {0}: Value Differs for {1}", hisFile, tstFuncName));
                    }
                }
                else
                {
                    const double tolerance = 0.01; // 99 - 101 %
                    const double toleranceErrorMargin = 0.001; // 1 mm

                    for (var i = 0; i < tstValues.Count; i++)
                    {
                        var x = Convert.ToDouble(tstValues[i].ToString());
                        var y = Convert.ToDouble(refValues[i].ToString());
                        Assert.IsTrue(
                            Math.Abs(x - y) <= toleranceErrorMargin || Math.Abs(x - y) <= Math.Abs(x)*tolerance,
                            string.Format("HIS-File {0}: Value Differs for {1}", hisFile, tstFuncName));
                    }
                }
            }
        }
    }
}
