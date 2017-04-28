using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeltaShell.Plugins.Fews
{
    public partial class BranchesComplexType
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.wldelft.nl/fews " +
                                          "http://fews.wldelft.nl/schemas/version1.0/branches.xsd";

        public enum geoDatumEnumStringType
        {

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("WGS 1984")]
            WGS1984 = 4326,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("WGS 1984 Radians")]
            WGS1984Radians = 4327,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Ordnance Survey Great Britain 1936")]
            OrdnanceSurveyGreatBritain1936 = 27700,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("TWD 1967")]
            TWD1967 = 3821,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("TWD 1997")]
            TWD1997 = 3824,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Gauss Krueger Meridian2")]
            GaussKruegerMeridian2,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Gauss Krueger Meridian3")]
            GaussKruegerMeridian3,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Gauss Krueger Austria M34")]
            GaussKruegerAustriaM34 = 31259,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Gauss Krueger Austria M31")]
            GaussKruegerAustriaM31 = 31258,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Rijks Driehoekstelsel")]
            RijksDriehoekstelsel = 28992,

            /// <remarks/>
            JRC,

            /// <remarks/>
            DWD,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("KNMI Radar")]
            KNMIRadar,

            /// <remarks/>
            CH1903 = 4149,

            /// <remarks/>
            PAK1,

            /// <remarks/>
            PAK2,

            /// <remarks/>
            SVY21 = 4757,

            /// <remarks/>
            GDA94 = 4283,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 48")]
            GDA94MGAZone48 = 28348,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 49")]
            GDA94MGAZone49 = 28349,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 50")]
            GDA94MGAZone50 = 28350,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 51")]
            GDA94MGAZone51 = 28351,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 52")]
            GDA94MGAZone52 = 28352,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 53")]
            GDA94MGAZone53 = 28353,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 54")]
            GDA94MGAZone54 = 28354,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 55")]
            GDA94MGAZone55 = 28355,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 56")]
            GDA94MGAZone56 = 28356,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 57")]
            GDA94MGAZone57 = 28357,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("GDA94 MGA Zone 58")]
            GDA94MGAZone58 = 28358,

            /// <remarks/>
            TM65 = 4299,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("ETRS 1989 UTM zone 29N")]
            ETRS1989UTMzone29N = 25829,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("ETRS 1989 LAEA")]
            ETRS1989LAEA = 3035,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Colombia West Zone")]
            ColombiaWestZone = 3115,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Colombia West West Zone")]
            ColombiaWestWestZone = 3114,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Colombia East Zone")]
            ColombiaEastZone = 3118,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Colombia East Central Zone")]
            ColombiaEastCentralZone = 3117,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Colombia Bogota Zone")]
            ColombiaBogotaZone = 3116,

            /// <remarks/>
            VN2000 = 4756,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("VN2000 UTM Zone 48N")]
            VN2000UTMZone48N = 3405,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("VN2000 UTM Zone 49N")]
            VN2000UTMZone49N = 3406,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 46.2")]
            IndonesiaTM3zone462 = 23830,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 47.1")]
            IndonesiaTM3zone471 = 23831,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 47.2")]
            IndonesiaTM3zone472 = 23832,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 48.1")]
            IndonesiaTM3zone481 = 23833,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 48.2")]
            IndonesiaTM3zone482 = 23834,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 49.1")]
            IndonesiaTM3zone491 = 23835,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 49.2")]
            IndonesiaTM3zone492 = 23836,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 50.1")]
            IndonesiaTM3zone501 = 23837,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 50.2")]
            IndonesiaTM3zone502 = 23838,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 51.1")]
            IndonesiaTM3zone511 = 23839,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 51.2")]
            IndonesiaTM3zone512 = 23840,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 52.1")]
            IndonesiaTM3zone521 = 23841,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 52.2")]
            IndonesiaTM3zone522 = 23842,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 53.1")]
            IndonesiaTM3zone531 = 23843,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 53.2")]
            IndonesiaTM3zone532 = 23844,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Indonesia TM-3 zone 54.1")]
            IndonesiaTM3zone541 = 23845,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("New Zealand Transverse Mercator 2000")]
            NewZealandTransverseMercator2000 = 2193,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Belgian Lambert 72")]
            BelgianLambert72 = 31370,
        }
        public static string GetXmlEnumAttributeValueFromEnum<TEnum>(TEnum value) where TEnum : struct, IConvertible
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum) return null;//or string.Empty, or throw exception

            var member = enumType.GetMember(value.ToString()).FirstOrDefault();
            if (member == null) return null;//or string.Empty, or throw exception

            var attribute = member.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
            if (attribute == null) return null;//or string.Empty, or throw exception
            return attribute.Name;
        }
    }
}
