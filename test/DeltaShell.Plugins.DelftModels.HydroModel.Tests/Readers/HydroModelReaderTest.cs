using System.IO;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelReaderTest
    {
        private IFileImportService fileImportService;

        [SetUp]
        public void SetUp()
        {
            fileImportService = Substitute.For<IFileImportService>();
        }

        [Test]
        public void ConstructEmptyHydroModel()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            HydroModelReader reader = CreateReader();

            HydroModel hydroModel = reader.Read(dimrPath);

            Assert.NotNull(hydroModel);
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(0));
        }

        private HydroModelReader CreateReader()
        {
            return new HydroModelReader(fileImportService);
        }
    }
}