using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NUnit.Framework;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class CalibrateRoughnessImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void CalibrateRoughnessImporterPropertiesTest()
        {
            var importer = new CalibratedRoughnessImporter();
            var expectedSupportedItemTypes = new List<Type>{
                typeof(IList<RoughnessSection>)};
            var importerSupportedItemTypes = importer.SupportedItemTypes;
            Assert.IsFalse(expectedSupportedItemTypes.Any(e => !importerSupportedItemTypes.Contains(e)));
            Assert.IsFalse(importerSupportedItemTypes.Any(e => !expectedSupportedItemTypes.Contains(e)));

            Assert.IsFalse(expectedSupportedItemTypes.Any(s => !importer.CanImportOn(s)));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CalibrateRoughnessImporterImportItemTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var sections = new List<RoughnessSection>{new RoughnessSection(new CrossSectionSectionType(), network)};

            var importer = new CalibratedRoughnessImporter();
            TestHelper.AssertLogMessagesCount(() => importer.ImportItem(null, sections), 1 );
            /* We do not care for the reading as that it's done in another function 
             RoughnessFileReader.ReadFile */
        }
    }
}