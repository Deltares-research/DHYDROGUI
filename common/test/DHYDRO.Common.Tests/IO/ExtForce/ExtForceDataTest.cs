using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.TestUtils.Logging;
using DHYDRO.Common.IO.ExtForce;
using log4net.Core;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.ExtForce
{
    [TestFixture]
    public class ExtForceDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(data.Comments, Is.Empty);
            Assert.That(data.ModelData, Is.Empty);
            Assert.That(data.LineNumber, Is.Zero);
            Assert.That(data.IsEnabled, Is.True);
            Assert.That(data.Quantity, Is.Null);
            Assert.That(data.FileName, Is.Null);
            Assert.That(data.VariableName, Is.Null);
            Assert.That(data.ParentDirectory, Is.Null);
            Assert.That(data.FileType, Is.Null);
            Assert.That(data.Method, Is.Null);
            Assert.That(data.Operand, Is.Null);
            Assert.That(data.Value, Is.Null);
            Assert.That(data.Factor, Is.Null);
            Assert.That(data.Offset, Is.Null);
        }

        [Test]
        public void AddComment_CommentIsNull_ThrowsArgumentNullException()
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(() => data.AddComment(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("test-comment")]
        public void AddComment_ValidCommentValue_CommentsContainValue(string comment)
        {
            ExtForceData data = CreateExtForceData();

            data.AddComment(comment);

            Assert.That(data.Comments, Has.Exactly(1).EqualTo(comment));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void GetModelData_KeyIsNullOrEmptyOrWhitespace_ThrowsArgumentException(string key)
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(() => data.GetModelData(key), Throws.ArgumentException);
        }

        [Test]
        public void GetModelData_UnknownKey_ThrowsKeyNotFoundException()
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(() => data.GetModelData("key"), Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GetModelData_ModelDataContainsKey_ReturnsValue()
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("test-key", "test-value");
            string value = data.GetModelData("test-key");

            Assert.That(value, Is.EqualTo("test-value"));
        }

        [Test]
        public void GetModelData_ModelDataContainsKeyCaseInsensitive_ReturnsValue()
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("KEY", "value");
            string value = data.GetModelData("key");

            Assert.That(value, Is.EqualTo("value"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void TryGetModelData_KeyIsNullOrEmptyOrWhitespace_ThrowsArgumentException(string key)
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(() => data.TryGetModelData(key, out double _), Throws.ArgumentException);
        }

        [Test]
        public void TryGetModelData_UnknownKey_ReturnsFalse()
        {
            ExtForceData data = CreateExtForceData();

            bool result = data.TryGetModelData("key", out double _);

            Assert.That(result, Is.False);
        }

        [Test]
        [TestCase("42", 42)]
        [TestCase("2.71", 2.71)]
        [TestCase("TestValue", "TestValue")]
        public void TryGetModelData_ValidValueAndType_ReturnsTrueAndConvertedValue<T>(string value, T expectedValue)
            where T : IConvertible
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("key", value);

            bool result = data.TryGetModelData("key", out T convertedValue);

            Assert.That(result, Is.True);
            Assert.That(convertedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(default(int))]
        [TestCase(default(float))]
        [TestCase(default(double))]
        public void TryGetModelData_InvalidFormattedValue_ReturnsFalseAndDefaultValue<T>(T defaultValue)
            where T : IConvertible
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("key", "value");

            bool result = data.TryGetModelData("key", out T convertedValue);

            Assert.That(result, Is.False);
            Assert.That(convertedValue, Is.EqualTo(defaultValue));
        }

        [Test]
        [TestCase(default(int))]
        [TestCase(default(float))]
        [TestCase(default(double))]
        public void TryGetModelData_InvalidFormattedValue_LogsError<T>(T defaultValue)
            where T : IConvertible
        {
            ExtForceData data = CreateExtForceData();

            data.LineNumber = 7;
            data.SetModelData("TestKey", "TestValue");

            void Call() => data.TryGetModelData("TestKey", out T _);
            string error = Log4NetTestHelper.GetAllRenderedMessages(Call, Level.Error).FirstOrDefault();

            Assert.That(error, Is.EqualTo($"Property 'TestKey' cannot be converted to a {defaultValue.GetType().Name} for value: 'TestValue'. Forcing line: 7."));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void SetModelData_KeyIsNullOrEmptyOrWhitespace_ThrowsArgumentException(string key)
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(() => data.SetModelData(key, "value"), Throws.ArgumentException);
        }

        [Test]
        [TestCase("")]
        [TestCase("TestValue")]
        [TestCase(null)]
        public void SetModelData_ValidStringValue_SetsModelData(string value)
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("key", value);

            Assert.That(data.ModelData, Has.One.Matches<KeyValuePair<string, string>>(
                            kvp => kvp.Key == "key" &&
                                   kvp.Value == value));
        }

        [Test]
        [TestCase(11, "11")]
        [TestCase(12.33f, "12.33")]
        [TestCase(0.123d, "0.123")]
        [TestCase(-9.1d, "-9.1")]
        [TestCase("TestValue", "TestValue")]
        public void SetModelData_ValidValueAndType_SetsConvertedValue<T>(T value, string expectedValue)
            where T : IConvertible
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("TestKey", value);

            Assert.That(data.ModelData, Has.One.Matches<KeyValuePair<string, string>>(
                            kvp => kvp.Key == "TestKey" &&
                                   kvp.Value == expectedValue));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void ContainsModelData_KeyIsNullOrEmptyOrWhitespace_ThrowsArgumentException(string key)
        {
            ExtForceData data = CreateExtForceData();

            Assert.That(() => data.ContainsModelData(key), Throws.ArgumentException);
        }

        [Test]
        public void ContainsModelData_ModelDataIsEmpty_ReturnsFalse()
        {
            ExtForceData data = CreateExtForceData();

            bool contains = data.ContainsModelData("test-key");

            Assert.That(contains, Is.False);
        }

        [Test]
        public void ContainsModelData_ModelDataContainsKey_ReturnsTrue()
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("test-key", "test-value");
            bool contains = data.ContainsModelData("test-key");

            Assert.That(contains, Is.True);
        }

        [Test]
        public void ContainsModelData_ModelDataContainsKeyCaseInsensitive_ReturnsTrue()
        {
            ExtForceData data = CreateExtForceData();

            data.SetModelData("KEY", "value");
            bool contains = data.ContainsModelData("key");

            Assert.That(contains, Is.True);
        }

        private static ExtForceData CreateExtForceData()
        {
            return new ExtForceData();
        }
    }
}