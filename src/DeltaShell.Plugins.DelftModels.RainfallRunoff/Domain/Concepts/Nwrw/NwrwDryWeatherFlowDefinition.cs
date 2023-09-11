using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing dry weather flow definitions from verloop.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    [Entity]
    public class NwrwDryWeatherFlowDefinition : ANwrwFeature
    {
        private static double[] defaultHourlyPercentageDailyVolume = new double[24];
        public NwrwDryWeatherFlowDefinition(ILogHandler logHandler):base(logHandler)
        {
            Name = "DefinitionName";
            Array.Copy(defaultHourlyPercentageDailyVolume, HourlyPercentageDailyVolume, HourlyPercentageDailyVolume.Length);
        }
        
        
        /// <summary>
        /// Name of the default dry weather flow definition to be used
        /// </summary>
        public static string DefaultDwaId { get; private set; }
        
        /// <summary>
        /// The dry weather flow distribution type.
        /// </summary>
        public DryweatherFlowDistributionType DistributionType { get; set; } 
        public int DayNumber { get; set; } // VER_DAG

        /// <summary>
        /// The constant daily volume of water use per capita [L/day].
        /// </summary>
        public double DailyVolumeConstant { get; set; }
        
        /// <summary>
        /// The variable daily volume of water use per capita [L/day].
        /// </summary>
        public double DailyVolumeVariable { get; set; }
        
        /// <summary>
        /// The hourly water use per capita expressed in percentages of <see cref="DailyVolumeVariable"/>.
        /// The sum of the 24 percentages should be 100. 
        /// </summary>
        public double[] HourlyPercentageDailyVolume { get; set; } = new double[24];

        public string Remark { get; set; } // ALG_TOE
        
        public override void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper)
        {
            if (rrModel == null)
            {
                logHandler?.ReportError("Cannot add NWRW catchment model data if model is not provided");
                return;
            }

            if (NotSupportedByKernel()) return;
            
            IEventedList<NwrwDryWeatherFlowDefinition> definitions = rrModel.NwrwDryWeatherFlowDefinitions;
            
            if (string.Equals(Name, DefaultDwaId, StringComparison.InvariantCultureIgnoreCase))
            {
                definitions.RemoveAllWhere(definition => string.Equals(definition.Name, DefaultDwaId, StringComparison.InvariantCultureIgnoreCase));
            } 
            else if (definitions.Any(definition => string.Equals(definition.Name,this.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            
            ConvertGwswUnitsToKernelUnits();
            definitions.Add(this);
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
                logHandler?.ReportWarning($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported.");
                return true;
            }

            if (DistributionType == DryweatherFlowDistributionType.Constant && DayNumber != default(int))
            {
                logHandler?.ReportWarning($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported in combination with a value of '{DayNumber}' for VER_DAG.");
                return true;
            }

            return false;
        }

        private static IEnumerable<NwrwDryWeatherFlowDefinition> ExtractNwrwDryWeatherFlowDefinitionDefaults(string content, ILogHandler logHandler)
        {
            Ensure.NotNullOrWhiteSpace(content, nameof(content));
            Ensure.NotNull(logHandler, nameof(logHandler));

            IDictionary<string, SobekRRDryWeatherFlow> defaultDryWeatherFlowDefinitions = new SobekRRDryWeatherFlowReader().Parse(content).ToDictionaryWithErrorDetails(content, item => item.Id, item => item);
            NwrwDryWeatherFlowDefinitionBuilder nwrwDryWeatherFlowDefinitionBuilder = new NwrwDryWeatherFlowDefinitionBuilder();
            foreach (KeyValuePair<string, SobekRRDryWeatherFlow> defaultDryWeatherFlowDefinition in defaultDryWeatherFlowDefinitions)
            {
                NwrwDryWeatherFlowDefinition nwrwDryWeatherFlowDefinition = nwrwDryWeatherFlowDefinitionBuilder.Build(defaultDryWeatherFlowDefinition.Value, logHandler);
                yield return nwrwDryWeatherFlowDefinition;
                
                if (string.IsNullOrWhiteSpace(DefaultDwaId))
                {
                    DefaultDwaId = defaultDryWeatherFlowDefinition.Key;
                    defaultHourlyPercentageDailyVolume = nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume;
                }
            }
        }

        public static IEventedList<NwrwDryWeatherFlowDefinition> CreateDefaultDwaDefinitions()
        {
            ILogHandler logHandler = new LogHandler("Importing Default RR NWRW Dry Weather Flow Definition data");
            var defaultsFileContent = RainfallRunoffModelFixedFiles.ReadFixedFileFromResource("PLUVIUS.DWA");
            var nwrwDryWeatherFlowDefinitions = new EventedList<NwrwDryWeatherFlowDefinition>(ExtractNwrwDryWeatherFlowDefinitionDefaults(defaultsFileContent, logHandler));
            logHandler.LogReport();
            return nwrwDryWeatherFlowDefinitions;
        }
    }
}
