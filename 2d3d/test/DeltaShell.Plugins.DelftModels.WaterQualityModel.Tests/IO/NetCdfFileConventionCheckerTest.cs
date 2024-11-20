using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class NetCdfFileConventionCheckerTest
    {
        [Category(TestCategory.DataAccess)]
        [Test]
        public void HasSupportedConvention_WhenFileDoesNotExist_ThenThrowsFileNotFoundException()
        {
            // Set-up
            const string filePath = "no_exist";

            // Pre-condition
            Assert.That(!File.Exists(filePath));

            // Action
            void TestAction()
            {
                NetCdfFileConventionChecker.HasSupportedConvention(filePath);
            }

            // Then
            Assert.That(TestAction, Throws.TypeOf<FileNotFoundException>());
        }

        [Category(TestCategory.DataAccess)]
        [TestCase("CF1.5_UGRID0.9.nc", false)]
        [TestCase("CF1.5_UGRID1.0.nc", false)]
        [TestCase("CF1.5_UGRID1.1.nc", false)]
        [TestCase("CF1.6_UGRID0.9.nc", false)]
        [TestCase("CF1.6_UGRID1.0.nc", true)]
        [TestCase("CF1.6_UGRID1.1.nc", true)]
        [TestCase("CF1.7_UGRID0.9.nc", false)]
        [TestCase("CF1.7_UGRID1.0.nc", true)]
        [TestCase("CF1.7_UGRID1.1.nc", true)]
        [TestCase("no_convention_attribute.nc", false)]
        [TestCase("no_cf_convention.nc", false)]
        [TestCase("no_ugrid_convention.nc", false)]
        public void HasSupportedConvention_WithExistingFiles_ThenCorrectResultIsReturned(
            string fileName, bool expectedResult)
        {
            // Set-up
            string filePath = TestHelper.GetTestFilePath($@"IO\NetCDFConventions\{fileName}");

            // Pre-condition
            Assert.That(File.Exists(filePath));

            // Action
            bool result = NetCdfFileConventionChecker.HasSupportedConvention(filePath);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}