using System;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniPropertyTests
    {
        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            Assert.Throws<ArgumentException>(() => _ = new IniProperty(key));
        }

        [Test]
        public void Constructor_ValidKey_InitializesProperties()
        {
            var property = new IniProperty("TestKey");

            Assert.IsEmpty(property.Value);
            Assert.IsEmpty(property.Comment);
            Assert.AreEqual(0, property.LineNumber);
        }

        [Test]
        [TestCase("TestProperty", "", "")]
        [TestCase("TestProperty", null, null)]
        [TestCase("TestProperty", "TestValue", "TestComment")]
        public void Constructor_ValidValues_InitializesProperties(string key, string value, string comment)
        {
            var property = new IniProperty(key, value, comment);

            Assert.AreEqual(key, property.Key);
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(comment, property.Comment);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_KeyIsNullOrEmptyAndValidProperty_ThrowsArgumentException(string key)
        {
            var property = new IniProperty("TestKey");

            Assert.Throws<ArgumentException>(() => _ = new IniProperty(key, property));
        }

        [Test]
        public void Constructor_PropertyIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new IniProperty("TestKey", null));
        }

        [Test]
        public void Constructor_ValidProperty_InitializesProperties()
        {
            var property = new IniProperty("TestKey", "TestValue", "TestComment") { LineNumber = 10 };

            var copiedProperty = new IniProperty("OtherKey", property);

            Assert.AreEqual("OtherKey", copiedProperty.Key);
            Assert.AreEqual("TestValue", copiedProperty.Value);
            Assert.AreEqual("TestComment", copiedProperty.Comment);
            Assert.AreEqual(10, copiedProperty.LineNumber);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Create_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            Assert.Throws<ArgumentException>(() => IniProperty.Create(key, "TestValue"));
        }

        [Test]
        [TestCase("", "")]
        [TestCase(42, "42")]
        [TestCase(2.71, "2.7100000e+000")]
        [TestCase(DayOfWeek.Friday, "Friday")]
        [TestCase("TestValue", "TestValue")]
        public void Create_ValidValue_CreatesProperty<T>(T value, string expectedValue)
            where T : IConvertible
        {
            var property = IniProperty.Create("TestKey", value);

            Assert.IsNotNull(property);
            Assert.AreEqual("TestKey", property.Key);
            Assert.AreEqual(expectedValue, property.Value);
        }

        [Test]
        public void Create_ValidDateTimeValue_CreatesProperty()
        {
            var value = new DateTime(2023, 8, 14, 12, 0, 0);
            var property = IniProperty.Create("TestKey", value);

            Assert.IsNotNull(property);
            Assert.AreEqual("TestKey", property.Key);
            Assert.AreEqual("2023-08-14 12:00:00", property.Value);
        }

        [Test]
        public void Create_NullStringValue_CreatesProperty()
        {
            var property = IniProperty.Create<string>("TestKey", null);

            Assert.IsNotNull(property);
            Assert.AreEqual("TestKey", property.Key);
            Assert.IsEmpty(property.Value);
        }

        [Test]
        public void HasValue_ValidValue_ReturnsTrue()
        {
            var property = new IniProperty("TestKey", "TestValue");

            bool result = property.HasValue();
            
            Assert.IsTrue(result);
        }
        
        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void HasComment_CommentIsNullOrEmpty_ReturnsFalse(string comment)
        {
            var property = new IniProperty("TestKey", "TestValue", comment);

            bool result = property.HasComment();
            
            Assert.IsFalse(result);
        }
        
        [Test]
        public void HasComment_ValidComment_ReturnsTrue()
        {
            var property = new IniProperty("TestKey", "TestValue", "TestComment");

            bool result = property.HasComment();
            
            Assert.IsTrue(result);
        }
        
        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void HasValue_ValueIsNullOrEmpty_ReturnsFalse(string value)
        {
            var property = new IniProperty("TestKey", value);

            bool result = property.HasValue();
            
            Assert.IsFalse(result);
        }
        
        [Test]
        [TestCase("", "")]
        [TestCase("42", 42)]
        [TestCase("2.7100000e+000", 2.71)]
        [TestCase("Monday", DayOfWeek.Monday)]
        [TestCase("TestValue", "TestValue")]
        public void TryGetConvertedValue_ValidValueAndType_ReturnsTrueAndConvertedValue<T>(string value, T expectedValue)
            where T : IConvertible
        {
            var property = new IniProperty("TestKey", value);

            bool result = property.TryGetConvertedValue(out T convertedValue);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedValue, convertedValue);
        }

        [Test]
        public void TryGetConvertedValue_ValidDateTimeValueAndType_ReturnsTrueAndConvertedValue()
        {
            var property = new IniProperty("TestKey", "2023-08-14");

            bool result = property.TryGetConvertedValue(out DateTime convertedValue);

            Assert.IsTrue(result);
            Assert.AreEqual(new DateTime(2023, 8, 14), convertedValue);
        }

        [Test]
        public void TryGetConvertedValue_StringTypeAndValueIsNull_ReturnsTrueAndDefaultValue()
        {
            var property = new IniProperty("TestKey") { Value = null };

            bool result = property.TryGetConvertedValue(out string convertedValue);

            Assert.IsTrue(result);
            Assert.IsNull(convertedValue);
        }

        [Test]
        [TestCase(default(int))]
        [TestCase(default(float))]
        [TestCase(default(double))]
        [TestCase(default(DayOfWeek))]
        public void TryGetConvertedValue_InvalidType_ReturnsFalseAndDefaultValue<T>(T defaultValue)
            where T : IConvertible
        {
            var property = new IniProperty("TestKey", "TestValue");

            bool result = property.TryGetConvertedValue(out T convertedValue);

            Assert.IsFalse(result);
            Assert.AreEqual(defaultValue, convertedValue);
        }

        [Test]
        [TestCase("", "")]
        [TestCase(11, "11")]
        [TestCase(12.33f, "1.2330000e+001")]
        [TestCase(0.123d, "1.2300000e-001")]
        [TestCase(DayOfWeek.Monday, "Monday")]
        [TestCase("TestValue", "TestValue")]
        public void SetConvertedValue_ValidValue_UpdatesValue<T>(T value, string expectedValue)
            where T : IConvertible
        {
            var property = new IniProperty("TestKey");

            property.SetConvertedValue(value);

            Assert.AreEqual(expectedValue, property.Value);
        }

        [Test]
        public void SetConvertedValue_ValidDateTimeValue_UpdatesValue()
        {
            var property = new IniProperty("TestKey");
            var value = new DateTime(2023, 8, 14, 12, 0, 0);

            property.SetConvertedValue(value);

            Assert.AreEqual("2023-08-14 12:00:00", property.Value);
        }

        [Test]
        public void SetConvertedValue_StringValueIsNull_UpdatesValue()
        {
            var property = new IniProperty("TestKey");

            property.SetConvertedValue<string>(null);

            Assert.IsEmpty(property.Value);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void IsKeyEqualTo_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var property = new IniProperty("TestKey");

            Assert.Throws<ArgumentException>(() => property.IsKeyEqualTo(key));
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void IsKeyEqualTo_SameCaseInsensitiveKey_ReturnsTrue(string key)
        {
            var property = new IniProperty("TestKey");

            bool result = property.IsKeyEqualTo(key);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsKeyEqualTo_DifferentKey_ReturnsFalse()
        {
            var property = new IniProperty("TestKey");

            bool result = property.IsKeyEqualTo("OtherKey");

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ObjectAndProperty_ReturnsFalse()
        {
            var obj = new object();
            var property = new IniProperty("TestKey");

            bool result = property.Equals(obj);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_PropertyAndNull_ReturnsFalse()
        {
            var property = new IniProperty("TestKey");

            bool result = property.Equals(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SamePropertyReference_ReturnsTrue()
        {
            var property = new IniProperty("TestKey");

            bool result = property.Equals(property);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_SamePropertiesCaseInsensitive_ReturnsTrue()
        {
            var property1 = new IniProperty("TestKey", "TestValue", "TestComment") { LineNumber = 2 };
            var property2 = new IniProperty("TESTKEY", "TESTVALUE", "TESTCOMMENT") { LineNumber = 2 };

            bool result = property1.Equals(property2);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_NullValues_ReturnsTrue()
        {
            var property1 = new IniProperty("TestKey", null, null);
            var property2 = new IniProperty("testkey", null, null);

            bool result = property1.Equals(property2);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var property1 = new IniProperty("TestKey", "TestValue");
            var property2 = new IniProperty("TestKey", "OtherValue");

            bool result = property1.Equals(property2);

            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_SameCaseInsensitiveKeys_SameHashCode()
        {
            var property1 = new IniProperty("TestKey");
            var property2 = new IniProperty("TESTKEY");

            int hashCode1 = property1.GetHashCode();
            int hashCode2 = property2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }

        [Test]
        public void GetHashCode_DifferentKeys_DifferentHashCode()
        {
            var property1 = new IniProperty("TestKey");
            var property2 = new IniProperty("OtherKey");

            int hashCode1 = property1.GetHashCode();
            int hashCode2 = property2.GetHashCode();

            Assert.AreNotEqual(hashCode1, hashCode2);
        }
    }
}