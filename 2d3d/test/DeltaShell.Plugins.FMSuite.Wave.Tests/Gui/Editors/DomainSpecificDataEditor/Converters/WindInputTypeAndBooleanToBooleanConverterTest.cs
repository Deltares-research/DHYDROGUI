using System;
using System.Collections.Generic;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.Converters
{
    [TestFixture]
    public class WindInputTypeAndBooleanToBooleanConverterTest
    {
        private WindInputTypeAndBooleanToBooleanConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new WindInputTypeAndBooleanToBooleanConverter();
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
        public void Convert_ThenExpectedValueIsReturned(object value1, object value2, Type targetType, object expectedReturnValue)
        {
            // Call
            object result = converter.Convert(new[]
            {
                value1,
                value2
            }, targetType, null, null);

            // Assert
            Assert.That(result, Is.EqualTo(expectedReturnValue));
        }

        private static IEnumerable<TestCaseData> TestCaseData()
        {
            yield return new TestCaseData(WindInputType.SpiderWebGrid, false, typeof(bool), true);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, true, typeof(bool), true);
            yield return new TestCaseData(WindInputType.WindVector, false, typeof(bool), false);
            yield return new TestCaseData(WindInputType.WindVector, true, typeof(bool), true);
            yield return new TestCaseData(WindInputType.XYComponents, false, typeof(bool), false);
            yield return new TestCaseData(WindInputType.XYComponents, true, typeof(bool), true);

            yield return new TestCaseData(new object(), true, typeof(bool), DependencyProperty.UnsetValue);
            yield return new TestCaseData(new object(), false, typeof(bool), DependencyProperty.UnsetValue);

            yield return new TestCaseData(WindInputType.SpiderWebGrid, new object(), typeof(bool), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, new object(), typeof(bool), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, new object(), typeof(bool), DependencyProperty.UnsetValue);

            yield return new TestCaseData(WindInputType.SpiderWebGrid, false, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, true, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, false, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, true, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, false, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, true, typeof(object), DependencyProperty.UnsetValue);
        }
    }
}