using System;
using System.Collections.Generic;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.Converters
{
    [TestFixture]
    public class WindInputTypeToVisibilityConverterTest
    {
        private WindInputTypeToVisibilityConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new WindInputTypeToVisibilityConverter();
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
        public void Convert_ThenExpectedValueIsReturned(object value, object parameter, Type targetType, object expectedReturnValue)
        {
            // Call
            object result = converter.Convert(value, targetType, parameter, null);

            // Assert
            Assert.That(result, Is.EqualTo(expectedReturnValue));
        }

        private static IEnumerable<TestCaseData> TestCaseData()
        {
            yield return new TestCaseData(WindInputType.SpiderWebGrid, WindInputType.SpiderWebGrid, typeof(Visibility), Visibility.Visible);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, WindInputType.WindVector, typeof(Visibility), Visibility.Collapsed);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, WindInputType.XYComponents, typeof(Visibility), Visibility.Collapsed);
            yield return new TestCaseData(WindInputType.WindVector, WindInputType.SpiderWebGrid, typeof(Visibility), Visibility.Collapsed);
            yield return new TestCaseData(WindInputType.WindVector, WindInputType.WindVector, typeof(Visibility), Visibility.Visible);
            yield return new TestCaseData(WindInputType.WindVector, WindInputType.XYComponents, typeof(Visibility), Visibility.Collapsed);
            yield return new TestCaseData(WindInputType.XYComponents, WindInputType.SpiderWebGrid, typeof(Visibility), Visibility.Collapsed);
            yield return new TestCaseData(WindInputType.XYComponents, WindInputType.WindVector, typeof(Visibility), Visibility.Collapsed);
            yield return new TestCaseData(WindInputType.XYComponents, WindInputType.XYComponents, typeof(Visibility), Visibility.Visible);

            yield return new TestCaseData(new object(), WindInputType.SpiderWebGrid, typeof(Visibility), DependencyProperty.UnsetValue);
            yield return new TestCaseData(new object(), WindInputType.SpiderWebGrid, typeof(Visibility), DependencyProperty.UnsetValue);
            yield return new TestCaseData(new object(), WindInputType.SpiderWebGrid, typeof(Visibility), DependencyProperty.UnsetValue);

            yield return new TestCaseData(WindInputType.SpiderWebGrid, new object(), typeof(Visibility), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, new object(), typeof(Visibility), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, new object(), typeof(Visibility), DependencyProperty.UnsetValue);

            yield return new TestCaseData(WindInputType.SpiderWebGrid, WindInputType.SpiderWebGrid, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, WindInputType.WindVector, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.SpiderWebGrid, WindInputType.XYComponents, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, WindInputType.SpiderWebGrid, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, WindInputType.WindVector, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.WindVector, WindInputType.XYComponents, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, WindInputType.SpiderWebGrid, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, WindInputType.WindVector, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(WindInputType.XYComponents, WindInputType.XYComponents, typeof(object), DependencyProperty.UnsetValue);
        }
    }
}