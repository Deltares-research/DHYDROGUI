using System;
using System.IO;
using System.Linq;
using DHYDRO.Common.IO.ExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.ExtForce
{
    [TestFixture]
    public class ExtForceFileParserTest
    {
        [Test]
        public void Parse_IniStringIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse((string)null), Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_StreamIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse((Stream)null), Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_TextReaderIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse((TextReader)null), Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_EmptyString_ReturnsDataWithoutForcings()
        {
            ExtForceFileParser parser = CreateParser();

            ExtForceFileData data = parser.Parse(string.Empty);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Is.Empty);
        }

        [Test]
        public void Parse_EmptyLinesString_ReturnsDataWithoutForcings()
        {
            ExtForceFileParser parser = CreateParser();

            ExtForceFileData data = parser.Parse(@"

");

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Is.Empty);
        }

        [Test]
        public void Parse_CommentBlockString_ReturnsDataWithoutForcings()
        {
            ExtForceFileParser parser = CreateParser();

            ExtForceFileData data = parser.Parse(@"
* comment line 1
* comment line 2
");

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Is.Empty);
        }

        [Test]
        [TestCase("key value")]
        [TestCase("value")]
        [TestCase("key")]
        public void Parse_InvalidTextForm_ThrowsFormatException(string line)
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse(line), Throws.TypeOf<FormatException>()
                                                        .And.Message.Contain("invalid formatted text"));
        }

        [Test]
        [TestCase("=value")]
        [TestCase(" = value")]
        [TestCase("\t=value")]
        public void Parse_InvalidPropertyFormat_ThrowsFormatException(string propertyLine)
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse(propertyLine), Throws.TypeOf<FormatException>()
                                                                .And.Message.Contain("property key cannot be empty"));
        }

        [Test]
        [TestCase("quantity=some_quantity")]
        [TestCase(" quantity = some_quantity")]
        [TestCase("\tquantity\t=\tsome_quantity")]
        public void Parse_ValidPropertyFormat_ForcingContainsQuantity(string propertyLine)
        {
            ExtForceFileParser parser = CreateParser();

            ExtForceFileData data = parser.Parse(propertyLine);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);
            Assert.That(data.Forcings.ElementAt(0).Quantity, Is.EqualTo("some_quantity"));
        }

        [Test]
        [TestCase("DISABLED_QUANTITY")]
        [TestCase("WUANTITY")]
        [TestCase("_UANTITY")]
        public void Parse_DisabledOrUnsupportedQuantity_ForcingIsDisabled(string quantity)
        {
            ExtForceFileParser parser = CreateParser();

            ExtForceFileData data = parser.Parse($"{quantity}=initialwaterlevel");

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);
            Assert.That(data.Forcings.ElementAt(0).IsEnabled, Is.False);
        }

        [Test]
        [TestCase("QUANTITY=\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=\r\nMETHOD=3\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=")]
        public void Parse_MandatoryPropertyValueIsEmpty_ThrowsFormatException(string str)
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse(str), Throws.TypeOf<FormatException>()
                                                       .And.Message.Contain("property value cannot be empty"));
        }

        [Test]
        [TestCase('*')]
        [TestCase('!')]
        [TestCase('#')]
        public void Parse_ForcingWithCommentBlock_ForcingContainsComments(char delim)
        {
            ExtForceFileParser parser = CreateParser();

            var str = $@"{delim}comment line 1
{delim}comment line 2

QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);

            ExtForceData forcing = data.Forcings.First();

            Assert.That(forcing.Quantity, Is.EqualTo("initialwaterlevel"));
            Assert.That(forcing.Comments, Is.EqualTo(new[] { "comment line 1", "comment line 2" }));
        }

        [Test]
        [TestCase('*')]
        [TestCase('!')]
        [TestCase('#')]
        public void Parse_MultipleForcingsWithCommentBlocks_ForcingContainsComments(char delim)
        {
            ExtForceFileParser parser = CreateParser();

            var str = $@"{delim}dischargebnd comment 1
{delim}dischargebnd comment 2

QUANTITY=dischargebnd
FILENAME=channeleastdis.pli
FILETYPE=9
METHOD=3
OPERAND=+

{delim}initialwaterlevel comment 1
{delim}initialwaterlevel comment 2

QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O
";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.Exactly(2).Items);
            Assert.That(data.Forcings.ElementAt(0).Comments, Is.EqualTo(new[] { "dischargebnd comment 1", "dischargebnd comment 2" }));
            Assert.That(data.Forcings.ElementAt(1).Comments, Is.EqualTo(new[] { "initialwaterlevel comment 1", "initialwaterlevel comment 2" }));
        }
        
        [Test]
        public void Parse_ForcingWithKnownInvalidCommentBlock_ForcingContainsFixedComments()
        {
            ExtForceFileParser parser = CreateParser();

            var str = @"* QUANTITY    : waterlevelbnd, velocitybnd, dischargebnd, tangentialvelocitybnd, normalvelocitybnd  filetype=9         method=2,3
*             : salinitybnd                                                                         filetype=9         method=2,3
*             : lowergatelevel, damlevel, pump                                                      filetype=9         method=2,3
              : frictioncoefficient, horizontaleddyviscositycoefficient, advectiontype              filetype=4,10      method=4
              : windx, windy, windxy, rain, atmosphericpressure                                     filetype=1,2,4,7,8 method=1,2,3
*

QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);

            ExtForceData forcing = data.Forcings.First();

            Assert.That(forcing.Quantity, Is.EqualTo("initialwaterlevel"));
            Assert.That(forcing.Comments, Is.EqualTo(new[]
            {
                " QUANTITY    : waterlevelbnd, velocitybnd, dischargebnd, tangentialvelocitybnd, normalvelocitybnd  filetype=9         method=2,3", 
                "             : salinitybnd                                                                         filetype=9         method=2,3",
                "             : lowergatelevel, damlevel, pump                                                      filetype=9         method=2,3",
                "             : frictioncoefficient, horizontaleddyviscositycoefficient, advectiontype              filetype=4,10      method=4",
                "             : windx, windy, windxy, rain, atmosphericpressure                                     filetype=1,2,4,7,8 method=1,2,3",
                ""
            }));
        }

        [Test]
        [TestCase("quantity=some_quantity # inline comment")]
        [TestCase("quantity=some_quantity ! inline comment")]
        public void Parse_ForcingWithInlineComment_ForcingCommentsIsEmpty(string propertyLine)
        {
            ExtForceFileParser parser = CreateParser();

            ExtForceFileData data = parser.Parse(propertyLine);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);

            ExtForceData forcing = data.Forcings.First();

            Assert.That(forcing.Quantity, Is.EqualTo("some_quantity"));
            Assert.That(forcing.Comments, Is.Empty);
        }

        [Test]
        public void Parse_ForcingWithMandatoryAndOptionalValues_ForcingContainsExpectedValues()
        {
            ExtForceFileParser parser = CreateParser();

            const string str = @"QUANTITY=dischargebnd
FILENAME=channeleastdis.pli
VARNAME=sst
FILETYPE=9
METHOD=3
OPERAND=+
VALUE=0.038
FACTOR=1.3
OFFSET=10.1

";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);

            ExtForceData forcing = data.Forcings.First();

            Assert.That(forcing.IsEnabled, Is.True);
            Assert.That(forcing.Quantity, Is.EqualTo("dischargebnd"));
            Assert.That(forcing.FileName, Is.EqualTo("channeleastdis.pli"));
            Assert.That(forcing.VariableName, Is.EqualTo("sst"));
            Assert.That(forcing.FileType, Is.EqualTo(ExtForceFileConstants.FileTypes.PolyTim));
            Assert.That(forcing.Method, Is.EqualTo(ExtForceFileConstants.Methods.SpaceAndTimeSaveWeights));
            Assert.That(forcing.Operand, Is.EqualTo(ExtForceFileConstants.Operands.Add));
            Assert.That(forcing.Value, Is.EqualTo(0.038));
            Assert.That(forcing.Factor, Is.EqualTo(1.3));
            Assert.That(forcing.Offset, Is.EqualTo(10.1));
        }

        [Test]
        public void Parse_ForcingWithMandatoryAndModelData_ForcingContainsExpectedValues()
        {
            ExtForceFileParser parser = CreateParser();

            const string str = @"QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=6
METHOD=4
OPERAND=O
AREA=12.1
IFRCTYP=1
RELATIVESEARCHCELLSIZE=2.2

";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);

            ExtForceData forcing = data.Forcings.First();

            Assert.That(forcing.IsEnabled, Is.True);
            Assert.That(forcing.Quantity, Is.EqualTo("initialwaterlevel"));
            Assert.That(forcing.FileName, Is.EqualTo("initialwaterlevel.xyz"));
            Assert.That(forcing.FileType, Is.EqualTo(ExtForceFileConstants.FileTypes.Curvi));
            Assert.That(forcing.Method, Is.EqualTo(ExtForceFileConstants.Methods.InsidePolygon));
            Assert.That(forcing.Operand, Is.EqualTo(ExtForceFileConstants.Operands.Override));

            forcing.TryGetModelData(ExtForceFileConstants.Keys.Area, out double area);
            forcing.TryGetModelData(ExtForceFileConstants.Keys.FrictionType, out int frictionType);
            forcing.TryGetModelData(ExtForceFileConstants.Keys.RelativeSearchCellSize, out double cellSize);

            Assert.That(area, Is.EqualTo(12.1));
            Assert.That(frictionType, Is.EqualTo(1));
            Assert.That(cellSize, Is.EqualTo(2.2));
        }

        [Test]
        public void Parse_MultipleForcings_ForcingsHaveLineNumbers()
        {
            ExtForceFileParser parser = CreateParser();

            const string str = @"QUANTITY=dischargebnd
FILENAME=channeleastdis.pli
FILETYPE=9
METHOD=3
OPERAND=+

QUANTITY=initialwaterlevel
FILENAME=initialwaterlevel.xyz
FILETYPE=7
METHOD=4
OPERAND=O
";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.Exactly(2).Items);
            Assert.That(data.Forcings.ElementAt(0).LineNumber, Is.EqualTo(1));
            Assert.That(data.Forcings.ElementAt(1).LineNumber, Is.EqualTo(7));
        }

        [Test]
        public void Parse_ForcingWithTriangulationFileTypeAndInsidePolygonMethod_ForcingMethodSetToTriangulation()
        {
            ExtForceFileParser parser = CreateParser();

            const string str = @"QUANTITY=initialsalinity
FILENAME=narrows_extra.pol
FILETYPE=7
METHOD=4
OPERAND=O

";

            ExtForceFileData data = parser.Parse(str);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Forcings, Has.One.Items);

            ExtForceData forcing = data.Forcings.First();

            Assert.That(forcing.IsEnabled, Is.True);
            Assert.That(forcing.Quantity, Is.EqualTo("initialsalinity"));
            Assert.That(forcing.FileName, Is.EqualTo("narrows_extra.pol"));
            Assert.That(forcing.FileType, Is.EqualTo(ExtForceFileConstants.FileTypes.Triangulation));
            Assert.That(forcing.Method, Is.EqualTo(ExtForceFileConstants.Methods.Triangulation));
            Assert.That(forcing.Operand, Is.EqualTo(ExtForceFileConstants.Operands.Override));
        }

        [Test]
        [TestCase("FILENAME=channeleastdis.pli\r\nQUANTITY=dischargebnd\r\nVARNAME=sst\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nFILETYPE=9\r\nFILENAME=channeleastdis.pli\r\nVARNAME=sst\r\nMETHOD=3\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nVARNAME=sst\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nVARNAME=sst\r\nFILETYPE=9\r\nOPERAND=O\r\nMETHOD=3")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nVARNAME=sst\r\nFILETYPE=9\r\nMETHOD=3\r\nAREA=12.1\r\nOPERAND=O")]
        public void Parse_UnexpectedPropertyOrder_ThrowsFormatException(string str)
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse(str), Throws.TypeOf<FormatException>()
                                                       .And.Message.Contain("Unexpected property"));
        }

        [Test]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=invalid\r\nMETHOD=3\r\nOPERAND=O\r\nVALUE=2.22\r\nFACTOR=1.2\r\nOFFSET=12.0")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=invalid\r\nOPERAND=O\r\nVALUE=2.22\r\nFACTOR=1.2\r\nOFFSET=12.0")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O\r\nVALUE=invalid\r\nFACTOR=1.2\r\nOFFSET=12.0")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O\r\nVALUE=2.22\r\nFACTOR=invalid\r\nOFFSET=12.0")]
        [TestCase("QUANTITY=dischargebnd\r\nFILENAME=channeleastdis.pli\r\nFILETYPE=9\r\nMETHOD=3\r\nOPERAND=O\r\nVALUE=2.22\r\nFACTOR=1.2\r\nOFFSET=invalid")]
        public void Parse_InvalidFormattedValue_ThrowsFormatException(string str)
        {
            ExtForceFileParser parser = CreateParser();

            Assert.That(() => parser.Parse(str), Throws.TypeOf<FormatException>()
                                                       .And.Message.Contain("Cannot convert value"));
        }
        
        [Test]
        public void Parse_ValidStream_KeepsStreamOpen()
        {
            ExtForceFileParser parser = CreateParser();

            using (var stream = new MemoryStream())
            {
                parser.Parse(stream);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        [Test]
        public void Parse_ValidStreamReaderFromStream_KeepsStreamOpen()
        {
            ExtForceFileParser parser = CreateParser();

            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream))
            {
                parser.Parse(streamReader);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        private static ExtForceFileParser CreateParser()
        {
            return new ExtForceFileParser();
        }
    }
}