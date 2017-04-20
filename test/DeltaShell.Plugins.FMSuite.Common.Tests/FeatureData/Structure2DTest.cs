using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    [TestFixture]
    public class Structure2DTest
    {
        [Test]
        public void GetPropertyTest()
        {
            var structure = new Structure2D("test");
            Assert.IsNull(structure.GetProperty("name"));

            structure.AddProperty("name", typeof(string), "value");

            var property = structure.GetProperty("NaMe");
            Assert.IsNotNull(property, "Case insensitivity expected.");
            Assert.AreEqual(structure.Properties.ElementAt(0), property);
        }
    }
}