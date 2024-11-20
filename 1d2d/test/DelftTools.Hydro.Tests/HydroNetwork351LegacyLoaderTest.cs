using System.Data;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNetwork351LegacyLoaderTest
    {
        [Test]
        public void LegacyLoaderShouldMakeCompositeStructuresInHydroNetworksUnique()
        {
            var mocks = new MockRepository();
            var network1 = mocks.StrictMock<IHydroNetwork>();
            var network2 = mocks.StrictMock<IHydroNetwork>();
            var connection = mocks.StrictMock<IDbConnection>();

            var compositeStructureList1 = Enumerable.Range(1, 100).Select(i => new CompositeBranchStructure("Test", 0)).ToList();
            var compositeStructureList2 = Enumerable.Range(1, 50).Select(i => new CompositeBranchStructure("Test 2", 0)).ToList();

            network1.Expect(n => n.BranchFeatures).Return(compositeStructureList1).Repeat.Any();
            network2.Expect(n => n.BranchFeatures).Return(compositeStructureList2).Repeat.Any();

            mocks.ReplayAll();

            Assert.IsFalse(compositeStructureList1.Select(c => c.Name).HasUniqueValues(), "Composite structures should not be unique");
            Assert.IsFalse(compositeStructureList2.Select(c => c.Name).HasUniqueValues(), "Composite structures should not be unique");

            var legacyLoader = new HydroNetwork351LegacyLoader();

            legacyLoader.OnAfterInitialize(network1, connection);
            legacyLoader.OnAfterInitialize(network2, connection);

            legacyLoader.OnAfterProjectMigrated(null);
            
            Assert.IsTrue(compositeStructureList1.Select(c => c.Name).HasUniqueValues(), "Composite structures are not unique");
            Assert.IsTrue(compositeStructureList2.Select(c => c.Name).HasUniqueValues(), "Composite structures are not unique");

            mocks.VerifyAll();
        }
    }
}