using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.IO.LogFileReading;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffRunLogFilesTests
    {
        [Test]
        public void ConstructorThrowsArgumentNullException_WhenLogFileReaderIsNull()
        {
            // Arrange
            var rainfallRunoffModelMock = Substitute.For<IRainfallRunoffModel>();
            void Call() => new RainfallRunoffRunLogFiles(null,rainfallRunoffModelMock);

            // Act, Assert 
            var exception = Assert.Throws<ArgumentNullException>(Call);
            StringAssert.AreEqualIgnoringCase("logFileReader", exception.ParamName);
        }
        [Test]
        public void ConstructorThrowsArgumentNullException_WhenRainfallRunoffModelIsNull()
        {
            // Arrange
            var logMock = Substitute.For<ILogFileReader>();
            void Call() => new RainfallRunoffRunLogFiles(logMock,null);

            // Act, Assert 
            var exception = Assert.Throws<ArgumentNullException>(Call);
            StringAssert.AreEqualIgnoringCase("rainfallRunoffModel", exception.ParamName);
        }

        [TestCase(null)]
        [TestCase("")]
        public void ConnectLoggingFiles_ThrowsArgumentNullException_WhenParameterIsNullOrEmpty(string outputPath)
        {
            // Arrange
            var logMock = Substitute.For<ILogFileReader>();
            var rainfallRunoffModelMock = Substitute.For<IRainfallRunoffModel>();

            var sut = new RainfallRunoffRunLogFiles(logMock,rainfallRunoffModelMock);

            void Call() => sut.ConnectLoggingFiles(outputPath);

            // Act, Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            StringAssert.AreEqualIgnoringCase("outputPath", exception.ParamName );
        }

        [Test]
        public void Clear_RemovesDataItems_WhenFound()
        {
            // Arrange
            var dataItemsCollection = new EventedList<IDataItem>();
            var runDataItem = Substitute.For<IDataItem>();
            runDataItem.Tag.Returns(RainfallRunoffOutputFiles.LogFileName);
            var reportDataItem = Substitute.For<IDataItem>();
            reportDataItem.Tag.Returns(RainfallRunoffOutputFiles.RunReportFilename);
            dataItemsCollection.Add(runDataItem);
            dataItemsCollection.Add(reportDataItem);
            
            var logMock = Substitute.For<ILogFileReader>();
            var rainfallRunoffModelMock = Substitute.For<IRainfallRunoffModel>();
            rainfallRunoffModelMock.DataItems.Returns(dataItemsCollection);

            var sut = new RainfallRunoffRunLogFiles(logMock, rainfallRunoffModelMock);

            // Act
            sut.Clear();

            // Assert
            CollectionAssert.IsEmpty(dataItemsCollection);
        }
        
        
        [Test]
        public void ConnectLoggingFiles_NoDataItemsCreated_WhenFilesNotFound()
        {
            // Arrange
            var logMock = Substitute.For<ILogFileReader>();
            var rainfallRunoffModelMock = Substitute.For<IRainfallRunoffModel>();
        
            var dataItemsList =  new EventedList<IDataItem>();
            rainfallRunoffModelMock.DataItems.Returns(dataItemsList);
        
            var sut = new RainfallRunoffRunLogFiles(logMock, rainfallRunoffModelMock);
        
            using (var tempDir = new TemporaryDirectory())
            {
                // No actual temp log file created.
        
                // Act
                sut.ConnectLoggingFiles(tempDir.Path);
            }
            
            // Assert
            CollectionAssert.IsEmpty(dataItemsList);
        }

        [TestCase("3b_bal.out")]
        [TestCase("sobek_3b.log")]
        public void ConnectLoggingFiles_CreatesDataItem_WhenNoDataItemAvailableAndFileAvailable(string logFileName)
        {
            // Arrange
            var logMock = Substitute.For<ILogFileReader>();
            const string someData = "someData";
            logMock.ReadCompleteStream(Arg.Any<Stream>()).Returns(someData); 
            var rainfallRunoffModelMock = Substitute.For<IRainfallRunoffModel>();
        
            var dataItemsCol =  new EventedList<IDataItem>();
            rainfallRunoffModelMock.DataItems.Returns(dataItemsCol);
        
            var sut = new RainfallRunoffRunLogFiles(logMock, rainfallRunoffModelMock);
        
            using (var tempDir = new TemporaryDirectory())
            {
                // create file in temp dir to make the code run for log file
                string tempFile = tempDir.CreateFile(logFileName);
        
                // Act
                sut.ConnectLoggingFiles(tempDir.Path);
            }
        
            // Assert
            Assert.AreEqual(1, dataItemsCol.Count);
            IDataItem logFileDataItem = dataItemsCol.First();
            var logFileText = (TextDocument)logFileDataItem.Value;
            StringAssert.AreEqualIgnoringCase(someData, logFileText.Content);
            StringAssert.AreEqualIgnoringCase(logFileName, logFileDataItem.Tag);
        }

        [TestCase("3b_bal.out")]
        [TestCase("sobek_3b.log")]
        public void ConnectLoggingFiles_ReUsesLogDataItems_WhenAvailable(string logFileName)
        {
            // Arrange
            var logMock = Substitute.For<ILogFileReader>();
            const string someData = "someData";
            logMock.ReadCompleteStream(Arg.Any<Stream>()).Returns(someData); 
            var rainfallRunoffModelMock = Substitute.For<IRainfallRunoffModel>();
        
            var dataItemList =  new EventedList<IDataItem>();
            rainfallRunoffModelMock.DataItems.Returns(dataItemList);
        
            var logFileTextDocument = new TextDocument { Content = "Original text" };
            var logFileDataItemMock = Substitute.For<IDataItem>();
            logFileDataItemMock.Tag.Returns(logFileName);
            logFileDataItemMock.Value.Returns(logFileTextDocument);
            
            dataItemList.Add(logFileDataItemMock);
        
            var sut = new RainfallRunoffRunLogFiles(logMock,rainfallRunoffModelMock);
        
            using (var tempDir = new TemporaryDirectory())
            {
                // create 1 files in temp dir to make the code run for parameterized log file
                string tempFile = tempDir.CreateFile(logFileName);
        
                // Act
                sut.ConnectLoggingFiles(tempDir.Path);
            }
        
            // Assert
            Assert.AreEqual(1, dataItemList.Count);
            IDataItem logFileDataItem = dataItemList.First();
            Assert.AreSame(logFileDataItemMock, logFileDataItem);
            var logFileText = (TextDocument)logFileDataItem.Value;
            Assert.AreSame(logFileTextDocument, logFileText);
            StringAssert.AreEqualIgnoringCase(someData, logFileText.Content);
        }

    }
}