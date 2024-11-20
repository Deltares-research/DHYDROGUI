using System;
using System.IO;
using NUnit.Framework;

namespace DeltaShell.Dimr.IntegrationTests
{
    [TestFixture]
    public class DimrApiTest
    {
        private readonly string dimrConfig = Path.Combine(tmpDir, "dimr.xml");
        private static readonly string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

        [Test]
        public void GivenDimrApiWhenInitializeThenNoExceptionThrown()
        {
            using (IDimrApi dimrApi = CreateDimrApi())
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
            using (IDimrApi dimrApi = CreateDimrApi())
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

        private static IDimrApi CreateDimrApi()
        {
            return new DimrApiFactory().CreateNew();
        }

        static DimrApiTest()
        {
            Directory.CreateDirectory(tmpDir);
        }
    }
}