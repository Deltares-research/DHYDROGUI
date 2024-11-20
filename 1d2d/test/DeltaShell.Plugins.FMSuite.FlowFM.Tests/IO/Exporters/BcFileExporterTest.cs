using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class BcFileExporterTest
    {
        private BcFileExporter exporter;

        [SetUp]
        public void Setup()
        {
            this.exporter = new BcFileExporter();
        }

        [Test]
        public void GivenAnBcFileExporterWhenExportIsCalledWithANullItemAndAPathThenFalseIsReturned()
        {
            Assert.That(exporter.Export(null, "myFile.tmp"), Is.False);
        }

        [Test]
        public void GivenAnBcFileExporterWhenSourceTypesIsCalledThenAnEnumerableContainingTheSourceTypesIsReturned()
        {
            var obtainedValues = exporter.SourceTypes();
            Assert.That(obtainedValues.Count(), Is.EqualTo(2));
            Assert.That(obtainedValues.Contains(typeof(BoundaryConditionSet)), Is.True);
            Assert.That(obtainedValues.Contains(typeof(IList<BoundaryConditionSet>)));
        }
    }
}
