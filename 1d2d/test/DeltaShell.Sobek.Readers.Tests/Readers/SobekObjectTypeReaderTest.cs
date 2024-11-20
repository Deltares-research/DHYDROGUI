using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekObjectTypeReaderTest
    {
        [Test]
        public void ReadRecord()
        {
            var source = "OBID id '339' ci 'SBK_GRIDPOINTFIXED' obid";
            var objectTypeData = SobekObjectTypeReader.GetSobekObjectTypeData(source);

            Assert.AreEqual("339", objectTypeData.ID);
            Assert.AreEqual(SobekObjectType.SBK_GRIDPOINTFIXED, objectTypeData.Type);

            source = "aisuhfrohgodp9g8uv n4h98yr 2t92-t9 df9285";
            objectTypeData = SobekObjectTypeReader.GetSobekObjectTypeData(source);
            Assert.IsNull(objectTypeData, "No match");

            source = "OBID aa '339' bb 'SBK_GRIDPOINTFIXED' obid";
            objectTypeData = SobekObjectTypeReader.GetSobekObjectTypeData(source);
            Assert.IsNull(objectTypeData, "No match");

            source = "OBID id '339' ci 'non_existent' obid";
            objectTypeData = SobekObjectTypeReader.GetSobekObjectTypeData(source);
            Assert.IsNull(objectTypeData, "No match");
        }
    }
}