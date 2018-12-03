using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.FileReaders;
using NUnit.Framework;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelConverterTest
    {
        [Test]
        public void IfNoFileImporterIsFoundLogInfoMessage()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>();
         
            var dimrObject = DelftConfigXmlFileParser.Read(dimrPath);
            var converter = new HydroModelConverter();
            converter.Convert(dimrObject, dimrPath, fileImporters);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => converter.Convert(dimrObject, dimrPath, fileImporters), "No importer found for extension:");
        }

        [Test]
        public void ConvertFlow1DModelAndAddToHydroModel()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>()
            {
                new WaterFlowModel1DFileImporter()
            };
         
            var dimrObject = DelftConfigXmlFileParser.Read(dimrPath);
            var converter = new HydroModelConverter();
            var result =  converter.Convert(dimrObject, dimrPath, fileImporters);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Activities.Count, Is.EqualTo(2));
            Assert.That(result.Activities.ElementAt(0), Is.TypeOf<WaterFlowModel1D>());
        }

    }
}
