using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public static class RainfallRunoffUnitConverter
    {
        public static double ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit unitFrom,
                                             RainfallRunoffEnums.RainfallCapacityUnit unitTo, double value)
        {
            if (unitFrom == unitTo)
            {
                return value;
            }
            if (unitFrom == RainfallRunoffEnums.RainfallCapacityUnit.mm_hr) //to = mm_day
            {
                return value*24.0;
            }
            return value/24.0;
        }

        public static double ConvertArea(RainfallRunoffEnums.AreaUnit unitFrom, RainfallRunoffEnums.AreaUnit unitTo,
                                         double value)
        {
            if (unitFrom == unitTo)
            {
                return value;
            }
            if (unitFrom == RainfallRunoffEnums.AreaUnit.m2)
            {
                switch (unitTo)
                {
                    case RainfallRunoffEnums.AreaUnit.ha:
                        return value/10000;
                    case RainfallRunoffEnums.AreaUnit.km2:
                        return value/1000000;
                }
            }
            if (unitFrom == RainfallRunoffEnums.AreaUnit.ha)
            {
                switch (unitTo)
                {
                    case RainfallRunoffEnums.AreaUnit.m2:
                        return value*10000;
                    case RainfallRunoffEnums.AreaUnit.km2:
                        return value/100;
                }
            }
            if (unitFrom == RainfallRunoffEnums.AreaUnit.km2)
            {
                switch (unitTo)
                {
                    case RainfallRunoffEnums.AreaUnit.m2:
                        return value*1000000;
                    case RainfallRunoffEnums.AreaUnit.ha:
                        return value*100;
                }
            }
            return value;
        }

        public static double ConvertStorage(RainfallRunoffEnums.StorageUnit unitFrom,
                                            RainfallRunoffEnums.StorageUnit unitTo, double value, double area, RainfallRunoffEnums.AreaUnit areaUnit= RainfallRunoffEnums.AreaUnit.m2)
        {
            if (unitFrom == unitTo)
            {
                return value;
            }

            double m2Area = ConvertArea(areaUnit, RainfallRunoffEnums.AreaUnit.m2, area);
            if (unitFrom == RainfallRunoffEnums.StorageUnit.mm) //to = m3
            {
                return value*m2Area/1000;
            }
            return value*1000/m2Area;
        }

        public static double ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit unitFrom,
                                                 PavedEnums.SewerPumpCapacityUnit unitTo, double value, double area, RainfallRunoffEnums.AreaUnit areaUnit = RainfallRunoffEnums.AreaUnit.m2)
        {
            if (unitFrom == unitTo)
            {
                return value;
            }
            
            double m2Area = ConvertArea(areaUnit, RainfallRunoffEnums.AreaUnit.m2, area);
            if (unitFrom == PavedEnums.SewerPumpCapacityUnit.m3_s)
            {
                switch (unitTo)
                {
                    case PavedEnums.SewerPumpCapacityUnit.m3_min:
                        return value*60;
                    case PavedEnums.SewerPumpCapacityUnit.m3_hr:
                        return value*3600;
                    case PavedEnums.SewerPumpCapacityUnit.mm_hr:
                        return value*3600000/m2Area;
                }
            }
            if (unitFrom == PavedEnums.SewerPumpCapacityUnit.m3_hr)
            {
                switch (unitTo)
                {
                    case PavedEnums.SewerPumpCapacityUnit.m3_min:
                        return value/60;
                    case PavedEnums.SewerPumpCapacityUnit.mm_hr:
                        return value*1000/m2Area;
                    case PavedEnums.SewerPumpCapacityUnit.m3_s:
                        return value/3600;
                }
            }
            if (unitFrom == PavedEnums.SewerPumpCapacityUnit.m3_min)
            {
                switch (unitTo)
                {
                    case PavedEnums.SewerPumpCapacityUnit.m3_hr:
                        return value*60;
                    case PavedEnums.SewerPumpCapacityUnit.mm_hr:
                        return value*60000/m2Area;
                    case PavedEnums.SewerPumpCapacityUnit.m3_s:
                        return value/60;
                }
            }
            if (unitFrom == PavedEnums.SewerPumpCapacityUnit.mm_hr)
            {
                switch (unitTo)
                {
                    case PavedEnums.SewerPumpCapacityUnit.m3_hr:
                        return value*m2Area/1000;
                    case PavedEnums.SewerPumpCapacityUnit.m3_min:
                        return value*m2Area/60000;
                    case PavedEnums.SewerPumpCapacityUnit.m3_s:
                        return value*m2Area/3600000;
                }
            }
            return value;
        }

        public static double ConvertWaterUse(PavedEnums.WaterUseUnit unitFrom, PavedEnums.WaterUseUnit unitTo,
                                             double value)
        {
            if (unitFrom == unitTo)
            {
                return value;
            }
            if (unitFrom == PavedEnums.WaterUseUnit.m3_s)
            {
                switch (unitTo)
                {
                    case PavedEnums.WaterUseUnit.l_hr:
                        return value*3600000; // value * 1000 * 3600
                    case PavedEnums.WaterUseUnit.l_day:
                        return value*86400000; // value * 1000 * 3600 * 24;
                }
            }
            if (unitFrom == PavedEnums.WaterUseUnit.l_hr)
            {
                switch (unitTo)
                {
                    case PavedEnums.WaterUseUnit.m3_s:
                        return value/3600000; // value / (1000 * 3600)
                    case PavedEnums.WaterUseUnit.l_day:
                        return value*24;
                }
            }
            if (unitFrom == PavedEnums.WaterUseUnit.l_day)
            {
                switch (unitTo)
                {
                    case PavedEnums.WaterUseUnit.m3_s:
                        return value/86400000; // value / (1000  * 24 * 3600)
                    case PavedEnums.WaterUseUnit.l_hr:
                        return value/24;
                }
            }
            return value;
        }
    }
}