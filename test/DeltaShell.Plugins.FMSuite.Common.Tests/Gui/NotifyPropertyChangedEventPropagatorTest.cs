using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui
{
    [TestFixture]
    public class NotifyPropertyChangedEventPropagatorTest
    {
        private static IEnumerable<TestCaseData> GetConstructorParameterNullData()
        {
            var observedObject = Substitute.For<INotifyPropertyChanged>();
            void PropertyChanged(string _) { }
            var propertyMapping = new Dictionary<string, string>();

            yield return new TestCaseData(null, (Action<string>)PropertyChanged, propertyMapping, "observedObject");
            yield return new TestCaseData(observedObject, null, propertyMapping, "propertyChangedAction");
            yield return new TestCaseData(observedObject, (Action<string>)PropertyChanged, null, "propertyMapping");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(INotifyPropertyChanged observedObject,
                                                                          Action<string> propertyChangedAction,
                                                                          IReadOnlyDictionary<string, string> propertyMapping,
                                                                          string expectedParameterName)
        {
            void Call() => new NotifyPropertyChangedEventPropagator(observedObject,
                                                                    propertyChangedAction,
                                                                    propertyMapping);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void ObservedObjectChanged_PropertyObserved_PropertyChangedActionCalled()
        {
            // Setup
            const string key = "someKey";
            const string value = "someValue";

            var calledProperties = new List<string>();

            var observedObject = Substitute.For<INotifyPropertyChanged>();
            void PropertyChanged(string prop) { calledProperties.Add(prop); }
            var propertyMapping = new Dictionary<string, string>() { { key, value } };

            using (var propagator = new NotifyPropertyChangedEventPropagator(observedObject,
                                                                             PropertyChanged,
                                                                             propertyMapping))
            {
                // Call
                observedObject.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(observedObject,
                                                                                           new PropertyChangedEventArgs(key));

                // Assert
                Assert.That(calledProperties, Has.Count.EqualTo(1));
                Assert.That(calledProperties, Has.Member(value));
            }
        }

        [Test]
        public void ObservedObjectChanged_PropertyNotObserved_PropertyChangedActionNotCalled()
        {
            // Setup
            const string key = "someKey";
            const string value = "someValue";

            var calledProperties = new List<string>();

            var observedObject = Substitute.For<INotifyPropertyChanged>();
            void PropertyChanged(string prop) { calledProperties.Add(prop); }
            var propertyMapping = new Dictionary<string, string>() { { key, value } };

            using (var propagator = new NotifyPropertyChangedEventPropagator(observedObject,
                                                                             PropertyChanged,
                                                                             propertyMapping))

            {
                // Call
                observedObject.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(observedObject,
                                                                                           new PropertyChangedEventArgs("someOtherKey"));

                // Assert
                Assert.That(calledProperties, Is.Empty);
            }
        }

        [Test]
        public void ObservedObjectChanged_SenderNotObserved_PropertyChangedActionNotCalled()
        {
            // Setup
            const string key = "someKey";
            const string value = "someValue";

            var calledProperties = new List<string>();

            var observedObject = Substitute.For<INotifyPropertyChanged>();
            void PropertyChanged(string prop) { calledProperties.Add(prop); }
            var propertyMapping = new Dictionary<string, string>() { { key, value } };

            using (var propagator = new NotifyPropertyChangedEventPropagator(observedObject,
                                                                             PropertyChanged,
                                                                             propertyMapping))

            {
                // Call
                observedObject.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(),
                                                                                           new PropertyChangedEventArgs(key));

                // Assert
                Assert.That(calledProperties, Is.Empty);
            }
        }
    }
}