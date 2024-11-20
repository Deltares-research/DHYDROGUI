using System;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.DataSets
{
    [TestFixture]
    public class FastZWDataTableTest
    {
        readonly Random random = new Random();
        
        [Test]
        [NUnit.Framework.Category(TestCategory.Performance)]
        public void SerializeAndDeserialize()
        {

            FastDataTableTestHelper.TestSerializationIsFastAndCorrect<FastZWDataTable>(8, 30, (t) =>
                                                                                               t.AddCrossSectionZWRow(
                                                                                                   random.NextDouble(),
                                                                                                   random.NextDouble(),
                                                                                                   random.NextDouble()));
        }
    }
}