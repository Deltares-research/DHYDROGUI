using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.ModelSchema
{
    [TestFixture]
    public class ModelPropertyValueParsingExtensionsTest
    {
        [TestCase("", new string[]
                      {})]
        [TestCase("one.ldb", new[]
        {
            "one.ldb"
        })]
        [TestCase("one.ldb two.ldb", new[]
        {
            "one.ldb",
            "two.ldb"
        })]
        [TestCase("one.ldb two.ldb three.ldb", new[]
        {
            "one.ldb",
            "two.ldb",
            "three.ldb"
        })]
        [TestCase("I have spaces.ldb", new[]
        {
            "I have spaces.ldb"
        })]
        [TestCase("I have spaces.ldb Me too.ldb", new[]
        {
            "I have spaces.ldb",
            "Me too.ldb"
        })]
        public void GetFileNamesTest(string concatenatedString, string[] expectedFileNames)
        {
            var mocks = new MockRepository();

            var modelPropertyDefinition = mocks.Stub<ModelPropertyDefinition>();
            modelPropertyDefinition.DataType = typeof(string);

            var modelProperty = new WaterFlowFMProperty(modelPropertyDefinition, KnownProperties.LandBoundaryFile) {Value = concatenatedString};

            List<string> fileNames = modelProperty.GetFileNames(".ldb", ' ').ToList();

            Assert.IsNotNull(fileNames);
            Assert.AreEqual(fileNames.Count, expectedFileNames.Count());
            CollectionAssert.AreEqual(fileNames, expectedFileNames);
        }
    }
}