using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FunctionStores
{
    [TestFixture]
    public class FMNetCdfFileFunctionStoreTest
    {
        private const string derivedNcRefDateAttribute = "ncRefDate";

        private class DerivedFMNetCdfFileFunctionStore : FMNetCdfFileFunctionStore
        {
            public DerivedFMNetCdfFileFunctionStore(string ncPath) : base(ncPath) {}

            protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
            {
                NetCdfVariableInfo netCdfVariableInfo = dataVariables.First(v => v.IsTimeDependent);
                string referenceDate = netCdfVariableInfo.ReferenceDate;
                
                var timeSeries = Substitute.For<ITimeSeries>();
                IVariable<DateTime> functionTimeVariable = timeSeries.Time;
                functionTimeVariable.Attributes[derivedNcRefDateAttribute] = referenceDate;

              return new List<IFunction>{functionTimeVariable};
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"HisFiles\FlowFMWithTimeZones_his.nc", "Monday, 01 January 2001 00:00:00")]
        [TestCase(@"HisFiles\HarWithoutTimeZones_his.nc", "Wednesday, 09 January 2008 00:00:00")]
        public void CreatingADerivedFromFMNetCdfFileFunctionStoreObject_ShouldSetReferenceDateInFunctionsCorrectly(string hisFilePath, string expectedReferenceDate)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string hisFile = tempDirectory.CopyTestDataFileToTempDirectory(TestHelper.GetTestFilePath(hisFilePath));
                
                // Act
                var derivedFMNetCdfFileFunctionStore = new DerivedFMNetCdfFileFunctionStore(hisFile);
                
                // Assert
                Assert.IsInstanceOf<FMNetCdfFileFunctionStore>(derivedFMNetCdfFileFunctionStore);
                
                string retrievedReferenceDate = derivedFMNetCdfFileFunctionStore.Functions.First().Attributes[derivedNcRefDateAttribute];
                Assert.AreEqual(expectedReferenceDate, retrievedReferenceDate);
            }
        }
    }
}