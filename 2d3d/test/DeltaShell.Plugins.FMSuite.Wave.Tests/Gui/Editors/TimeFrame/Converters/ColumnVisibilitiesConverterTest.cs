using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Converters
{
    [TestFixture]
    public class ColumnVisibilitiesConverterTest
    {
        public static IEnumerable<TestCaseData> GetConvertValidArgumentsData()
        {
            var constantHydroConstantWindResult = new[] { true, false, false, false, false, false };
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant, WindInputDataType.Constant, },
                                          constantHydroConstantWindResult);
            yield return new TestCaseData(new object[] { WindInputDataType.Constant, HydrodynamicsInputDataType.Constant, },
                                          constantHydroConstantWindResult);


            var constantHydroFileWindResult = new[] { true, false, false, false, false, false };
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant, WindInputDataType.FileBased, },
                                          constantHydroFileWindResult);
            yield return new TestCaseData(new object[] { WindInputDataType.FileBased, HydrodynamicsInputDataType.Constant, },
                                          constantHydroFileWindResult);

            var constantHydroVaryingWindResult = new[] { true, false, false, false, true, true };
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant, WindInputDataType.TimeVarying, },
                                          constantHydroVaryingWindResult);
            yield return new TestCaseData(new object[] { WindInputDataType.TimeVarying, HydrodynamicsInputDataType.Constant, },
                                          constantHydroVaryingWindResult);

            var varyingHydroConstantWindResult = new[] { true, true, true, true, false, false };
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.TimeVarying, WindInputDataType.Constant, },
                                          varyingHydroConstantWindResult);
            yield return new TestCaseData(new object[] { WindInputDataType.Constant, HydrodynamicsInputDataType.TimeVarying, },
                                          varyingHydroConstantWindResult);

            var varyingHydroFileWindResult = new[] { true, true, true, true, false, false };
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.TimeVarying, WindInputDataType.FileBased, },
                                          varyingHydroFileWindResult);
            yield return new TestCaseData(new object[] { WindInputDataType.FileBased, HydrodynamicsInputDataType.TimeVarying, },
                                          varyingHydroFileWindResult);

            var varyingHydroVaryingWindResult = new[] { true, true, true, true, true, true };
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.TimeVarying, WindInputDataType.TimeVarying, },
                                          varyingHydroVaryingWindResult);
            yield return new TestCaseData(new object[] { WindInputDataType.TimeVarying, HydrodynamicsInputDataType.TimeVarying, },
                                          varyingHydroVaryingWindResult);
        }

        [Test]
        [TestCaseSource(nameof(GetConvertValidArgumentsData))]
        public void Convert_ValidArguments_ReturnsExpectedColumnVisibilities(object[] values,
                                                                             bool[] expectedResult)
        {
            // Setup
            var converter = new ColumnVisibilitiesConverter();

            // Call
            object result = converter.Convert(values,
                                              typeof(IList<bool>),
                                              null,
                                              CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        public static IEnumerable<TestCaseData> GetConvertInvalidArgumentsData()
        {
            yield return new TestCaseData(null,
                                          typeof(IList<bool>));
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant, WindInputDataType.Constant },
                                          typeof(object));
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant, WindInputDataType.Constant, WindInputDataType.Constant },
                                          typeof(IList<bool>));
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant },
                                          typeof(IList<bool>));
            yield return new TestCaseData(new object[] { HydrodynamicsInputDataType.Constant, HydrodynamicsInputDataType.Constant },
                                          typeof(IList<bool>));
        }

        [Test]
        [TestCaseSource(nameof(GetConvertInvalidArgumentsData))]
        public void Convert_InvalidArguments_ReturnsDependencyPropertyUnset(object[] values, Type targetType)
        {
            // Setup
            var converter = new ColumnVisibilitiesConverter();

            // Call
            object result = converter.Convert(values,
                                              targetType,
                                              null,
                                              CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }


        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Setup
            var converter = new ColumnVisibilitiesConverter();

            // Call | Assert
            void Call() => converter.ConvertBack(null,
                                                 new Type[0],
                                                 null,
                                                 CultureInfo.InvariantCulture);

            Assert.Throws<NotSupportedException>(Call);
        }
    }
}