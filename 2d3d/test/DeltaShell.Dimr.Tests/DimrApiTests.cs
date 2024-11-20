using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrApiTests
    {
        private string dimrConfig;
        private static readonly string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

        [SetUp]
        public void SetUp()
        {
            dimrConfig = Path.Combine(tmpDir, "dimr.xml");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(dimrConfig);
        }

        [Test]
        public void TestDimrApi()
        {
            var dimrRefDate = new DateTime(1981, 8, 31, 0, 0, 0);
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

        [Test]
        public void TestDimrApiWithoutMessageBuffering()
        {
            using (var api = new DimrApi(false))
            {
                var useMessagesBuffering = (bool) TypeUtils.GetField(api, "useMessagesBuffering");
                Assert.False(useMessagesBuffering);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestInitializeUpdateFinishAndGetValues()
        {
            string mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(localCopy);

                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                model.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();

                var exporter = new FMModelFileExporter();
                string exporterPath = model.GetExporterPath(Path.Combine(tmpDir, model.DirectoryName));
                exporter.Export(model, exporterPath);
                DimrRunner.GenerateDimrXML(model, tmpDir);

                using (var dimrApi = new DimrApi())
                {
                    dimrApi.KernelDirs = model.KernelDirectoryLocation;
                    ValidationReport report = model.Validate();
                    Assert.AreEqual(0, report.ErrorCount, "Errors found during model validation");
                    dimrApi.Initialize(dimrConfig);
                    TestHelper.AssertAtLeastOneLogMessagesContains(dimrApi.ProcessMessages, "Run");
                    dimrApi.Update(dimrApi.TimeStep.TotalSeconds);
                    Array waterLevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(0.0, (double) waterLevels.GetValue(0), 0.1);
                    dimrApi.SetValues(model.Name + "/s0", null);
                    waterLevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(0.0d, (double) waterLevels.GetValue(0), 0.01);
                    dimrApi.SetValuesDouble(model.Name + "/s0", null);
                    waterLevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(0.0d, (double) waterLevels.GetValue(0), 0.01);
                    var newValues = new[]
                    {
                        80.1d
                    };
                    dimrApi.SetValues(model.Name + "/s0", newValues);
                    waterLevels = dimrApi.GetValues(model.Name + "/s0");
                    Assert.AreEqual(80.1d, (double) waterLevels.GetValue(0), 0.01);
                    dimrApi.Finish();
                }
            }
        }

        [Test]
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

        [Test]
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

        [Test]
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

        static DimrApiTests()
        {
            Directory.CreateDirectory(tmpDir);
        }
    }
}