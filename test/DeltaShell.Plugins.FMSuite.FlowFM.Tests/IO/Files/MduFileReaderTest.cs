using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using log4net.Core;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class MduFileReaderTest
    {
        [Test]
        public void Read_KnownProperty_ThenPropertySortIndexHasChanged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "Program = MyProgram # Program name";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                Assert.AreEqual(2, property.PropertyDefinition.SortIndex);
            });
        }
        
        [Test]
        public void Read_KnownPropertyNonDefaultValue_ThenPropertyValueHasChanged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "Program = MyProgram # Program name";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_CommentLinesInContent_ThenCommentLinesAreSkippedAndReadingIsExecutedWithoutProblems()
        {
            string fileContent = "# Generated on 2019-10-22 14:02:32"
                                 + Environment.NewLine
                                 + "# Deltares, Delft3D FM 2018 Suite Version 1.6.0.0, D-Flow FM Version 1.2.63.64757M"
                                 + Environment.NewLine
                                 + "[General]"
                                 + Environment.NewLine
                                 + "Program = MyProgram # Program name"
                                 + Environment.NewLine
                                 + "# Another comment line here that should be ignored while reading.";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_KnownPropertyNonDefaultComment_ThenPropertyCommentHasNotChanged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "Program = D-Flow FM # MyDescription";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "D-Flow FM", "Program name");
            });
        }

        [Test]
        public void Read_KnownPropertyInLowerCase_ThenPropertyValueHasChanged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "program = MyProgram # Program name";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_CustomProperty_ThenNewPropertyIsAddedToModelDefinition()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "MyCustomProperty = MyValue # MyDescription";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "General", "MyValue", "MyDescription");
            });
        }
        
        [Test]
        public void Read_CustomProperty_ThenInfoMessageIsLogged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "MyCustomProperty = MyValue # MyDescription";

            var filePath = "FlowFM.mdu";

            string expectedMessage = $"During reading the {filePath} file the following infos were reported:"
                                     + Environment.NewLine
                                     + "- An unrecognized keyword *MyCustomProperty* has been detected. The setting will be preserved and written back to the MDU file on export.";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var definition = new WaterFlowFMModelDefinition();

            void Call() => MduFileReader.Read(stream, filePath, definition);
            
            TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage);
        }
        
        [Test]
        public void Read_CustomProperty_ThenPropertySortIndexHasChanged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "MyCustomProperty = MyValue # MyDescription";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                Assert.AreEqual(2, property.PropertyDefinition.SortIndex);
            });
        }

        [Test]
        public void Read_LegacyCategory_ThenCategoryNameIsUpdated()
        {
            string fileContent = "[Model]"
                                 + Environment.NewLine
                                 + "Program = D-Flow FM # Program name"
                                 + Environment.NewLine
                                 + "MyProperty = MyValue # MyComment";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty knownProperty = definition.GetModelProperty("Program");
                AssertPropertyValues(knownProperty, "General", "D-Flow FM", "Program name");

                WaterFlowFMProperty customProperty = definition.GetModelProperty("MyProperty");
                AssertPropertyValues(customProperty, "General", "MyValue", "MyComment");
            });
        }

        [Test]
        public void Read_KnownPropertyWithoutComment_ThenNewPropertyIsAddedToModelDefinitionWithPredefinedComment()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "Program = D-Flow FM";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "D-Flow FM", "Program name");
            });
        }

        [Test]
        public void Read_CustomPropertyWithoutComment_ThenNewPropertyIsAddedToModelDefinitionWithNullComment()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "MyCustomProperty = MyValue";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "General", "MyValue", string.Empty);
            });
        }

        [Test]
        public void Read_KnownPropertyEmptyValue_ThenPropertyValueIsNotChanged()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "Program =   # Program name";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "D-Flow FM", "Program name");
            });
        }

        [Test]
        public void Read_CustomPropertyEmptyValue_ThenNewPropertyValueIsAddedWithEmptyValue()
        {
            string fileContent = "[General]"
                                 + Environment.NewLine
                                 + "MyCustomProperty =   # MyDescription";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "General", string.Empty, "MyDescription");
            });
        }

        [Test]
        [TestCaseSource(nameof(GetMultiValuedPropertiesFileContents))]
        public void Read_MultiValuedProperty_ThenNewPropertyValueIsAddedWithMultipleValues(string fileContent)
        {
            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("DryPointsFile");
                AssertPropertyValues(property, "geometry", new List<string>
                                     {
                                         "myPoints1_dry.pol",
                                         "myPoints2_dry.pol"
                                     },
                                     "Dry points file *.xyz (third column dummy z values), or dry areas polygon file *.pol (third column 1/-1: inside/outside)");
            });
        }

        [Test]
        public void Read_KnownCategoryLowerCase_ThenPropertyIsAddedToKnownCategory()
        {
            string fileContent = "[general]"
                                 + Environment.NewLine
                                 + "Program = MyProgram # Program name";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_CustomCategoryCustomProperty_ThenNewCategoryWithNewPropertyIsAdded()
        {
            string fileContent = "[MyCustomCategory]"
                                 + Environment.NewLine
                                 + "MyCustomProperty = MyValue # MyComment";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "MyCustomCategory", "MyValue", "MyComment");
            });
        }

        [Test]
        public void Read_CustomCategoryKnownProperty_ThenPropertyIsAddedToKnownCategoryAndCustomCategoryIsLost()
        {
            string fileContent = "[MyCustomCategory]"
                                 + Environment.NewLine
                                 + "Program = MyProgram # Program name";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");

                Assert.IsEmpty(definition.Properties.Where(p => p.PropertyDefinition.FileSectionName == "MyCustomCategory"));
            });
        }

        [Test]
        public void Read_HdamPropertyInFile_ThenPropertyIsNotAddedToModelDefinition()
        {
            string fileContent = "[numerics]"
                                 + Environment.NewLine
                                 + "Hdam = 0. # Threshold for minimum bottomlevel step at which to apply energy conservation factor i.c. flow contraction";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("hdam");
                Assert.IsNull(property);
            });
        }

        [Test]
        public void Read_LegacyEnclosureFileProperty_ThenPropertyNameIsUpdated()
        {
            string fileContent = "[geometry]"
                                 + Environment.NewLine
                                 + "EnclosureFile = enclosures_enc.pol # MyComment";

            ReadWithAssert(fileContent, definition =>
            {
                Assert.IsNull(definition.GetModelProperty("EnclosureFile"));

                WaterFlowFMProperty property = definition.GetModelProperty("GridEnclosureFile");
                Assert.That(property.PropertyDefinition.FileSectionName, Is.EqualTo("geometry"));
                Assert.That(property.Value, Is.EqualTo(new List<string> {"enclosures_enc.pol"}));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Enclosure polygon file *.pol (third column 1/-1: inside/outside)"));
            });
        }

        [Test]
        public void Read_InvalidFixedWeirSchemeValue_ThenPropertyValueIsSetTo6()
        {
            string fileContent = "[numerics]"
                                 + Environment.NewLine
                                 + "FixedWeirScheme = 222 # Fixed weir scheme (0: None, 6: Numerical, 8: Tabellenboek, 9: Villemonte)";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("FixedWeirScheme");
                Assert.That(property.GetValueAsString(), Is.EqualTo("9"));
            });
        }

        [Test]
        public void Read_BadFormatForMduCategory_ThrowsFormatException()
        {
            // Setup
            const string tempFilePath = @"C:/myPath";
            string fileContent = "[]"
                                 + Environment.NewLine
                                 + "Program = D-Flow FM # Program name";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var definition = new WaterFlowFMModelDefinition();

            // Call
            void Call() => MduFileReader.Read(stream, tempFilePath, definition);

            // Assert
            var exception = Assert.Throws<FormatException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Invalid group on line 1 in file {tempFilePath}"));
        }

        [Test]
        public void Read_MultiplePropertyValuesOutOfRange_ThenWarningMessageIsLogged()
        {
            // Setup
            string fileContent = "[physics]"
                                 + Environment.NewLine
                                 + "UnifFrictType = 3000 # Uniform friction type (0: Chezy, 1: Manning, 2: White-Colebrook)"
                                 + Environment.NewLine
                                 + Environment.NewLine
                                 + "[numerics]"
                                 + Environment.NewLine
                                 + "Turbulencemodel = 5 # Turbulence model (0: none, 1: constant, 2: algebraic, 3: k-epsilon, 4: k-tau)";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var definition = new WaterFlowFMModelDefinition();
            var filePath = "FlowFM.mdu";

            // Call
            void Call() => MduFileReader.Read(stream, filePath, definition);

            // Assert
            string expectedMessage = $"During reading the {filePath} file the following warnings were reported:"
                                     + Environment.NewLine
                                     + "- An unsupported option for *Uniform friction type* has been detected and the default value will be used."
                                     + Environment.NewLine
                                     + "- An unsupported option for *Turbulence model* has been detected and the default value will be used.";
            TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("transportmethod", "numerics")]
        [TestCase("transporttimestepping", "numerics")]
        [TestCase("hdam", "numerics")]
        [TestCase("writebalancefile", "output")]
        public void Read_WithObsoleteProperty_PropertyIsRemovedFromModelDefinition(string property, string category)
        {
            // Setup
            string mduFileContent =
                $"[{category}]{Environment.NewLine}" +
                $"{property}=";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(mduFileContent));
            var definition = new WaterFlowFMModelDefinition();
            var mduFilePath = "FlowFM.mdu";

            // Call
            MduFileReader.Read(stream, mduFilePath, definition);

            // Assert
            Assert.That(definition.Properties.Select(p => p.PropertyDefinition.MduPropertyName), Does.Not.Contain(property));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("transportmethod", "numerics")]
        [TestCase("hdam", "numerics")]
        [TestCase("writebalancefile", "output")]
        public void Read_WithObsoleteProperty_LogsWarning(string property, string category)
        {
            // Setup
            string mduFileContent =
                $"[{category}]{Environment.NewLine}" +
                $"{property}=";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(mduFileContent));
            var definition = new WaterFlowFMModelDefinition();
            var mduFilePath = "FlowFM.mdu";

            // Call
            void Call() => MduFileReader.Read(stream, mduFilePath, definition);
            string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();

            // Assert
            string expectedWarning =
                $"During reading the {mduFilePath} file the following warnings were reported:" +
                Environment.NewLine +
                $"- Key {property} is deprecated and automatically removed from model.";
            Assert.That(warnings, Does.Contain(expectedWarning));
        }

        private static IEnumerable<string> GetMultiValuedPropertiesFileContents()
        {
            yield return "[Geometry]"
                         + Environment.NewLine
                         + "DryPointsFile = myPoints1_dry.pol myPoints2_dry.pol # Dry points files";

            yield return "[Geometry]"
                         + Environment.NewLine
                         + @"DryPointsFile = myPoints1_dry.pol \"
                         + Environment.NewLine
                         + "myPoints2_dry.pol # Dry points files";
        }

        [Test]
        public void Read_MapIntervalIsReadCorrectly()
        {
            string fileContent = "[output]"
                                 + Environment.NewLine
                                 + "MapInterval = 9.6";

            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty(GuiProperties.MapOutputDeltaT);
                Assert.That(property.Value, Is.InstanceOf<TimeSpan>());
                var time = (TimeSpan) property.Value;

                Assert.That(time.Seconds, Is.EqualTo(9));
                Assert.That(time.Milliseconds, Is.EqualTo(600));
            });
        }
        
        [Test]
        public void GivenMduFileContentWithStartDateTimeAndStopDateTime_CorrectlyReadsTheseKeywords()
        {
            // Setup
            string fileContent = "[time]"                           + Environment.NewLine
                                 + "StartDateTime = 20230622123000" + Environment.NewLine
                                 + "StopDateTime = 20230623000000";
            
            ReadWithAssert(fileContent, definition =>
            {
                var startDateTime = (DateTime)definition.GetModelProperty(KnownProperties.StartDateTime).Value;
                var stopDateTime = (DateTime)definition.GetModelProperty(KnownProperties.StopDateTime).Value;
                
                var expectedStartDateTime = new DateTime(2023, 06, 22, 12, 30, 0);
                Assert.That(startDateTime, Is.EqualTo(expectedStartDateTime));
                
                var expectedStopDateTime = new DateTime(2023, 06, 23, 0, 0, 0);
                Assert.That(stopDateTime, Is.EqualTo(expectedStopDateTime));
            });

        }

        [Test]
        public void GivenMduFileContentWithOnlyOldKeywordsForComputationStartTime_WhenReading_ConvertsToNewKeywords()
        {
            string fileContent = "[time]"                     + Environment.NewLine
                                 + "RefDate = 20230622000000" + Environment.NewLine
                                 + "TStart = 1"               + Environment.NewLine // old keyword
                                 + "TStop = 2"                + Environment.NewLine // old keyword
                                 + "TUnit = S";
            ReadWithAssert(fileContent, definition =>
            {
                var startDateTime = (DateTime)definition.GetModelProperty(KnownProperties.StartDateTime).Value; // new keyword
                var stopDateTime = (DateTime)definition.GetModelProperty(KnownProperties.StopDateTime).Value; // new keyword
                
                var expectedStartDateTime = new DateTime(2023, 06, 22, 0, 0, 1); // ref date + 1s
                Assert.That(startDateTime, Is.EqualTo(expectedStartDateTime));
                
                var expectedStopDateTime = new DateTime(2023, 06, 22, 0, 0, 2); // ref date + 2s
                Assert.That(stopDateTime, Is.EqualTo(expectedStopDateTime));

            });
        }

        [Test]
        public void GivenMduFileContentWithBothOldAndNewKeywordsForComputationStartTime_WhenReading_NewKeywordsAreUsed()
        {
            string fileContent = "[time]"                           + Environment.NewLine
                                 + "RefDate = 19900718"             + Environment.NewLine
                                 + "TStart = 1"                     + Environment.NewLine // old keyword
                                 + "TStop = 2"                      + Environment.NewLine // old keyword
                                 + "TUnit = S"                      + Environment.NewLine
                                 + "StartDateTime = 20230622123000" + Environment.NewLine // new keyword
                                 + "StopDateTime = 20230623000000";                       // new keyword
            ReadWithAssert(fileContent, definition =>
            {
                var startDateTime = (DateTime)definition.GetModelProperty(KnownProperties.StartDateTime).Value; // new keyword
                var stopDateTime = (DateTime)definition.GetModelProperty(KnownProperties.StopDateTime).Value;   // new keyword
                
                var expectedStartDateTime = new DateTime(2023, 06, 22, 12, 30, 0);
                Assert.That(startDateTime, Is.EqualTo(expectedStartDateTime));
                
                var expectedStopDateTime = new DateTime(2023, 06, 23, 0, 0, 0);
                Assert.That(stopDateTime, Is.EqualTo(expectedStopDateTime));

            });
        }
        
        [Test]
        public void GivenMduFileContentWithBothOldAndNewKeywordsForComputationStartTime_WhenReading_RemovesOldKeywords()
        {
            string fileContent = "[time]"                           + Environment.NewLine
                                                                    + "RefDate = 19900718"             + Environment.NewLine
                                                                    + "TStart = 1"                     + Environment.NewLine // old keyword
                                                                    + "TStop = 2"                      + Environment.NewLine // old keyword
                                                                    + "TUnit = S"                      + Environment.NewLine
                                                                    + "StartDateTime = 20230622123000" + Environment.NewLine // new keyword
                                                                    + "StopDateTime = 20230623000000";                       // new keyword
            ReadWithAssert(fileContent, definition =>
            {
                WaterFlowFMProperty tStart = definition.GetModelProperty(KnownLegacyProperties.TStart);
                Assert.That(tStart, Is.Null);

                WaterFlowFMProperty tStop = definition.GetModelProperty(KnownLegacyProperties.TStop);
                Assert.That(tStop, Is.Null);

            });
        }

        private static void AssertPropertyValues(WaterFlowFMProperty property, string categoryName, object propertyValue, string propertyComment)
        {
            Assert.That(property.PropertyDefinition.FileSectionName, Is.EqualTo(categoryName));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.PropertyDefinition.Description, Is.EqualTo(propertyComment));
        }

        private static void ReadWithAssert(string fileContent, Action<WaterFlowFMModelDefinition> assertAction)
        {
            // Setup
            var definition = new WaterFlowFMModelDefinition();
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));

            // Call
            MduFileReader.Read(stream, string.Empty, definition);

            // Assert
            assertAction(definition);
        }
    }
}