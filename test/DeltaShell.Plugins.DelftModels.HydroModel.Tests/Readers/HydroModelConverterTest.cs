using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
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
                                  Resources.HydroModelConverter_AddModels_Could_not_import_sub_model_defined_at_location__0__to_integrated_model_, xmlFilePath))
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
            string expectedLogMessage = string.Format(Resources.HydroModelConverter_AddModels_Renamed_model__0__to__1_, activityInitialName, ComponentName);
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
            Assert.IsTrue(hydroModel.Region.SubRegions.IsNullOrEmpty());
        }

        [Test]
        public void Convert_SetsTheCorrectTimesOnTheHydroModel()
        {
            // Setup
            const string masterFileExtension = "some_extension";
            const string timeElementValue = "86400 1200 172800";

            IDimrModel model = CreateDimrModel(DateTime.Today);

            IDimrModelFileImporter dimrImporter = CreateDimrImporter(masterFileExtension, model);

            dimrXML dimrXml = CreateDimrXml(masterFileExtension, timeElementValue);

            var loggingHandler = Substitute.For<ILogHandler>();
            var converter = new HydroModelConverter(loggingHandler);

            // Call
            HydroModel hydroModel = converter.Convert(dimrXml, "path/to/the/dimr/config.xml", new List<IDimrModelFileImporter> {dimrImporter});

            // Assert
            Assert.That(hydroModel.StartTime, Is.EqualTo(DateTime.Today.AddSeconds(86400)));
            Assert.That(hydroModel.TimeStep, Is.EqualTo(TimeSpan.FromSeconds(1200)));
            Assert.That(hydroModel.StopTime, Is.EqualTo(DateTime.Today.AddSeconds(172800)));

            Assert.That(loggingHandler.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void Convert_ImportedModelNotADimrModel_LogsError()
        {
            // Setup
            const string masterFileExtension = "some_extension";
            const string timeElementValue = "86400 1200 172800";

            var model = Substitute.For<IActivity>();
            model.Name = "some_name";

            IDimrModelFileImporter dimrImporter = CreateDimrImporter(masterFileExtension, model);

            dimrXML dimrXml = CreateDimrXml(masterFileExtension, timeElementValue);

            var loggingHandler = Substitute.For<ILogHandler>();
            var converter = new HydroModelConverter(loggingHandler);

            // Call
            converter.Convert(dimrXml, "path/to/the/dimr/config.xml", new List<IDimrModelFileImporter> {dimrImporter});

            // Assert
            Assert.That(loggingHandler.ReceivedCalls(), Has.Length.EqualTo(1));
            loggingHandler.Received(1).ReportErrorFormat("The imported model '{0}' is not a dimr model.", model.Name);
        }

        [Test]
        [TestCaseSource(nameof(Convert_MissingElement_Cases))]
        public void Convert_MissingElement_HandlesErrorCorrectly(object[] controlElement, Action<ILogHandler> assertAction)
        {
            // Setup
            const string masterFileExtension = "some_extension";

            IDimrModel model = CreateDimrModel(DateTime.Today);

            IDimrModelFileImporter dimrImporter = CreateDimrImporter(masterFileExtension, model);

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        workingDir = "some_dir",
                        inputFile = $"model_file.{masterFileExtension}"
                    }
                },
                control = controlElement
            };

            var loggingHandler = Substitute.For<ILogHandler>();
            var converter = new HydroModelConverter(loggingHandler);

            // Call
            converter.Convert(dimrXml, "path/to/the/dimr/config.xml", new List<IDimrModelFileImporter> {dimrImporter});

            // Assert
            assertAction.Invoke(loggingHandler);
        }

        private static IEnumerable<TestCaseData> Convert_MissingElement_Cases()
        {
            void ValidateReportsMissingStartGroup(ILogHandler logHandlerMock)
            {
                const string expectedError = "The <startGroup> element is missing from the dimr config.";
                Assert.That(logHandlerMock.ReceivedCalls(), Has.Length.EqualTo(1));
                logHandlerMock.Received(1).ReportError(expectedError);

            }
            yield return new TestCaseData(
                new object[] { new dimrParallelXML {Items = new object[0]} },
                (Action<ILogHandler>) ValidateReportsMissingStartGroup);

            void ValidateReportsNoErrorWhenMissingParallelElement(ILogHandler logHandlerMock) =>
                logHandlerMock.DidNotReceiveWithAnyArgs().ReportError("");
            yield return new TestCaseData(
                new object[0],
                (Action<ILogHandler>) ValidateReportsNoErrorWhenMissingParallelElement);
        }

        private static IDimrModel CreateDimrModel(DateTime startTime)
        {
            IDimrModel model = Substitute.For<IDimrModel, IActivity>();
            model.IsMasterTimeStep.Returns(true);
            model.StartTime.Returns(startTime);
            return model;
        }

        private static IDimrModelFileImporter CreateDimrImporter(string masterFileExtension, IActivity model)
        {
            var dimrImporter = Substitute.For<IDimrModelFileImporter>();
            dimrImporter.MasterFileExtension.Returns(masterFileExtension);
            dimrImporter.ImportItem("").ReturnsForAnyArgs(model);
            return dimrImporter;
        }

        private static dimrXML CreateDimrXml(string extension, string time)
        {
            return new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        workingDir = "some_dir",
                        inputFile = $"model_file.{extension}"
                    }
                },
                control = new object[]
                {
                    new dimrParallelXML
                    {
                        Items = new object[]
                        {
                            new dimrStartGroupXML {time = time}
                        }
                    }
                },
            };
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
    }
}