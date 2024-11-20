using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Extentions
{
    [TestFixture]
    public class LocationAliasesExtensionsTest
    {
        [Test]
        public void TestParseLocationAliases()
        {
            var mockRepository = new MockRepository();
            var aliases = mockRepository.Stub<IHasLocationAliases>();
            aliases.Stub(x => x.LocationAliases).Return("I, was, ,     , made, \t, for   , loving you baby,,,  \t \n");
            mockRepository.ReplayAll();

            Assert.AreEqual("I, was, ,     , made, \t, for   , loving you baby,,,  \t \n", aliases.LocationAliases);

            List<string> result = aliases.ParseLocationAliases();

            CollectionAssert.AreEquivalent(new[]
            {
                "I",
                "was",
                "made",
                "for",
                "loving you baby"
            }, result);
        }
    }
}