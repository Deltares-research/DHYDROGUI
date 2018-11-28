using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelReaderTest
    {
        //[Test]
        //public void ConstructEmptyHydroModel()
        //{
        //    var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
        //    var list = new List<IFileImporter>();
        //    list.Add(new WaterFlowModel1DFileImporter());

        //   //var hydroModel = HydroModelConverter.Read(dimrPath, list);

        //    //Assert.NotNull(hydroModel);
        //    //Assert.That(hydroModel.Activities.Count, Is.EqualTo(0));
        //}

        //[Test]
        //public void ConstructHydroModelWithFlow1DAndRtc()
        //{
        //    var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
        //    var hydroModel = HydroModelReader.Read(dimrPath, null);

        //    Assert.NotNull(hydroModel);
        //    Assert.That(hydroModel.Activities.Count, Is.EqualTo(2));
        //    Assert.That(hydroModel.Activities.ElementAt(0).Name, Is.EqualTo("flow1d"));
        //    Assert.That(hydroModel.Activities.ElementAt(1).Name, Is.EqualTo("rtc"));
        //}  
    }
}
