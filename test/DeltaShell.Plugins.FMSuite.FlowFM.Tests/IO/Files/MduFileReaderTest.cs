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
            ReadWithAssert("OneKnownPropertyNonDefaultValue.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("MyProgram"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Program name"));
            });
        }

        [Test]
        public void Read_KnownPropertyNonDefaultComment_ThenPropertyCommentHasNotChanged_PlusCheckNewReadResult()
        {
            ReadWithAssert("OneKnownPropertyNonDefaultComment.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("D-Flow FM"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Program name"));
            });
        }

        [Test]
        public void Read_UnknownProperty_ThenNewPropertyIsAddedToModelDefinition_PlusCheckNewReadResult()
        {
            ReadWithAssert("OneUnknownProperty.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("MyValue"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("MyDescription"));
            });
        }

        [Test]
        public void Read_LegacyCategory_ThenCategoryNameIsUpdated_PlusCheckNewReadResult()
        {
            ReadWithAssert("LegacyCategory.mdu", definition =>
            {
                WaterFlowFMProperty knownProperty = definition.GetModelProperty("Program");
                Assert.That(knownProperty.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(knownProperty.Value, Is.EqualTo("D-Flow FM"));
                Assert.That(knownProperty.PropertyDefinition.Description, Is.EqualTo("Program name"));

                WaterFlowFMProperty unknownProperty = definition.GetModelProperty("MyProperty");
                Assert.That(unknownProperty.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(unknownProperty.Value, Is.EqualTo("MyValue"));
                Assert.That(unknownProperty.PropertyDefinition.Description, Is.EqualTo("MyComment"));
            });
        }

        [Test]
        public void Read_KnownPropertyWithoutComment_ThenNewPropertyIsAddedToModelDefinitionWithPredefinedComment_PlusCheckNewReadResult()
        {
            ReadWithAssert("KnownPropertyWithoutComment.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("Program");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("D-Flow FM"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Program name"));
            });
        }

        [Test]
        public void Read_UnknownPropertyWithoutComment_ThenNewPropertyIsAddedToModelDefinitionWithNullComment_PlusCheckNewReadResult()
        {
            ReadWithAssert("UnknownPropertyWithoutComment.mdu", definition =>
            {
                WaterFlowFMProperty property = definition.GetModelProperty("MyCustomProperty");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("MyValue"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo(null));
            });
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

        private static void ReadWithAssert(string fileName, Action<WaterFlowFMModelDefinition> assertAction)
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", fileName));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // When
                new MduFileReader().Read(tempFilePath, oldDefinition);

                // Then
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

                // When
                void Call() => new MduFileReader().Read(tempFilePath, oldDefinition);

                // Then
                var exception = Assert.Throws<FormatException>(Call);
                Assert.That(exception.Message, Is.EqualTo($"Invalid group on line {4} in file {tempFilePath}"));

                // Equal result as new reader
                void NewCall() => NewMduFileReader.Read(tempFilePath, oldDefinition);
                var newException = Assert.Throws<FormatException>(NewCall);

                Assert.That(newException.Message, Is.EqualTo($"Invalid group on line {4} in file {tempFilePath}"));
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