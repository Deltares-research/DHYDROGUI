using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class HydrodynamicsConstantDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var data = new HydrodynamicsConstantData();

            // Asserts
            Assert.That(data, Is.InstanceOf<INotifyPropertyChanged>());

            Assert.That(data.WaterLevel, Is.EqualTo(0.0));
            Assert.That(data.VelocityX, Is.EqualTo(0.0));
            Assert.That(data.VelocityY, Is.EqualTo(0.0));
        }

        public static IEnumerable<TestCaseData> GetParameterChangedData()
        {
            void UpdateWaterLevel(HydrodynamicsConstantData data) => data.WaterLevel = 10.0;
            yield return new TestCaseData((Action<HydrodynamicsConstantData>)UpdateWaterLevel,
                                          nameof(HydrodynamicsConstantData.WaterLevel));

            void UpdateVelocityX(HydrodynamicsConstantData data) => data.VelocityX = 10.0;
            yield return new TestCaseData((Action<HydrodynamicsConstantData>)UpdateVelocityX,
                                          nameof(HydrodynamicsConstantData.VelocityX));

            void UpdateVelocityY(HydrodynamicsConstantData data) => data.VelocityY = 10.0;
            yield return new TestCaseData((Action<HydrodynamicsConstantData>)UpdateVelocityY,
                                          nameof(HydrodynamicsConstantData.VelocityY));
        }

        [Test]
        [TestCaseSource(nameof(GetParameterChangedData))]
        public void PropertyChanged_CallsNotifyPropertyChangedCorrectly(Action<HydrodynamicsConstantData> updateProperty,
                                                                         string expectedPropertyName)
        {
            // Setup
            var data = new HydrodynamicsConstantData();
            var observer = new EventTestObserver<PropertyChangedEventArgs>();

            ((INotifyPropertyChanged)data).PropertyChanged += observer.OnEventFired;

            // Call
            updateProperty(data);

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders.First(), Is.SameAs(data));

            PropertyChangedEventArgs args = observer.EventArgses.First();
            Assert.That(args.PropertyName, Is.EqualTo(expectedPropertyName));
        }
    }
}