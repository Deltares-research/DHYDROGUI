using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Csv.Importer;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public static class CrossSectionZWCsvImportExportSettings
    {
        public const string MainSectionName = RoughnessDataSet.MainSectionTypeName;
        public const string FloodPlain1SectionName = RoughnessDataSet.Floodplain1SectionTypeName;
        public const string FloodPlain2SectionName = RoughnessDataSet.Floodplain2SectionTypeName;

        public const string IdHeader = "id";
        public const string NameHeader = "Name";
        public const string RecordTypeHeader = "Data_type";
        public const string LevelHeader = "level";
        public const string TotalWidthHeader = "Total width";
        public const string FlowWidthHeader = "Flow width";
        public const string ProfileTypeHeader = "Profile_type";
        public const string BranchHeader = "branch";
        public const string ChainageHeader = "chainage";
        public const string MainWidthHeader = "width main channel";
        public const string FloodPlain1Header = "width floodplain 1";
        public const string FloodPlain2Header = "width floodplain 2";
        public const string SedimentWidthHeader = "width sediment transport";
        public const string UseSummerdikeHeader = "Use Summerdike";
        public const string CrestLevelSummerdikeHeader = "Crest level summerdike";
        public const string FloodPlainLevelSummerdikeHeader = "Floodplain baselevel behind summerdike";
        public const string FlowAreaSummerdikeHeader = "Flow area behind summerdike";
        public const string TotalAreaSummerdikeHeader = "Total area behind summerdike";
        public const string UseGroundlayerHeader = "Use groundlayer";
        public const string GroundLayerDepthHeader = "Ground layer depth";

        public static readonly IDictionary<CsvRequiredField, CsvColumnInfo> ColumnsMapping =
            new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField(IdHeader, typeof (string)), 
                        new CsvColumnInfo(0, null)
                    },
                    {
                        new CsvRequiredField(NameHeader, typeof (string)), 
                        new CsvColumnInfo(1, null)
                    },
                    {
                        new CsvRequiredField(RecordTypeHeader, typeof (string)), 
                        new CsvColumnInfo(2, null)
                    },
                    {
                        new CsvRequiredField(LevelHeader, typeof (double)), 
                        new CsvColumnInfo(3, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(TotalWidthHeader, typeof (double)), 
                        new CsvColumnInfo(4, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(FlowWidthHeader, typeof (double)), 
                        new CsvColumnInfo(5, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(ProfileTypeHeader, typeof (string)), 
                        new CsvColumnInfo(6, null)
                    },
                    {
                        new CsvRequiredField(BranchHeader, typeof (string)), 
                        new CsvColumnInfo(7, null)
                    },
                    {
                        new CsvRequiredField(ChainageHeader, typeof (double)), 
                        new CsvColumnInfo(8, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(MainWidthHeader, typeof (double)),
                        new CsvColumnInfo(9, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(FloodPlain1Header, typeof (double)),
                        new CsvColumnInfo(10, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(FloodPlain2Header, typeof (double)),
                        new CsvColumnInfo(11, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(SedimentWidthHeader, typeof (double)),
                        new CsvColumnInfo(12, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(UseSummerdikeHeader, typeof (int)), //parses to bool
                        new CsvColumnInfo(13, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(CrestLevelSummerdikeHeader, typeof (double)),
                        new CsvColumnInfo(14, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(FloodPlainLevelSummerdikeHeader, typeof (double)),
                        new CsvColumnInfo(15, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(FlowAreaSummerdikeHeader, typeof (double)),
                        new CsvColumnInfo(16, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(TotalAreaSummerdikeHeader, typeof (double)),
                        new CsvColumnInfo(17, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(UseGroundlayerHeader, typeof (int)), //parses to bool
                        new CsvColumnInfo(18, new NumberFormatInfo())
                    },
                    {
                        new CsvRequiredField(GroundLayerDepthHeader, typeof (double)),
                        new CsvColumnInfo(19, new NumberFormatInfo())
                    }
                };

        public static readonly Dictionary<bool, string> GeoMetaMapping = new Dictionary<bool, string>
            {
                {true, "meta"},
                {false, "geom"}
            }; 

        public static bool IsMeta(DataRow row)
        {
            var entry = row[RecordTypeHeader] as string;
            return GeoMetaMapping.ContainsValue(entry) && GeoMetaMapping.First(p => p.Value == entry).Key;
        }

        public static bool IsGeom(DataRow row)
        {
            var entry = row[RecordTypeHeader] as string;
            return GeoMetaMapping.ContainsValue(entry) && !GeoMetaMapping.First(p => p.Value == entry).Key;
        }

        public static string ToGeoMetaString(bool isMeta)
        {
            return GeoMetaMapping[isMeta];
        }

        public static bool ParseToBool(object o)
        {
            if (o is int)
            {
                return (int) o != 0;
            }
            return false;
        }
    }
}
