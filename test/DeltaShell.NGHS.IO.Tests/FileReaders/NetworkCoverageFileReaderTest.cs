using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class NetworkCoverageFileReaderTest
    {
        [Test]
        public void GivenSpatialFileData_WhenAnErrorOccursDuringReadSpatialFileData_ThenAnExceptionIsThrown()
        {
            const string filePath = "blah.file";
            const string errorMessage = "A message";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            var parseMock = mocks.StrictMock<Func<string, IList<DelftIniCategory>>>();
            parseMock.Expect(e => e.Invoke(filePath)).Return(null).Throw(new Exception(errorMessage)).Repeat.Once();

            var convertMock = mocks.StrictMock<Func<IList<DelftIniCategory>, IList<IChannel>, IList<string>, INetworkCoverage>>();
            convertMock.Expect(e => e.Invoke(null, null, null)).Return(null).IgnoreArguments().Repeat.Once();

            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            mocks.ReplayAll();
            var spatialFileDataReader = new NetworkCoverageFileReader(parseMock, convertMock, someErrorReportFunction);
            spatialFileDataReader.ReadSpatialFileData(filePath, null);

            Assert.IsTrue(errorHandlingHasBeenCalled);
            Assert.AreEqual("While reading the spatial data from the file, an error occured", errorHeader);
            Assert.AreEqual(1, loggedErrors.Count);
            Assert.AreEqual("A message", loggedErrors[0]);
        }

        [Test]
        public void GivenSpatialDataFileWithoutContentHeader_WhenReadingSpatialData_ThenWarningMessageIsReturnedAboutDefaultValue()
        {
            // Given
            var sourceFilePath = TestHelper.GetTestFilePath(@"FileReaders\NetworkCoverageFileReader\InitialDischargeWithoutContentHeader.ini");
            var filePath = TestHelper.CreateLocalCopy(sourceFilePath);

            var messages = new List<string>();
            var channel = new Channel
            {
                Name = "Maasmond",
                Length = 300.0
            };
            var channels = new List<IChannel> {channel};

            // When
            var fileReader = new NetworkCoverageFileReader((header, errorMessages) => messages.AddRange(errorMessages));
            fileReader.ReadSpatialFileData(filePath, channels);

            // Then
            var expectedMessage = $"Spatial data file at location '{filePath}' does not contain a '{SpatialDataRegion.ContentIniHeader}' tab. The corresponding interpolation type has been set to Constant.";
            Assert.Contains(expectedMessage, messages);
        }
    }
}