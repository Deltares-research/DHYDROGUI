using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Utils.Extensions;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test.Extensions
{
    [TestFixture]
    public class DictionaryExtensionsTest
    {
        [Test]
        public void AddToList_DictionaryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IDictionary<string, IList<string>>)null).AddToList("key", "value");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("dictionary"));
        }

        [Test]
        public void AddToList_KeyExists_AddsValueToList()
        {
            // Setup
            var dictionary = new Dictionary<string, IList<string>> { ["some_key"] = new List<string>() };

            // Call
            dictionary.AddToList("some_key", "some_value");

            // Assert
            Assert.That(dictionary["some_key"], Is.EqualTo(new[]
            {
                "some_value"
            }));
        }

        [Test]
        public void AddToList_KeyDoesNotExist_CreatesListWithValue()
        {
            // Setup
            var dictionary = new Dictionary<string, IList<string>>();

            // Call
            dictionary.AddToList("some_key", "some_value");

            // Assert
            Assert.That(dictionary["some_key"], Is.EqualTo(new[]
            {
                "some_value"
            }));
        }
    }
}