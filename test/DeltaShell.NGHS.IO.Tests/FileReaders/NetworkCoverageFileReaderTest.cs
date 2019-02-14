using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
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
    }
}