using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.IO.ExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.ExtForce
{
    [TestFixture]
    public class ExtForceFileDataTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            ExtForceFileData data = CreateExtForceFileData();

            Assert.That(data.Forcings, Is.Empty);
        }

        [Test]
        public void Constructor_ForcingsIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => CreateExtForceFileData(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_EmptyForcings_ForcingsIsEmpty()
        {
            ExtForceFileData data = CreateExtForceFileData(Enumerable.Empty<ExtForceData>());

            Assert.That(data.Forcings, Is.Empty);
        }

        [Test]
        public void Constructor_MultipleExtForceDataObjects_ForcingsContainsSameInstances()
        {
            ExtForceData forcing1 = CreateExtForceData();
            ExtForceData forcing2 = CreateExtForceData();
            ExtForceFileData data = CreateExtForceFileData(new[] { forcing1, forcing2 });

            Assert.That(data.Forcings, Is.EqualTo(new[] { forcing1, forcing2 }));
        }

        [Test]
        public void AddForcing_ForcingIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileData data = CreateExtForceFileData();

            Assert.That(() => data.AddForcing(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddForcing_ExtForceDataObject_ForcingsContainsExtForceDataObjects()
        {
            ExtForceFileData data = CreateExtForceFileData();
            ExtForceData forcing = CreateExtForceData();

            data.AddForcing(forcing);

            Assert.That(data.Forcings, Has.Exactly(1).SameAs(forcing));
        }

        [Test]
        public void AddForcing_ExtForceDataObjectAddedTwice_ForcingsContainsExtForceDataTwice()
        {
            ExtForceFileData data = CreateExtForceFileData();
            ExtForceData forcing = CreateExtForceData();

            data.AddForcing(forcing);
            data.AddForcing(forcing);

            Assert.That(data.Forcings, Has.Exactly(2).SameAs(forcing));
        }

        [Test]
        public void AddMultipleForcings_ForcingsIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileData data = CreateExtForceFileData();

            Assert.That(() => data.AddMultipleForcings(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddMultipleForcings_ExtForceDataObject_ForcingsContainsExtForceDataObjects()
        {
            ExtForceFileData data = CreateExtForceFileData();
            ExtForceData forcing1 = CreateExtForceData();
            ExtForceData forcing2 = CreateExtForceData();

            data.AddMultipleForcings(new[] { forcing1, forcing2 });

            Assert.That(data.Forcings, Is.EqualTo(new[] { forcing1, forcing2 }));
        }

        private static ExtForceFileData CreateExtForceFileData()
        {
            return new ExtForceFileData();
        }

        private static ExtForceFileData CreateExtForceFileData(IEnumerable<ExtForceData> forcings)
        {
            return new ExtForceFileData(forcings);
        }

        private static ExtForceData CreateExtForceData()
        {
            return new ExtForceData();
        }
    }
}