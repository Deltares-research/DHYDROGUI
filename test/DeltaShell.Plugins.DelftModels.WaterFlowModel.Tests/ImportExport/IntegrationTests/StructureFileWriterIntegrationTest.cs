using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.IntegrationTests
{
    [TestFixture]
    public class StructureFileWriterIntegrationTest
    {
        [Test]
        public void TestStructureFileWriterGivesExpectedResults_CompoundStructureIDs()
        {
            var flow1Dmodel = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(1) };
            var branch = flow1Dmodel.Network.Channels.First();

            var pump = (IPump) new Pump(false);
            branch.BranchFeatures.Add(pump);

            var weir = new Weir();
            branch.BranchFeatures.Add(weir);

            var culvert = Culvert.CreateDefault();
            branch.BranchFeatures.Add(culvert);

            var composite1 = new CompositeBranchStructure("composite1", 0.5);
            branch.BranchFeatures.Add(composite1);
            composite1.Structures.Add(pump);
            pump.ParentStructure = composite1;
            composite1.Structures.Add(culvert);
            culvert.ParentStructure = composite1;

            var composite2 = new CompositeBranchStructure("composite2", 0.5);
            branch.BranchFeatures.Add(composite2);
            composite2.Structures.Add(weir);
            weir.ParentStructure = composite2;

            StructureFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Structures, flow1Dmodel.Network);

            var categories = new DelftIniReader().ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.Structures).ToList();
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, categories.Count(op => op.Name == StructureRegion.Header));

            var pumpProperties = categories.First(c => c.Properties.Any(p => p.Name == StructureRegion.Id.Key && p.Value == pump.Name)).Properties;
            var weirProperties = categories.First(c => c.Properties.Any(p => p.Name == StructureRegion.Id.Key && p.Value == weir.Name)).Properties;
            var culvertProperties = categories.First(c => c.Properties.Any(p => p.Name == StructureRegion.Id.Key && p.Value == culvert.Name)).Properties;

            var pumpCompoundProperty = pumpProperties.First(p => p.Name == StructureRegion.Compound.Key);
            var weirCompoundProperty = weirProperties.First(p => p.Name == StructureRegion.Compound.Key);
            var culvertCompoundProperty = culvertProperties.First(p => p.Name == StructureRegion.Compound.Key);
            
            int pumpCompoundStructureId;
            Assert.IsTrue(int.TryParse(pumpCompoundProperty.Value, out pumpCompoundStructureId));
            Assert.IsTrue(pumpCompoundStructureId > 0, "A valid CompoundStructureId must be greater than zero");

            int weirCompoundStructureId;
            Assert.IsTrue(int.TryParse(weirCompoundProperty.Value, out weirCompoundStructureId));
            Assert.IsTrue(weirCompoundStructureId <= 0, "An individual structure should have a CompoundStructureId less than or equal to zero");    

            Assert.AreEqual(pumpCompoundProperty.Value, culvertCompoundProperty.Value, "CompoundStructureIds should match for structure with the same parent CompoundStructure");

            var pumpCompoundNameProperty = pumpProperties.FirstOrDefault(p => p.Name == StructureRegion.CompoundName.Key);
            var weirCompoundNameProperty = weirProperties.FirstOrDefault(p => p.Name == StructureRegion.CompoundName.Key);
            var culvertCompoundNameProperty = culvertProperties.FirstOrDefault(p => p.Name == StructureRegion.CompoundName.Key);

            Assert.NotNull(pumpCompoundNameProperty);
            Assert.AreEqual(composite1.Name, pumpCompoundNameProperty.Value);

            // Names for compounds with only 1 sub-structure should not be written to file!
            Assert.Null(weirCompoundNameProperty);

            Assert.NotNull(culvertCompoundNameProperty);
            Assert.AreEqual(composite1.Name, culvertCompoundNameProperty.Value);
        }
    }
}