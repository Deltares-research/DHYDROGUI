using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class KeyValuePairPropertyDescriptorTest
    {
        private readonly Random random = new Random();

        [Test]
        public void GetValue_ReturnsCorrectResult()
        {
            // Setup
            var kvpArray = new[]
            {
                new KeyValuePair<string, string>("A", "value_a"),
                new KeyValuePair<string, string>("B", "value_b"),
                new KeyValuePair<string, string>("C", "value_c")
            };

            KeyValuePair<string, string> kvp = kvpArray[random.Next(kvpArray.Length)];
            var descriptor = new KeyValuePairPropertyDescriptor<string>(kvp.Key, null, false);

            // Call
            object result = descriptor.GetValue(kvpArray);

            // Assert
            Assert.That(result, Is.EqualTo(kvp.Value));
        }

        [Test]
        public void GetValue_ArrayDoesNotContainKey_ReturnsCorrectResult()
        {
            // Setup
            var kvpArray = new[]
            {
                new KeyValuePair<string, string>("A", "value_a"),
                new KeyValuePair<string, string>("B", "value_b"),
                new KeyValuePair<string, string>("C", "value_c")
            };
            var descriptor = new KeyValuePairPropertyDescriptor<string>("D", null, false);

            // Call
            object result = descriptor.GetValue(kvpArray);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetValue_ComponentNotExpectedType_ThrowsArgumentException()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, false);

            // Call
            void Call() => descriptor.GetValue(new object());

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            StringAssert.StartsWith($@"Must be of type {typeof(KeyValuePair<string, string>[])}.", exception.Message);
            Assert.That(exception.ParamName, Is.EqualTo("component"));
        }

        [Test]
        public void SetValue_SetsCorrectValue()
        {
            // Setup
            var kvpArray = new[]
            {
                new KeyValuePair<string, string>("A", "value_a"),
                new KeyValuePair<string, string>("B", "value_b"),
                new KeyValuePair<string, string>("C", "value_c")
            };
            int i = random.Next(kvpArray.Length);
            var descriptor = new KeyValuePairPropertyDescriptor<string>(kvpArray[i].Key, null, false);
            const string setValue = "new_value";

            // Call
            descriptor.SetValue(kvpArray, setValue);

            // Assert
            Assert.That(kvpArray[i].Value, Is.EqualTo(setValue));
        }

        [Test]
        public void SetValue_IsReadOnly_ThrowsInvalidOperationException()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, true);
            var kvpArray = new KeyValuePair<string, string>[0];

            // Call
            void Call() => descriptor.SetValue(kvpArray, "new_value");

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(Call);
            Assert.That(exception.Message, Is.EqualTo("Property is read-only."));
        }

        [Test]
        public void SetValue_ComponentUnexpectedType_ThrowsArgumentException()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, false);

            // Call
            void Call() => descriptor.SetValue(new object(), "new_value");

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            StringAssert.StartsWith($@"Must be of type {typeof(KeyValuePair<string, string>[])}.", exception.Message);
            Assert.That(exception.ParamName, Is.EqualTo("component"));
        }

        [Test]
        public void CanResetValue_ReturnsFalse()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, true);
            var kvpArray = new KeyValuePair<string, string>[0];

            // Call
            bool result = descriptor.CanResetValue(kvpArray);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ResetValue_ThrowsNotSupportedException()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, true);
            var kvpArray = new KeyValuePair<string, string>[0];

            // Call
            void Call() => descriptor.ResetValue(kvpArray);

            // Assert
            var exception = Assert.Throws<NotSupportedException>(Call);
            Assert.That(exception.Message, Is.EqualTo("Resetting property value is not supported."));
        }

        [Test]
        public void ShouldSerializeValue_ReturnsFalse()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, true);
            var kvpArray = new KeyValuePair<string, string>[0];

            // Call
            bool result = descriptor.ShouldSerializeValue(kvpArray);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ComponentType_ReturnsCorrectResult()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, true);

            // Call
            Type result = descriptor.ComponentType;

            // Assert
            Assert.That(result, Is.EqualTo(typeof(KeyValuePair<string, string>[])));
        }

        [Test]
        public void PropertyType_ReturnsCorrectResult()
        {
            // Setup
            var descriptor = new KeyValuePairPropertyDescriptor<string>("key", null, true);

            // Call
            Type result = descriptor.PropertyType;

            // Assert
            Assert.That(result, Is.EqualTo(typeof(string)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Constructor_InitializesInstanceCorrectly(bool isReadOnly)
        {
            // Setup
            const string key = "descriptor_key";
            var attribute = Substitute.For<Attribute>();
            attribute.TypeId.Returns("");
            Attribute[] attributes =
            {
                attribute
            };

            // Call
            var descriptor = new KeyValuePairPropertyDescriptor<string>(key, attributes, isReadOnly);

            // Assert
            Assert.That(descriptor.Name, Is.EqualTo(key));
            Assert.That(descriptor.Attributes.Count, Is.EqualTo(1));
            Assert.That(descriptor.Attributes[0], Is.SameAs(attribute));
            Assert.That(descriptor.IsReadOnly, Is.EqualTo(isReadOnly));
        }
    }
}