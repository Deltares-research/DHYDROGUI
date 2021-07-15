using System;
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

        [SetUp]
        public void SetUp()
        {
            mockedWriter = mocks.DynamicMock<IRRModelHybridFileWriter>();
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

            MeteoDataModelController.AddMeteoData(mockedWriter, meteoData,
                                                     startDate, endDate, timeStepSize);

            mocks.VerifyAll();
        }
    }
}
