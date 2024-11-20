using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class ImportedNetFileXyzExporterTest
    {
        [Test]
        public void GivenImportedNetFileXyzExporterWhenGettingSourceTypesThenReturnImportedFMNetFile()
        {
            var xyzExporter = new ImportedNetFileXyzExporter();
            IList<Type> types = xyzExporter.SourceTypes().AsList();

            Assert.That(types.Count, Is.EqualTo(1));
            Assert.That(types[0], Is.EqualTo(typeof(ImportedFMNetFile)));
        }
    }
}