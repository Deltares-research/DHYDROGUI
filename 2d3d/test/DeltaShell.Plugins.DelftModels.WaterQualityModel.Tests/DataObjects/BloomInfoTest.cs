using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects
{
    [TestFixture]
    public class BloomInfoTest
    {
        [Test]
        public void GetKortsInFunctionsTest()
        {
            BloomInfo info = CreateBloomInfo();

            IEventedList<IFunction> functions = CreateFunctionList();

            IEnumerable<string> result = info.GetKortsPresentInFunctions(functions);

            var expected = new[]
            {
                "1",
                "2",
                "3",
                "5"
            };

            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void GetHeadersInFunctionsTest()
        {
            BloomInfo info = CreateBloomInfo();

            IEventedList<IFunction> functions = CreateFunctionList();

            IEnumerable<string> result = info.GetHeadersPresentInFunctions(functions);

            var expected = new[]
            {
                "AALG",
                "BALG",
                "CALG"
            };

            CollectionAssert.AreEquivalent(expected, result);
        }

        public static BloomInfo CreateBloomInfo()
        {
            return new BloomInfo(
                new List<string>()
                {
                    "AALG",
                    "BALG",
                    "CALG",
                    "EALG"
                },
                new List<string>()
                {
                    "1",
                    "2",
                    "3",
                    "5"
                },
                new List<string>()
                {
                    "first",
                    "second",
                    "third",
                    "fifth"
                });
        }

        public static IEventedList<IFunction> CreateFunctionList()
        {
            return new EventedList<IFunction>
            {
                WaterQualityFunctionFactory.CreateConst("A1", 0.3d, "value", "m", "A"),
                WaterQualityFunctionFactory.CreateConst("B2", 0.3d, "value", "m", "B"),
                WaterQualityFunctionFactory.CreateConst("B5", 0.3d, "value", "m", "B"),
                WaterQualityFunctionFactory.CreateConst("C3", 0.3d, "value", "m", "C"),
                WaterQualityFunctionFactory.CreateConst("B4", 0.3d, "value", "m", "B"),
                WaterQualityFunctionFactory.CreateConst("D4", 0.3d, "value", "m", "D")
            };
        }
    }
}