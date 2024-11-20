using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class KeyValuePairArrayConverterTest
    {
        private readonly Random random = new Random();

        [Test]
        public void ConvertTo_ReturnsCorrectResult()
        {
            // Setup
            int nKvps = random.Next(10);
            KeyValuePair<string, string>[] kvpArray = Enumerable.Range(0, nKvps)
                                                                .Select(_ => new KeyValuePair<string, string>())
                                                                .ToArray();

            var converter = new KeyValuePairArrayConverter<string>();

            // Call
            object result = converter.ConvertTo(Substitute.For<ITypeDescriptorContext>(),
                                                CultureInfo.CurrentCulture,
                                                kvpArray,
                                                typeof(string));

            // Assert
            var resultStr = result as string;
            Assert.That(resultStr, Is.Not.Null);
            Assert.That(resultStr, Is.EqualTo($"({nKvps})"));
        }

        [Test]
        public void ConvertTo_ValueUnexpectedType_ReturnsCorrectResult()
        {
            // Setup
            var converter = new KeyValuePairArrayConverter<string>();

            // Call
            void Call() => converter.ConvertTo(Substitute.For<ITypeDescriptorContext>(),
                                               CultureInfo.CurrentCulture,
                                               new object(),
                                               typeof(string));

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            StringAssert.StartsWith($@"Must be of type {typeof(KeyValuePair<string, string>[])}.", exception.Message);
            Assert.That(exception.ParamName, Is.EqualTo("value"));
        }

        [Test]
        public void GetProperties_ReturnsCorrectResult()
        {
            // Setup
            var keyValuePairArray = new[]
            {
                new KeyValuePair<string, string>("A", "value_a"),
                new KeyValuePair<string, string>("B", "value_b"),
                new KeyValuePair<string, string>("C", "value_c")
            };
            var converter = new KeyValuePairArrayConverter<string>();

            // Call
            PropertyDescriptorCollection propertyDescriptorCollection = converter.GetProperties(
                Substitute.For<ITypeDescriptorContext>(),
                keyValuePairArray,
                null);

            // Assert
            int count = keyValuePairArray.Length;
            Assert.That(propertyDescriptorCollection, Has.Count.EqualTo(count));
            for (var i = 0; i < count; i++)
            {
                PropertyDescriptor descriptor = propertyDescriptorCollection[i];
                Assert.That(propertyDescriptorCollection[i].Name, Is.EqualTo(keyValuePairArray[i].Key));
                Assert.That(descriptor.IsReadOnly, Is.True);
                Assert.That(descriptor.Attributes, Is.Empty);
            }
        }

        [Test]
        public void GetProperties_ValueUnexpectedType_ReturnsCorrectResult()
        {
            // Setup
            var converter = new KeyValuePairArrayConverter<string>();

            // Call
            void Call() => converter.GetProperties(Substitute.For<ITypeDescriptorContext>(),
                                                   new object(),
                                                   null);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            StringAssert.StartsWith($@"Must be of type {typeof(KeyValuePair<string, string>[])}.", exception.Message);
            Assert.That(exception.ParamName, Is.EqualTo("value"));
        }
    }
}