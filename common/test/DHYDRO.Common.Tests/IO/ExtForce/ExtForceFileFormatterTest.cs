using System.Collections.Generic;
using System.IO;
using DHYDRO.Common.IO.ExtForce;
using DHYDRO.Common.TestUtils.IO.ExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.ExtForce
{
    [TestFixture]
    public class ExtForceFileFormatterTest
    {
        [Test]
        public void Format_ExtForceFileDataIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();

            Assert.That(() => extForceFileFormatter.Format(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Format_ExtForceFileDataIsNullAndStreamIsNotNull_ThrowsArgumentNullException()
        {
            ExtForceFileFormatter formatter = CreateFormatter();
            Stream stream = Stream.Null;

            Assert.That(() => formatter.Format(null, stream), Throws.ArgumentNullException);
        }

        [Test]
        public void Format_ExtForceFileDataIsNotNullAndStreamIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileFormatter formatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().Build();

            Assert.That(() => formatter.Format(extForceFileData, (Stream)null), Throws.ArgumentNullException);
        }

        [Test]
        public void Format_ExtForceFileDataIsNullAndTextWriterIsNotNull_ThrowsArgumentNullException()
        {
            ExtForceFileFormatter formatter = CreateFormatter();
            TextWriter writer = TextWriter.Null;

            Assert.That(() => formatter.Format(null, writer), Throws.ArgumentNullException);
        }

        [Test]
        public void Format_ExtForceFileDataIsNotNullAndTextWriterIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileFormatter formatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().Build();

            Assert.That(() => formatter.Format(extForceFileData, (TextWriter)null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(GetMandatoryFieldIsNullOrEmptyTestCases))]
        public void Format_MandatoryFieldIsNullOrEmpty_ThrowsInvalidOperationException(ExtForceData forcing)
        {
            ExtForceFileFormatter formatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(forcing).Build();

            Assert.That(() => formatter.Format(extForceFileData), Throws.InvalidOperationException);
        }

        private static IEnumerable<TestCaseData> GetMandatoryFieldIsNullOrEmptyTestCases()
        {
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithQuantity(null).Build()).SetName("Null quantity name");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithFileName(null).Build()).SetName("Null file name");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithFileType(null).Build()).SetName("Null file type");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithMethod(null).Build()).SetName("Null method");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithOperand(null).Build()).SetName("Null operand");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithQuantity(string.Empty).Build()).SetName("Empty quantity name");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithFileName(string.Empty).Build()).SetName("Empty file name");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithOperand(string.Empty).Build()).SetName("Empty operand");
        }

        [Test]
        public void Format_EmptyExtForceFileData_ReturnsEmptyString()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().Build();

            string ini = extForceFileFormatter.Format(extForceFileData);

            Assert.That(ini, Is.Empty);
        }

        [Test]
        public void Format_ForcingWithMandatoryFields_ReturnsFormattedString()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(x => x.AddRequiredValues()).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo(@"QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O

"));
        }

        [Test]
        public void Format_ForcingIsDisabled_ReturnsFormattedString()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(x => x.AddRequiredValues().IsEnabled(false)).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo(@"DISABLED_QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O

"));
        }

        [Test]
        [TestCase("", "channeleastdis.pli", 9, 3, "O")]
        [TestCase(null, "channeleastdis.pli", 9, 3, "O")]
        [TestCase("initialsalinity", "", 10, 4, "+")]
        [TestCase("initialsalinity", null, 10, 4, "+")]
        [TestCase("frictioncoefficient", "grasscrete1.pol", null, 4, "O")]
        [TestCase("frictioncoefficient", "grasscrete1.pol", 10, null, "O")]
        [TestCase("waveperiod", "swanout.nc", 11, 3, "")]
        [TestCase("waveperiod", "swanout.nc", 11, 3, null)]
        public void Format_ForcingWithMandatoryFieldNullOrEmpty_ThrowsInvalidOperationException(string quantity, string fileName, int? fileType, int? method, string operand)
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();

            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .WithQuantity(quantity)
                                                           .WithFileName(fileName)
                                                           .WithFileType(fileType)
                                                           .WithMethod(method)
                                                           .WithOperand(operand)
                                                           .Build();

            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(extForceData).Build();

            Assert.That(() => extForceFileFormatter.Format(extForceFileData), Throws.InvalidOperationException.And
                                                                                    .Message.EqualTo("Mandatory fields must be set."));
        }

        [Test]
        [TestCase("dischargebnd", "channeleastdis.pli", 9, 3, "O")]
        [TestCase("initialsalinity", "narrows_extra.pol", 10, 4, "+")]
        [TestCase("frictioncoefficient", "grasscrete1.pol", 10, 4, "O")]
        [TestCase("waveperiod", "swanout.nc", 11, 3, "O")]
        public void Format_ForcingWithMixedValues_ReturnsFormattedString(string quantity, string fileName, int fileType, int method, string operand)
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();

            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .WithQuantity(quantity)
                                                           .WithFileName(fileName)
                                                           .WithFileType(fileType)
                                                           .WithMethod(method)
                                                           .WithOperand(operand)
                                                           .Build();

            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(extForceData).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo($@"QUANTITY={quantity}
FILENAME={fileName}
FILETYPE={fileType}
METHOD={method}
OPERAND={operand}

"));
        }

        [Test]
        public void Format_ForcingWithOptionalValues_ReturnsFormattedString()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(x => x.AddRequiredValues().AddOptionalValues()).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo(@"QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
VARNAME=ssr
FILETYPE=7
METHOD=4
OPERAND=O
VALUE=0.038
FACTOR=1.3
OFFSET=10.1

"));
        }

        [Test]
        [TestCaseSource(nameof(GetOptionalFieldIsNullOrEmptyOrNaNTestCases))]
        public void Format_OptionalFieldIsNullOrEmptyOrNaN_ReturnsFormattedString(ExtForceData forcing)
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(forcing).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo(@"QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O

"));
        }

        private static IEnumerable<TestCaseData> GetOptionalFieldIsNullOrEmptyOrNaNTestCases()
        {
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithVariableName(null).Build()).SetName("Null variable name");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithValue(null).Build()).SetName("Null value");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithFactor(null).Build()).SetName("Null factor");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithOffset(null).Build()).SetName("Null offset");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithVariableName(string.Empty).Build()).SetName("Empty variable name");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithValue(double.NaN).Build()).SetName("NaN value");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithFactor(double.NaN).Build()).SetName("NaN factor");
            yield return new TestCaseData(ExtForceDataBuilder.Start().AddRequiredValues().WithOffset(double.NaN).Build()).SetName("NaN offset");
        }

        [Test]
        public void Format_ForcingWithComments_ReturnsFormattedString()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();

            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddComments("comment line 1", "comment line 2", "comment line 3")
                                                           .Build();

            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(extForceData).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo(@"*comment line 1
*comment line 2
*comment line 3

QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O

"));
        }

        [Test]
        public void Format_ForcingWithModelData_ReturnsFormattedString()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();

            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddModelData(ExtForceFileConstants.Keys.Area, 12.1)
                                                           .AddModelData(ExtForceFileConstants.Keys.FrictionType, 1)
                                                           .AddModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, 2.2)
                                                           .Build();

            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(extForceData).Build();

            string str = extForceFileFormatter.Format(extForceFileData);

            Assert.That(str, Is.EqualTo(@"QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O
AREA=12.1
IFRCTYP=1
RELATIVESEARCHCELLSIZE=2.2

"));
        }

        [Test]
        public void Format_ValidStream_KeepsStreamOpen()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(x => x.AddRequiredValues()).Build();

            using (var stream = new MemoryStream())
            {
                extForceFileFormatter.Format(extForceFileData, stream);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        [Test]
        public void Format_ValidTextWriterFromStream_KeepsStreamOpen()
        {
            ExtForceFileFormatter extForceFileFormatter = CreateFormatter();
            ExtForceFileData extForceFileData = ExtForceFileDataBuilder.Start().AddForcing(x => x.AddRequiredValues()).Build();

            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                extForceFileFormatter.Format(extForceFileData, streamWriter);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        private static ExtForceFileFormatter CreateFormatter()
        {
            return new ExtForceFileFormatter();
        }
    }
}