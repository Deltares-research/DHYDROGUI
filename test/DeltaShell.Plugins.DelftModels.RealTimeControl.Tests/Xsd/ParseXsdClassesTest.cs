using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Xsd
{
    [TestFixture]
    public class ParseXsdClassesTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingStateImportXmlFilesDoesNotThrow()
        {
            var fileName = "state_import.xml";

            var directory = @"XsdClassesXml";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read<TreeVectorFileXML>(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<TreeVectorFileXML>(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<TreeVectorFileXML>(path), fileName);

            Assert.NotNull(dataAccesModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingTimeSeriesImportXmlFilesDoesNotThrown()
        {
            var fileName = "timeseries_import.xml";

            var directory = @"XsdClassesXml";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read<TimeSeriesCollectionComplexType>(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<TimeSeriesCollectionComplexType>(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<TimeSeriesCollectionComplexType>(path), fileName);

            var timeSeriesCollection = dataAccesModel;

            Assert.NotNull(timeSeriesCollection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingToolsConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcToolsConfig.xml";

            var directory = @"XsdClassesXml";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read<RtcToolsConfigXML>(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<RtcToolsConfigXML>(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<RtcToolsConfigXML>(path), fileName);

            var rtcToolsConfig = dataAccesModel as RtcToolsConfigXML;

            Assert.NotNull(rtcToolsConfig);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingRuntimeConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcRuntimeConfig.xml";

            var directory = @"XsdClassesXml";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read<RtcRuntimeConfigXML>(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<RtcRuntimeConfigXML>(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<RtcRuntimeConfigXML>(path), fileName);

            var rtcRuntimeConfig = dataAccesModel;

            Assert.NotNull(rtcRuntimeConfig);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingDataConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcDataConfig.xml";

            var directory = @"XsdClassesXml";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read<RTCDataConfigXML>(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<RTCDataConfigXML>(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<RTCDataConfigXML>(path), fileName);

            var rtcDataConfig = dataAccesModel;

            Assert.NotNull(rtcDataConfig);
        }
    }
}