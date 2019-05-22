using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests   
{
    [TestFixture]
    public class WeirNamerTest
    {
        [Test]
        public void GetName()
        {
            var namer = new WeirNamer();
            var structure = new SobekStructureDefinition();
            Assert.AreEqual("Weir1", namer.GetName(structure));
            Assert.AreEqual("Weir2", namer.GetName(structure));
            //if a name is given use that
            structure.Name = "MegaWeir";
            Assert.AreEqual("MegaWeir", namer.GetName(structure));
        }

    }
}