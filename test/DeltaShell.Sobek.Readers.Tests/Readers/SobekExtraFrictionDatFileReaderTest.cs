using System;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekExtraFrictionDatFileReaderTest
    {
        [Test]
        public void Split()
        {
            string source =
                @"XRST id '5580432' nm 'vak103_104' ci '052' lc 3635 rt rs 'Extra Resistance Table' PDIN 0 0 '' pdin CLTT 'Water Level' 'Extra Resistance' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"19 0 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" xrst" + Environment.NewLine +
                @"XRST id '5580594' nm 'Berg' ci '004' lc 19521 rt rs 'Extra Resistance Table' PDIN 0 0 '' pdin CLTT 'Water Level' 'Extra Resistance' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"27 0 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" xrst";

            var sobekFriction = new SobekReExtraFrictionDatFileReader().Parse(source);      
            Assert.AreEqual(2, sobekFriction.Count()); 
        }

        [Test]
        public void SimpleExtraFrictionTest()
        {
            string source =
                @"XRST id '5580432' nm 'vak103_104' ci '052' lc 3635 rt rs 'Extra Resistance Table' PDIN 0 0 '' pdin CLTT 'Water Level' 'Extra Resistance' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"19 0 < " + Environment.NewLine +
                @"24.15 0 < " + Environment.NewLine +
                @"25.4 0 < " + Environment.NewLine +
                @"27.22 0 < " + Environment.NewLine +
                @"27.92 0 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" xrst";
            var sobekFriction = new SobekReExtraFrictionDatFileReader().Parse(source);

            //SobekFriction sobekFriction = SobekFrictionDatFileReader.ReadSobekFriction(TestHelper.GetTestDataDirectory() + @"\friction\SimpleBedFriction.dat");
            Assert.AreEqual(1, sobekFriction.Count());
            var extraFriction = sobekFriction.FirstOrDefault();
            Assert.AreEqual("5580432", extraFriction.Id);
            Assert.AreEqual("vak103_104", extraFriction.Name);
            Assert.AreEqual("052", extraFriction.BranchId);
            Assert.AreEqual(3635.0, extraFriction.Chainage, 1.0e-6);
            Assert.AreEqual(5, extraFriction.Table.Rows.Count);
        }
    }
}
