using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelReaderTest
    {
        [Test]
        public void ConstructEmptyHydroModel()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var list = new List<IDimrModelFileImporter>();

            HydroModel hydroModel = HydroModelReader.Read(dimrPath, list);

            Assert.NotNull(hydroModel);
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(0));
        }
    }
}