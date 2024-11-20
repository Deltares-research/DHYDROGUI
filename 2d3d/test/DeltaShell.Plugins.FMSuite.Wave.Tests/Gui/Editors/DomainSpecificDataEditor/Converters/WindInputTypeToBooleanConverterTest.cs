using System;
using System.Collections.Generic;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.Converters
{
    [TestFixture]
    public class WindInputTypeToBooleanConverterTest
    {
        private WindInputTypeToBooleanConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new WindInputTypeToBooleanConverter();
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
            yield return new TestCaseData(WindInputType.WindVector, typeof(bool), true);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, typeof(bool), false);
            yield return new TestCaseData(WindInputType.XYComponents, typeof(bool), true);

            yield return new TestCaseData(new object(), typeof(bool), DependencyProperty.UnsetValue);

            yield return new TestCaseData(WindInputType.WindVector, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, typeof(object), DependencyProperty.UnsetValue);
        }
    }
}