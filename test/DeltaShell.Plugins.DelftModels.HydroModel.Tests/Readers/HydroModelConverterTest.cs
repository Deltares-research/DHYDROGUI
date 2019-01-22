using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelConverterTest
    {
        private MockRepository mocks = new MockRepository();
        
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

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot convert empty dimr data object.")]
        public void GivenNullDimrXmlObject_WhenConvertingToHydroModel_ThenArgumentExceptionIsThrown()
        {
            HydroModelConverter.Convert(null, string.Empty, new List<IDimrModelFileImporter>());
        }

        [Test]
        public void GivenValidDimrXmlObject_WhenConvertingToHydroModelWithAnImporterThatDoesNotReturnAModel_ThenMessageIsLoggedAndTheReturnedModelDoesNotHaveSubModels()
        {
            var subFolder = "flow1d";
            var xmlExtension = "xml";
            var directoryPath = $@"D:\{subFolder}";
            var fileName = $"md1d.{xmlExtension}";
            var xmlFilePath = Path.Combine(directoryPath, fileName);

            // Given
            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.ImportItem(xmlFilePath)).Return(new object()).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return(xmlExtension).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.SubFolders).Return(new List<string>{subFolder}).Repeat.Any();

            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        inputFile = fileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};
            HydroModel hydroModel = null;

            // When / Then
            var expectedLogMessage = string.Format(Resources.HydroModelConverter_AddModels_Could_not_import_sub_model_defined_at_location__0__to_integrated_model_, xmlFilePath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => hydroModel = HydroModelConverter.Convert(dimrXml, directoryPath, importers),
                expectedLogMessage);

            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.IsEmpty(hydroModel.Activities, "No sub model should have been imported.");

            mocks.VerifyAll();
        }

        [Test]
        public void GivenValidDimrXmlObject_WhenConvertingToHydroModelWithAnImporterThatReturnsAModelWithADifferentName_ThenMessageIsLoggedAndTheReturnedModelDoesNotHaveSubModels()
        {
            var subFolder = "flow1d";
            var xmlExtension = "xml";
            var directoryPath = $@"D:\{subFolder}";
            var componentName = "md1d";
            var fileName = $"{componentName}.{xmlExtension}";
            var xmlFilePath = Path.Combine(directoryPath, fileName);

            // Given
            var activityInitialName = "someOtherName";
            var activity = mocks.Stub<IActivity>();
            activity.Name = activityInitialName;

            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.ImportItem(xmlFilePath)).Return(activity).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return(xmlExtension).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.SubFolders).Return(new List<string> { subFolder }).Repeat.Any();

            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        name = componentName,
                        inputFile = fileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            HydroModel hydroModel = null;
            Assert.That(activity.Name, Is.EqualTo(activityInitialName)); // Test initial state

            // When / Then
            var expectedLogMessage = string.Format(Resources.HydroModelConverter_AddModels_Renamed_model__0__to__1_, activityInitialName, componentName);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => hydroModel = HydroModelConverter.Convert(dimrXml, directoryPath, importers),
                expectedLogMessage);

            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(1));
            Assert.That(hydroModel.Activities[0].Name, Is.EqualTo(componentName), "The sub model name was not adjusted to the component name.");

            mocks.VerifyAll();
        }
    }
}
