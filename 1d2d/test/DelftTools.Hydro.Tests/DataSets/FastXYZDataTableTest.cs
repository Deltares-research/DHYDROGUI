using System;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.DataSets
{
    [TestFixture]
    public class FastXYZDataTableTest
    {
        readonly Random random = new Random();

        [Test]
        [Category(TestCategory.Performance)]
        public void SerializeAndDeserialize()
        {
            FastDataTableTestHelper.TestSerializationIsFastAndCorrect<FastXYZDataTable>(35, 30,
                                                                                        (t) =>
                                                                                            {
                                                                                                t.EnforceConstraints = false;
                                                                                                t.AddCrossSectionXYZRow(
                                                                                                    random.NextDouble(),
                                                                                                    random.NextDouble(),
                                                                                                    random.NextDouble());
                                                                                            });
        }
    }
}