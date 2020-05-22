using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Annotations;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing dry weather flow definitions from verloop.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    [Entity]
    public class NwrwDryWeatherFlowDefinition : Unique<long>, INwrwFeature
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDryWeatherFlowDefinition));

        public string Name { get; set; } //VER_IDE
        public DryweatherFlowDistributionType DistributionType { get; set; } // VER_TYPE
        public int DayNumber { get; set; } // VER_DAG
        public double DailyVolumeVariable { get; set; } // VER_VOL
        public double DailyVolumeConstant { get; set; } // VER_VOL
        public double[] HourlyPercentageDailyVolume { get; set; } = new double[24]; // U00_DAG -- U23_DAG
        public string Remark { get; set; } // ALG_TOE

        public IGeometry Geometry { get; set; }


        public void AddNwrwCatchmentModelDataToModel(IHydroModel model, NwrwImporterHelper helper)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null) throw new ArgumentException();
            if (rrModel.NwrwDryWeatherFlowDefinitions.Any(definition => definition.Name.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase))) return;
            if (NotSupportedByKernel()) return;

            ConvertGwswUnitsToKernelUnits();
            rrModel?.NwrwDryWeatherFlowDefinitions.Add(this);
        }

        public void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            //Nothing to initialize
        }

        private void ConvertGwswUnitsToKernelUnits()
        {
            // In Gwsw the unit for DailyVolumeVariable is given in m³/day.
            // RR expects DailyVolumeVariable to be dm³/day.
            DailyVolumeVariable *= 1000;

            // In Gwsw the unit for DailyVolumeConstant is given in m³/day.
            // RR expects DailyVolumeConstant to be dm³/h.
            DailyVolumeConstant = DailyVolumeConstant * 1000 / 24;
        }

        private bool NotSupportedByKernel()
        {
            // The kernel only supports DWF definitions of type 'DAG' or
            // of type 'CST' where VER_DAG is empty.
            if (DistributionType == DryweatherFlowDistributionType.Variable)
            {
                throw new NotSupportedException($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported.");
            }

            if (DistributionType == DryweatherFlowDistributionType.Constant && DayNumber != default(int))
            {
                throw new NotSupportedException($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported in combination with a value of '{DayNumber}' for VER_DAG.");
            }

            return false;
        }

        public static NwrwDryWeatherFlowDefinition CreateDefaultDwaDefinition()
        {
            return new NwrwDryWeatherFlowDefinition
            {
                Name = "Default_DWA",
                DistributionType = DryweatherFlowDistributionType.Constant,
                DailyVolumeConstant = 12,
                DailyVolumeVariable = 120,
                HourlyPercentageDailyVolume = new []{1.5, 1.5, 1.5, 1.5, 1.5, 3.0, 4.0, 5.0, 6.0, 6.5, 7.5, 8.5, 7.5, 6.5, 6.0, 5.0, 5.0, 5.0, 4.0, 3.5, 3.0, 2.5, 2.0, 2.0 }
            };
        }
    }
}
