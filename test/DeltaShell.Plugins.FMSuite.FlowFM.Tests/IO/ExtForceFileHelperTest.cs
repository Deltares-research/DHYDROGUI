using System;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ExtForceFileHelperTest
    {
        // [TestCase(true, @"chezy_samples\subfolder\chezy.xyz")]
        // [TestCase(true, @"chezy_samples\chezy.xyz")]
        // [TestCase(false, @"chezy_samples\subfolder\chezy.xyz")]
        // [TestCase(false, @"chezy_samples\chezy.xyz")]
        // public void GivenASampleForcingFile_WhenWritingToAnotherDirectory_ThenTheFileShouldBeCopiedToTheSameDirectoryAsTheWrittenExtFile(bool existingExtForceFileItemFound, string relativeFilePath)
        // {
        //     // Given
        //     var samplePath = TestHelper.GetTestFilePath(relativeFilePath);
        //
        //     samplePath = TestHelper.CreateLocalCopy(samplePath);
        //     var saveDirectory = Path.Combine(Path.GetDirectoryName(samplePath), "..", "chezy_samples_saved");
        //
        //     FileUtils.DeleteIfExists(saveDirectory);
        //     Directory.CreateDirectory(saveDirectory);
        //
        //     string targetPath = Path.Combine(saveDirectory ,"chezy.ext");
        //
        //     ExtForceFileItem extForceFileItem;
        //     if (existingExtForceFileItemFound)
        //     {
        //         extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.FrictCoef)
        //         {
        //             FileName = relativeFilePath.Replace("chezy_samples",".")
        //         };
        //     }
        //     else
        //     {
        //         extForceFileItem = null;
        //     }
        //
        //     var importSamplesOperation = new ImportSamplesSpatialOperation
        //     {
        //                         FilePath = samplePath,
        //     };
        //     
        //     // When
        //     ExtForceFileItem item = ExtForceFileHelper.WriteInitialConditionsSamples(targetPath, "quantity", importSamplesOperation, extForceFileItem, true);
        //
        //     // Then
        //     Assert.AreEqual("chezy.xyz", item.FileName);
        // }
        //
        // [Test]
        // public void TestReadSourceAndSinkData()
        // {
        //     var testFilePath = TestHelper.GetTestFilePath(@"timFiles\10Columns10Values.tim");
        //
        //     // setup
        //     var feature = new Feature2D();
        //     var extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.SourceAndSink);
        //
        //     // do the import
        //     var sourceAndSink = ExtForceFileHelper.ReadSourceAndSinkData(testFilePath, feature, extForceFileItem, DateTime.Now);
        //
        //     // check results
        //     var sourceAndSinkAttributes = sourceAndSink.Feature.Attributes.Where(a => a.Key.StartsWith(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix)).ToList();
        //     Assert.AreEqual(10, sourceAndSinkAttributes.Count);
        //     Assert.True(sourceAndSinkAttributes.Select(a => a.Value).OfType<MultiDimensionalArray<double>>().All(v => v.Count == 10));
        // }
        //
        // [Test]
        // public void TestReadSourceAndSinkValues_HandlesNullFunction()
        // {
        //     var sourceAndSink = new SourceAndSink { Data = null };
        //     var arguments = new object[] { sourceAndSink, string.Empty, DateTime.MinValue };
        //     var expectedError = string.Format(Resources.Read_SourceAndSink_values_failed__no_function_detected_for_SourceAndSink__0_, sourceAndSink.Name);
        //
        //     TestHelper.AssertAtLeastOneLogMessagesContains(() =>
        //     {
        //         TypeUtils.CallPrivateStaticMethod(typeof(ExtForceFileHelper), "ReadSourceAndSinkValues", arguments );
        //     }, 
        //     expectedError);
        // }
        //
        // [TestCase(false, HeatFluxModelType.None, "NoSalinityOrTemperature.tim")]
        // [TestCase(true, HeatFluxModelType.None, "SalinityOnly.tim")]
        // [TestCase(false, HeatFluxModelType.TransportOnly, "TemperatureOnly.tim")]
        // [TestCase(true, HeatFluxModelType.TransportOnly, "BothSalinityAndTemperature.tim")]
        // public void TestWriteSourceAndSinkData(bool useSalinity, HeatFluxModelType temperature, string fileName)
        // {
        //     var expectedFile = TestHelper.GetTestFilePath(@"timFiles\" + fileName);
        //
        //     // setup
        //     var sourceAndSink = new SourceAndSink
        //     {
        //         Feature = new Feature2D
        //         {
        //             Geometry = new Point(0.0, 0.0)
        //         }
        //     };
        //
        //     var fmModel = new WaterFlowFMModel();
        //     fmModel.SourcesAndSinks.Add(sourceAndSink);
        //
        //     var modelDefinition = fmModel.ModelDefinition;
        //     var useSalinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
        //     Assert.NotNull(useSalinityProperty);
        //     useSalinityProperty.Value = useSalinity;
        //
        //     var temperatureProperty = modelDefinition.GetModelProperty(KnownProperties.Temperature) ;
        //     Assert.NotNull(temperatureProperty);
        //     temperatureProperty.SetValueFromString(((int)temperature).ToString());
        //
        //     var function = sourceAndSink.Function;
        //
        //     var timeVariable = function.Arguments.FirstOrDefault(c => c.Name == SourceAndSink.TimeVariableName);
        //     Assert.NotNull(timeVariable);
        //     var timeIndex0 = new DateTime(1984, 11, 11, 11, 11, 11, DateTimeKind.Utc);
        //     timeVariable.Values.AddRange(new List<DateTime> { timeIndex0, timeIndex0.AddMinutes(10), timeIndex0.AddMinutes(20) });
        //
        //     var dischargeVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.DischargeVariableName);
        //     Assert.NotNull(dischargeVariable);
        //     dischargeVariable.Values.Clear();
        //     dischargeVariable.Values.AddRange(new List<double> { 123.456, 234.567, 345.678 });
        //
        //     var salinityVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.SalinityVariableName);
        //     Assert.NotNull(salinityVariable);
        //     salinityVariable.Values.Clear();
        //     salinityVariable.Values.AddRange(new List<double> { 123.456, 234.567, 345.678 });
        //
        //     var temperatureVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.TemperatureVariableName);
        //     Assert.NotNull(temperatureVariable);
        //     temperatureVariable.Values.Clear();
        //     temperatureVariable.Values.AddRange(new List<double> { 123.456, 234.567, 345.678 });
        //
        //     var exportedFile = Path.Combine(FileUtils.CreateTempDirectory(), fileName);
        //     FileUtils.DeleteIfExists(exportedFile);
        //
        //     var extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.SourceAndSink)
        //     {
        //         FileName = fileName.Replace(".tim", ".pli")
        //     };
        //
        //     // do the export
        //     ExtForceFileHelper.WriteSourceAndSinkData(exportedFile, sourceAndSink, fmModel.ReferenceTime, extForceFileItem, true, modelDefinition);
        //
        //     // check results
        //     Assert.IsTrue(FileUtils.FilesAreEqual(expectedFile, exportedFile));
        //     FileUtils.DeleteIfExists(exportedFile);
        // }
        //
        // [Test]
        // public void TestWriteSourceAndSinkData_HandlesNullFunction()
        // {
        //     // setup
        //     var sourceAndSink = new SourceAndSink
        //     {
        //         Feature = new Feature2D { Geometry = new Point(0.0, 0.0) },
        //         Data = null
        //     };
        //
        //     var fmModel = new WaterFlowFMModel();
        //     fmModel.SourcesAndSinks.Add(sourceAndSink);
        //     var modelDefinition = fmModel.ModelDefinition;
        //
        //     var exportedFile = Path.Combine(FileUtils.CreateTempDirectory(), "test.tim");
        //     FileUtils.DeleteIfExists(exportedFile);
        //     var extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.SourceAndSink) { FileName = "test.pli" };
        //
        //     // do the export
        //     ExtForceFileHelper.WriteSourceAndSinkData(exportedFile, sourceAndSink, fmModel.ReferenceTime, extForceFileItem, true, modelDefinition);
        // }

        [Test]
        public void GetPliFileName_FeatureWithoutName_ReturnsNull()
        {
            // Setup
            var featureData = new SourceAndSink
            {
                Feature = new Feature2D {Geometry = new Point(0.0, 0.0)},
                Data = null
            };

            // Call
            string fileName = ExtForceFileHelper.GetPliFileName(featureData);

            // Assert
            Assert.That(fileName, Is.Null);
        }

        [Test]
        public void GetPliFileName_FeatureWithName_ReturnsExpectedName()
        {
            // Setup
            const string name = "Test";
            var featureData = new SourceAndSink
            {
                Feature = new Feature2D
                {
                    Name = name,
                    Geometry = new Point(0.0, 0.0)
                },
                Data = null
            };

            // Call
            string fileName = ExtForceFileHelper.GetPliFileName(featureData);

            // Assert
            Assert.That(fileName, Is.EqualTo($"{name}.pli"));
        }

        [Test]
        public void GetNumberedFilePath_WithZero_ReturnsExpectedFilePath()
        {
            // Call
            string filePath = ExtForceFileHelper.GetNumberedFilePath("pliFilePath.pli", "pli", 0);

            // Assert
            Assert.That(filePath, Is.EqualTo("pliFilePath.pli"));
        }

        [Test]
        public void GetNumberedFilePath_WithOne_ReturnsExpectedFilePath()
        {
            // Call
            string filePath = ExtForceFileHelper.GetNumberedFilePath("pliFilePath.pli", "pli", 1);

            // Assert
            Assert.That(filePath, Is.EqualTo("pliFilePath_0001.pli"));
        }

        [Test]
        public void GetNumberedFilePath_FilePathNull_ThrowsFormatException()
        {
            // Setup
            string pliFilePath = null;

            // Call
            void Call() => ExtForceFileHelper.GetNumberedFilePath(pliFilePath, "pli", 1);

            // Assert
            var exception = Assert.Throws<FormatException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Invalid file path {pliFilePath}"));
        }
    }
}