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
            Assert.That(() => _ = new IniSection(name), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_ValidName_InitializesProperties()
        {
            var section = new IniSection("TestSection");

            Assert.That(section.Name, Is.EqualTo("TestSection"));
            Assert.That(section.Properties, Is.Empty);
            Assert.That(section.Comments, Is.Empty);
            Assert.That(section.LineNumber, Is.EqualTo(0));
            Assert.That(section.PropertyCount, Is.EqualTo(0));
            Assert.That(section.CommentCount, Is.EqualTo(0));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_NameIsNullOrEmptyAndValidSection_ThrowsArgumentException(string name)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => _ = new IniSection(name, section), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_SectionIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new IniSection("TestSection", null), Throws.ArgumentNullException);
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

            Assert.That(copiedSection.Name, Is.EqualTo("OtherSection"));
            Assert.That(copiedSection.LineNumber, Is.EqualTo(2));
            Assert.That(copiedProperty, Is.Not.Null);
            Assert.That(copiedProperty, Is.Not.SameAs(property));
            Assert.That(copiedProperty, Is.EqualTo(property));
            Assert.That(copiedComment, Is.EqualTo(comment));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void AddProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.AddProperty(key, "Value"), Throws.ArgumentException);
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
            Assert.That(addedProperty, Is.Not.Null);
            Assert.That(addedProperty.Key, Is.EqualTo("TestKey"));
            Assert.That(addedProperty.Value, Is.EqualTo(expectedValue));
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
            Assert.That(addedProperty, Is.Not.Null);
            Assert.That(addedProperty.Key, Is.EqualTo("TestKey"));
            Assert.That(addedProperty.Value, Is.Empty);
        }

        [Test]
        public void AddProperty_PropertyIsNull_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.AddProperty(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddProperty_ValidProperty_AddsProperty()
        {
            var section = new IniSection("TestSection");
            var property = new IniProperty("TestKey", "TestValue");

            section.AddProperty(property);

            IniProperty addedProperty = section.Properties.FirstOrDefault();
            Assert.That(addedProperty, Is.Not.Null);
            Assert.That(addedProperty.Key, Is.EqualTo("TestKey"));
            Assert.That(addedProperty.Value, Is.EqualTo("TestValue"));
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

            Assert.That(() => section.AddMultipleProperties(null), Throws.ArgumentNullException);
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

            Assert.That(() => section.AddOrUpdateProperty(key, "TestValue"), Throws.ArgumentException);
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

            Assert.That(updatedProperty, Is.Not.Null);
            Assert.That(updatedProperty.Key, Is.EqualTo("TestKey"));
            Assert.That(updatedProperty.Value, Is.EqualTo("UpdatedValue"));
        }

        [Test]
        public void AddOrUpdateProperty_NonExistingProperty_AddsProperty()
        {
            var section = new IniSection("TestSection");

            IniProperty addedProperty = section.AddOrUpdateProperty("TestKey", "TestValue");

            Assert.That(addedProperty, Is.Not.Null);
            Assert.That(addedProperty.Key, Is.EqualTo("TestKey"));
            Assert.That(addedProperty.Value, Is.EqualTo("TestValue"));
        }

        [Test]
        public void AddOrUpdateProperty_NonExistingProperty_AddsPropertyWithEmptyValue()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IniProperty addedProperty = section.AddOrUpdateProperty<string>("TestKey", null);

            Assert.That(addedProperty, Is.Not.Null);
            Assert.That(addedProperty.Key, Is.EqualTo("TestKey"));
            Assert.That(addedProperty.Value, Is.Empty);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ContainsProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.ContainsProperty(key), Throws.ArgumentException);
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void ContainsProperty_ExistingCaseInsensitiveKey_ReturnsTrue(string key)
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "TestValue");

            bool containsProperty = section.ContainsProperty(key);

            Assert.That(containsProperty, Is.True);
        }

        [Test]
        public void ContainsProperty_PropertyDoesNotExist_ReturnsFalse()
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "TestValue");

            bool containsProperty = section.ContainsProperty("OtherKey");

            Assert.That(containsProperty, Is.False);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void FindProperty_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.FindProperty(key), Throws.ArgumentException);
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void FindProperty_ExistingCaseInsensitiveKey_ReturnsProperty(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IniProperty foundProperty = section.FindProperty(key);

            Assert.That(foundProperty, Is.Not.Null);
            Assert.That(foundProperty.Key, Is.EqualTo("TestKey"));
        }

        [Test]
        public void FindProperty_NonExistingKey_ReturnsNull()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IniProperty foundProperty = section.FindProperty("NonExistingKey");

            Assert.That(foundProperty, Is.Null);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetAllProperties_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.GetAllProperties(key), Throws.ArgumentException);
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

            Assert.That(foundProperties, Has.Exactly(2).Matches<IniProperty>(x => x.Key == "Key1"));
        }

        [Test]
        public void GetAllProperties_NonExistingKey_ReturnsEmptyCollection()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            IEnumerable<IniProperty> foundProperties = section.GetAllProperties("NonExistingKey");

            Assert.That(foundProperties, Is.Empty);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetPropertyValue_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.GetPropertyValue(key), Throws.ArgumentException);
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void GetPropertyValue_ExistingCaseInsensitiveKey_ReturnsValue(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            string value = section.GetPropertyValue(key);

            Assert.That(value, Is.EqualTo("TestValue"));
        }

        [Test]
        public void GetPropertyValue_NonExistingKey_ReturnsDefaultValue()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            string value = section.GetPropertyValue("NonExistentKey", "DefaultValue");

            Assert.That(value, Is.EqualTo("DefaultValue"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetPropertyValueGeneric_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.GetPropertyValue<string>(key), Throws.ArgumentException);
        }

        [Test]
        [TestCase("testkey")]
        [TestCase("TestKey")]
        [TestCase("TESTKEY")]
        public void GetPropertyValueGeneric_ExistingCaseInsensitiveKey_ReturnsValue(string key)
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            var value = section.GetPropertyValue<string>(key);

            Assert.That(value, Is.EqualTo("TestValue"));
        }

        [Test]
        public void GetPropertyValueGeneric_NonExistingKey_ReturnsDefaultValue()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "TestValue");

            var value = section.GetPropertyValue<string>("NonExistentKey", "DefaultValue");

            Assert.That(value, Is.EqualTo("DefaultValue"));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("42", 42)]
        [TestCase("2.7100000e+000", 2.71)]
        [TestCase("Monday", DayOfWeek.Monday)]
        [TestCase("TestValue", "TestValue")]
        public void GetPropertyValueGeneric_ExistingKeyAndValidType_ReturnsConvertedValue<T>(string value, T expectedValue)
            where T : IConvertible
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", value);

            var convertedValue = section.GetPropertyValue<T>("TestKey");

            Assert.That(convertedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(-999.0f)]
        [TestCase(-999.0d)]
        [TestCase(DayOfWeek.Monday)]
        public void GetPropertyValueGeneric_ExistingKeyAndInvalidValue_ReturnsDefault<T>(T defaultValue)
            where T : IConvertible
        {
            var section = new IniSection("SectionName");
            section.AddProperty("TestKey", "TestValue");

            T convertedValue = section.GetPropertyValue("TestKey", defaultValue);

            Assert.That(convertedValue, Is.EqualTo(defaultValue));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(-999.0f)]
        [TestCase(-999.0d)]
        [TestCase(DayOfWeek.Monday)]
        public void GetPropertyValueGeneric_ExistingKeyAndNullValue_ReturnsDefault<T>(T defaultValue)
            where T : IConvertible
        {
            var section = new IniSection("SectionName");
            section.AddProperty<string>("TestKey", null);

            T convertedValue = section.GetPropertyValue("TestKey", defaultValue);

            Assert.That(convertedValue, Is.EqualTo(defaultValue));
        }

        [Test]
        public void RemoveProperty_NullProperty_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.RemoveProperty(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveProperty_ExistingProperty_RemovesProperty()
        {
            var section = new IniSection("TestSection");
            var property = new IniProperty("TestKey");

            section.AddProperty(property);
            section.RemoveProperty(property);

            Assert.That(section.Properties, Is.Empty);
        }

        [Test]
        public void RemoveProperty_SamePropertyDifferentInstance_RemovesProperty()
        {
            var section = new IniSection("TestSection");
            var property1 = new IniProperty("TestKey");
            var property2 = new IniProperty("TestKey");

            section.AddProperty(property1);
            section.RemoveProperty(property2);

            Assert.That(section.Properties, Is.Empty);
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

            Assert.That(() => section.RemoveAllProperties(key), Throws.ArgumentException);
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

            bool containsProperty = section.ContainsProperty("Key1");

            Assert.That(containsProperty, Is.False);
            Assert.That(section.PropertyCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAllProperties_NonExistingKey_DoesNothing()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "Value1");

            section.RemoveAllProperties("NonExistingKey");

            Assert.That(section.PropertyCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAllProperties_PredicateIsNullOrEmpty_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.RemoveAllProperties((Predicate<IniProperty>)null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveAllProperties_PredicateMatches_RemovesMatchingProperties()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");
            section.AddProperty("Key1", "Value1");

            section.RemoveAllProperties(property => property.Value == "Value1");

            bool containsProperty = section.ContainsProperty("Key1");

            Assert.That(containsProperty, Is.False);
            Assert.That(section.PropertyCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAllProperties_PredicateDoesNotMatch_DoesNothing()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("TestKey", "Value1");

            section.RemoveAllProperties(property => false);

            Assert.That(section.PropertyCount, Is.EqualTo(1));
        }

        [Test]
        public void ClearProperties_WithProperties_RemovesAllProperties()
        {
            var section = new IniSection("TestSection");
            section.AddProperty("Key1", "Value1");
            section.AddProperty("Key2", "Value2");

            section.ClearProperties();

            Assert.That(section.Properties, Is.Empty);
        }

        [Test]
        public void ClearProperties_WithoutProperties_DoesNothing()
        {
            var section = new IniSection("TestSection");

            section.ClearProperties();

            Assert.That(section.Properties, Is.Empty);
        }

        [Test]
        [TestCase("", "TestProperty")]
        [TestCase(null, "TestProperty")]
        [TestCase("TestProperty", "")]
        [TestCase("TestProperty", null)]
        public void RenameProperties_KeyIsNullOrEmpty_ThrowsArgumentException(string oldKey, string newKey)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.RenameProperties(oldKey, newKey), Throws.ArgumentException);
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

            var expected = new[] { new IniProperty("NewKey", "Value1"), new IniProperty("Key2", "Value2"), new IniProperty("NewKey", "Value3") };

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

            Assert.That(() => section.AddComment(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("TestComment")]
        public void AddComment_ValidComment_AddsComment(string comment)
        {
            var section = new IniSection("TestSection");

            section.AddComment(comment);

            Assert.That(section.CommentCount, Is.EqualTo(1));
            Assert.That(section.Comments, Has.Exactly(1).EqualTo(comment));
        }

        [Test]
        public void AddMultipleComments_CommentsIsNull_ThrowsArgumentNullException()
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.AddMultipleComments(null), Throws.ArgumentNullException);
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

            Assert.That(() => section.RemoveComment(null), Throws.ArgumentException);
        }

        [Test]
        public void RemoveComment_ExistingComment_RemovesComment()
        {
            var section = new IniSection("TestSection");
            var comment = "TestComment";

            section.AddComment(comment);
            section.RemoveComment(comment);

            Assert.That(section.Comments, Is.Empty);
        }

        [Test]
        public void RemoveComment_DifferentComment_DoesNotRemoveComment()
        {
            var section = new IniSection("TestSection");

            section.AddComment("TestComment");
            section.RemoveComment("OtherComment");

            Assert.That(section.CommentCount, Is.EqualTo(1));
            Assert.That(section.Comments, Has.Exactly(1).EqualTo("TestComment"));
        }

        [Test]
        public void ClearComments_WithComments_RemovesAllComments()
        {
            var section = new IniSection("TestSection");
            section.AddComment("Comment1");
            section.AddComment("Comment2");

            section.ClearComments();

            Assert.That(section.Comments, Is.Empty);
        }

        [Test]
        public void ClearComments_WithoutComments_DoesNothing()
        {
            var section = new IniSection("TestSection");

            section.ClearComments();

            Assert.That(section.Comments, Is.Empty);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void IsNameEqualTo_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var section = new IniSection("TestSection");

            Assert.That(() => section.IsNameEqualTo(name), Throws.ArgumentException);
        }

        [Test]
        [TestCase("testsection")]
        [TestCase("TestSection")]
        [TestCase("TESTSECTION")]
        public void IsNameEqualTo_SameCaseInsensitiveName_ReturnsTrue(string name)
        {
            var section = new IniSection("TestSection");

            bool equalTo = section.IsNameEqualTo(name);

            Assert.That(equalTo, Is.True);
        }

        [Test]
        public void IsNameEqualTo_DifferentName_ReturnsFalse()
        {
            var section = new IniSection("TestSection");

            bool equalTo = section.IsNameEqualTo("OtherSection");

            Assert.That(equalTo, Is.False);
        }

        [Test]
        public void Equals_ObjectAndSection_ReturnsFalse()
        {
            var obj = new object();
            var section = new IniSection("TestSection");

            bool equals = section.Equals(obj);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_SectionAndNull_ReturnsFalse()
        {
            var section = new IniSection("TestSection");

            bool equals = section.Equals(null);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_SameSectionReference_ReturnsTrue()
        {
            var section = new IniSection("TestSection");

            bool equals = section.Equals(section);

            Assert.That(equals, Is.True);
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

            bool equals = section1.Equals(section2);

            Assert.That(equals, Is.True);
        }

        [Test]
        public void Equals_DifferentSections_ReturnsFalse()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("OtherSection");

            bool equals = section1.Equals(section2);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_SameSectionsDifferentProperties_ReturnsFalse()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            section1.AddProperty("TestKey", "TestValue");
            section2.AddProperty("TestKey", "OtherValue");

            bool equals = section1.Equals(section2);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_SameSectionsDifferentComments_ReturnsFalse()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            section1.AddComment("TestComment");
            section1.AddComment("OtherComment");

            bool equals = section1.Equals(section2);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void GetHashCode_SameCaseInsensitiveNames_SameHashCode()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TESTSECTION");

            int hashCode1 = section1.GetHashCode();
            int hashCode2 = section2.GetHashCode();

            Assert.That(hashCode2, Is.EqualTo(hashCode1));
        }

        [Test]
        public void GetHashCode_DifferentNames_DifferentHashCode()
        {
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("OtherSection");

            int hashCode1 = section1.GetHashCode();
            int hashCode2 = section2.GetHashCode();

            Assert.That(hashCode2, Is.Not.EqualTo(hashCode1));
        }
    }
}