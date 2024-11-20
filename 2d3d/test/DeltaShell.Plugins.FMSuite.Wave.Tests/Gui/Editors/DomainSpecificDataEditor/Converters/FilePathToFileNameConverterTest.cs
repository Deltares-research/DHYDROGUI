using System;
using System.Collections.Generic;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.Converters
{
    [TestFixture]
    public class FilePathToFileNameConverterTest
    {
        private FilePathToFileNameConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new FilePathToFileNameConverter();
        }

        [Test]
        public void ConvertBack_ThenNotSupportedExceptionIsThrown()
        {
            // Call
            void Call() => converter.ConvertBack(null, null, null, null);

            // Assert
            Assert.That(Call, Throws.TypeOf<NotSupportedException>());
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Convert_ThenExpectedValueIsReturned(object value, Type targetType, object expectedReturnValue)
        {
            // Call
            object result = converter.Convert(value, targetType, null, null);

            // Assert
            Assert.That(result, Is.EqualTo(expectedReturnValue));
        }

        private static IEnumerable<TestCaseData> TestCaseData()
        {
            yield return new TestCaseData("D:/folder/file.txt", typeof(string), "file.txt");
            yield return new TestCaseData("D:/folder", typeof(string), "folder");
            yield return new TestCaseData("D:/folder/fi>le.txt", typeof(string), "D:/folder/fi>le.txt");
            yield return new TestCaseData(new object(), typeof(string), DependencyProperty.UnsetValue);
            yield return new TestCaseData("D:/folder/file.txt", typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData("D:/folder/fi:le.txt", typeof(object), DependencyProperty.UnsetValue);
        }
    }
}