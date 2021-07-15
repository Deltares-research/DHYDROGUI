using System;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers
{
    [TestFixture]
    public class MeteoDataModelApiControllerTest
    {
        private MockRepository mocks = new MockRepository();
        private IRRModelHybridFileWriter mockedWriter;
        private MeteoData mockedDummyData;
        private MeteoData mockedDummyTempData;

        [SetUp]
        public void SetUp()
        {
            mockedWriter = mocks.DynamicMock<IRRModelHybridFileWriter>();
            mockedDummyData = mocks.DynamicMock<MeteoData>(MeteoDataAggregationType.Cumulative);
            mockedDummyTempData = mocks.DynamicMock<MeteoData>(MeteoDataAggregationType.NonCumulative);
        }

        [Test]
        public void AddPrecipitationDataEqualsToModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);

            for(var i = 0; i <= nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(i*timeStepSize.Ticks)] = i * 1.1;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddPrecipitationStation(null,null)).IgnoreArguments().
                WhenCalled(m =>
                               {
                                   Assert.AreEqual("Global", m.Arguments[0]);
                                   Assert.That(m.Arguments[1],
                                               Is.EqualTo(new[] {0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 11.0})
                                                 .Within(0.0001));
                               });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, mockedDummyData, mockedDummyTempData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }
        
        [Test]
        public void AddPrecipitationDataWithALargerTimeSpanThenModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);

            var nPrecipitationSteps = nTimeSteps*2;
            for (var i = -(nTimeSteps/2); i <= nPrecipitationSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(i * timeStepSize.Ticks)] = i * 1.1;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddPrecipitationStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] {0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 11.0})
                                  .Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, mockedDummyData, mockedDummyTempData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddPrecipitationDataWithDoubleFrequencyAsModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);

            var nPrecipitationSteps = nTimeSteps * 2;
            for (var i = 0; i <= nPrecipitationSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan((long)(i/2.0 * timeStepSize.Ticks))] = i * 1.0;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddPrecipitationStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] {1.0, 5.0, 9.0, 13.0, 17.0, 21.0, 25.0, 29.0, 33.0, 37.0, 20.0})
                                  .Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, mockedDummyData, mockedDummyTempData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddPrecipitationDataWithHalfFrequencyAsModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);

            for (var i = 0; i <= 5; i++)
            {
                meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks * i * 2)] = i * 1.0;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddPrecipitationStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] {0.0, 0.0, 0.5, 0.5, 1.0, 1.0, 1.5, 1.5, 2.0, 2.0, 5.0}).Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, mockedDummyData, mockedDummyTempData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddPrecipitationDataWithExtrapolation()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);
           
            meteoData.Data[startDate] = 0.0;
            meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks)] = 0.0;
            meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks * 2)] = 1.0;
            meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks * 3)] = 2.0;

            meteoData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;

            mockedWriter.Expect(fileWriter => fileWriter.AddPrecipitationStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] {0.0, 0.0, 1.0, 2.0, 0.0, 0.0, 1.0, 2.0, 0.0, 0.0, 1.0}).Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData, mockedDummyData, mockedDummyTempData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }
        
        [Test]
        public void AddEvaporationDataEqualToModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(1,0, 0, 0); //per day
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative);

            for (var i = 0; i <= nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(i * timeStepSize.Ticks)] = i * 1.1;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddEvaporationStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] {0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 11.0, 0.0})
                                  .Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, mockedDummyData, meteoData, mockedDummyTempData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddTemperatureDataWithSameFrequencyAsModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 5;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.NonCumulative);

            for (var i = 0; i <= nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks * i)] = (double)i;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddTemperatureStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 }).Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, mockedDummyData, mockedDummyData, meteoData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddBlockInterpolatedTemperatureDataWithHalfFrequencyAsModel()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.NonCumulative);

            for (var i = 0; i <= nTimeSteps / 2; i++)
            {
                meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks * i * 2)] = (double)i;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddTemperatureStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] { 0.0, 0.0, 1.0, 1.0, 2.0, 2.0, 3.0, 3.0, 4.0, 4.0, 5.0 }).Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, mockedDummyData, mockedDummyData, meteoData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddLinearlyInterpolatedTemperatureDataWithHalfFrequencyAsModelRun()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 10;
            var endDate = startDate + new TimeSpan((nTimeSteps * timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.NonCumulative);
            meteoData.Data.Arguments[0].InterpolationType = InterpolationType.Linear;

            for (var i = 0; i <= nTimeSteps/2; i++)
            {
                meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks * i * 2)] = (double)i;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddTemperatureStation(null, null)).IgnoreArguments().
                WhenCalled(m =>
                {
                    Assert.AreEqual("Global", m.Arguments[0]);
                    Assert.That(m.Arguments[1],
                                Is.EqualTo(new[] { 0.0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0 }).Within(0.0001));
                });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, mockedDummyData, mockedDummyData, meteoData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }

        [Test]
        public void AddTemperatureDataWithDoubleFrequencyAsModelRunSettings()
        {
            var startDate = new DateTime(2012, 3, 1);
            var timeStepSize = new TimeSpan(0, 1, 0, 0); //per hour
            var nTimeSteps = 4;
            var endDate = startDate + new TimeSpan((nTimeSteps*timeStepSize.Ticks));

            var meteoData = new MeteoData(MeteoDataAggregationType.NonCumulative);

            for (var i = 0; i <= 2*nTimeSteps; i++)
            {
                meteoData.Data[startDate + new TimeSpan(timeStepSize.Ticks*i/2)] = (double) i;
            }

            mockedWriter.Expect(fileWriter => fileWriter.AddTemperatureStation(null, null)).IgnoreArguments().
                           WhenCalled(m =>
                               {
                                   Assert.AreEqual("Global", m.Arguments[0]);
                                   Assert.That(m.Arguments[1],
                                               Is.EqualTo(new[] {0.0, 2.0, 4.0, 6.0, 8.0}).Within(0.0001));
                               });

            mocks.ReplayAll();

            MeteoDataModelController.AddMeteoData(mockedWriter, mockedDummyData, mockedDummyData, meteoData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }
    }
}
