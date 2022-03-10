using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests   
{
    [TestFixture]
    public class PumpBuilderTest
    {
        [Test]
        public void BuilderCreatesTwoPumpsFromSobekStructureDefinitionAndPumpData()
        {
            var sobekPump = new SobekPump { Direction = -1 };
            sobekPump.CapacityTable.Rows.Add(new object[] { 2.0, 0.0, 0.0, 0.0, 0.0 });
            sobekPump.CapacityTable.Rows.Add(new object[] { 4.0, 0.0, 0.0, 0.0, 0.0 });
            var structure = new SobekStructureDefinition
                                {
                                    Name = "LowCapacityPump", 
                                    Definition = sobekPump,
                                    Type = 9 // UrbanPump
                                };

            var builder = new PumpBuilder();
            var pumps = builder.GetBranchStructures(structure);
            Assert.AreEqual(2, pumps.Count());
        }

        [Test]
        public void BuilderRenamesNewPumpsCorrectly()
        {
            const string pumpName = "HighCapacityPump";
            var sobekPump = new SobekPump();
            sobekPump.CapacityTable.Rows.Add(new object[] { 220.0, 0.0, 0.0, 0.0, 0.0 });
            sobekPump.CapacityTable.Rows.Add(new object[] { 440.0, 0.0, 0.0, 0.0, 0.0 });
            var structure = new SobekStructureDefinition
            {
                
                Name = pumpName,
                Definition = sobekPump,
                Type = 9 // UrbanPump
            };

            var builder = new PumpBuilder();
            var pumps = builder.GetBranchStructures(structure);
            
            Assert.AreEqual("", pumps.ElementAt(0).Name); //will be used as profix for the structure name
            Assert.AreEqual("2", pumps.ElementAt(1).Name);
        }

        [Test]
        public void BuilderCreatesPumpWithCorrectPropertyValues()
        {
            var sobekPump = new SobekPump();
            sobekPump.CapacityTable.Rows.Add(new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
            var structure = new SobekStructureDefinition
            {
                Definition = sobekPump,
                Type = 9 // UrbanPump
            };

            var builder = new PumpBuilder();
            var pump = builder.GetBranchStructures(structure).FirstOrDefault();
            
            Assert.AreEqual(1, pump.Capacity);
            Assert.AreEqual(2, pump.StartSuction);
            Assert.AreEqual(3, pump.StopSuction);
            Assert.AreEqual(4, pump.StartDelivery);
            Assert.AreEqual(5, pump.StopDelivery);
        }

        [Test]
        public void BuilderCreatesPumpWithCorrectDirectionValues()
        {
            var sobekPump = new SobekPump { Direction = 1 };
            sobekPump.CapacityTable.Rows.Add(new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
            var structure = new SobekStructureDefinition
            {
                Definition = sobekPump,
                Type = 9 // UrbanPump
            };

            // Rules:
            // type 9 && dir 1 -> PumpControlDirection.SuctionSideControl; DirectionIsPositive = true;
            // type 9 && dir 2 -> PumpControlDirection.DeliverySideControl; DirectionIsPositive = true;
            // type 9 && dir 3 -> PumpControlDirection.SuctionAndDeliverySideControl; DirectionIsPositive = true;
            // type 9 && dir -1 -> PumpControlDirection.SuctionSideControl; DirectionIsPositive = false;
            // type 9 && dir -2 -> PumpControlDirection.DeliverySideControl; DirectionIsPositive = false; 
            // type 9 && dir -3 -> PumpControlDirection.SuctionAndDeliverySideControl; DirectionIsPositive = false;
            // type 3 && dir 1 -> PumpControlDirection.SuctionSideControl; DirectionIsPositive = true;
            // type 3 && dir -1 -> PumpControlDirection.SuctionSideControl; DirectionIsPositive = false;
            // type 3 && dir 2 -> PumpControlDirection.DeliverySideControl; DirectionIsPositive = true;
            // type 3 && dir -2 -> PumpControlDirection.DeliverySideControl; DirectionIsPositive = false; 

            var builder = new PumpBuilder();

            var pump = builder.GetBranchStructures(structure).FirstOrDefault();
            Assert.AreEqual(PumpControlDirection.SuctionSideControl, pump.ControlDirection);
            Assert.IsTrue(pump.DirectionIsPositive);
        }

        [Test]
        public void ConstantReductionFactorIsAddedByReducingCapacity()
        {
            var sobekPump = new SobekPump
                                {
                                    Direction = 1,
                                };
            sobekPump.ReductionTable.Rows.Add(new object[] {0, 0.95});
            sobekPump.CapacityTable.Rows.Add(new object[]
                                                 {
                                                     100.0, //Capacity
                                                     2.0,   //StartSuction
                                                     3.0,   //StopSuction
                                                     4.0,   //StartDelivery
                                                     5.0    //StopDelivery
                                                 });
            
            var structure = new SobekStructureDefinition
            {
                Definition = sobekPump
            };

            var builder = new PumpBuilder();
            var pump = builder.GetBranchStructures(structure).FirstOrDefault();

            Assert.AreEqual(100, pump.Capacity);
            Assert.AreEqual(0.95, (double)sobekPump.ReductionTable.Rows[0][1], 1.0e-6);

        }

        [Test]
        public void BuilderCreatesPompPerCapacityStageCheckCapacity()
        {
            var sobekPump = new SobekPump { Direction = -1 };
            sobekPump.CapacityTable.Rows.Add(new object[] { 2.0, 0.0, 0.0, 0.0, 0.0 });
            sobekPump.CapacityTable.Rows.Add(new object[] { 6.0, 0.0, 0.0, 0.0, 0.0 });
            sobekPump.CapacityTable.Rows.Add(new object[] { 9.0, 0.0, 0.0, 0.0, 0.0 });
            var structure = new SobekStructureDefinition
            {
                Name = "3StageCapacityPump",
                Definition = sobekPump,
                Type = 9 // UrbanPump
            };

            var builder = new PumpBuilder();
            var pumps = builder.GetBranchStructures(structure).ToList();
            Assert.AreEqual(3, pumps.Count());
            Assert.AreEqual(2.0, pumps[0].Capacity);
            Assert.AreEqual(4.0, pumps[1].Capacity);
            Assert.AreEqual(3.0, pumps[2].Capacity);
        }
    }
}