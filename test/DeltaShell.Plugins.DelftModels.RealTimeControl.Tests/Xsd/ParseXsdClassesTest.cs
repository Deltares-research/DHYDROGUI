using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.NGHS.IO.FileReaders;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Xsd
{
    [TestFixture]
    public class ParseXsdClassesTest
    {
        public static IEnumerable<TestCaseData> GetReadingXmlObjectData()
        {
            object ReadFunc<T>(DelftConfigXmlFileParser configParser, string path) where T : class => 
                configParser.Read<T>(path);

            yield return new TestCaseData("timeseries_import.xml", 
                                          (Func<DelftConfigXmlFileParser, string, object>) ReadFunc<TimeSeriesCollectionComplexType>);
            yield return new TestCaseData("rtcToolsConfig.xml", 
                                          (Func<DelftConfigXmlFileParser, string, object>) ReadFunc<RtcToolsConfigComplexType>);
            yield return new TestCaseData("rtcRuntimeConfig.xml", 
                                          (Func<DelftConfigXmlFileParser, string, object>) ReadFunc<RtcRuntimeConfigComplexType>);
            yield return new TestCaseData("rtcDataConfig.xml", 
                                          (Func<DelftConfigXmlFileParser, string, object>) ReadFunc<RTCDataConfigComplexType>);

        }

        [Test]
        [TestCaseSource(nameof(GetReadingXmlObjectData))]
        public void ReadingXmlObject_DoesNotThrowException(string fileName,
                                                           Func<DelftConfigXmlFileParser, string, object> readFunc)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);

            const string directory = "XsdClassesXml";
            string path = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), directory, fileName));

            Assert.True(File.Exists(path), $"File path '{path}' should exist.");

            // Call 
            object dataAccessModel = readFunc(delftConfigXmlParser, path);

            // Assert
            Assert.NotNull(dataAccessModel);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }
    }
}