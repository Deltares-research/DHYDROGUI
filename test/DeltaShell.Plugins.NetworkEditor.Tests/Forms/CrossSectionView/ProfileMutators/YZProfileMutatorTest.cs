using System.Linq;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView.ProfileMutators
{
    [TestFixture]
    public class YZProfileMutatorTest
    {
        [Test]
        [Category("Quarantine")]
        public void AddPointSetsStorageWidth()
        {
            var crossSection = new CrossSectionDefinitionYZ();

            var mutator = new YZProfileMutator(crossSection);
            mutator.AddPoint(20, 0);
            Assert.AreEqual(0, crossSection.YZDataTable[0].DeltaZStorage);

            crossSection.YZDataTable[0].DeltaZStorage = 10;
            mutator.AddPoint(10, -20);
            Assert.AreEqual(10, crossSection.YZDataTable.First(r => r.Yq == 10).DeltaZStorage,
                "Should have set to StorageWidth of direct neighbor");

            crossSection.YZDataTable[0].DeltaZStorage = 0;
            mutator.AddPoint(15, -7);
            Assert.AreEqual(5, crossSection.YZDataTable.First(r => r.Yq == 15).DeltaZStorage,
                "Should have linearly interpolated StorageWidth of direct neighbors");
        }
    }
}