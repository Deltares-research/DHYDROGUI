using System.IO;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.IO.LogFileReading;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrRunHelperTest
    {
        [Test]
        public void ConnectDimrRunLogFile_ShouldReadLogFileAndStoreInfoInDataItem()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                tempDirectory.CreateFile("dimr_redirected.log");
                const string text = "This is some text in the file.";

                var model = Substitute.For<IModel>();
                var logReaderMock = Substitute.For<ILogFileReader>();
                logReaderMock.ReadCompleteStream(Arg.Any<Stream>()).Returns(text);
                
                var dataItems = new EventedList<IDataItem>();
                var dataItem = Substitute.For<IDataItem>();
                dataItem.Tag.Returns("DimrRunLog");
                dataItem.Value.Returns(new TextDocument());
                dataItems.Add(dataItem);

                model.DataItems = dataItems;

                var sut = new DimrRunHelper(logReaderMock);
                
                // Act
                sut.ConnectDimrRunLogFile(model, tempDirectory.Path);

                // Assert
                Assert.AreEqual(text, ((TextDocument) dataItem.Value).Content);
            }
        }

        [Test]
        public void DimrRunLogfileDataItemTag_ShouldReturnCorrectTagName()
        {
            Assert.AreEqual("DimrRunLog", DimrRunHelper.DimrRunLogfileDataItemTag);
        }
    }
}