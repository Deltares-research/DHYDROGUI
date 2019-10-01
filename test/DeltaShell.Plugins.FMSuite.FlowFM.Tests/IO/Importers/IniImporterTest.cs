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

            var iniPath = Path.GetFullPath(filePath);

            if (!File.Exists(iniPath))
            {
                throw new FileNotFoundException("Structures ini file not found");
            }

            // Setup 
            HydroArea targetArea = new HydroArea();
            var flowFmModel = new WaterFlowFMModel();
            Area2DStructuresImporter importer = new Area2DStructuresImporter();
            importer.GetModelForArea = area => flowFmModel;
            object hydroArea = null;

            // Call

            Action call = () => hydroArea = importer.ImportItem(iniPath, targetArea);

            // Assert
            TestHelper.AssertLogMessageIsGenerated(call, "Read 11 structures (Pumps: 0; Weirs: 7; Gates: 3; General Structures 1).");
        }
    }
}