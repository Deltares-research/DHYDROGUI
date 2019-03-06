using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections.Writer
{
    [TestFixture]
    public class CrossSectionDefinitionFileWriterTest
    {
        private CrossSectionDefinitionFileWriter writer;
        private CrossSectionDefinitionFileConverter converter;
        private IniFileWriter iniFileWriter;
        private IEnumerable<DelftIniCategory> categories;

        [SetUp]
        public void Setup()
        {
            converter = MockRepository.GeneratePartialMock<CrossSectionDefinitionFileConverter>();
            iniFileWriter = MockRepository.GenerateMock<IniFileWriter>();
            writer = new CrossSectionDefinitionFileWriter(converter,iniFileWriter);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenACrossSectionDefinitionFileWriter_WhenWritingAWaterFlow1DModelToFile_ThenConverterAndWriterAreCalled()
        {
            //Given
            var testDirectory = Path.Combine("ImportExport", "CrossSections", "Writer");
            var file = TestHelper.GetTestFilePath(Path.Combine(testDirectory, "test.txt"));
            using (File.Create(Path.Combine(file))) { }
            var flowModel = CreateSimpleWaterFlowModel1DWithEmptyCrossSectionCategories();

            //When
            writer.WriteFile(file, flowModel);

            //Then
            Assert.False(File.Exists(file));
            converter.VerifyAllExpectations();
            iniFileWriter.AssertWasCalled(i => i.WriteIniFile(categories, file));
        }

        private WaterFlowModel1D CreateSimpleWaterFlowModel1DWithEmptyCrossSectionCategories()
        {
            var mockedModel = MockRepository.GeneratePartialMock<WaterFlowModel1D>();
            categories = MockRepository.GenerateMock<IEnumerable<DelftIniCategory>>();
            converter.Expect(c => converter.Convert(mockedModel)).Return(categories);

            return mockedModel;
        }
    }
}
