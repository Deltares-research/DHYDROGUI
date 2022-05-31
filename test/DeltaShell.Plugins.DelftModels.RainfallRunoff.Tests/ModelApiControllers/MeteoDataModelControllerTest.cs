using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers
{
    [TestFixture]
    public class MeteoDataModelControllerTest
    {
        [Test]
        public void AddGlobalEvaporationDataEqualToModelRunSettings()
        {
            var mockedWriter = Substitute.For<IRRModelHybridFileWriter>();
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(1,0, 0, 0); //per day
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);

            for (var i = 0; i <= nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(i * timeStepSize.Ticks)] = i * 1.1;
            }

            var expectedValues = new[]
            {
                0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 11.0, 0.0
            };
            
            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, startDate, endDate, timeStepSize);
            mockedWriter.Received().AddEvaporationStation(Arg.Is<string>(s => "Global" == s),
                                                          Arg.Is<double[]>(a => expectedValues.SequenceEqual(a, new DoubleComparer(0.0001))));
        }

        [Test]
        public void AddPerFeatureEvaporationDataEqualToModelRunSettings()
        {
            var mockedWriter = Substitute.For<IRRModelHybridFileWriter>();
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(1, 0, 0, 0); //per day
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
            {
                DataDistributionType = MeteoDataDistributionType.PerFeature
            };

            var catchment1 = new Catchment{Name = "Catchment 1" };
            var catchment2 = new Catchment { Name = "Catchment 2" };

            var featureCoverage = (IFeatureCoverage)meteoData.Data;

            featureCoverage.FeatureVariable.AddValues(new [] {catchment1, catchment2});
            featureCoverage.Features.AddRange(new[] { catchment1, catchment2 });
            
            for (var i = 0; i <= nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(i * timeStepSize.Ticks)] = new []{ i * 1.1 , i * 1.2};
            }

            var expectedValues1 = new[]
            {
                0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 11.0, 0.0
            };
            var expectedValues2 = new[]
            {
                0.0, 1.2, 2.4, 3.6, 4.8, 6.0, 7.2, 8.4, 9.6, 10.8, 12.0, 0.0
            };

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, startDate, endDate, timeStepSize);

            var equalityComparer = new DoubleComparer(0.0001);
            mockedWriter.Received().AddEvaporationStation(Arg.Is<string>(s => catchment1.Name == s || catchment2.Name == s),
                                                          Arg.Is<double[]>(a =>
                                                           expectedValues1.SequenceEqual(a, equalityComparer) 
                                                           || expectedValues2.SequenceEqual(a, equalityComparer)));
        }

        [Test]
        public void AddPerStationEvaporationDataEqualToModelRunSettings()
        {
            var mockedWriter = Substitute.For<IRRModelHybridFileWriter>();
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(1, 0, 0, 0); //per day
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
            {
                DataDistributionType = MeteoDataDistributionType.PerStation
            };

            var stationName1 = "Station 1";
            var stationName2 = "Station 2";
            
            meteoData.Data.Arguments[1].AddValues(new[] { stationName1, stationName2});
            
            for (var i = 0; i <= nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(i * timeStepSize.Ticks)] = new[] { i * 1.1, i * 1.2 };
            }

            var expectedValues1 = new[]
            {
                0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 11.0, 0.0
            };
            var expectedValues2 = new[]
            {
                0.0, 1.2, 2.4, 3.6, 4.8, 6.0, 7.2, 8.4, 9.6, 10.8, 12.0, 0.0
            };

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, startDate, endDate, timeStepSize);

            var equalityComparer = new DoubleComparer(0.0001);
            mockedWriter.Received().AddEvaporationStation(Arg.Is<string>(s => stationName1 == s || stationName2 == s),
                                                          Arg.Is<double[]>(a =>
                                                           expectedValues1.SequenceEqual(a, equalityComparer)
                                                           || expectedValues2.SequenceEqual(a, equalityComparer)));
        }
    }

    internal class DoubleComparer : IEqualityComparer<double>
    {
        private readonly double tolerance;

        public DoubleComparer(double tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool Equals(double x, double y)
        {
            return Math.Abs(x-y) < tolerance;
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }
}
