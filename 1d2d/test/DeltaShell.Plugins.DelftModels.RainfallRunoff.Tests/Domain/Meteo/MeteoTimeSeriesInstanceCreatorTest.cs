using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoTimeSeriesInstanceCreatorTest
    {
        [Test]
        public void CreateGlobalTimeSeries_UnitIsNull_ThrowsArgumentNullException()
        {
            //Arrange
            var instanceCreator = new MeteoTimeSeriesInstanceCreator();
            
            //Act
            void Call() => instanceCreator.CreateGlobalTimeSeries(null);
            
            //Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void CreateGlobalTimeSeries_GivesExpectedData()
        {
            // Arrange
            var instanceCreator = new MeteoTimeSeriesInstanceCreator(); 
            var unit = new Unit("TestName", "TestSymbol");

            //Act
            TimeSeries timeSeries = instanceCreator.CreateGlobalTimeSeries(unit);
            
            //Assert
            IVariable<DateTime> time = timeSeries.Time;
            
            Assert.That(timeSeries.Name, Is.EqualTo("Global"));
            Assert.That(time.DefaultValue, Is.EqualTo(new DateTime(2000, 1, 1)));
            Assert.That(time.InterpolationType, Is.EqualTo(InterpolationType.Constant));
            Assert.That(time.AllowSetInterpolationType, Is.False);
            Assert.That(time.ExtrapolationType, Is.EqualTo(ExtrapolationType.None));
            Assert.That(timeSeries.Components, Is.Not.Null);
            object componentDefaultValue = timeSeries.Components.First().Values.DefaultValue;
            Assert.That(componentDefaultValue, Is.EqualTo(0.0));
            IUnit timeSeriesUnit = timeSeries.Components.First().Unit;
            Assert.That(timeSeriesUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(timeSeriesUnit.Symbol, Is.EqualTo(unit.Symbol));
        }
    }
}