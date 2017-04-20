using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqReaderHelperTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Warn);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        [Test]
        public void ParseDateTime()
        {
            Assert.AreEqual(new DateTime(2010, 1, 2, 3, 4, 5), SobekWaqReaderHelper.ParseDateTime("2010/01/02-03:04:05"));
        }

        [Test]
        public void ParseTimeStep()
        {
            const string timeStepText = "000010018";

            var timeStep = SobekWaqReaderHelper.ParseTimeStep(timeStepText, "YYDDHHMMSS");
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), timeStep);

            timeStep = SobekWaqReaderHelper.ParseTimeStep(timeStepText, "YYDDHHMM");
            Assert.AreEqual(new TimeSpan(1, 0, 18, 0), timeStep);

            timeStep = SobekWaqReaderHelper.ParseTimeStep(timeStepText, "YYDDDH");
            Assert.AreEqual(new TimeSpan(366, 8, 0, 0), timeStep);
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no valid time step was found")]
        public void ParseTimeStepThrowsWhenNoTimeStepWasFound()
        {
            const string timeStepText = "text";

            SobekWaqReaderHelper.ParseTimeStep(timeStepText, "YYDDHHMMSS");
        }

        [Test]
        public void CreateDoubleDictionary()
        {
            var keys = new[] { "Test1", "Test2" };
            var values = new[] { 10, 10.5 };

            var dictionary = SobekWaqReaderHelper.CreateDoubleDictionary(keys, values, "Warning message");

            Assert.AreEqual(2, dictionary.Values.Count);
            Assert.IsTrue(dictionary.ContainsKey("Test1"));
            Assert.IsTrue(dictionary.ContainsKey("Test2"));
            Assert.AreEqual(10.0, dictionary["Test1"]);
            Assert.AreEqual(10.5, dictionary["Test2"]);
        }

        [Test]
        public void CreateDoubleDictionaryWithLessValuesThanKeys()
        {
            var keys = new[] { "Test1", "Test2", "Test3" };
            var values = new[] { 10, 10.5 };

            var dictionary = SobekWaqReaderHelper.CreateDoubleDictionary(keys, values, "Warning message");

            Assert.AreEqual(2, dictionary.Values.Count);
            Assert.IsTrue(dictionary.ContainsKey("Test1"));
            Assert.IsTrue(dictionary.ContainsKey("Test2"));
            Assert.AreEqual(10.0, dictionary["Test1"]);
            Assert.AreEqual(10.5, dictionary["Test2"]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => SobekWaqReaderHelper.CreateDoubleDictionary(keys, values, "Warning message"));
            Assert.IsTrue(log.Contains("Warning message"));
        }

        [Test]
        public void CreateDoubleDictionaryWithLessKeysThanValues()
        {
            var keys = new[] { "Test1", "Test2" };
            var values = new[] { 10, 10.5, 11.5 };

            var dictionary = SobekWaqReaderHelper.CreateDoubleDictionary(keys, values, "Warning message");

            Assert.AreEqual(2, dictionary.Values.Count);
            Assert.IsTrue(dictionary.ContainsKey("Test1"));
            Assert.IsTrue(dictionary.ContainsKey("Test2"));
            Assert.AreEqual(10.0, dictionary["Test1"]);
            Assert.AreEqual(10.5, dictionary["Test2"]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => SobekWaqReaderHelper.CreateDoubleDictionary(keys, values, "Warning message"));
            Assert.IsTrue(log.Contains("Warning message"));
        }

        [Test]
        public void GetUnCommentedText()
        {
            var text = "Test1\r\n" +
                       ";;Test2\r\n" +
                       "Test3\r\n" +
                       ";;Test4\r\n" +
                       ";;Test5\r\n" +
                       "Test6\r\n";

            text = SobekWaqReaderHelper.GetUnCommentedText(text, ";;");

            Assert.AreEqual("Test1\r\nTest3\r\nTest6\r\n", text);
        }

        [Test]
        public void GetTextLines()
        {
            const string text = "Test1\r\n" +
                                ";Test2\r\n";

            var textLines = SobekWaqReaderHelper.GetTextLines(text);
            Assert.AreEqual(2, textLines.Count());
            Assert.AreEqual("Test1", textLines.ElementAt(0));
            Assert.AreEqual(";Test2", textLines.ElementAt(1));

            textLines = SobekWaqReaderHelper.GetTextLines(text, ";");
            Assert.AreEqual(1, textLines.Count());
            Assert.AreEqual("Test1", textLines.ElementAt(0));
        }

        [Test]
        public void GetTextBlock()
        {
            const string text = "CONCENTRATION\r\n" +
                                "TEXT1\r\n" +
                                "TEXT2\r\n" +
                                "DATA\r\n" +
                                "CONCENTRATION\r\n" +
                                "TEXT3\r\n" +
                                "TEXT4\r\n" +
                                "DATA\r\n" +
                                "CONCENTRATION\r\n" +
                                "TEXT5\r\n" +
                                "TEXT6\r\n" +
                                "DATA";

            var textBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONCENTRATION");
            Assert.AreEqual(text, textBlock);

            textBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONCENTRATION", "DATA");
            Assert.AreEqual("CONCENTRATION\r\nTEXT1\r\nTEXT2\r\nDATA", textBlock);
        }

        [Test]
        public void GetTextBlockWithExtraStartingAndEndingBlankSpace()
        {
            const string text = " CONCENTRATION \r\n" +
                                " TEXT1 \r\n" +
                                " TEXT2 \r\n" +
                                " DATA \r\n" +
                                " CONCENTRATION \r\n" +
                                " TEXT3 \r\n" +
                                " TEXT4 \r\n" +
                                " DATA \r\n" +
                                " CONCENTRATION \r\n" +
                                " TEXT5 \r\n" +
                                " TEXT6 \r\n" +
                                " DATA";

            var textBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONCENTRATION");
            Assert.AreEqual(text.TrimStart(), textBlock);

            textBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONCENTRATION", "DATA");
            Assert.AreEqual("CONCENTRATION \r\n TEXT1 \r\n TEXT2 \r\n DATA", textBlock);
        }

        [Test]
        public void GetTextBlocks()
        {
            const string text = "CONCENTRATION\r\n" +
                                "TEXT1\r\n" +
                                "TEXT2\r\n" +
                                "DATA\r\n" +
                                "CONCENTRATION\r\n" +
                                "TEXT3\r\n" +
                                "TEXT4\r\n" +
                                "DATA\r\n" +
                                "CONCENTRATION\r\n" +
                                "TEXT5\r\n" +
                                "TEXT6\r\n" +
                                "DATA";

            var textBlocks = SobekWaqReaderHelper.GetTextBlocks(text, "CONCENTRATION");
            Assert.AreEqual(1, textBlocks.Count());            
            Assert.AreEqual(text, textBlocks.ElementAt(0));

            textBlocks = SobekWaqReaderHelper.GetTextBlocks(text, "CONCENTRATION", "DATA");
            Assert.AreEqual(3, textBlocks.Count());
            Assert.AreEqual("CONCENTRATION\r\nTEXT1\r\nTEXT2\r\nDATA", textBlocks.ElementAt(0));
            Assert.AreEqual("CONCENTRATION\r\nTEXT3\r\nTEXT4\r\nDATA", textBlocks.ElementAt(1));
            Assert.AreEqual("CONCENTRATION\r\nTEXT5\r\nTEXT6\r\nDATA", textBlocks.ElementAt(2));
        }

        [Test]
        public void GetTextBlocksWithExtraStartingAndEndingBlankSpace()
        {
            const string text = " CONCENTRATION \r\n" +
                                " TEXT1 \r\n" +
                                " TEXT2 \r\n" +
                                " DATA \r\n" +
                                " CONCENTRATION \r\n" +
                                " TEXT3 \r\n" +
                                " TEXT4 \r\n" +
                                " DATA \r\n" +
                                " CONCENTRATION \r\n" +
                                " TEXT5 \r\n" +
                                " TEXT6 \r\n" +
                                " DATA";

            var textBlocks = SobekWaqReaderHelper.GetTextBlocks(text, "CONCENTRATION");
            Assert.AreEqual(1, textBlocks.Count());
            Assert.AreEqual(text.TrimStart(), textBlocks.ElementAt(0));

            textBlocks = SobekWaqReaderHelper.GetTextBlocks(text, "CONCENTRATION", "DATA");
            Assert.AreEqual(3, textBlocks.Count());
            Assert.AreEqual("CONCENTRATION \r\n TEXT1 \r\n TEXT2 \r\n DATA", textBlocks.ElementAt(0));
            Assert.AreEqual("CONCENTRATION \r\n TEXT3 \r\n TEXT4 \r\n DATA", textBlocks.ElementAt(1));
            Assert.AreEqual("CONCENTRATION \r\n TEXT5 \r\n TEXT6 \r\n DATA", textBlocks.ElementAt(2));
        }

        [Test]
        public void GetTextBlockWithEmptyLinesBetweenFunctions()
        {
            const string text = "FUNCTIONS\r\n" +
                                "'Rad_01'\r\n" +
                                "DATA\r\n" +
                                "'1970/01/01-00:00:00' 55.20833\r\n" +
                                "'1970/01/02-00:00:00' 20.37037\r\n" +
                                "'1970/01/03-00:00:00' 34.375\r\n" +
                                "\r\n" +
                                "FUNCTIONS\r\n" +
                                "'VWind_01'\r\n" +
                                "DATA\r\n" +
                                "'1970/01/01-00:00:00' 5.7\r\n" +
                                "'1970/01/03-00:00:00' 9.3";

            var textBlock = SobekWaqReaderHelper.GetTextBlock(text, "DATA");
            Assert.AreEqual(102, textBlock.Length);
        }

        [Test]
        public void GetDouble()
        {
            Assert.AreEqual(12.3, SobekWaqReaderHelper.GetDouble("   12.3 \r\n"));
        }

        [Test]
        public void GetDoublesFromSingleTextLine()
        {
            var doubleValues = SobekWaqReaderHelper.GetDoublesFromSingleTextLine(" 10   10.5         11.5 \r\n");
            
            Assert.AreEqual(3, doubleValues.Count());
            Assert.AreEqual(10.0, doubleValues.ElementAt(0));
            Assert.AreEqual(10.5, doubleValues.ElementAt(1));
            Assert.AreEqual(11.5, doubleValues.ElementAt(2));
        }

        [Test]
        public void GetDoublesFromMultipleTextLines()
        {
            var doubleValues = SobekWaqReaderHelper.GetDoublesFromMultipleTextLines("\r\n 10 \r\n10.5\r\n         11.5 \r\n");

            Assert.AreEqual(3, doubleValues.Count());
            Assert.AreEqual(10.0, doubleValues.ElementAt(0));
            Assert.AreEqual(10.5, doubleValues.ElementAt(1));
            Assert.AreEqual(11.5, doubleValues.ElementAt(2));
        }
    }
}
