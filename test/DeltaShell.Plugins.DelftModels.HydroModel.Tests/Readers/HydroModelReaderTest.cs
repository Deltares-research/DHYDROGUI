using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
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

        [Test]
        public void ConstructHydroModelWithFlow1DAndRtc()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
            var list = new List<IDimrModelFileImporter>()
            {
                new WaterFlowModel1DFileImporter(),
                new RealTimeControlModelImporter()
            };

            var hydroModel = HydroModelReader.Read(dimrPath, list);

            Assert.NotNull(hydroModel);
            Assert.That(hydroModel, Is.TypeOf<HydroModel>());
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(2));
            Assert.That(hydroModel.Activities.ElementAt(0), Is.TypeOf<RealTimeControlModel>());
            Assert.That(hydroModel.Activities.ElementAt(1), Is.TypeOf<WaterFlowModel1D>());
        }
    }
}
