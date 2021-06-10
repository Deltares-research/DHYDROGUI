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
    public class WindConstantDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var data = new WindConstantData();

            // Asserts
            Assert.That(data, Is.InstanceOf<INotifyPropertyChanged>());

            Assert.That(data.Speed, Is.EqualTo(0.0));
            Assert.That(data.Direction, Is.EqualTo(0.0));
        }

        public static IEnumerable<TestCaseData> GetPropertyChangedData()
        {
            void UpdateSpeed(WindConstantData data) => data.Speed = 10.0;
            yield return new TestCaseData((Action<WindConstantData>)UpdateSpeed,
                                          nameof(WindConstantData.Speed));
            void UpdateDirection(WindConstantData data) => data.Direction = 10.0;
            yield return new TestCaseData((Action<WindConstantData>)UpdateDirection,
                                          nameof(WindConstantData.Direction));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyChangedData))]
        public void PropertyChanged_CallsNotifyPropertyChangedCorrectly(Action<WindConstantData> updateProperty,
                                                                         string expectedPropertyName)
        {
            // Setup
            var data = new WindConstantData();
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