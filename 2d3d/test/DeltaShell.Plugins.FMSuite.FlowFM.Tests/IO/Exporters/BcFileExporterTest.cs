using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
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
            exporter = new BcFileExporter();
        }

        [Test]
        public void GivenAnBcFileExporter_WhenExportIsCalledWithANullItem_ThenFalseIsReturned()
        {
            Assert.That(exporter.Export(null, "myFile.tmp"), Is.False);
        }

        [Test]
        public void GivenAnBcFileExporter_WhenSourceTypesIsCalled_ThenAnEnumerableContainingTheSourceTypesIsReturned()
        {
            IEnumerable<Type> obtainedValues = exporter.SourceTypes();
            Assert.That(obtainedValues.Count(), Is.EqualTo(2));
            Assert.That(obtainedValues.Contains(typeof(BoundaryConditionSet)), Is.True);
            Assert.That(obtainedValues.Contains(typeof(IList<BoundaryConditionSet>)));
        }
    }
}