using System;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class IniImporterTest
    {
        [Test]
        public void givenStructuresFile_CountsStructuresCorrectly()
        {
            string filePath = TestHelper.GetTestFilePath(@"GeneralStructures\BasicModel\FlowFM_structures2.ini");

            string iniPath = Path.GetFullPath(filePath);

            if (!File.Exists(iniPath))
            {
                throw new FileNotFoundException("Structures ini file not found");
            }

            // Setup 
            var targetArea = new HydroArea();
            var flowFmModel = new WaterFlowFMModel();
            var importer = new Area2DStructuresImporter();
            importer.GetModelForArea = area => flowFmModel;

            // Call

            Action call = () => importer.ImportItem(iniPath, targetArea);

            // Assert
            TestHelper.AssertLogMessageIsGenerated(call, "Read: 6 structures (Weirs: 3 Gates : 2 General structures: 1)");
        }
    }
}