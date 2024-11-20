using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.ModelSchema
{
    [TestFixture]
    public class ModelPropertyTest
    {
        [Test]
        public void SetValueFromStrings_NullCollection_ThrowsArgumentNullException()
        {
            ModelProperty property = CreateModelProperty<string>();

            Assert.Throws<ArgumentNullException>(() => property.SetValueFromStrings(null));
        }

        [Test]
        public void SetValueFromStrings_EmptyCollection_ValueIsEmptyString()
        {
            ModelProperty property = CreateModelProperty<string>();

            property.SetValueFromStrings(Enumerable.Empty<string>());

            Assert.That(property.Value, Is.Empty);
        }

        [Test]
        [TestCase(' ')]
        [TestCase(';')]
        public void SetValueFromStrings_MultipleStrings_ValueEqualsCombinedString(char separator)
        {
            ModelProperty property = CreateModelProperty<string>();

            property.SetValueFromStrings(new[] { "a", "b", "c" }, separator);

            Assert.That($"a{separator}b{separator}c", Is.EqualTo(property.Value));
        }

        [Test]
        public void SetValueFromStrings_DoesNotMatchDataType_ThrowsFormatException()
        {
            ModelProperty property = CreateModelProperty<int>();

            Assert.Throws<FormatException>(() => property.SetValueFromStrings(new[] { "a", "b", "c" }));
        }

        [Test]
        public void GetFileLocationValues_IsNotFileBased_ThrowsInvalidOperationException()
        {
            ModelProperty property = CreateModelProperty<string>();

            Assert.Throws<InvalidOperationException>(() => property.GetFileLocationValues());
        }

        [Test]
        public void GetFileLocationValues_EmptyFileLocation_ReturnsEmptyCollection()
        {
            ModelProperty property = CreateFileBasedModelProperty();

            Assert.That(property.GetFileLocationValues(), Is.Empty);
        }

        [Test]
        public void GetFileLocationValues_SingleFileLocation_ReturnsCollectionWithOneElement()
        {
            const string fileName = @"c:\file.txt";

            ModelProperty property = CreateFileBasedModelProperty();
            property.SetValueFromString(fileName);

            Assert.That(property.GetFileLocationValues(), Is.EqualTo(new[] { fileName }));
        }

        [Test]
        [TestCase(' ')]
        [TestCase(';')]
        public void GetFileLocationValues_MultipleFileLocations_ReturnsCollectionWithMultipleElements(char separator)
        {
            var fileNames = new[] { "file1.txt", "file2.txt" };

            ModelProperty property = CreateMultipleFileBasedModelProperty();
            property.SetValueFromString(string.Join(separator.ToString(), fileNames));

            Assert.That(property.GetFileLocationValues(), Is.EqualTo(fileNames));
        }

        private static ModelProperty CreateMultipleFileBasedModelProperty()
        {
            ModelProperty property = CreateModelProperty<IList<string>>();
            property.PropertyDefinition.IsMultipleFile = true;
            property.PropertyDefinition.IsFile = true;

            return property;
        }

        private static ModelProperty CreateFileBasedModelProperty()
        {
            ModelProperty property = CreateModelProperty<string>();
            property.PropertyDefinition.IsFile = true;

            return property;
        }

        private static ModelProperty CreateModelProperty<T>()
        {
            var propertyDefinition = new TestModelPropertyDefinition
            {
                FilePropertyKey = "PropertyKey",
                DataType = typeof(T)
            };

            return new TestModelProperty(propertyDefinition, default(T)?.ToString());
        }
    }
}