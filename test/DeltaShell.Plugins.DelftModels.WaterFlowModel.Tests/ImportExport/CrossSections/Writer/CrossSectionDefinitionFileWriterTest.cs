using System.Collections.Generic;
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
        private string filePath = "testPath";

        [SetUp]
        public void Setup()
        {
            converter = MockRepository.GenerateMock<CrossSectionDefinitionFileConverter>();
            iniFileWriter = MockRepository.GenerateMock<IniFileWriter>();
            writer = new CrossSectionDefinitionFileWriter(converter,iniFileWriter);
        }

        [Test]
        public void GivenACrossSectionDefinitionFileWriter_WhenWritingAWaterFlow1DModelToFile_ThenConverterAndWriterAreCalled()
        {
            var mockedModel = MockRepository.GeneratePartialMock<WaterFlowModel1D>();
            var categories = new List<DelftIniCategory>(){new DelftIniCategory("crossSections")};
            converter.Expect(c => c.Convert(mockedModel)).Return(categories);

            writer.WriteFile(filePath, mockedModel);

            converter.VerifyAllExpectations();
            iniFileWriter.AssertWasCalled(i => i.WriteIniFile(categories, filePath));
        }
    }
}
