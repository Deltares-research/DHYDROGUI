using ProtoBuf;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    [ProtoContract(AsReferenceDefault = true)]
    public class LinkInformation
    {
        [ProtoMember(1)]
        public int[] fromIndices;

        [ProtoMember(2)]
        public int[] toIndices;
    }
}