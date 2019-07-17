using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelReaderTest
    {
        [Test]
        public void ConstructEmptyHydroModel()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var list = new List<IDimrModelFileImporter>();

            var hydroModel = HydroModelReader.Read(dimrPath, list);

            Assert.NotNull(hydroModel);
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(0));
        }
    }
}
