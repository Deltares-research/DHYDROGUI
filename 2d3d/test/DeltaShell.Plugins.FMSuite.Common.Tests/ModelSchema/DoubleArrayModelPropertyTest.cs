using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.ModelSchema
{
    [TestFixture]
    public class DoubleArrayModelPropertyTest
    {
        [Test]
        public void ModelPropertyAcceptsListOfDouble()
        {
            var doubleArrayDefinition = new TestModelPropertyDefinition { DataType = typeof(IList<double>) };
            var doubleArrayProperty = new TestModelProperty(doubleArrayDefinition, "1 2 3");
            doubleArrayProperty.Value = new List<double> {5, 6, 7, 8}; // Should not throw
            Assert.AreEqual(new List<double>{5,6,7,8}, doubleArrayProperty.Value);
        }
    }
}