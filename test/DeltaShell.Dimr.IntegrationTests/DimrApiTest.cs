using System;
using System.IO;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Dimr.IntegrationTests
{
    [TestFixture]
    public class DimrApiTest
    {
        private readonly string dimrConfig = Path.Combine(tmpDir, "dimr.xml");
        private static readonly string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

        static DimrApiTest()
        {
            Directory.CreateDirectory(tmpDir);
        }

        [Test]
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-816
        public void GivenDimrApiWhenFinalizeThenNoExceptionThrown()
        {
            using (var dimrApi = DimrApiFactory.CreateNew())
            {
                try
                {
                    dimrApi.set_feedback_logger();
                    dimrApi.Initialize(dimrConfig);
                    dimrApi.Update(0.1d);
                    dimrApi.Finish();
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }

        [Test]
        public void GivenDimrApiWhenInitializeThenNoExceptionThrown()
        {
            using (var dimrApi = DimrApiFactory.CreateNew())
            {
                try
                {
                    dimrApi.set_feedback_logger();
                    dimrApi.Initialize(dimrConfig);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }

        [Test]
        public void GivenDimrApiWhenSetLoggerThenNoExceptionThrown()
        {
            using (var dimrApi = DimrApiFactory.CreateNew())
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
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-816
        public void GivenDimrApiWhenUpdateThenNoExceptionThrown()
        {
            using (var dimrApi = DimrApiFactory.CreateNew())
            {
                try
                {
                    dimrApi.set_feedback_logger();
                    dimrApi.Initialize(dimrConfig);
                    dimrApi.Update(0.1d);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
        }
    }
}