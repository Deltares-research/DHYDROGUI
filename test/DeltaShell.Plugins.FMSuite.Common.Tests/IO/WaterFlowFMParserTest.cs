using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMParserTest
    {
        [Test]
        public void GetClrTypeTest()
        {
            var captionField = "";
            Assert.AreEqual(typeof(int), FMParser.GetClrType(null, "Integer", ref captionField, null, 0));
            Assert.AreEqual(typeof(double), FMParser.GetClrType(null, "Double", ref captionField, null, 0));
            Assert.AreEqual(typeof(IList<double>), FMParser.GetClrType(null, "DoubleArray", ref captionField, null, 0));
            Assert.AreEqual(typeof(DateTime), FMParser.GetClrType(null, "DateTime", ref captionField, null, 0));
            Assert.AreEqual(typeof(string), FMParser.GetClrType(null, "String", ref captionField, null, 0));
            Assert.AreEqual(typeof(string), FMParser.GetClrType(null, "FileName", ref captionField, null, 0));
            Assert.AreEqual(typeof(bool), FMParser.GetClrType(null, "0|1", ref captionField, null, 0));
            Assert.AreEqual(typeof(bool), FMParser.GetClrType(null, "1|0", ref captionField, null, 0));
            Assert.AreEqual(typeof(Steerable), FMParser.GetClrType(null, "Steerable", ref captionField, null, 0));
            captionField = "Test:1|2|3|4";
            Assert.IsNotNull(FMParser.GetClrType("TestVar", "T|e|s|t", ref captionField, null, 0));
            captionField = "Test:a|b|c|d";
            Assert.IsNotNull(FMParser.GetClrType("TestVar", "T|e|s|t", ref captionField, null, 0));

            captionField = "It's a syntax error if no colon is specified when using '|' characters";
            Assert.Throws<FormatException>(() => FMParser.GetClrType(null, "T|e|s|t", ref captionField, null, 0), "");
            captionField = "Syntax error: Number of '|' characters should be the same as in typefield";
            Assert.Throws<FormatException>(() => FMParser.GetClrType(null, "T|e|s|t", ref captionField, null, 0), "");
            Assert.Throws<ArgumentException>(() => FMParser.GetClrType(null, "I am not defined", ref captionField, null, 0), "");
        }

        [Test]
        public void GetDoubleArrayTest()
        {
            var list = FMParser.FromString<IList<double>>("");
            Assert.AreEqual(0, list.Count);

            // Can deal with ints:
            list = FMParser.FromString<IList<double>>("1");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new[]{1.0}, list);

            // Always read as Culture Invariant:
            list = FMParser.FromString<IList<double>>("1.000");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new[] { 1.0 }, list);
            list = FMParser.FromString<IList<double>>("1,000");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new[] { 1000.0 }, list);

            // Can deal with space separated:
            list = FMParser.FromString<IList<double>>("1.0 2 3.2 4 5.4");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[]{1.0, 2.0, 3.2, 4.0, 5.4}, list);

            // Can deal with tab separated:
            list = FMParser.FromString<IList<double>>("1.0\t2\t3.2\t4\t5.4");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[] { 1.0, 2.0, 3.2, 4.0, 5.4 }, list);

            // Can deal with tab and space separated combined:
            list = FMParser.FromString<IList<double>>("1.0\t2 3.2\t4 5.4");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[] { 1.0, 2.0, 3.2, 4.0, 5.4 }, list);

            Assert.Throws<ArgumentNullException>(() => FMParser.FromString<IList<double>>(null));
        }

        [Test]
        [SetCulture("NL-nl")]
        public void ToStringTestInNlCulture()
        {
            Assert.AreEqual("1.2", FMParser.ToString(1.2, typeof(double)));
            Assert.AreEqual("1.2 3.4", FMParser.ToString(new List<double>(new[]{1.2, 3.4}), typeof(IList<double>)));
        }

        [Test]
        public void ToStringForSteerable()
        {
            var steerable = new Steerable
                {
                    ConstantValue = 1.2,
                    TimeSeriesFilename = "weir01_crest_level.tim",
                    Mode = SteerableMode.ConstantValue
                };
            Assert.AreEqual("1.2", FMParser.ToString(steerable, typeof(Steerable)));

            steerable.Mode = SteerableMode.TimeSeries;

            Assert.AreEqual("weir01_crest_level.tim", FMParser.ToString(steerable, typeof(Steerable)));

            steerable.Mode = SteerableMode.External;

            Assert.AreEqual("REALTIME", FMParser.ToString(steerable, typeof(Steerable)));
        }

        [Test]
        public void FromStringTest()
        {
            var captionField = "Test:1|2|3|4";
            var dataType = FMParser.GetClrType("TestVar", "T|e|s|t", ref captionField, null, 0);

            Assert.Throws<FormatException>(() => FMParser.FromString("1", dataType));
            Assert.AreEqual(dataType.GetEnumValues().GetValue(1), FMParser.FromString("e", dataType));
        }
    }
}