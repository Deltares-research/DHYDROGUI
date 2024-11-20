using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public static class UnpavedEnums
    {
        #region CropType enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum CropType
        {
            Grass,
            Corn,
            Potatoes,
            Sugarbeet,
            Grain,
            Miscellaneous,
            [Description("Non-arable land")] NonArableLand,
            [Description("Greenhouse Area")] GreenhouseArea,
            Orchard,
            [Description("Bulbous Plants")] BulbousPlants,
            [Description("Foliage Forest")] FoliageForest,
            [Description("Pine Forest")] PineForest,
            Nature,
            Fallow,
            Vegetables,
            Flowers
        };

        #endregion

        #region DrainageComputationOption enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum DrainageComputationOption
        {
            [Description("De Zeeuw / Hellinga")] DeZeeuwHellinga,
            [Description("Krayenhoff / Van de Leur")] KrayenhoffVdLeur,
            Ernst
        }

        #endregion

        #region GroundWaterSourceType enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum GroundWaterSourceType
        {
            Constant,
            Series,
            [Description("From linked node")] FromLinkedNode
        }

        #endregion

        #region SeepageSourceType enum

        public enum SeepageSourceType
        {
            Constant,
            Series,
            H0Series,
            //future: MODFLOW
        }

        #endregion

        #region SoilType enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum SoilType
        {
            [Description("sand (maximum) [μ = 0.117 per m]")] sand_maximum = 1,
            [Description("peat (maximum) [μ = 0.078 per m]")] peat_maximum = 2,
            [Description("clay (maximum) [μ = 0.049 per m]")] clay_maximum = 3,
            [Description("peat (average) [μ = 0.067 per m]")] peat_average = 4,
            [Description("sand (average) [μ = 0.088 per m]")] sand_average = 5,
            [Description("silt (maximum) [μ = 0.051 per m]")] silt_maximum = 6,
            [Description("peat (minimum) [μ = 0.051 per m]")] peat_minimum = 7,
            [Description("clay (average) [μ = 0.036 per m]")] clay_average = 8,
            [Description("sand (minimum) [μ = 0.060 per m]")] sand_minimum = 9,
            [Description("silt (average) [μ = 0.038 per m]")] silt_average = 10,
            [Description("clay (minimum) [μ = 0.026 per m]")] clay_minimum = 11,
            [Description("silt (minimum) [μ = 0.021 per m]")] silt_minimum = 12,
        }

        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public enum SoilTypeCapsim
        {
           [Description("Veengrond met veraarde bovengrond")] soiltype_capsim_1 = 101,
           [Description("Veengrond met veraarde bovengrond, zand")] soiltype_capsim_2 = 102,
           [Description("Veengrond met kleidek")] soiltype_capsim_3 = 103,
           [Description("Veengrond met kleidek op zand")] soiltype_capsim_4 = 104,
           [Description("Veengrond met zanddek op zand")] soiltype_capsim_5 = 105,
           [Description("Veengrond op ongerijpte klei")] soiltype_capsim_6 = 106,
           [Description("Stuifzand")] soiltype_capsim_7 = 107,
           [Description("Podzol (Leemarm, fijn zand)")] soiltype_capsim_8 = 108,
           [Description("Podzol (zwak lemig, fijn zand)")] soiltype_capsim_9 = 109,
           [Description("Podzol (zwak lemig, fijn zand op grof zand)")] soiltype_capsim_10 = 110,
           [Description("Podzol (lemig keileem)")] soiltype_capsim_11 = 111,
           [Description("Enkeerd (zwak lemig, fijn zand)")] soiltype_capsim_12 = 112,
           [Description("Beekeerd (lemig fijn zand)")] soiltype_capsim_13 = 113,
           [Description("Podzol (grof zand)")] soiltype_capsim_14 = 114,
           [Description("Zavel")] soiltype_capsim_15 = 115,
           [Description("Lichte klei")] soiltype_capsim_16 = 116,
           [Description("Zware klei")] soiltype_capsim_17 = 117,
           [Description("Klei op veen")] soiltype_capsim_18 = 118,
           [Description("Klei op zand")] soiltype_capsim_19 = 119,
           [Description("Klei op grof zand")] soiltype_capsim_20 = 120,
           [Description("Leem")] soiltype_capsim_21 = 121
        }

        #endregion
    }
}