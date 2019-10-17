using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelConverterTest
    {
        private const string SubFolder = "flow1d";
        private const string XmlExtension = "xml";
        private const string ComponentName = "md1d";
        private static readonly string DirectoryPath = $@"D:\{SubFolder}";
        private static readonly string FileName = $"{ComponentName}.{XmlExtension}";
        private readonly string xmlFilePath = Path.Combine(DirectoryPath, FileName);
        private readonly MockRepository mocks = new MockRepository();
        private ILogHandler logHandler;
        private HydroModelConverter hydroModelConverter;

        [SetUp]
        public void SetUp()
        {
            logHandler = MockRepository.GenerateMock<ILogHandler>();
            hydroModelConverter = new HydroModelConverter(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            hydroModelConverter = null;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void IfNoFileImporterIsFoundLogInfoMessage()
        {
            // Given
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>();

            var argExpectation = Arg<string>.Matches(arg => arg.StartsWith("No importer found for extension:"));
            logHandler.Expect(obj => obj.ReportInfo(argExpectation))
                .Repeat.Once();

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            hydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            // When
            hydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            // Then
            logHandler.VerifyAllExpectations();
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

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            var result = hydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Activities.Count, Is.EqualTo(1));
            Assert.That(result.Activities.ElementAt(0), Is.TypeOf<RealTimeControlModel>());
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
            Assert.DoesNotThrow(() => hydroModelConverter.Convert(dimrXml, tempFilePath, new List<IDimrModelFileImporter>()));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot convert empty dimr data object.")]
        public void GivenNullDimrXmlObject_WhenConvertingToHydroModel_ThenArgumentExceptionIsThrown()
        {
            hydroModelConverter.Convert(null, string.Empty, new List<IDimrModelFileImporter>());
        }

        [Test]
        public void GivenValidDimrXmlObject_WhenConvertingToHydroModelWithAnImporterThatDoesNotReturnAModel_ThenMessageIsLoggedAndTheReturnedModelDoesNotHaveSubModels()
        {
            // Given
            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.ImportItem(xmlFilePath)).Return(new object()).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return(XmlExtension).Repeat.Any();
           
            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        workingDir = SubFolder,
                        inputFile = FileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};

            logHandler.Expect(l => l.ReportErrorFormat(
                    Resources.HydroModelConverter_AddModels_Could_not_import_sub_model_defined_at_location__0__to_integrated_model_, xmlFilePath))
                .Repeat.Once();

            // When
            var hydroModel = hydroModelConverter.Convert(dimrXml, DirectoryPath, importers);

            // Then
            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.IsEmpty(hydroModel.Activities, "No sub model should have been imported.");

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        [Test]
        public void GivenValidDimrXmlObject_WhenConvertingToHydroModelWithAnImporterThatReturnsAnActivityWithADifferentName_ThenMessageIsLoggedAndTheReturnedModelDoesNotHaveSubModels()
        {
            // Given
            const string activityInitialName = "someOtherName";
            var dimrFileImporter = GetDimrModelFileImporter<IActivity>(activityInitialName);

            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        name = ComponentName,
                        workingDir = SubFolder,
                        inputFile = FileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            HydroModel hydroModel = null;

            // When / Then
            var expectedLogMessage = string.Format(Resources.HydroModelConverter_AddModels_Renamed_model__0__to__1_, activityInitialName, ComponentName);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => hydroModel = hydroModelConverter.Convert(dimrXml, DirectoryPath, importers),
                expectedLogMessage);

            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(1));
            Assert.That(hydroModel.Activities[0].Name, Is.EqualTo(ComponentName), "The sub model name was not adjusted to the component name.");

            mocks.VerifyAll();
        }

        [Test]
        public void GivenDimrXmlObject_WhenConvertingToHydroModelWithNoneMatchingImportedSourceOrTargetModel_ThenLogMessageIsReturned()
        {
            // Given
            var dimrFileImporter = GetDimrModelFileImporter<IDimrModel>(ComponentName);

            mocks.ReplayAll();

            var sourceComponentName = "source";
            var targetComponentName = "target";
            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        name = ComponentName,
                        workingDir = SubFolder,
                        inputFile = FileName
                    }
                },
                coupler = new []{new dimrCouplerXML
                    {
                        sourceComponent = sourceComponentName,
                        targetComponent = targetComponentName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            HydroModel hydroModel;

            logHandler.Expect(l => l.ReportErrorFormat(
                Resources.HydroModelConverter_CoupleSubModels_Could_not_couple_models____0___to___1___,
                sourceComponentName, targetComponentName)).Repeat.Once();

            // When
            hydroModel = hydroModelConverter.Convert(dimrXml, DirectoryPath, importers);

            // Then
            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(1));
            Assert.That(hydroModel.Activities[0].Name, Is.EqualTo(ComponentName));

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        [Test]
        public void GivenInValidDimrXmlObjectWithWorkingDirectoryIsNull_WhenConvertingToHydroModel_ThenReturnArgumentException()
        {
            // Given
            IDimrModelFileImporter dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return("mdu").Repeat.Any();

            string dimrPath = Path.Combine("FileReader", "dimr.xml");
            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        name = "FlowFM",
                        workingDir = null,
                        inputFile = "myFile.mdu"
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            
            // When / Then
            ArgumentException ex =
                Assert.Throws<ArgumentException>(() => hydroModelConverter.Convert(dimrXml, dimrPath, importers));
            Assert.AreEqual(string.Format(Resources.HydroModelConverter_AddModels_The_working_directory_is_missing_for_component__0__in_the_dimr_xml_,
                                          dimrXml.component[0].name), ex.Message,
                            "The exception message is different than expected"); 

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        [Test]
        public void GivenInValidDimrXmlObjectWithInputFileIsNull_WhenConvertingToHydroModel_ThenReturnArgumentException()
        {
            // Given
            IDimrModelFileImporter dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return("mdu").Repeat.Any();

            string dimrPath = Path.Combine("FileReader", "dimr.xml");

            var dimrXml = new dimrXML
            {
                component = new[] {new dimrComponentXML
                    {
                        name = "FlowFM",
                        workingDir = ".",
                        inputFile = null
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };

            mocks.ReplayAll();

            // When Then
            ArgumentException ex =
                Assert.Throws<ArgumentException>(() => hydroModelConverter.Convert(dimrXml, dimrPath, importers));
            Assert.AreEqual(string.Format(Resources.HydroModelConverter_AddModels_The_input_file_is_missing_for_component__0__in_the_dimr_xml_,
                                          dimrXml.component[0].name), ex.Message,
                            "The exception message is different than expected");

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenValidDimrXmlObjectWithXmlDirIsNullInRtcJsonFile_WhenConvertingToHydroModel_ThenRtcModelShouldNotBeImported()
        {
            // Given
            IDimrModelFileImporter dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return("json").Repeat.Any();

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };

            logHandler.Expect(l => l.ReportError(Resources.HydroModelConverter_ComposeFilePath_Could_not_import_RTC_model_the_settings_json_file_should_contain_an_xml_directory_)).Repeat.Once();
            
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            var dimrXml = new dimrXML
            {
                component = new[] 
                {new dimrComponentXML
                    {
                        name = "RTC",
                        workingDir = "IncorrectSettingsJson",
                        inputFile = "."
                    }
                }
            };

            mocks.ReplayAll();

            // When
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, dimrPath, importers);

            //Then
            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(0));

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        private IDimrModelFileImporter GetDimrModelFileImporter<T>(string subModelName) where T : IActivity
        {
            var dimrModel = mocks.Stub<IDimrModel>();
            dimrModel.Name = subModelName;

            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.ImportItem(xmlFilePath)).Return(dimrModel).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return(XmlExtension).Repeat.Any();
            
            return dimrFileImporter;
        }
    }
}
