using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DHYDRO.Common.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelConverterTest
    {
        private const string subFolder = "flow1d";
        private const string xmlExtension = "xml";
        private const string componentName = "md1d";

        private static readonly string directoryPath = $@"D:\{subFolder}";
        private static readonly string fileName = $"{componentName}.{xmlExtension}";
        private static readonly string xmlFilePath = Path.Combine(directoryPath, fileName);

        private ILogHandler logHandler;
        private IFileImportService fileImportService;
        private HydroModelConverter hydroModelConverter;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            fileImportService = Substitute.For<IFileImportService>();

            hydroModelConverter = new HydroModelConverter(logHandler, fileImportService);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void IfNoFileImporterIsFoundLogInfoMessage()
        {
            // Given
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            hydroModelConverter.Convert(dimrObject, dimrPath);

            // When
            hydroModelConverter.Convert(dimrObject, dimrPath);

            // Then
            logHandler.Received().ReportError(Arg.Is<string>(x => x.StartsWith("No importer found for input file:")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConvertRtcModelAndAddToHydroModel()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var fileImporters = new List<IDimrModelFileImporter> { CreateRtcModelImporter() };
            fileImportService.FileImporters.Returns(fileImporters);

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            HydroModel result = hydroModelConverter.Convert(dimrObject, dimrPath);

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
            string workingDir = TestHelper.GetTestFilePath(nameof(HydroModelConverterTest));

            var fileImporters = new List<IDimrModelFileImporter>
            {
                CreateRtcModelImporter(),
                CreateFmModelImporter(workingDir)
            };

            fileImportService.FileImporters.Returns(fileImporters);

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dimrObject = delftConfigXmlParser.Read<dimrXML>(dimrPath);
            HydroModel result = hydroModelConverter.Convert(dimrObject, dimrPath);

            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<HydroModel>());
            Assert.That(result.Models.OfType<RealTimeControlModel>().Single().ControlGroups.SelectMany(cg => cg.Inputs).All(input => input.Name == "ObservationPoint01_water_level"));
        }

        [Test]
        public void GivenDimrXmlObjectWithNullCoupler_WhenConvertingToHydroModel_ThenNoExceptionIsThrown()
        {
            // Given
            string tempFilePath = Path.Combine("myDirectory", "myFile.here");
            var dimrXml = new dimrXML { component = new dimrComponentXML[] {} };
            Assert.IsNull(dimrXml.coupler); // Test initial requirement

            // When/Then
            Assert.DoesNotThrow(() => hydroModelConverter.Convert(dimrXml, tempFilePath));
        }

        [Test]
        public void GivenNullDimrXmlObject_WhenConvertingToHydroModel_ThenArgumentExceptionIsThrown()
        {
            Assert.That(() => hydroModelConverter.Convert(null, string.Empty),
                        Throws.InstanceOf<ArgumentException>().With.Message.EqualTo("Cannot convert empty dimr data object."));
        }

        [Test]
        public void GivenValidDimrXmlObject_WhenConvertingToHydroModelWithAnImporterThatDoesNotReturnAModel_ThenMessageIsLoggedAndTheReturnedModelDoesNotHaveSubModels()
        {
            // Given
            var dimrFileImporter = Substitute.For<IDimrModelFileImporter>();
            dimrFileImporter.ImportItem(Arg.Any<string>()).Returns(new object());
            dimrFileImporter.CanImportDimrFile(Arg.Any<string>()).Returns(true);

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        workingDir = subFolder,
                        inputFile = fileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            fileImportService.FileImporters.Returns(importers);

            // When
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, directoryPath);

            // Then
            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.IsEmpty(hydroModel.Activities, "No sub model should have been imported.");

            logHandler.Received().ReportErrorFormat(Resources.HydroModelConverter_AddModels_Could_not_import_sub_model_defined_at_location__0__to_integrated_model_, xmlFilePath);
        }

        [Test]
        public void GivenValidDimrXmlObject_WhenConvertingToHydroModelWithAnImporterThatReturnsAnActivityWithADifferentName_ThenMessageIsLoggedAndTheReturnedModelDoesNotHaveSubModels()
        {
            // Given
            const string activityInitialName = "someOtherName";
            IDimrModelFileImporter dimrFileImporter = GetDimrModelFileImporter(activityInitialName);

            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = componentName,
                        workingDir = subFolder,
                        inputFile = fileName
                    }
                }
            };

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            fileImportService.FileImporters.Returns(importers);

            HydroModel hydroModel = null;

            // When / Then
            string expectedLogMessage = string.Format(Resources.HydroModelConverter_AddModels_Renamed_model__0__to__1_, activityInitialName, componentName);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => hydroModel = hydroModelConverter.Convert(dimrXml, directoryPath), expectedLogMessage);

            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(1));
            Assert.That(hydroModel.Activities[0].Name, Is.EqualTo(componentName), "The sub model name was not adjusted to the component name.");
        }

        [Test]
        public void GivenDimrXmlObject_WhenConvertingToHydroModelWithNoneMatchingImportedSourceOrTargetModel_ThenLogMessageIsReturned()
        {
            // Given
            IDimrModelFileImporter dimrFileImporter = GetDimrModelFileImporter(componentName);

            var sourceComponentName = "source";
            var targetComponentName = "target";
            var dimrXml = new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        name = componentName,
                        workingDir = subFolder,
                        inputFile = fileName
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

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            fileImportService.FileImporters.Returns(importers);

            // When
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, directoryPath);

            // Then
            Assert.IsNotNull(hydroModel, "The returned model was expected to be not null.");
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(1));
            Assert.That(hydroModel.Activities[0].Name, Is.EqualTo(componentName));

            logHandler.Received().ReportErrorFormat(
                Resources.HydroModelConverter_CoupleSubModels_Could_not_couple_models____0___to___1___,
                sourceComponentName, targetComponentName);
        }

        [Test]
        public void GivenInValidDimrXmlObjectWithWorkingDirectoryIsNull_WhenConvertingToHydroModel_ThenReturnArgumentException()
        {
            // Given
            var dimrFileImporter = Substitute.For<IDimrModelFileImporter>();
            dimrFileImporter.CanImportDimrFile(Arg.Any<string>()).Returns(true);

            string dimrPath = Path.Combine("FileReader", "dimr.xml");

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

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            fileImportService.FileImporters.Returns(importers);

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => hydroModelConverter.Convert(dimrXml, dimrPath));
            Assert.AreEqual("The working directory is missing for component FlowFM in the dimr xml.", ex.Message,
                            "The exception message is different than expected");
        }

        [Test]
        public void GivenInValidDimrXmlObjectWithInputFileIsNull_WhenConvertingToHydroModel_ThenReturnArgumentException()
        {
            // Given
            var dimrFileImporter = Substitute.For<IDimrModelFileImporter>();
            dimrFileImporter.CanImportDimrFile(Arg.Any<string>()).Returns(true);

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

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            fileImportService.FileImporters.Returns(importers);

            // When Then
            var ex = Assert.Throws<ArgumentException>(() => hydroModelConverter.Convert(dimrXml, dimrPath));
            Assert.AreEqual("The input file is missing for component FlowFM in the dimr xml.", ex.Message,
                            "The exception message is different than expected");
        }

        [Test]
        public void ConvertingDimrXmlOfAHydroModelWithSubModelRegionIsNull_ThenTheHydroModelShouldBeReturnedAndRegionOfSubModelIsNotAddedToRegionOfHydroModel()
        {
            // Given
            IHydroModel modelWithNullRegion = Substitute.For<IHydroModel, IDimrModel>();
            modelWithNullRegion.Region.ReturnsNull();

            var dimrFileImporter = Substitute.For<IDimrModelFileImporter>();
            dimrFileImporter.CanImportDimrFile(Arg.Any<string>()).Returns(true);
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

            var importers = new List<IDimrModelFileImporter> { dimrFileImporter };
            fileImportService.FileImporters.Returns(importers);

            // When 
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, dimrPath);

            // Then
            Assert.IsTrue(hydroModel.Models.Contains(modelWithNullRegion));
            Assert.IsFalse(hydroModel.Region.SubRegions.Any());
        }

        [Test]
        public void Convert_SetsTheTimesFromTheMasterModelOnTheHydroModel()
        {
            // Setup
            const string timeElementValue = "86400 1200 172800";

            var startTime = new DateTime(2023, 6, 22);
            var timeStep = new TimeSpan(0, 1, 0);
            DateTime stopTime = startTime.AddDays(1);
            IDimrModel model = CreateDimrModel(startTime, timeStep, stopTime);

            var dimrImporter = Substitute.For<IDimrModelFileImporter>();
            dimrImporter.CanImportDimrFile(Arg.Any<string>()).Returns(true);
            dimrImporter.ImportItem("").ReturnsForAnyArgs(model);
            
            fileImportService.FileImporters.Returns(new[] { dimrImporter });
            
            dimrXML dimrXml = CreateDimrXml(timeElementValue);

            // Call
            HydroModel hydroModel = hydroModelConverter.Convert(dimrXml, "path/to/the/dimr/config.xml");

            // Assert
            Assert.That(hydroModel.StartTime, Is.EqualTo(startTime));
            Assert.That(hydroModel.TimeStep, Is.EqualTo(timeStep));
            Assert.That(hydroModel.StopTime, Is.EqualTo(stopTime));
        }

        private static IDimrModel CreateDimrModel(DateTime startTime, TimeSpan timeStep, DateTime stopTime)
        {
            IDimrModel model = Substitute.For<IDimrModel, IActivity>();
            model.IsMasterTimeStep.Returns(true);
            model.StartTime.Returns(startTime);
            model.TimeStep.Returns(timeStep);
            model.StopTime.Returns(stopTime);
            return model;
        }

        private static dimrXML CreateDimrXml(string time)
        {
            return new dimrXML
            {
                component = new[]
                {
                    new dimrComponentXML
                    {
                        workingDir = "some_dir",
                        inputFile = "model_file.extension"
                    }
                },
                control = new object[] { new dimrParallelXML { Items = new object[] { new dimrStartGroupXML { time = time } } } },
            };
        }

        private IDimrModelFileImporter GetDimrModelFileImporter(string subModelName)
        {
            var dimrModel = Substitute.For<IDimrModel>();
            dimrModel.Name.Returns(subModelName);

            var dimrFileImporter = Substitute.For<IDimrModelFileImporter>();
            dimrFileImporter.ImportItem(xmlFilePath).Returns(dimrModel);
            dimrFileImporter.CanImportDimrFile(Arg.Is<string>(x => x.EndsWith(xmlExtension))).Returns(true);

            return dimrFileImporter;
        }

        private static RealTimeControlModelImporter CreateRtcModelImporter()
        {
            return new RealTimeControlModelImporter { XmlReaders = { new RealTimeControlModelXmlReader() } };
        }

        private static WaterFlowFMFileImporter CreateFmModelImporter(string workingDir)
        {
            return new WaterFlowFMFileImporter(() => workingDir);
        }
    }
}