using System;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Import
{
    [TestFixture]
    public class ModelTimersTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            DateTime startTime = DateTime.Today;
            TimeSpan timeStep = TimeSpan.FromHours(4);
            DateTime stopTime = DateTime.Today.AddDays(1);

            // Call
            var timers = new ModelTimers(startTime, timeStep, stopTime);

            // Assert
            Assert.That(timers.StartTime, Is.EqualTo(startTime));
            Assert.That(timers.TimeStep, Is.EqualTo(timeStep));
            Assert.That(timers.StopTime, Is.EqualTo(stopTime));
        }
    }
}