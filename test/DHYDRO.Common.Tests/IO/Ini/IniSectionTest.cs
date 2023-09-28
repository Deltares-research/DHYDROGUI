using System;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniSectionTests
    {
        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => _ = new IniSection(name));
        }

        [Test]
        public void Constructor_ValidName_InitializesProperties()
        {
            var section = new IniSection("TestSection");

            Assert.AreEqual("TestSection", section.Name);
            Assert.IsEmpty(section.Properties);
            Assert.IsEmpty(section.Comments);
            Assert.AreEqual(0, section.LineNumber);
            Assert.AreEqual(0, section.PropertyCount);
            Assert.AreEqual(0, section.CommentCount);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_NameIsNullOrEmptyAndValidSection_ThrowsArgumentException(string name)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => _ = new IniSection(name, section));
        }

        [Test]
        public void Constructor_SectionIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new IniSection("TestSection", null));
        }

        [Test]
        public void Constructor_ValidSection_InitializesProperties()
        {
            var comment = "TestComment";
            var section = new IniSection("TestSection") { LineNumber = 2 };
            var property = new IniProperty("TestProperty");

            section.AddProperty(property);
            section.AddComment(comment);

            var copiedSection = new IniSection("OtherSection", section);

            IniProperty copiedProperty = copiedSection.Properties.FirstOrDefault();
            string copiedComment = copiedSection.Comments.FirstOrDefault();

            Assert.AreEqual("OtherSection", copiedSection.Name);
            Assert.AreEqual(2, copiedSection.LineNumber);
            Assert.NotNull(copiedProperty);
            Assert.AreNotSame(property, copiedProperty);
            Assert.AreEqual(property, copiedProperty);
            Assert.AreEqual(comment, copiedComment);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void AddProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.AddProperty(key, "Value"));
        }

        [Test]
        [TestCase(42, "42")]
        [TestCase(2.71, "2.7100000e+000")]
        [TestCase("TestValue", "TestValue")]
        public void AddProperty_ValidValue_AddsProperty<T>(T value, string expectedValue)
            where T : IConvertible
        {
            var section = new IniSection("TestSection");

            section.AddProperty("TestKey", value);

            IniProperty addedProperty = section.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.AreEqual("TestKey", addedProperty.Key);
            Assert.AreEqual(expectedValue, addedProperty.Value);
        }

        [Test]
        public void AddProperty_SamePropertyKey_AddsProperty()
        {
            var section = new IniSection("TestSection");

            IniProperty property1 = section.AddProperty("TestKey", "TestValue");
            IniProperty property2 = section.AddProperty("TestKey", "TestValue");

            IniProperty[] expected = { property1, property2 };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        public void AddProperty_StringValueIsNull_ValueIsEmptyString()
        {
            var section = new IniSection("TestSection");

            section.AddProperty<string>("TestKey", null);

            IniProperty addedProperty = section.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.AreEqual("TestKey", addedProperty.Key);
            Assert.IsEmpty(addedProperty.Value);
        }

        [Test]
        public void AddProperty_PropertyIsNull_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentNullException>(() => section.AddProperty(null));
        }

        [Test]
        public void AddProperty_ValidProperty_AddsProperty()
        {
            var section = new IniSection("TestSection");
            var property = new IniProperty("TestKey", "TestValue");

            section.AddProperty(property);

            IniProperty addedProperty = section.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.AreEqual("TestKey", addedProperty.Key);
            Assert.AreEqual("TestValue", addedProperty.Value);
        }

        [Test]
        public void AddProperty_SamePropertyAdded_AddsProperty()
        {
            var section = new IniSection("TestSection");
            var property1 = new IniProperty("TestKey", "TestValue");
            var property2 = new IniProperty("TestKey", "TestValue");

            section.AddProperty(property1);
            section.AddProperty(property2);

            IniProperty[] expected = { property1, property2 };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        public void AddProperty_ValidProperties_PreservesOrder()
        {
            var section = new IniSection("TestSection");
            var property1 = new IniProperty("TestKey1", "TestValue1");
            var property2 = new IniProperty("TestKey2", "TestValue2");
            var property3 = new IniProperty("TestKey3", "TestValue3");

            section.AddProperty(property3);
            section.AddProperty(property2);
            section.AddProperty(property1);

            IniProperty[] expected = { property3, property2, property1 };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        public void AddMultipleProperties_PropertiesIsNull_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentNullException>(() => section.AddMultipleProperties(null));
        }

        [Test]
        public void AddMultipleProperties_ValidProperties_AddsProperties()
        {
            var section = new IniSection("TestSection");

            var property1 = new IniProperty("TestKey1", "TestValue1");
            var property2 = new IniProperty("TestKey2", "TestValue2");

            IniProperty[] properties = { property1, property2 };

            section.AddMultipleProperties(properties);

            Assert.That(section.Properties, Is.EqualTo(properties));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void AddOrUpdateProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.AddOrUpdateProperty(key, "TestValue"));
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void AddOrUpdateProperty_ExistingCaseInsensitiveKey_UpdatesValue(string key)
        {
            var section = new IniSection("TestSection");
            var property = new IniProperty("TestKey", "TestValue");
            section.AddProperty(property);

            IniProperty updatedProperty = section.AddOrUpdateProperty(key, "UpdatedValue");

            Assert.IsNotNull(updatedProperty);
            Assert.AreEqual("TestKey", updatedProperty.Key);
            Assert.AreEqual("UpdatedValue", updatedProperty.Value);
        }

        [Test]
        public void AddOrUpdateProperty_NonExistingProperty_AddsProperty()
        {
            var section = new IniSection("TestSection");

            IniProperty addedProperty = section.AddOrUpdateProperty("TestKey", "TestValue");

            Assert.IsNotNull(addedProperty);
            Assert.AreEqual("TestKey", addedProperty.Key);
            Assert.AreEqual("TestValue", addedProperty.Value);
        }

        [Test]
        public void AddOrUpdateProperty_NonExistingProperty_AddsPropertyWithEmptyValue()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IniProperty addedProperty = section.AddOrUpdateProperty<string>("TestKey", null);

            Assert.IsNotNull(addedProperty);
            Assert.IsEmpty(addedProperty.Value);
            Assert.AreEqual("TestKey", addedProperty.Key);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ContainsProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.ContainsProperty(key));
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void ContainsProperty_ExistingCaseInsensitiveKey_ReturnsTrue(string key)
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "TestValue");

            bool result = section.ContainsProperty(key);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsProperty_PropertyDoesNotExist_ReturnsFalse()
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "TestValue");

            bool result = section.ContainsProperty("OtherKey");

            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.GetProperty(key));
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void GetProperty_ExistingCaseInsensitiveKey_ReturnsProperty(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IniProperty foundProperty = section.GetProperty(key);

            Assert.NotNull(foundProperty);
            Assert.AreEqual("TestKey", foundProperty.Key);
        }

        [Test]
        public void GetProperty_NonExistingKey_ReturnsNull()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IniProperty foundProperty = section.GetProperty("NonExistingKey");

            Assert.Null(foundProperty);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetAllProperties_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.GetAllProperties(key));
        }

        [Test]
        [TestCase("key1")]
        [TestCase("Key1")]
        [TestCase("KEY1")]
        public void GetAllProperties_ExistingCaseInsensitiveKey_ReturnsMatchingProperties(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");
            section.AddProperty("Key1", "Value3");

            IEnumerable<IniProperty> foundProperties = section.GetAllProperties(key);

            Assert.NotNull(foundProperties);
            Assert.AreEqual(2, foundProperties.Count(s => s.Key == "Key1"));
        }

        [Test]
        public void GetAllProperties_NonExistingKey_ReturnsEmptyCollection()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IEnumerable<IniProperty> foundProperties = section.GetAllProperties("NonExistingKey");

            Assert.NotNull(foundProperties);
            Assert.IsEmpty(foundProperties);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetPropertyValueOrDefault_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.GetPropertyValueOrDefault(key));
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void GetPropertyValueOrDefault_ExistingCaseInsensitiveKey_ReturnsValue(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            string value = section.GetPropertyValueOrDefault(key);

            Assert.AreEqual("TestValue", value);
        }

        [Test]
        public void GetPropertyValueOrDefault_NonExistingKey_ReturnsDefaultValue()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            string value = section.GetPropertyValueOrDefault("NonExistentKey", "DefaultValue");

            Assert.AreEqual("DefaultValue", value);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void TryGetConvertedPropertyValue_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.TryGetConvertedPropertyValue(key, out double _));
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void TryGetConvertedPropertyValue_ExistingCaseInsensitiveKey_ReturnsTrueAndConvertedValue(string key)
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "123.01");

            bool result = section.TryGetConvertedPropertyValue(key, out double convertedValue);

            Assert.IsTrue(result);
            Assert.AreEqual(123.01, convertedValue);
        }

        [Test]
        [TestCase("", "")]
        [TestCase("42", 42)]
        [TestCase("2.7100000e+000", 2.71)]
        [TestCase("Monday", DayOfWeek.Monday)]
        [TestCase("TestValue", "TestValue")]
        public void TryGetConvertedPropertyValue_ExistingKeyAndValidType_ReturnsTrueAndConvertedValue<T>(string value, T expectedValue)
            where T : IConvertible
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", value);

            bool result = section.TryGetConvertedPropertyValue("TestKey", out T convertedValue);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedValue, convertedValue);
        }

        [Test]
        [TestCase(default(int))]
        [TestCase(default(float))]
        [TestCase(default(double))]
        [TestCase(default(DayOfWeek))]
        public void TryGetConvertedPropertyValue_ExistingKeyAndInvalidType_ReturnsFalseAndDefault<T>(T defaultValue)
            where T : IConvertible
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "TestValue");

            bool result = section.TryGetConvertedPropertyValue("TestKey", out T convertedValue);

            Assert.IsFalse(result);
            Assert.AreEqual(defaultValue, convertedValue);
        }

        [Test]
        public void TryGetConvertedPropertyValue_NonExistingKey_ReturnsFalseAndDefault()
        {
            var section = new IniSection("SectionName");

            bool result = section.TryGetConvertedPropertyValue("NonExistentKey", out int convertedValue);

            Assert.IsFalse(result);
            Assert.AreEqual(default(int), convertedValue);
        }

        [Test]
        public void RemoveProperty_NullProperty_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentNullException>(() => section.RemoveProperty(null));
        }

        [Test]
        public void RemoveProperty_ExistingProperty_RemovesProperty()
        {
            var section = new IniSection("TestSection");
            var property = new IniProperty("TestKey");

            section.AddProperty(property);
            section.RemoveProperty(property);

            Assert.IsEmpty(section.Properties);
        }

        [Test]
        public void RemoveProperty_SamePropertyDifferentInstance_RemovesProperty()
        {
            var section = new IniSection("TestSection");
            var property1 = new IniProperty("TestKey");
            var property2 = new IniProperty("TestKey");

            section.AddProperty(property1);
            section.RemoveProperty(property2);

            Assert.IsEmpty(section.Properties);
        }

        [Test]
        public void RemoveProperty_DifferentProperty_DoesNotRemoveProperty()
        {
            var section = new IniSection("TestSection");
            var property1 = new IniProperty("TestKey");
            var property2 = new IniProperty("OtherKey");

            section.AddProperty(property1);
            section.RemoveProperty(property2);

            IniProperty[] expected = { property1 };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveProperty_ExistingProperty_PreservesOrder()
        {
            var section = new IniSection("TestSection");
            IniProperty property1 = section.AddProperty("Key1", "Value1");
            IniProperty property2 = section.AddProperty("Key2", "Value2");
            IniProperty property3 = section.AddProperty("Key1", "Value3");

            section.RemoveProperty(property1);

            IniProperty property4 = section.AddProperty("Key3", "Value4");
            IniProperty[] expected = { property2, property3, property4 };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void RemoveAllProperties_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.RemoveAllProperties(key));
        }

        [Test]
        [TestCase("key1")]
        [TestCase("Key1")]
        [TestCase("KEY1")]
        public void RemoveAllProperties_ExistingCaseInsensitiveKey_RemovesMatchingProperties(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");
            section.AddProperty("Key1", "Value3");

            section.RemoveAllProperties(key);

            Assert.AreEqual(1, section.PropertyCount);
            Assert.IsFalse(section.ContainsProperty("Key1"));
        }

        [Test]
        public void RemoveAllProperties_NonExistingKey_DoesNothing()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "Value1");

            section.RemoveAllProperties("NonExistingKey");

            Assert.AreEqual(1, section.PropertyCount);
        }

        [Test]
        public void RemoveAllProperties_PredicateIsNullOrEmpty_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentNullException>(() => section.RemoveAllProperties((Predicate<IniProperty>)null));
        }

        [Test]
        public void RemoveAllProperties_PredicateMatches_RemovesMatchingProperties()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");
            section.AddProperty("Key1", "Value1");

            section.RemoveAllProperties(property => property.Value == "Value1");

            Assert.AreEqual(1, section.PropertyCount);
            Assert.IsFalse(section.ContainsProperty("Key1"));
        }

        [Test]
        public void RemoveAllProperties_PredicateDoesNotMatch_DoesNothing()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "Value1");

            section.RemoveAllProperties(property => false);

            Assert.AreEqual(1, section.PropertyCount);
        }

        [Test]
        public void ClearProperties_WithProperties_RemovesAllProperties()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");

            section.ClearProperties();

            Assert.IsEmpty(section.Properties);
        }

        [Test]
        public void ClearProperties_WithoutProperties_DoesNothing()
        {
            var section = new IniSection("TestSection");

            section.ClearProperties();

            Assert.IsEmpty(section.Properties);
        }

        [Test]
        [TestCase("", "TestProperty")]
        [TestCase(null, "TestProperty")]
        [TestCase("TestProperty", "")]
        [TestCase("TestProperty", null)]
        public void RenameProperties_KeyIsNullOrEmpty_ThrowsArgumentException(string oldKey, string newKey)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.RenameProperties(oldKey, newKey));
        }

        [Test]
        [TestCase("key1")]
        [TestCase("Key1")]
        [TestCase("KEY1")]
        public void RenameProperties_ExistingCaseInsensitiveKey_KeyRenamed(string oldKey)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");
            section.AddProperty("Key1", "Value3");

            section.RenameProperties(oldKey, "NewKey");

            var expected = new[] 
            { 
                new IniProperty("NewKey", "Value1"), 
                new IniProperty("Key2", "Value2"), 
                new IniProperty("NewKey", "Value3") 
            };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        public void RenameProperties_NonExistingKey_NoChanges()
        {
            var section = new IniSection("SectionName");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");

            section.RenameProperties("NonExistentKey", "NewKey");

            var expected = new[] 
            { 
                new IniProperty("Key1", "Value1"), 
                new IniProperty("Key2", "Value2") 
            };

            Assert.That(section.Properties, Is.EqualTo(expected));
        }

        [Test]
        public void AddComment_CommentIsNull_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentNullException>(() => section.AddComment(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("TestComment")]
        public void AddComment_ValidComment_AddsComment(string comment)
        {
            var section = new IniSection("TestSection");

            section.AddComment(comment);

            Assert.AreEqual(1, section.CommentCount);
            Assert.AreEqual(comment, section.Comments.First());
        }

        [Test]
        public void AddMultipleComments_CommentsIsNull_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentNullException>(() => section.AddMultipleComments(null));
        }

        [Test]
        public void AddMultipleComments_ValidComments_AddsComments()
        {
            var section = new IniSection("TestSection");

            string[] comments = { "TestComment1", "TestComment2" };

            section.AddMultipleComments(comments);

            Assert.That(section.Comments, Is.EqualTo(comments));
        }

        [Test]
        public void RemoveComment_CommentIsNull_ThrowsArgumentException()
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.RemoveComment(null));
        }

        [Test]
        public void RemoveComment_ExistingComment_RemovesComment()
        {
            var section = new IniSection("TestSection");
            var comment = "TestComment";

            section.AddComment(comment);
            section.RemoveComment(comment);

            Assert.IsEmpty(section.Comments);
        }

        [Test]
        public void RemoveComment_DifferentComment_DoesNotRemoveComment()
        {
            var section = new IniSection("TestSection");

            section.AddComment("TestComment");
            section.RemoveComment("OtherComment");

            Assert.AreEqual(1, section.CommentCount);
            Assert.AreEqual("TestComment", section.Comments.First());
        }

        [Test]
        public void ClearComments_WithComments_RemovesAllComments()
        {
            var section = new IniSection("TestSection");
            section.AddComment("Comment1");
            section.AddComment("Comment2");

            section.ClearComments();

            Assert.IsEmpty(section.Comments);
        }

        [Test]
        public void ClearComments_WithoutComments_DoesNothing()
        {
            var section = new IniSection("TestSection");

            section.ClearComments();

            Assert.IsEmpty(section.Comments);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void IsNameEqualTo_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var section = new IniSection("TestSection");

            Assert.Throws<ArgumentException>(() => section.IsNameEqualTo(name));
        }

        [Test]
        [TestCase("testsection")]
        [TestCase("TestSection")]
        [TestCase("TESTSECTION")]
        public void IsNameEqualTo_SameCaseInsensitiveName_ReturnsTrue(string name)
        {
            var section = new IniSection("TestSection");

            bool result = section.IsNameEqualTo(name);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsNameEqualTo_DifferentName_ReturnsFalse()
        {
            var section = new IniSection("TestSection");

            bool result = section.IsNameEqualTo("OtherSection");

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ObjectAndSection_ReturnsFalse()
        {
            var obj = new object();
            var section = new IniSection("TestSection");

            bool result = section.Equals(obj);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SectionAndNull_ReturnsFalse()
        {
            var section = new IniSection("TestSection");

            bool result = section.Equals(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameSectionReference_ReturnsTrue()
        {
            var section = new IniSection("TestSection");

            bool result = section.Equals(section);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_SameSectionsCaseInsensitive_ReturnsTrue()
        {
            var section1 = new IniSection("TestSection") { LineNumber = 3 };
            var section2 = new IniSection("TESTSECTION") { LineNumber = 3 };

            section1.AddProperty("TestKey", "TestValue");
            section2.AddProperty("TESTKEY", "TESTVALUE");
            
            section1.AddComment("TestComment");
            section2.AddComment("TESTCOMMENT");

            bool result = section1.Equals(section2);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentSections_ReturnsFalse()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("OtherSection");

            bool result = section1.Equals(section2);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameSectionsDifferentProperties_ReturnsFalse()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            section1.AddProperty("TestKey", "TestValue");
            section2.AddProperty("TestKey", "OtherValue");

            bool result = section1.Equals(section2);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameSectionsDifferentComments_ReturnsFalse()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            section1.AddComment("TestComment");
            section1.AddComment("OtherComment");

            bool result = section1.Equals(section2);

            Assert.IsFalse(result);
        }
        
        [Test]
        public void GetHashCode_SameCaseInsensitiveNames_SameHashCode()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TESTSECTION");

            int hashCode1 = section1.GetHashCode();
            int hashCode2 = section2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }

        [Test]
        public void GetHashCode_DifferentNames_DifferentHashCode()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("OtherSection");

            int hashCode1 = section1.GetHashCode();
            int hashCode2 = section2.GetHashCode();

            Assert.AreNotEqual(hashCode1, hashCode2);
        }
    }
}