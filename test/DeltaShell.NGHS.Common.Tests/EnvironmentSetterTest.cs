using System;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests
{
    [TestFixture]
    public class EnvironmentSetterTest
    {
        private const string key = "TestKey";
        private string previousValue;

        [SetUp]
        public void SetUp()
        {
            previousValue = Environment.GetEnvironmentVariable(key);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable(key, previousValue);
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var environmentSetter = new EnvironmentSetter();
            
            // Assert
            Assert.That(environmentSetter, Is.InstanceOf<IEnvironmentSetter>());
        }

        [Test]
        public void SetVariable_ExpectedResults()
        {
            // Setup
            var environmentSetter = new EnvironmentSetter();
            const string value = "SomeValue";

            // Call
            environmentSetter.SetVariable(key, value);

            // Assert
            Assert.That(Environment.GetEnvironmentVariable(key), Is.EqualTo(value));
        }
    }
}