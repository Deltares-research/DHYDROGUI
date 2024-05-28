using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class ChannelStructureBuilderTest
    {
        [Test]
        public void ReadNetworkWithSimpleWeir()
        {
            var sobekStructureDefinition = new SobekStructureDefinition {Type = (int) SobekStructureType.weir};
            var sobekWeirDefinition = new SobekWeir();

            sobekStructureDefinition.Definition = sobekWeirDefinition;
            var channel = new Channel();
            ChannelStructureBuilderTestHelper.SetStructureOnChannel(sobekStructureDefinition, channel);

            Assert.AreEqual(2,channel.BranchFeatures.Count);
        }

        [Test]
        public void ReadNetworkWithGatedWeir()
        {
            var sobekStructureDefinition = new SobekStructureDefinition {Type = (int) SobekStructureType.orifice};
            var sobekOrificeDefinition = new SobekOrifice();
            sobekStructureDefinition.Definition = sobekOrificeDefinition;
            var channel = new Channel();

            //let channel structure builder set in on a the channel
            ChannelStructureBuilderTestHelper.SetStructureOnChannel(sobekStructureDefinition, channel);

            //read back a deltashell object
            Assert.AreEqual(2, channel.BranchFeatures.Count);
            Weir weir = channel.BranchFeatures.OfType<Weir>().FirstOrDefault();
            Assert.IsTrue(weir.WeirFormula is GatedWeirFormula );
        }

        [Test]
        public void ReadNetworkWithPump()
        {
            //setup sobek structure
            var sobekStructureDefinition = new SobekStructureDefinition {Type = (int) SobekStructureType.pump};
            var sobekPumpDefinition = new SobekPump();
            sobekPumpDefinition.CapacityTable.Rows.Add(new object[] { 2.0, 0.0, 0.0, 0.0, 0.0 });
            sobekStructureDefinition.Definition = sobekPumpDefinition;
            var channel = new Channel();

            //let channel structure builder set in on a the channel
            ChannelStructureBuilderTestHelper.SetStructureOnChannel(sobekStructureDefinition, channel);

            //read back a deltashell object
            Assert.AreEqual(2, channel.BranchFeatures.Count);
            Pump pump = channel.BranchFeatures.OfType<Pump>().FirstOrDefault();
            Assert.AreEqual(2,pump.Capacity);
        }

        [Test]
        public void ReadNetworkWithBridge()
        {
            //setup sobek structure
            var sobekStructureDefinition = new SobekStructureDefinition {Type = (int) SobekStructureType.bridge};
            var sobekBridgeDefinition = new SobekBridge();
            sobekStructureDefinition.Definition = sobekBridgeDefinition;
            sobekStructureDefinition.Name = "bridge1";

            var channel = new Channel();
            //let channel structure builder set in on a the channel
            ChannelStructureBuilderTestHelper.SetStructureOnChannel(sobekStructureDefinition, channel);

            //read back a deltashell object
            Assert.AreEqual(2, channel.BranchFeatures.Count); // composite structure and bridge
            Bridge bridge = channel.BranchFeatures.OfType<Bridge>().FirstOrDefault();
            // name of the structure is not id of definition but id found in structure.dat: mapping id
            Assert.AreEqual("mapping", bridge.Name);
        }

        [Test]
        public void ReadNetworkWithCulvert()
        {
            //setup sobek structure
            var sobekStructureDefinition = new SobekStructureDefinition { Type = (int)SobekStructureType.culvert };
            var sobekCulvertDefinition = new SobekCulvert();
            sobekStructureDefinition.Definition = sobekCulvertDefinition;
            sobekStructureDefinition.Name = "culvert";

            var channel = new Channel();
            //let channel structure builder set in on a the channel
            ChannelStructureBuilderTestHelper.SetStructureOnChannel(sobekStructureDefinition, channel);

            //read back a deltashell object
            Assert.AreEqual(2, channel.BranchFeatures.Count); // composite structure and bridge
            Culvert culvert = channel.BranchFeatures.OfType<Culvert>().FirstOrDefault();
            // name of the structure is not id of definition but id found in structure.dat: mapping id
            Assert.AreEqual("mapping", culvert.Name);
        }

        [Test]
        public void ReadNetworkWithCulvertAndRoughness()
        {
            //setup sobek structure
            var sobekStructureDefinition = new SobekStructureDefinition { Type = (int)SobekStructureType.culvert };
            var culvertRoughness = new SobekStructureFriction
                                       {
                                           MainFrictionType = 4, // 4 = White-Colebrook
                                           MainFrictionFunctionType = SobekFrictionFunctionType.Constant,
                                           MainFrictionConst = 23,
                                           ID = "frictionID",
                                           StructureDefinitionID = "DefinitionID"
                                       };

            var sobekCulvertDefinition = new SobekCulvert();
            sobekStructureDefinition.Definition = sobekCulvertDefinition;
            sobekStructureDefinition.Name = "DefinitionID";

            var channel = new Channel();
            //let channel structure builder set in on a the channel
            ChannelStructureBuilderTestHelper.SetStructureOnChannel("DefinitionID", "StructureId", sobekStructureDefinition, channel, culvertRoughness);

            //read back a deltashell object
            Assert.AreEqual(2, channel.BranchFeatures.Count); // composite structure and bridge
            Culvert culvert = channel.BranchFeatures.OfType<Culvert>().FirstOrDefault();
            // name of the structure is not id of definition but id found in structure.dat: mapping id
            Assert.AreEqual("StructureId", culvert.Name);
            Assert.AreEqual(CulvertFrictionType.WhiteColebrook, culvert.FrictionType);
            Assert.AreEqual(23, culvert.Friction);
        }

        [Test]
        public void SetLongNameOfWeir()
        {
            var sobekStructureDefinition = new SobekStructureDefinition { Type = (int)SobekStructureType.weir };
            sobekStructureDefinition.Name = "Struct Def Name";
            var sobekWeirDefinition = new SobekWeir();

            sobekStructureDefinition.Definition = sobekWeirDefinition;
            var channel = new Channel();
            ChannelStructureBuilderTestHelper.SetStructureOnChannel(sobekStructureDefinition, channel);

            Assert.AreEqual(2, channel.BranchFeatures.Count);
            Weir weir = channel.BranchFeatures.OfType<Weir>().FirstOrDefault();
            // longname of the weir is not the name of the definition but name of the location
            // NB. When it is part of a compound it is the name of the mapping, in case of Sobek 2.12. That is not tested here.
            Assert.AreEqual("struct loc 1", weir.LongName);
        }

    }
}
