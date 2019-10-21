using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class MduFileReaderTest
    {
        [Test]
        public void Read_KnownPropertyNonDefaultPropertyValue_ThenPropertyValueHasChanged_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownPropertyNonDefaultValue.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_KnownPropertyNonDefaultComment_ThenPropertyCommentHasNotChanged_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownPropertyNonDefaultComment.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "D-Flow FM", "Program name");
            });
        }

        [Test]
        public void Read_KnownPropertyInLowerCase_ThenPropertyValueHasChanged_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownPropertyLowerCase.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_CustomProperty_ThenNewPropertyIsAddedToModelDefinition_PlusCheckNewReadResult()
        {
            ReadWithAssert("CustomProperty.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "General", "MyValue", "MyDescription");
            });
        }

        [Test]
        public void Read_LegacyCategory_ThenCategoryNameIsUpdated_PlusCheckNewReadResult()
        {
            ReadWithAssert("LegacyCategory.mdu", definition =>
            {
                WaterFlowFMProperty knownProperty = definition.GetModelProperty("Program");
                AssertPropertyValues(knownProperty, "General", "D-Flow FM", "Program name");

                WaterFlowFMProperty customProperty = definition.GetModelProperty("MyProperty");
                AssertPropertyValues(customProperty, "General", "MyValue", "MyComment");
            });
        }

        [Test]
        public void Read_KnownPropertyWithoutComment_ThenNewPropertyIsAddedToModelDefinitionWithPredefinedComment_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownPropertyWithoutComment.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "D-Flow FM", "Program name");
            });
        }

        [Test]
        public void Read_CustomPropertyWithoutComment_ThenNewPropertyIsAddedToModelDefinitionWithNullComment_PlusCheckNewReadResult()
        {
            ReadWithAssert("CustomPropertyWithoutComment.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "General", "MyValue", null);
            });
        }

        [Test]
        public void Read_KnownPropertyEmptyValue_ThenPropertyValueIsNotChanged_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownPropertyEmptyValue.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "D-Flow FM", "Program name");
            });
        }

        [Test]
        public void Read_CustomPropertyEmptyValue_ThenNewPropertyValueIsAddedWithEmptyValue_PlusCheckNewReadResult()
        {
            ReadWithAssert("CustomPropertyEmptyValue.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "General", string.Empty, "MyDescription");
            });
        }

        [Test]
        public void Read_EmptyPropertyName_ThenNewPropertyValueIsAddedWithEmptyValue_PlusCheckNewReadResult()
        {
            ReadWithAssert("MultipleValuedProperty.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("DryPointsFile");
                AssertPropertyValues(property, "geometry", new List<string> { "myPoints1_dry.pol", "myPoints2_dry.pol" }, 
                                     "Dry points file *.xyz (third column dummy z values), or dry areas polygon file *.pol (third column 1/-1: inside/outside)");
            });
        }

        [Test]
        public void Read_KnownCategoryLowerCase_ThenPropertyIsAddedToKnownCategory_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownCategoryLowerCase.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");
            });
        }

        [Test]
        public void Read_CustomCategoryCustomProperty_ThenNewCategoryWithNewPropertyIsAdded_PlusCheckNewReadResult()
        {
            ReadWithAssert("CustomCategoryCustomProperty.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                AssertPropertyValues(property, "MyCustomCategory", "MyValue", "MyComment");
            });
        }

        [Test]
        public void Read_CustomCategoryKnownProperty_ThenPropertyIsAddedToKnownCategoryAndCustomCategoryIsLost_PlusCheckNewReadResult()
        {
            ReadWithAssert("CustomCategoryKnownProperty.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                AssertPropertyValues(property, "General", "MyProgram", "Program name");

                Assert.IsEmpty(definition.Properties.Where(p => p.PropertyDefinition.FileCategoryName == "MyCustomCategory"));
            });
        }

        private static void AssertPropertyValues(WaterFlowFMProperty property, string categoryName, string propertyValue, string propertyComment)
        {
            Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo(categoryName));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.PropertyDefinition.Description, Is.EqualTo(propertyComment));
        }

        private static void AssertPropertyValues(WaterFlowFMProperty property, string categoryName, List<string> propertyValue, string propertyComment)
        {
            Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo(categoryName));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.PropertyDefinition.Description, Is.EqualTo(propertyComment));
        }

        [Test]
        public void Read_HdamPropertyInFile_ThenPropertyIsNotAddedToModelDefinition_PlusCheckNewReadResult()
        {
            ReadWithAssert("HdamPropertyInFile.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("hdam");
                Assert.IsNull(property);
            });
        }

        [Test]
        public void Read_LegacyEnclosureFileProperty_ThenPropertyNameIsUpdated_PlusCheckNewReadResult()
        {
            ReadWithAssert("LegacyProperty_EnclosureFile.mdu", definition =>
            {
                Assert.IsNull(definition.GetModelProperty("EnclosureFile"));

                WaterFlowFMProperty property = definition.GetModelProperty("GridEnclosureFile");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("geometry"));
                Assert.That(property.Value, Is.EqualTo(new List<string>
                {
                    "enclosures_enc.pol"
                }));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Enclosure polygon file *.pol (third column 1/-1: inside/outside)"));
            });
        }

        [Test]
        public void Read_InvalidFixedWeirSchemeValue_ThenPropertyValueIsSetTo6_PlusCheckNewReadResult()
        {
            ReadWithAssert("InvalidFixedWeirSchemeValue.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("FixedWeirScheme");
                Assert.That(property.GetValueAsString(), Is.EqualTo("6"));
            });
        }

        [Test]
        public void Read_InvalidFixedWeirSchemeValue_ThenWarningMessageIsLogged()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "InvalidFixedWeirSchemeValue.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                void Call() => MduFileReader.Read(tempFilePath, oldDefinition);

                // Assert
                const string expectedMessage = "Obsolete Fixed Weir Scheme 222 detected and it will be corrected to the default Numerical Scheme.";
                TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage);
            }
        }

        private static void ReadWithAssert(string fileName, Action<WaterFlowFMModelDefinition> assertAction)
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", fileName));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                MduFileReader.Read(tempFilePath, oldDefinition);

                // Assert
                assertAction(oldDefinition);
            }
        }

        [Test]
        public void Read_BadFormatForMduCategory_ThrowsFormatException()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "BadFormatForMduCategory.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                void Call() => MduFileReader.Read(tempFilePath, oldDefinition);

                // Assert
                var exception = Assert.Throws<FormatException>(Call);
                Assert.That(exception.Message, Is.EqualTo($"Invalid group on line {4} in file {tempFilePath}"));
            }
        }

        [Test]
        public void Read_MultiplePropertyValuesOutOfRange_ThenWarningMessageIsLogged()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "MultiplePropertyValuesOutOfRange.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                void Call() => MduFileReader.Read(tempFilePath, oldDefinition);

                // Assert
                string expectedMessage = "During reading the mdu file the following warnings were reported:"
                                         + Environment.NewLine
                                         + "- An unsupported option for *Uniform friction type* has been detected and the default value will be used."
                                         + Environment.NewLine
                                         + "- An unsupported option for *Turbulence model* has been detected and the default value will be used.";
                TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage);
            }
        }
    }
}