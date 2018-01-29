using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture()]
    public class DimrApiTests
    {
        private readonly string dimrConfig = Path.Combine(tmpDir, "dimr.xml");
        private static readonly string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        static DimrApiTests()
        {
            Directory.CreateDirectory(tmpDir);
        }

        [Test()]
        public void TestDimrApi()
        {
            var dimrRefDate = new DateTime(1981,8,31,0,0,0);
            using (var api = new DimrApi {DimrRefDate = dimrRefDate})
            {
                Assert.AreEqual(dimrRefDate, api.StartTime);
                Assert.AreEqual(dimrRefDate, api.StopTime);
                Assert.AreEqual(TimeSpan.Zero, api.TimeStep);
                Assert.AreEqual(dimrRefDate, api.CurrentTime);
                Assert.AreEqual(1, api.Messages.Length);
                var useMessagesBuffering = (bool) TypeUtils.GetField(api, "useMessagesBuffering");
                Assert.True(useMessagesBuffering);
            }
        }

        [Test()]
        public void TestDimrApiWithOutMessageBuffering()
        {
            using (var api = new DimrApi(false))
            {
                var useMessagesBuffering = (bool)TypeUtils.GetField(api, "useMessagesBuffering");
                Assert.False(useMessagesBuffering);    
            }
            
        }

        [Test()]
        public void Testset_feedback_logger()
        {
            using (var dimrApi = new DimrApi())
            {
                try
                {
                    dimrApi.set_feedback_logger();
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }

        [Test()]
        public void Testset_logger()
        {
            using (var dimrApi = new DimrApi())
            {
                try
                {
                    dimrApi.set_logger();
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }

        [Test()]
        public void TestInitializeUpdateFinishAndGetValues()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                var exporter = new WaterFlowFMFileExporter();
                exporter.Export(model, Path.Combine(tmpDir, model.DirectoryName, model.Name + ".mdu"));
                DimrRunner.GenerateDimrXML(model, tmpDir);

                using (var dimrApi = new DimrApi())
                {

                    dimrApi.KernelDirs = model.KernelDirectoryLocation;
                    var report = model.Validate();
                    Assert.AreEqual(0, report.ErrorCount, "Errors found during model validation");
                    dimrApi.Initialize(dimrConfig);
                    TestHelper.AssertAtLeastOneLogMessagesContains(dimrApi.ProcessMessages, "Run");
                    dimrApi.Update(dimrApi.TimeStep.TotalSeconds);
                    Array waterlevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(0.0, (double) waterlevels.GetValue(0), 0.1);
                    dimrApi.SetValues(model.Name + "/s0", null);
                    waterlevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(0.0d, (double) waterlevels.GetValue(0), 0.01);
                    dimrApi.SetValuesDouble(model.Name + "/s0", null);
                    waterlevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(0.0d, (double) waterlevels.GetValue(0), 0.01);
                    var newValues = new[] {80.1d};
                    dimrApi.SetValues(model.Name + "/s0", newValues);
                    waterlevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(80.1d, (double) waterlevels.GetValue(0), 0.01);
                    dimrApi.Finish();
                }
            }
        }

        [Test()]
        public void TestMessages()
        {
            using (var dimrApi = new DimrApi())
            {
                try
                {
                    dimrApi.Initialize(dimrConfig);
                    TypeUtils.SetField(dimrApi, "messages", null);
                    Assert.False(dimrApi.Messages.Any(m => m.Contains("Running dimr in")));
                    Assert.AreEqual(1, dimrApi.Messages.Length);

                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }
    }
}