using System;
using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum LinkGeneratingType
    {
        [Description("1D2D embedded (1-to-1)")]
        EmbeddedOneToOne,

        [Description("1D2D embedded (1-to-n)")]
        EmbeddedOneToMany,

        [Description("1D2D lateral")]
        Lateral,

        [Description("Gully sewer")]
        GullySewer
    }

    public static class LinkGeneratingTypeConversionExtensions
    {
        public static LinkStorageType GetLinkStorageType(this LinkGeneratingType linkGeneratingType)
        {
            switch (linkGeneratingType)
            {
                case LinkGeneratingType.EmbeddedOneToOne:
                case LinkGeneratingType.EmbeddedOneToMany:
                case LinkGeneratingType.Lateral:
                    return LinkStorageType.Embedded;
                case LinkGeneratingType.GullySewer:
                    return LinkStorageType.GullySewer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(linkGeneratingType), linkGeneratingType, null);
            }
        }
    }
}