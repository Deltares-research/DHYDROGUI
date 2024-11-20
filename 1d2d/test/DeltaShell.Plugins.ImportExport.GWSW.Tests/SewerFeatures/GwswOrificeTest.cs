using DelftTools.Hydro;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.SewerFeatures
{
    [TestFixture]
    public class GwswOrificeTest
    {
        
        [Test]
        public void GivenGwswConnectionOrifice_WhenGettingStructureType_ThenOrificeTypeIsReturned()
        {
            var connectionOrifice = new GwswConnectionOrifice("myOrifice");
            Assert.That(connectionOrifice.GetStructureType(), Is.EqualTo(StructureType.Orifice));
        }
    }
}