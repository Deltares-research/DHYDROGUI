using System.Collections;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class MultiDimentionalArrayAdapterTest
    {
        [Test]
        public void ReadFromMultiDimentionalArrayAdapter()
        {
            var values = new List<int>
            {
                2,
                4,
                6,
                7,
                8
            };
            var mda = new MultiDimentionalArrayAdapter<int>(values);

            var mdaCollection = (ICollection) mda;
            var mdaCollectionTyped = (ICollection<int>) mda;
            var mdaTyped = (IMultiDimensionalArray<int>) mda;
            var mdaGeneral = (IMultiDimensionalArray) mda;
            var mdaList = (IList) mda;
            var mdaTypedList = (IList<int>) mda;

            Assert.AreEqual(5, mdaCollection.Count);
            Assert.AreEqual(5, mdaCollectionTyped.Count);
            Assert.AreEqual(5, mdaTyped.Count);
            Assert.AreEqual(5, mdaGeneral.Count);
            Assert.AreEqual(5, mdaList.Count);

            Assert.IsTrue(mdaCollectionTyped.IsReadOnly);
            Assert.IsTrue(mdaGeneral.IsReadOnly);
            Assert.IsTrue(mdaList.IsReadOnly);

            Assert.AreEqual(2, mda.MinValue);
            Assert.AreEqual(8, mda.MaxValue);

            Assert.AreEqual(2, mda.IndexOf(6));
            Assert.AreEqual(2, mda.IndexOf((object) 6));

            Assert.AreEqual(6, mdaTyped[2]);
            Assert.AreEqual(6, mdaGeneral[2]);
            Assert.AreEqual(6, mdaList[2]);
            Assert.AreEqual(6, mdaTypedList[2]);

            Assert.True(mda.Contains(7));
            Assert.True(mda.Contains((object) 7));
        }
    }
}