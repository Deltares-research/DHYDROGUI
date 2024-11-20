using System;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests
{
    [TestFixture]
    public class SystemEnvironmentTest
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
            var systemEnvironment = new SystemEnvironment();

            // Assert
            Assert.That(systemEnvironment, Is.InstanceOf<IEnvironment>());
        }

        [Test]
        public void SetVariable_ExpectedResults()
        {
            // Setup
            var systemEnvironment = new SystemEnvironment();
            const string value = "SomeValue";

            // Call
            systemEnvironment.SetVariable(key, value);

            // Assert
            Assert.That(Environment.GetEnvironmentVariable(key), Is.EqualTo(value));
        }

        [Test]
        public void GetVariable_ExpectedResults()
        {
            // Setup
            var systemEnvironment = new SystemEnvironment();
            const string expectedValue = "SomeValue";
            Environment.SetEnvironmentVariable(key, expectedValue);

            // Call
            string result = systemEnvironment.GetVariable(key);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }
    }
}