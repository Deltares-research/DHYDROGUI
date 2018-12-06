using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NUnit.Framework;
using Rhino.Mocks;
using Is = NUnit.Framework.Is;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class DHydroConfigXmlImporterTest
    {
        [Test]
        public void ImportTestWithoutActivities()
        {
            var dimrXmlPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            var mocks = new MockRepository();
            var importers = mocks.DynamicMock(typeof(Func<List<IDimrModelFileImporter>>));
            var importer = mocks.DynamicMock<DHydroConfigXmlImporter>(importers);
       
            var model =importer.ImportItem(dimrXmlPath);

            Assert.IsNotNull(model);
            Assert.That(model, Is.TypeOf<HydroModel>());
        }
    }
}
