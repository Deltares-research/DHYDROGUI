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
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Directory.CreateDirectory(tmpDir);
        }
        
        [Test]
		public void GivenDimrApiWhenSetLoggerThenNoExceptionThrown()
		{
            using (var dimrApi = new DimrApiFactory().CreateNew())
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
        public void GivenDimrApiWhenInitializeThenNoExceptionThrown()
        {
            using (var dimrApi = new DimrApiFactory().CreateNew())
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
    }
}