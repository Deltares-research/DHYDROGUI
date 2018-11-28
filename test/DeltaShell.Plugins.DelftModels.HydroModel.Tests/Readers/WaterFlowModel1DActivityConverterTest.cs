using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class WaterFlowModel1DActivityConverterTest
    {
        private WaterFlowModel1DActivityConverter converter;
        private Func<List<IDimrModelFileImporter>> importers;

        [Test]
        public void Setup()
        {
            var mocks = new MockRepository();
            converter = mocks.DynamicMock<WaterFlowModel1DActivityConverter>();
            //converter = new WaterFlowModel1DActivityConverter();
            importers = mocks.DynamicMock<Func<List<IDimrModelFileImporter>>>();
        }

        //[Test]
        //public void Convert()
        //{
        //    var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));
        //    var dimrObjectModel = DelftConfigXmlFileParser.Read(dimrPath);
            
        //    //converter.Convert(dimrObjectModel, dimrPath, importers);
        //}
    }
}
