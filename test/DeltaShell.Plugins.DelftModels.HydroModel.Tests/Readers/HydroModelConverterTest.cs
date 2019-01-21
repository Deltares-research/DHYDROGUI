using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelConverterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void IfNoFileImporterIsFoundLogInfoMessage()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>();

            var dimrObject = DelftConfigXmlFileParser.Read<dimrXML>(dimrPath);
            HydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => HydroModelConverter.Convert(dimrObject, dimrPath, fileImporters), "No importer found for extension:");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConvertFlow1DModelAndAddToHydroModel()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>
            {
                new WaterFlowModel1DFileImporter()
            };

            var dimrObject = DelftConfigXmlFileParser.Read<dimrXML>(dimrPath);
            var result = HydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Activities.Count, Is.EqualTo(1));
            Assert.That(result.Activities.ElementAt(0), Is.TypeOf<WaterFlowModel1D>());
            Assert.That(result.Activities.Any(), Is.Not.TypeOf<RealTimeControlModel>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConvertRtcModelAndAddToHydroModel()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>
            {
                new RealTimeControlModelImporter()
            };

            var dimrObject = DelftConfigXmlFileParser.Read<dimrXML>(dimrPath);
            var result = HydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Activities.Count, Is.EqualTo(1));
            Assert.That(result.Activities.ElementAt(0), Is.TypeOf<RealTimeControlModel>());
            Assert.That(result.Activities.Any(), Is.Not.TypeOf<WaterFlowModel1D>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ConvertFlow1DAndRtcModelAndAddToHydroModel()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>
            {
                new RealTimeControlModelImporter(),
                new WaterFlowModel1DFileImporter()
            };

            var dimrObject = DelftConfigXmlFileParser.Read<dimrXML>(dimrPath);
            var result = HydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);
            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Activities.Count, Is.EqualTo(2));
            Assert.That(result.Activities.ElementAt(0), Is.TypeOf<RealTimeControlModel>());
            Assert.That(result.Activities.ElementAt(1), Is.TypeOf<WaterFlowModel1D>());
        }

        [Test]
        public void GivenDimrXmlObjectWithNullCoupler_WhenConvertingToHydroModel_ThenNoExceptionIsThrown()
        {
            // Given
            var tempFilePath = Path.Combine("myDirectory", "myFile.here");
            var dimrXml = new dimrXML
            {
                component = new dimrComponentXML[] { }
            };
            Assert.IsNull(dimrXml.coupler); // Test initial requirement

            // When/Then
            Assert.DoesNotThrow(() => HydroModelConverter.Convert(dimrXml, tempFilePath, new List<IDimrModelFileImporter>()));
        }
    }
}
