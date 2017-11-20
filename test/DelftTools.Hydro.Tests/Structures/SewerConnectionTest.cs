using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class SewerConnectionTest
    {
        [Test]
        public void CreateSewerConnection()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection);
        }

        [Test]
        public void CreateSewerConnectionGivingName()
        {
            var nameSewer = "TestSewer";
            var sewerConnection = new SewerConnection(nameSewer);
            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(nameSewer, sewerConnection.Name);
        }

        [Test]
        public void CreateSewerConnectionGivingSourceManholeTargetManholeAndLength()
        {
            var sourceManhole = new Manhole("manholSource");
            var targetManhole = new Manhole("manholTarget");

            var length = 0.0;
            var sewerConnection = new SewerConnection(sourceManhole, targetManhole, length);
            Assert.IsNotNull(sewerConnection);

            Assert.AreEqual(sourceManhole, sewerConnection.Source);
            Assert.AreEqual(targetManhole, sewerConnection.Target);
            Assert.AreEqual(length, sewerConnection.Length);
        }

        [Test]
        public void CreateSewerConnectionGivingNameSourceManholeTargetManholeAndLength()
        {
            var nameSewer = "TestSewer";

            var sourceManhole = new Manhole("manholSource");
            var targetManhole = new Manhole("manholTarget");

            var length = 0.0;
            var sewerConnection = new SewerConnection(nameSewer, sourceManhole, targetManhole, length);

            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(nameSewer, sewerConnection.Name);
            Assert.AreEqual(sourceManhole, sewerConnection.Source);
            Assert.AreEqual(targetManhole, sewerConnection.Target);
            Assert.AreEqual(length, sewerConnection.Length);
        }

        [Test]
        public void ChangingSourceCompartmentChangesSourceTest()
        {
            var manholeTest = new Manhole("manholeTest");
            var compartmentTest = new Compartment("compartmentTest");
            manholeTest.Compartments.Add(compartmentTest);

            var sewerConnection = new SewerConnection();
            
            Assert.IsNull(sewerConnection.Source);
            Assert.IsNull(sewerConnection.SourceCompartment);

            sewerConnection.SourceCompartment = compartmentTest;

            Assert.AreEqual(compartmentTest, sewerConnection.SourceCompartment);
            Assert.AreEqual(manholeTest, sewerConnection.Source);
        }

        [Test]
        public void ChangingTargetCompartmentChangesTargetTest()
        {
            var manholeTest = new Manhole("manholeTest");
            var compartmentTest = new Compartment("compartmentTest");
            manholeTest.Compartments.Add(compartmentTest);

            var sewerConnection = new SewerConnection();

            Assert.IsNull(sewerConnection.Target);
            Assert.IsNull(sewerConnection.TargetCompartment);

            sewerConnection.TargetCompartment = compartmentTest;

            Assert.AreEqual(compartmentTest, sewerConnection.TargetCompartment);
            Assert.AreEqual(manholeTest, sewerConnection.Target);
        }

        [Test]
        public void SewerConnectionTypeDefaultValueIsNotNull()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.SewerConnectionType);
            //This default value might change, but at least we have here a trigger.
            Assert.AreEqual(SewerConnectionType.Orifice, sewerConnection.SewerConnectionType);
        }

        [Test]
        public void SewerConnectionWaterTypeDefaultValueIsNotNull()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.WaterType);
            //This default value might change, but at least we have here a trigger.
            Assert.AreEqual(SewerConnectionWaterType.None, sewerConnection.WaterType);
        }
    }
}