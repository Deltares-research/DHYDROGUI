using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Utils;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class NameableCollectionExtensionsTest
    {
        private readonly Random random = new Random();

        [Test]
        public void GetByName_ReturnsCorrectResult()
        {
            // Setup
            INameable[] nameables = GetNameableCollection(5).ToArray();
            INameable expectedObj = nameables[random.Next(0, 4)];

            // Call
            INameable result = nameables.GetByName(expectedObj.Name);

            // Assert
            Assert.That(result, Is.SameAs(expectedObj));
        }

        [Test]
        public void GetByName_ReturnsFirst()
        {
            // Setup
            const string name = "name";
            INameable expectedObj = GetNameable(name);

            INameable[] nameables =
            {
                GetNameable("unique_1"),
                expectedObj,
                GetNameable("unique_2"),
                GetNameable(name),
                GetNameable("unique_3"),
            };

            // Call
            INameable result = nameables.GetByName(name);

            // Assert
            Assert.That(result, Is.SameAs(expectedObj));
        }

        [Test]
        public void GetByName_CollectionDoesNotContainsObjectWithName_ReturnsDefault()
        {
            // Setup
            INameable[] nameables = GetNameableCollection(5).ToArray();

            // Call
            INameable result = nameables.GetByName("name");

            // Assert
            Assert.That(result, Is.EqualTo(default(INameable)));
        }

        [Test]
        public void GetByName_ObjectsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => NameableCollectionExtensions.GetByName<INameable>(null, "name");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("objects"));
        }

        [Test]
        public void GetAllByName_ReturnsCorrectResult()
        {
            // Setup
            const string name = "name";
            INameable first = GetNameable(name);
            INameable second = GetNameable(name);

            INameable[] nameables =
            {
                GetNameable("unique_1"),
                first,
                GetNameable("unique_2"),
                second,
                GetNameable("unique_3"),
            };

            // Call
            IEnumerable<INameable> result = nameables.GetAllByName(name);

            // Assert
            CollectionAssert.AreEqual(result, new[]{first, second});
        }

        [Test]
        public void GetAllByName_CollectionDoesNotContainsObjectWithName_ReturnsDefault()
        {
            // Setup
            INameable[] nameables = GetNameableCollection(5).ToArray();

            // Call
            IEnumerable<INameable> result = nameables.GetAllByName("name");

            // Assert
            CollectionAssert.AreEqual(result, Enumerable.Empty<INameable>());
        }

        [Test]
        public void GetAllByName_ObjectsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => NameableCollectionExtensions.GetAllByName<INameable>(null, "name");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("objects"));
        }

        private IEnumerable<INameable> GetNameableCollection(int n)
        {
            for (var i = 0; i < n; i++)
            {
                yield return GetNameable($"nameable{i}");
            }
        }

        private static INameable GetNameable(string name)
        {
            var nameable = Substitute.For<INameable>();
            nameable.Name = name;
            return nameable;
        }
    }
}
