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
        public void Read_InvalidFixedWeirSchemeValue_ThenWarningMessageIsLogged_PlusCheckNewReadResult()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "InvalidFixedWeirSchemeValue.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                void Call() => new MduFileReader().Read(tempFilePath, oldDefinition);
                void NewCall() => NewMduFileReader.Read(tempFilePath, oldDefinition);

                // Assert
                const string expectedMessage = "Obsolete Fixed Weir Scheme 222 detected and it will be corrected to the default Numerical Scheme.";
                TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage);
                TestHelper.AssertLogMessageIsGenerated(NewCall, expectedMessage);
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
                new MduFileReader().Read(tempFilePath, oldDefinition);

                // Assert
                assertAction(oldDefinition);

                // Equal result as new reader
                var newDefinition = new WaterFlowFMModelDefinition();
                NewMduFileReader.Read(tempFilePath, newDefinition);
                Assert.That(Equals(oldDefinition, newDefinition));
            }
        }

        [Test]
        public void Read_BadFormatForMduCategory_ThrowsFormatException_PlusCheckNewReadResult()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "BadFormatForMduCategory.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                void Call() => new MduFileReader().Read(tempFilePath, oldDefinition);

                // Assert
                var exception = Assert.Throws<FormatException>(Call);
                Assert.That(exception.Message, Is.EqualTo($"Invalid group on line {4} in file {tempFilePath}"));

                // Equal result as new reader
                void NewCall() => NewMduFileReader.Read(tempFilePath, oldDefinition);
                var newException = Assert.Throws<FormatException>(NewCall);

                Assert.That(newException.Message, Is.EqualTo($"Invalid group on line {4} in file {tempFilePath}"));
            }
        }

        [Test]
        public void Read_MultiplePropertyValuesOutOfRange_ThenWarningMessageIsLogged_PlusCheckNewReadResult()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "MultiplePropertyValuesOutOfRange.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // Call
                void Call() => new MduFileReader().Read(tempFilePath, oldDefinition);
                void NewCall() => NewMduFileReader.Read(tempFilePath, oldDefinition);

                // Assert
                string expectedMessage = "During reading the mdu file the following warnings were reported:"
                                         + Environment.NewLine
                                         + "- An unsupported option for *Uniform friction type* has been detected and the default value will be used."
                                         + Environment.NewLine
                                         + "- An unsupported option for *Turbulence model* has been detected and the default value will be used.";
                TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage);
                TestHelper.AssertLogMessageIsGenerated(NewCall, expectedMessage);
            }
        }

        private static bool Equals(WaterFlowFMModelDefinition definition1, WaterFlowFMModelDefinition definition2)
        {
            IEnumerable<string> difference1 = definition2.Properties.Select(p => p.PropertyDefinition.MduPropertyName)
                                                         .Except(definition1.Properties.Select(p => p.PropertyDefinition.MduPropertyName));
            IEnumerable<string> difference2 = definition1.Properties.Select(p => p.PropertyDefinition.MduPropertyName)
                                                         .Except(definition2.Properties.Select(p => p.PropertyDefinition.MduPropertyName));

            Assert.That(definition1.Properties.Count, Is.EqualTo(definition2.Properties.Count), "The amount of properties is not the same.");

            int propertyCount = definition1.Properties.Count;
            for (var i = 0; i < propertyCount; i++)
            {
                WaterFlowFMProperty property1 = definition1.Properties[i];
                WaterFlowFMProperty property2 = definition2.Properties[i];

                Assert.That(property1.MinValue, Is.EqualTo(property2.MinValue), $"{property1.PropertyDefinition.Caption} - MinValue");
                Assert.That(property1.MaxValue, Is.EqualTo(property2.MaxValue), $"{property1.PropertyDefinition.Caption} - MaxValue");

                WaterFlowFMPropertyDefinition propertyDefinition1 = property1.PropertyDefinition;
                WaterFlowFMPropertyDefinition propertyDefinition2 = property2.PropertyDefinition;

                Assert.That(propertyDefinition1.Caption, Is.EqualTo(propertyDefinition2.Caption), $"{property1.PropertyDefinition.Caption} - Caption");
                Assert.That(propertyDefinition1.MduPropertyName, Is.EqualTo(propertyDefinition2.MduPropertyName), $"{property1.PropertyDefinition.Caption} - MduPropertyName");
                Assert.That(propertyDefinition1.FileCategoryName, Is.EqualTo(propertyDefinition2.FileCategoryName), $"{property1.PropertyDefinition.Caption} - FileCategoryName");
                Assert.That(propertyDefinition1.FilePropertyName, Is.EqualTo(propertyDefinition2.FilePropertyName), $"{property1.PropertyDefinition.Caption} - FilePropertyName");
                Assert.That(propertyDefinition1.Category, Is.EqualTo(propertyDefinition2.Category), $"{property1.PropertyDefinition.Caption} - Category");
                Assert.That(propertyDefinition1.SubCategory, Is.EqualTo(propertyDefinition2.SubCategory), $"{property1.PropertyDefinition.Caption} - SubCategory");
                Assert.That(propertyDefinition1.DataType, Is.EqualTo(propertyDefinition2.DataType), $"{property1.PropertyDefinition.Caption} - DataType");
                Assert.That(propertyDefinition1.DefaultValueAsString, Is.EqualTo(propertyDefinition2.DefaultValueAsString), $"{property1.PropertyDefinition.Caption} - DefaultValueAsString");
                Assert.That(propertyDefinition1.EnabledDependencies, Is.EqualTo(propertyDefinition2.EnabledDependencies), $"{property1.PropertyDefinition.Caption} - EnabledDependencies");
                Assert.That(propertyDefinition1.VisibleDependencies, Is.EqualTo(propertyDefinition2.VisibleDependencies), $"{property1.PropertyDefinition.Caption} - VisibleDependencies");
                Assert.That(propertyDefinition1.Description, Is.EqualTo(propertyDefinition2.Description), $"{property1.PropertyDefinition.Caption} - Description");
                Assert.That(propertyDefinition1.IsDefinedInSchema, Is.EqualTo(propertyDefinition2.IsDefinedInSchema), $"{property1.PropertyDefinition.Caption} - IsDefinedInSchema");
                Assert.That(propertyDefinition1.IsFile, Is.EqualTo(propertyDefinition2.IsFile), $"{property1.PropertyDefinition.Caption} - IsFile");
                Assert.That(propertyDefinition1.IsEnabled, Is.EqualTo(propertyDefinition2.IsEnabled), $"{property1.PropertyDefinition.Caption} - IsEnabled");
                Assert.That(propertyDefinition1.IsVisible, Is.EqualTo(propertyDefinition2.IsVisible), $"{property1.PropertyDefinition.Caption} - IsVisible");
                Assert.That(propertyDefinition1.UnknownPropertySource, Is.EqualTo(propertyDefinition2.UnknownPropertySource), $"{property1.PropertyDefinition.Caption} - UnknownPropertySource");

                Assert.That(propertyDefinition1.DefaultValueDependentOn, Is.EqualTo(propertyDefinition2.DefaultValueDependentOn), $"{property1.PropertyDefinition.Caption} - DefaultValueDependentOn");
                Assert.That(propertyDefinition1.DocumentationSection, Is.EqualTo(propertyDefinition2.DocumentationSection), $"{property1.PropertyDefinition.Caption} - DocumentationSection");
                Assert.That(propertyDefinition1.FromRevision, Is.EqualTo(propertyDefinition2.FromRevision), $"{property1.PropertyDefinition.Caption} - FromRevision");
                Assert.That(propertyDefinition1.IsMultipleFile, Is.EqualTo(propertyDefinition2.IsMultipleFile), $"{property1.PropertyDefinition.Caption} - IsMultipleFile");
                Assert.That(propertyDefinition1.MinValueAsString, Is.EqualTo(propertyDefinition2.MinValueAsString), $"{property1.PropertyDefinition.Caption} - MinValueAsString");
                Assert.That(propertyDefinition1.MaxValueAsString, Is.EqualTo(propertyDefinition2.MaxValueAsString), $"{property1.PropertyDefinition.Caption} - MaxValueAsString");
                Assert.That(propertyDefinition1.ModelFileOnly, Is.EqualTo(propertyDefinition2.ModelFileOnly), $"{property1.PropertyDefinition.Caption} - ModelFileOnly");
                Assert.That(propertyDefinition1.MultipleDefaultValues, Is.EqualTo(propertyDefinition2.MultipleDefaultValues), $"{property1.PropertyDefinition.Caption} - MultipleDefaultValues");
                Assert.That(propertyDefinition1.MultipleDefaultValuesAvailable, Is.EqualTo(propertyDefinition2.MultipleDefaultValuesAvailable), $"{property1.PropertyDefinition.Caption} - MultipleDefaultValuesAvailable");
                Assert.That(propertyDefinition1.Unit, Is.EqualTo(propertyDefinition2.Unit), $"{property1.PropertyDefinition.Caption} - Unit");
                Assert.That(propertyDefinition1.UntilRevision, Is.EqualTo(propertyDefinition2.UntilRevision), $"{property1.PropertyDefinition.Caption} - UntilRevision");

                if (propertyDefinition1.Caption != "Restart Time")
                {
                    Assert.That(property1.Value, Is.EqualTo(property2.Value), $"{property1.PropertyDefinition.Caption} - Value");
                }
                else
                {
                    DateTime date1 = ((DateTime) property1.Value).Date;
                    DateTime date2 = ((DateTime) property2.Value).Date;
                    Assert.That(date1, Is.EqualTo(date2), $"{property1.PropertyDefinition.Caption} - Value");
                }
            }

            return true;
        }
    }
}