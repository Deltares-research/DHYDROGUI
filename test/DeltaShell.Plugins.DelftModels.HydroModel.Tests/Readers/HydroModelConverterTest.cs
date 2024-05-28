using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DHYDRO.Common.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Rhino.Mocks;
using Arg = NSubstitute.Arg;

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
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>();

            string argExpectation = Arg<string>.Matches(arg => arg.StartsWith("No importer found for extension:"));
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
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter> {new RealTimeControlModelImporter()};

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            HydroModel result = hydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Activities.Count, Is.EqualTo(1));
            Assert.That(result.Activities.ElementAt(0), Is.TypeOf<RealTimeControlModel>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRtcModelWithMultipleInputsConnectedToTheSameParameter_WhenImported_ThenAllInputsCorrectlyLinked()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine(nameof(HydroModelConverterTest), "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter>
            {
                new RealTimeControlModelImporter(),
                new WaterFlowFMFileImporter(() => TestHelper.GetTestFilePath(nameof(HydroModelConverterTest)))
            };

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            HydroModel result = hydroModelConverter.Convert(dimrObject, dimrPath, fileImporters);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Models.OfType<RealTimeControlModel>().Single().ControlGroups.SelectMany(cg => cg.Inputs).All(input => input.Name == "ObservationPoint01_water_level"));
        }

        [Test]
        public void GivenDimrXmlObjectWithNullCoupler_WhenConvertingToHydroModel_ThenNoExceptionIsThrown()
        {
            // Given
            string tempFilePath = Path.Combine("myDirectory", "myFile.here");
            var dimrXml = new dimrXML
            {
                component = new dimrComponentXML[]
                    {}
            };
            Assert.IsNull(dimrXml.coupler); // Test initial requirement

            // When/Then
            Assert.DoesNotThrow(() => hydroModelConverter.Convert(dimrXml, tempFilePath, new List<IDimrModelFileImporter>()));
        }

        [Test]
        public void GivenNullDimrXmlObject_WhenConvertingToHydroModel_ThenArgumentExceptionIsThrown()
        {
            Assert.That(() => hydroModelConverter.Convert(null, string.Empty, new List<IDimrModelFileImporter>()),
                        Throws.InstanceOf<ArgumentException>().With.Message.EqualTo("Cannot convert empty dimr data object."));
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
                component = new[]
                {
                    new dimrComponentXML
                    {
                        workingDir = SubFolder,
                        inputFile = FileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};

            logHandler.Expect(l => l.ReportErrorFormat(
                                  "Could not import sub model defined at location {0} to integrated model.", xmlFilePath))
                      .Repeat.Once();

            // When
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, DirectoryPath, importers);

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
            IDimrModelFileImporter dimrFileImporter = GetDimrModelFileImporter(activityInitialName);

            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = ComponentName,
                        workingDir = SubFolder,
                        inputFile = FileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};
            HydroModel hydroModel = null;

            // When / Then
            string expectedLogMessage = string.Format("Renamed model {0} to {1}.", activityInitialName, ComponentName);
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
            IDimrModelFileImporter dimrFileImporter = GetDimrModelFileImporter(ComponentName);

            mocks.ReplayAll();

            var sourceComponentName = "source";
            var targetComponentName = "target";
            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = ComponentName,
                        workingDir = SubFolder,
                        inputFile = FileName
                    }
                },
                coupler = new[]
                {
                    new dimrCouplerXML
                    {
                        sourceComponent = sourceComponentName,
                        targetComponent = targetComponentName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};
            HydroModel hydroModel;

            logHandler.Expect(l => l.ReportErrorFormat(
                                  "Could not couple models: \'{0}\' to \'{1}\'.",
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
            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return("mdu").Repeat.Any();

            string dimrPath = Path.Combine("FileReader", "dimr.xml");
            mocks.ReplayAll();

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = "FlowFM",
                        workingDir = null,
                        inputFile = "myFile.mdu"
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};

            // When / Then
            var ex =
                Assert.Throws<ArgumentException>(() => hydroModelConverter.Convert(dimrXml, dimrPath, importers));
            Assert.AreEqual("The working directory is missing for component FlowFM in the dimr xml.", ex.Message,
                            "The exception message is different than expected");

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        [Test]
        public void GivenInValidDimrXmlObjectWithInputFileIsNull_WhenConvertingToHydroModel_ThenReturnArgumentException()
        {
            // Given
            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return("mdu").Repeat.Any();

            string dimrPath = Path.Combine("FileReader", "dimr.xml");

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = "FlowFM",
                        workingDir = ".",
                        inputFile = null
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};

            mocks.ReplayAll();

            // When Then
            var ex =
                Assert.Throws<ArgumentException>(() => hydroModelConverter.Convert(dimrXml, dimrPath, importers));
            Assert.AreEqual("The input file is missing for component FlowFM in the dimr xml.", ex.Message,
                            "The exception message is different than expected");

            mocks.VerifyAll();
            logHandler.VerifyAllExpectations();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenValidDimrXmlObjectWithXmlDirIsNullInRtcJsonFile_WhenConvertingToHydroModel_ThenRtcModelShouldNotBeImported()
        {
            // Given
            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return("json").Repeat.Any();

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};

            logHandler.Expect(l => l.ReportError("Could not import RTC model, the settings.json file should contain an xml directory.")).Repeat.Once();

            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
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

        [Test]
        public void ConvertingDimrXmlOfAHydroModelWithSubModelRegionIsNull_ThenTheHydroModelShouldBeReturnedAndRegionOfSubModelIsNotAddedToRegionOfHydroModel()
        {
            // Given
            var modelWithNullRegion = Substitute.For<IHydroModel>();
            modelWithNullRegion.Region.ReturnsNull();

            var dimrFileImporter = Substitute.For<IDimrModelFileImporter>();
            dimrFileImporter.MasterFileExtension.Returns("extension");
            dimrFileImporter.ImportItem(Arg.Any<string>()).Returns(modelWithNullRegion);

            string dimrPath = Path.Combine("FileReader", "dimr.xml");

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = "ModelWithRegionIsNull",
                        workingDir = ".",
                        inputFile = "ModelWithRegionIsNull.extension"
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> {dimrFileImporter};

            // When 
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, dimrPath, importers);

            // Then
            Assert.IsTrue(hydroModel.Models.Contains(modelWithNullRegion));
            Assert.IsFalse(hydroModel.Region.SubRegions.Any());
        }

        private IDimrModelFileImporter GetDimrModelFileImporter(string subModelName)
        {
            var dimrModel = mocks.Stub<IDimrModel>();
            dimrModel.Name = subModelName;

            var dimrFileImporter = mocks.DynamicMock<IDimrModelFileImporter>();
            dimrFileImporter.Expect(importer => importer.ImportItem(xmlFilePath)).Return(dimrModel).Repeat.Any();
            dimrFileImporter.Expect(importer => importer.MasterFileExtension).Return(XmlExtension).Repeat.Any();

            return dimrFileImporter;
        }

        [Test]
        public void GivenHydroModelConverter_Convert_ShouldBeRecreatingLinksWithoutClearingOutput()
        {
            //Arrange
            var model1 = Substitute.For<IDimrModel>();
            var model2 = Substitute.For<IDimrModel>();
            
            var coupling1 = Substitute.For<IHydroCoupling>();
            var coupling2 = Substitute.For<IHydroCoupling>();

            var hydroObject1 = Substitute.For<IHydroObject>();
            var hydroObject2 = Substitute.For<IHydroObject>();

            model1.DimrCoupling.Returns(coupling1);
            model2.DimrCoupling.Returns(coupling2);

            coupling1.GetLinkHydroObjectsByItemString("m1_feature").Returns(new []{ hydroObject1 });
            coupling2.GetLinkHydroObjectsByItemString("m2_feature").Returns(new []{ hydroObject2 });

            var dimrModelImporter1 = Substitute.For<IDimrModelFileImporter>();
            var dimrModelImporter2 = Substitute.For<IDimrModelFileImporter>();

            dimrModelImporter1.Name.Returns("Model 1 importer");
            dimrModelImporter1.MasterFileExtension.Returns("ini");

            dimrModelImporter2.Name.Returns("Model 1 importer");
            dimrModelImporter2.MasterFileExtension.Returns("txt");
            
            dimrModelImporter1.ImportItem(Arg.Any<string>()).Returns(model1);
            dimrModelImporter2.ImportItem(Arg.Any<string>()).Returns(model2);

            string dimrPath = Path.Combine("FileReader", "dimr.xml");

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = "Model1",
                        workingDir = ".",
                        inputFile = "temp.ini"
                    },
                    new dimrComponentXML
                    {
                        name = "Model2",
                        workingDir = ".",
                        inputFile = "temp.txt"
                    }
                },
                coupler = new []
                {
                    new dimrCouplerXML
                    {
                        sourceComponent = "Model1",
                        targetComponent = "Model2",
                        name = "test",
                        item = new []
                        {
                            new dimrCoupledItemXML
                            {
                                sourceName = "m1_feature",
                                targetName= "m2_feature"
                            }
                        }
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrModelImporter1, dimrModelImporter2 };

            // Act
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, dimrPath, importers);


            // Assert
            model1.DimrCoupling.Received(1).CreateLink(hydroObject1, hydroObject2);
            var susp = model1.Received(1).SuspendClearOutputOnInputChange;
            model1.SuspendClearOutputOnInputChange = true;
            model1.SuspendClearOutputOnInputChange = false;

            susp = model2.Received(1).SuspendClearOutputOnInputChange;
            model2.SuspendClearOutputOnInputChange = true;
            model2.SuspendClearOutputOnInputChange = false;
        }
    }
}