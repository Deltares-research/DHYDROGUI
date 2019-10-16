using System;
using System.IO;
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
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "OneKnownPropertyNonDefaultValue.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // When
                new MduFileReader().Read(tempFilePath, oldDefinition);

                // Then
                WaterFlowFMProperty property = oldDefinition.GetModelProperty("Program");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("MyProgram"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Program name"));

                // Equal result as new reader
                var newDefinition = new WaterFlowFMModelDefinition();
                new NewMduFileReader().Read(tempFilePath, newDefinition);
                Assert.That(Equals(oldDefinition, newDefinition));
            }
        }

        [Test]
        public void Read_KnownPropertyNonDefaultComment_ThenPropertyCommentHasNotChanged_PlusCheckNewReadResult()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("MduFileReaderTest", "OneKnownPropertyNonDefaultComment.mdu"));
            using (var temporaryDir = new TemporaryDirectory())
            {
                string tempFilePath = temporaryDir.CopyTestDataFileToTempDirectory(testFilePath);
                var oldDefinition = new WaterFlowFMModelDefinition();

                // When
                new MduFileReader().Read(tempFilePath, oldDefinition);

                // Then
                WaterFlowFMProperty property = oldDefinition.GetModelProperty("Program");
                Assert.That(property.PropertyDefinition.FileCategoryName, Is.EqualTo("General"));
                Assert.That(property.Value, Is.EqualTo("D-Flow FM"));
                Assert.That(property.PropertyDefinition.Description, Is.EqualTo("Program name"));

                // Equal result as new reader
                var newDefinition = new WaterFlowFMModelDefinition();
                new NewMduFileReader().Read(tempFilePath, newDefinition);
                Assert.That(Equals(oldDefinition, newDefinition));
            }
        }

        private static bool Equals(WaterFlowFMModelDefinition definition1, WaterFlowFMModelDefinition definition2)
        {
            if (definition1.Properties.Count != definition2.Properties.Count)
            {
                return false;
            }

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