using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class SpatialFileDataReaderTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAMd1dFile_WhenReading_ThenAModelIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D.Network);

            Assert.AreEqual(267, waterFlowModel1D.Network.Branches.Count);
            Assert.AreEqual(212, waterFlowModel1D.Network.HydroNodes.Count());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnMd1dFile_WhenReadingAnIncorrectSpatialDataFile_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1dIncorrect.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

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
            var spatialFileDataReader = new SpatialFileDataReader(parseMock, convertMock, someErrorReportFunction);
            spatialFileDataReader.ReadSpatialFileData(filePath, null);

            Assert.IsTrue(errorHandlingHasBeenCalled);
            Assert.AreEqual("While reading the spatial data from the file, an error occured", errorHeader);
            Assert.AreEqual(1, loggedErrors.Count);
            Assert.AreEqual("A message", loggedErrors[0]);
        }





    }
}