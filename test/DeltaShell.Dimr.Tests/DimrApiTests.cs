using System;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
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