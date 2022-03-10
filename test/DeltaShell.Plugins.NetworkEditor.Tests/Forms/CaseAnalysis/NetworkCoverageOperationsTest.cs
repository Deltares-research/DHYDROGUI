using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CaseAnalysis
{
    [TestFixture]
    public class NetworkCoverageOperationsTest
    {
        private static readonly Random Random = new Random();

        public static NetworkCoverage CreateRandomNetworkCoverage(IHydroNetwork network, int numberOfYears = 4)
        {
            var networkCoverage = new NetworkCoverage("coverage " + Random.Next(100), true) { Network = network };

            var dates = new List<DateTime>(numberOfYears);
            for (int i = 0; i < numberOfYears; i++)
            {
                dates.Add(new DateTime(2000 + i,1,1));
            }
            var locations = new List<NetworkLocation>();

            foreach (var branch in network.Branches)
            {
                for (int offset = 0; offset <= branch.Length; offset += 10)
                {
                    locations.Add(new NetworkLocation(branch, offset));
                }
            }

            networkCoverage.Time.SetValues(dates);
            networkCoverage.Locations.SetValues(locations);

            var numValues = dates.Count * locations.Count;

            var values = new double[numValues];

            for (int i = 0; i < numValues; i++)
            {
                values[i] = Random.NextDouble() * 5;
            }

            networkCoverage.Components[0].SetValues(values);

            return networkCoverage;
        }

        #region Min operation

        [Test]
        public void CoverageMinOperationWithNoDataValues()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 10.0, 20.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new []
                {
                    1.0,  -999.0, -999.0,
                    5.0,  -5.0,   -999.0,
                    -1.0, -2.0,   -999.0
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageMinOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage);

            var expectedValues = new[]
                {
                    -1.0, -5.0, -999.0
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }
        }

        [Test]
        public void CoverageMaxOperationWithNoDataValues()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 10.0, 20.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new[]
                {
                    1.0,  -999.0, -999.0,
                    5.0,  -5.0,   -999.0,
                    -1.0, -2.0,   -999.0
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageMaxOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage);

            var expectedValues = new[]
                {
                    5.0, -2.0, -999.0
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }
        }

        [Test]
        public void CoverageMeanOperationWithNoDataValues()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 10.0, 20.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new[]
                {
                    1.0,  -999.0, -999.0,
                    5.0,  -5.0,   -999.0,
                    -1.0, -2.0,   -999.0
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageMeanOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage);

            var expectedValues = new[]
                {
                    5.0/3.0, -3.5, -999.0
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }
        }

        #endregion

        #region Scalar add operation

        [Test]
        public void ScalarAdd()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);

            var timeDependentCoverage = CreateRandomNetworkCoverage(network);

            var tempCoverage = CreateRandomNetworkCoverage(network);
            var nonTimeDependentCoverage = tempCoverage.AddTimeFilter(tempCoverage.Time.Values[0]);

            var numTimesteps = timeDependentCoverage.Time.Values.Count;

            var valuesForLocation = timeDependentCoverage.GetTimeSeries(timeDependentCoverage.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();
            var valueForLocation = (double)nonTimeDependentCoverage.Components[0].Values[0];

            Assert.IsFalse(nonTimeDependentCoverage.IsTimeDependent);

            var addOperation = new NetworkCoverageOperations.CoverageAddOperation();

            var result = addOperation.Perform(timeDependentCoverage, nonTimeDependentCoverage);

            Assert.IsTrue(result.IsTimeDependent);

            var resultValuesForLocation = result.GetTimeSeries(result.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();

            Assert.AreEqual(valuesForLocation.Count, resultValuesForLocation.Count);

            for (int i = 0; i < numTimesteps; i++)
            {
                Assert.AreEqual(valuesForLocation[i] + valueForLocation, resultValuesForLocation[i]);
            }
        }

        [Test]
        public void ScalarAddWithNoDataValues()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);

            var timeDependentCoverage = CreateRandomNetworkCoverage(network);
            timeDependentCoverage.Components[0].NoDataValue = -999.99;

            var tempCoverage = CreateRandomNetworkCoverage(network);
            tempCoverage.Components[0].NoDataValue = -1.2;
            var nonTimeDependentCoverage = tempCoverage.AddTimeFilter(tempCoverage.Time.Values[0]);
            nonTimeDependentCoverage.Components[0].Values[0] = -1.2;

            var numTimesteps = timeDependentCoverage.Time.Values.Count;

            var valuesForLocation = timeDependentCoverage.GetTimeSeries(timeDependentCoverage.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();

            Assert.IsFalse(nonTimeDependentCoverage.IsTimeDependent);

            var addOpperation = new NetworkCoverageOperations.CoverageAddOperation();

            var result = addOpperation.Perform(timeDependentCoverage, nonTimeDependentCoverage);

            Assert.IsTrue(result.IsTimeDependent);

            var resultValuesForLocation = result.GetTimeSeries(result.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();

            Assert.AreEqual(valuesForLocation.Count, resultValuesForLocation.Count);

            for (int i = 0; i < numTimesteps; i++)
            {
                Assert.AreEqual(valuesForLocation[i], resultValuesForLocation[i]);
            }

            nonTimeDependentCoverage.Components[0].Values[0] = 2.4;
            timeDependentCoverage.AddValuesForTime(
                Enumerable.Repeat(timeDependentCoverage.Components[0].NoDataValue, 4),
                timeDependentCoverage.Time.Values[0]);

            result = addOpperation.Perform(timeDependentCoverage, nonTimeDependentCoverage);

            Assert.IsTrue(result.IsTimeDependent);

            resultValuesForLocation = result.GetTimeSeries(result.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();

            Assert.AreEqual(valuesForLocation.Count, resultValuesForLocation.Count);

            // Source value is 'No Data'
            Assert.AreEqual(2.4, resultValuesForLocation[0]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ScalarAddWithNetCdf()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);

            var timeDependentCoverage = CreateRandomNetworkCoverage(network);

            timeDependentCoverage.Arguments[0].FixedSize = -1;
            timeDependentCoverage.Arguments[1].FixedSize = timeDependentCoverage.Arguments[1].Values.Count;

            var store = new NetCdfFunctionStore();
            string tempFileName = System.IO.Path.GetTempFileName();
            store.CreateNew(tempFileName);
            store.Functions.Add(timeDependentCoverage);

            var tempCoverage = CreateRandomNetworkCoverage(network);
            var nonTimeDependentCoverage = tempCoverage.AddTimeFilter(tempCoverage.Time.Values[0]);

            var numTimesteps = timeDependentCoverage.Time.Values.Count;

            var valuesForLocation = timeDependentCoverage.GetTimeSeries(timeDependentCoverage.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();
            var valueForLocation = (double)nonTimeDependentCoverage.Components[0].Values[0];

            Assert.IsFalse(nonTimeDependentCoverage.IsTimeDependent);

            var addOpperation = new NetworkCoverageOperations.CoverageAddOperation();

            var result = addOpperation.Perform(timeDependentCoverage, nonTimeDependentCoverage);

            Assert.IsTrue(result.IsTimeDependent);

            var resultValuesForLocation = result.GetTimeSeries(result.Locations.Values[0]).Components[0].Values.OfType<double>().ToList();

            Assert.AreEqual(valuesForLocation.Count, resultValuesForLocation.Count);

            for (int i = 0; i < numTimesteps; i++)
            {
                Assert.AreEqual(valuesForLocation[i] + valueForLocation, resultValuesForLocation[i]);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ScalarAddPerformace()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(100);

            var timeDependentCoverage = CreateRandomNetworkCoverage(network, 50);

            var tempCoverage = CreateRandomNetworkCoverage(network);
            var nonTimeDependentCoverage = tempCoverage.AddTimeFilter(tempCoverage.Time.Values[0]);

            var addOpperation = new NetworkCoverageOperations.CoverageAddOperation();

            TestHelper.AssertIsFasterThan(35000, () => addOpperation.Perform(timeDependentCoverage, nonTimeDependentCoverage));
        }

        #endregion

        #region GreaterThanOperation

        [Test]
        public void GreaterThanAsDoubleOperation()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 10.0, 20.0, 30.0, 40.0, 50.0, 60.0, 70.0, 80.0, 90.0, 100.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var numValues = dates.Length * branchChainages.Length; // should be 33

            var values = new double[numValues];

            for (int i = 0; i < numValues; i++)
            {
                values[i] = i;
            }
            values[32] = -999.0;

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageGreaterThanAsDoubleOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage, 16.5);

            var expectedValues = new[,]
                {
                    {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
                    {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0},
                    {1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 
                        -999.0}// No data value
                };

            int dateTimeCounter = 0;
            foreach (var dateTime in dates)
            {
                int networkLocationCounter = 0;
                foreach (var networkLocation in networkCoverage.Locations.Values)
                {
                    Assert.AreEqual(expectedValues[dateTimeCounter, networkLocationCounter], result[dateTime, networkLocation],
                        String.Format("Expected value for time = {0} and location = {1} not matching", dateTime, networkLocation));
                    networkLocationCounter++;
                }
                dateTimeCounter++;
            }

            Assert.IsFalse(ReferenceEquals(result, networkCoverage));
            Assert.AreEqual("", result.Components[0].Unit.Name);
            Assert.AreEqual("", result.Components[0].Unit.Symbol);
        }

        #endregion

        #region LessThanOperation

        [Test]
        public void LessThanOperationAsDoubleOperation()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 10.0, 20.0, 30.0, 40.0, 50.0, 60.0, 70.0, 80.0, 90.0, 100.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var numValues = dates.Length * branchChainages.Length; // should be 33

            var values = new double[numValues];

            for (int i = 0; i < numValues; i++)
            {
                values[i] = i;
            }
            values[32] = -999.0;

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageLessThanAsDoubleOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage, 14.5);

            var expectedValues = new[,]
                {
                    {1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0},
                    {1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
                    {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 
                        -999.0} // No data value
                };

            int dateTimeCounter = 0;
            foreach (var dateTime in dates)
            {
                int networkLocationCounter = 0;
                foreach (var networkLocation in networkCoverage.Locations.Values)
                {
                    Assert.AreEqual(expectedValues[dateTimeCounter, networkLocationCounter], result[dateTime, networkLocation],
                        String.Format("Expected value for time = {0} and location = {1} not matching", dateTime, networkLocation));
                    networkLocationCounter++;
                }
                dateTimeCounter++;
            }

            Assert.IsFalse(ReferenceEquals(result, networkCoverage));
            Assert.AreEqual("", result.Components[0].Unit.Name);
            Assert.AreEqual("", result.Components[0].Unit.Symbol);
        }

        #endregion

        #region DurationMeasured operation (not interpolated over time)

        [Test]
        public void GreaterThanDurationAsDoubleOperation()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 100.0 / 10.0, 200.0 / 10.0, 300.0 / 10.0, 400.0 / 10.0, 500.0 / 10.0, 600.0 / 10.0, 700.0 / 10.0, 800.0 / 10.0, 900.0 / 10.0, 100.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new[]
                {
                    0, 9, 5,  11, 8,  14, 2,  18, 21,     2,      -999.0,
                    1, 3, 10, 12, 0,  1,  16, 19, -999.0, -999.0, -999.0,
                    2, 4, 6,  7,  13, 15, 17, 20, 2,      22,     -999.0,
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageGreaterThanDurationAsDoubleOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage, 8.5);

            // Expected output for GreaterThan operation
            //  0       9      5      11     8      14     2      18    21      2     -999.0
            // {false, true,  false, true,  false, true,  false, true, true,   false, ? }
            //  1       3      10     12     0      1      16    19    -999.0  -999.0 -999.0
            // {false, false, true,  true,  false, false, true,  true,   ?,     ?,    ?   }
            //  2       4      6      7      13     15     17    20      2      22    -999.0
            // {false, false, false, false, true,  true,  true,  true, false,  true,  ? }

            var dt1 = dates[1] - dates[0];
            var dt2 = dates[2] - dates[1];

            var span1 = new TimeSpan(0);
            var span2 = dt1;
            var span3 = dt2;
            var span4 = dt1 + dt2;
            var span5 = new TimeSpan(0);
            var span6 = dt1;
            var span7 = dt2;
            var span8 = dt1 + dt2;
            var span9 = dt1 + dt2;
            var span10 = new TimeSpan(0);
            var span11 = new TimeSpan(0);

            var expectedValues = new[]
                {
                    span1.TotalDays, span2.TotalDays, span3.TotalDays, span4.TotalDays, span5.TotalDays, 
                    span6.TotalDays, span7.TotalDays, span8.TotalDays, span9.TotalDays, span10.TotalDays, span11.TotalDays
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }

            Assert.IsFalse(ReferenceEquals(result, networkCoverage));
            Assert.AreEqual("", result.Components[0].Unit.Name);
            Assert.AreEqual("", result.Components[0].Unit.Symbol);
        }
    
        [Test]
        public void LessThanDurationAsDoubleOperation()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 100.0 / 10.0, 200.0 / 10.0, 300.0 / 10.0, 400.0 / 10.0, 500.0 / 10.0, 600.0 / 10.0, 700.0 / 10.0, 800.0 / 10.0, 900.0 / 10.0, 100.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new[]
                {
                    9,  8,  14, 6,  17, 3,  20, -1, 21,     5,      -999.0,
                    10, 12, 7,  5,  18, 19, 1,  -2, -999.0, -999.0, -999.0,
                    11, 13, 15, 16, 4,  2,  0,  -3, -4,     22,     -999.0
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageLessThanDurationAsDoubleOperation();

            INetworkCoverage result = coverageOperation.Perform(networkCoverage, 8.5);

            // Expected output for LessThan operation
            //  9       8      14      6     17      3    20      -1    21     -5    -999.0
            // {false, true,  false, true,  false, true,  false, true, false, true,  ?}
            //  10      12      7     5      18     19      1     -2   -999.0 -999.0 -999.0
            // {false, false, true,  true,  false, false, true,  true, ?,     ?,     ?}
            //  11      13      15     16     4     2     0      -3     -4    22     -999.0
            // {false, false, false, false, true,  true,  true,  true, true,  false, ?}

            var dt1 = dates[1] - dates[0];
            var dt2 = dates[2] - dates[1];
            var span1 = new TimeSpan(0);
            var span2 = dt1;
            var span3 = dt2;
            var span4 = dt1 + dt2;
            var span5 = new TimeSpan(0);
            var span6 = dt1;
            var span7 = dt2;
            var span8 = dt1 + dt2;
            var span9 = new TimeSpan(0);
            var span10 = dt1 + dt2;
            var span11 = new TimeSpan(0);

            var expectedValues = new[]
                {
                    span1.TotalDays, span2.TotalDays, span3.TotalDays, span4.TotalDays, span5.TotalDays,
                    span6.TotalDays, span7.TotalDays, span8.TotalDays, span9.TotalDays, span10.TotalDays, span11.TotalDays
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }

            Assert.IsFalse(ReferenceEquals(result, networkCoverage));
            Assert.AreEqual("", result.Components[0].Unit.Name);
            Assert.AreEqual("", result.Components[0].Unit.Symbol);
        } 

        #endregion

        #region DurationMeasured operation (linearly interpolated over time)

        [Test]
        public void GreaterThanDurationAsDoubleOperationLinear()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 100.0 / 10.0, 200.0 / 10.0, 300.0 / 10.0, 400.0 / 10.0, 500.0 / 10.0, 600.0 / 10.0, 700.0 / 10.0, 800.0 / 10.0, 900.0 / 10.0, 100.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new[]
                {
                    0, 9, 5, 11, 8, 14, 2, 18,   21,      -5,    -999.0,
                    1, 3, 10, 12, 0, 1, 16, 19, -999.0, -999.0, -999.0,
                    2, 4, 6, 7, 13, 15, 17, 20,   -4,     22,     -999.0
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageGreaterThanDurationAsDoubleOperation{ TimeInterpolationType = InterpolationType.Linear };

            INetworkCoverage result = coverageOperation.Perform(networkCoverage, 8.5);

            // Expected output for GreaterThan operation
            //  0       9      5      11     8      14     2      18   21     -5    -999.0
            // {false, true,  false, true,  false, true,  false, true, true,  false,  ?}
            //  1       3      10     12     0      1      16    19   -999.0 -999.0 -999.0
            // {false, false, true,  true,  false, false, true,  true, ?,     ?,     ?}
            //  2       4      6      7      13     15     17    20    -4    22     -999.0
            // {false, false, false, false, true,  true,  true,  true, false,  true, ?}

            var dt1 = dates[1] - dates[0];
            var dt2 = dates[2] - dates[1];

            var span1 = new TimeSpan(0);
            var span2 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 9, 3, true));
            var span3 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 5, 10, false)) + new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 10, 6, true));
            var span4 = dt1 + new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 12, 7, true));
            var span5 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 0, 13, false));
            var span6 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 14, 1, true)) + new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 1, 15, false));
            var span7 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 2, 16, false)) + dt2;
            var span8 = dt1 + dt2;
            var span9 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1 + dt2, 8.5, 21, -4, true));
            var span10 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1 + dt2, 8.5, -5, 22, false));
            var span11 = new TimeSpan(0);

            var expectedValues = new[]
                {
                    span1.TotalDays, span2.TotalDays, span3.TotalDays, span4.TotalDays, span5.TotalDays,
                    span6.TotalDays, span7.TotalDays, span8.TotalDays, span9.TotalDays, span10.TotalDays, span11.TotalDays
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }

            Assert.IsFalse(ReferenceEquals(result, networkCoverage));
            Assert.AreEqual("", result.Components[0].Unit.Name);
            Assert.AreEqual("", result.Components[0].Unit.Symbol);
        }

        [Test]
        public void LessThanDurationAsDoubleOperationLinear()
        {
            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            var branchChainages = new[] { 0.0, 100.0 / 10.0, 200.0 / 10.0, 300.0 / 10.0, 400.0 / 10.0, 500.0 / 10.0, 600.0 / 10.0, 700.0 / 10.0, 800.0 / 10.0, 900.0 / 10.0, 100.0 };

            var networkCoverage = CreateNetworkCoverage(dates, branchChainages);
            networkCoverage.Components[0].NoDataValues = new List<double> { -999.0 };

            var values = new[]
                {
                    9, 8, 14, 6, 17, 3, 20, -1,  -4,     21,     -999.0,
                    10, 12, 7, 5, 18, 19, 1, -2, -999.0, -999.0, -999.0,
                    11, 13, 15, 16, 4, 2, 0, -3,  22,     -5,     -999.0
                };

            networkCoverage.Components[0].SetValues(values);

            var coverageOperation = new NetworkCoverageOperations.CoverageLessThanDurationAsDoubleOperation{ TimeInterpolationType = InterpolationType.Linear };

            INetworkCoverage result = coverageOperation.Perform(networkCoverage, 8.5);

            // Expected output for LessThan operation
            //  9       8      14      6     17      3    20      -1    -4      21    -999.0
            // {false, true,  false, true,  false, true,  false, true, true,   false, ?}
            //  10      12      7     5      18     19      1     -2   -999.0 -999.0 -999.0
            // {false, false, true,  true,  false, false, true,  true, ?,      ?,     ?}
            //  11      13      15     16     4     2     0      -3     22     -5    -999.0
            // {false, false, false, false, true,  true,  true,  true, false, true,   ?}

            var dt1 = dates[1] - dates[0];
            var dt2 = dates[2] - dates[1];
            var span1 = new TimeSpan(0);
            var span2 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 8, 12, true));
            var span3 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 14, 7, false)) + new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 7, 15, true));
            var span4 = dt1 + new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 5, 16, true));
            var span5 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 18, 4, false));
            var span6 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 3, 19, true)) + new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt2, 8.5, 19, 2, false));
            var span7 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1, 8.5, 20, 1, false)) + dt2;
            var span8 = dt1 + dt2;
            var span9 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1 + dt2, 8.5, -4, 22, true));
            var span10 = new TimeSpan(GetLinearlyInterpolatedTimeTicks(dt1 + dt2, 8.5, 21, -5, false));
            var span11 = new TimeSpan(0);

            var expectedValues = new[]
                {
                    span1.TotalDays, span2.TotalDays, span3.TotalDays, span4.TotalDays, span5.TotalDays,
                    span6.TotalDays, span7.TotalDays, span8.TotalDays, span9.TotalDays, span10.TotalDays, span11.TotalDays
                };

            int networkLocationCounter = 0;
            foreach (var networkLocation in networkCoverage.Locations.Values)
            {
                Assert.AreEqual(expectedValues[networkLocationCounter], result[networkLocation],
                    String.Format("Expected value for location = {0} not matching", networkLocation));
                networkLocationCounter++;
            }

            Assert.IsFalse(ReferenceEquals(result, networkCoverage));
            Assert.AreEqual("", result.Components[0].Unit.Name);
            Assert.AreEqual("", result.Components[0].Unit.Symbol);
        } 

        /// <summary>
        /// Uses linear interpolation to calculate the intersection of the line from
        /// left point to right point with a reference.
        /// </summary>
        /// <param name="fullTimeSpan">Full horizontal range</param>
        /// <param name="referenceValue">Reference value to intersect with.</param>
        /// <param name="t0">Left side vertical value.</param>
        /// <param name="t1">Right side vertical value.</param>
        /// <param name="takeLeft">
        /// True: Take duration from left side of intersection.
        /// False: Take duration from right side of intersection.</param>
        /// <returns>Linearly interpolated duration based on intersection.</returns>
        private static long GetLinearlyInterpolatedTimeTicks(TimeSpan fullTimeSpan, double referenceValue,
                                                             double t0, double t1, bool takeLeft)
        {
            var linearlyInterpolatedTimeTicks = Convert.ToInt64(fullTimeSpan.Ticks*(Math.Abs(t0 - referenceValue))/Math.Abs(t0 - t1));
            if (!takeLeft) linearlyInterpolatedTimeTicks = fullTimeSpan.Ticks - linearlyInterpolatedTimeTicks;
            return linearlyInterpolatedTimeTicks;
        }

        #endregion

        private NetworkCoverage CreateNetworkCoverage(IEnumerable<DateTime> dateTimes, IEnumerable<double> branchChainages)
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var isTimeDependent = dateTimes.Any();
            var networkCoverage = new NetworkCoverage("coverage", isTimeDependent) { Network = network };
            if (isTimeDependent)
            {
                networkCoverage.Time.SetValues(dateTimes);
            }

            var branch = network.Branches.First();
            var locations = branchChainages.Select(branchChainage => new NetworkLocation(branch, branchChainage)).ToList();
            networkCoverage.Locations.SetValues(locations);

            return networkCoverage;
        }
    }
}