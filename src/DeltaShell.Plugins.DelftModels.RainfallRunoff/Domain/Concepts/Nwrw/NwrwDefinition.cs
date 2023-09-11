using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing nwrw definitions from nwrw.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    [Entity]
    public class NwrwDefinition : ANwrwFeature
    {
        public NwrwDefinition(ILogHandler logHandler): base(logHandler)
        {
        }
        public NwrwSurfaceType SurfaceType { get; set; } // AFV_IDE
        public double SurfaceStorage { get; set; } // AFV_BRG
        public double InfiltrationCapacityMax { get; set; } // AFV_IFX
        public double InfiltrationCapacityMin { get; set; } // AFV_IFN
        public double InfiltrationCapacityReduction { get; set; } // AFV_IFH
        public double InfiltrationCapacityRecovery { get; set; } // AFV_AFS
        public double RunoffDelay { get; set; } // AFV_AFS
        public double RunoffLength { get; set; } // AFV_LEN
        public double RunoffSlope { get; set; } // AFV_HEL
        public double TerrainRoughness { get; set; } // AFV_RUW
        public string Remark { get; set; } // ALG_TOE
        

        public override void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper)
        {
            if (rrModel == null)
            {
                logHandler?.ReportWarning($"Could not add {nameof(NwrwDefinition)} to {nameof(RainfallRunoffModel)}.");
                return;
            }

            lock (rrModel.NwrwDefinitions)
            {
                var nwrwDefinitionIndex = rrModel.NwrwDefinitions.ToList()
                    .FindIndex(nd => nd.SurfaceType.Equals(this.SurfaceType));
                if (nwrwDefinitionIndex == -1)
                {
                    logHandler?.ReportWarning($"Could not add {nameof(NwrwDefinition)} to {nameof(RainfallRunoffModel)}.");
                    return;
                }

                rrModel.NwrwDefinitions[nwrwDefinitionIndex] = this;
            }

                
        }
        
        public static IEventedList<NwrwDefinition> CreateDefaultNwrwDefinitions()
        {
            var nwrwDefinitions = new EventedList<NwrwDefinition>();
            ILogHandler logHandler = new LogHandler("Creating default definitions");

            foreach (var surfaceType in NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder)
            {
                nwrwDefinitions.Add(
                    new NwrwDefinition(logHandler)
                    {
                        SurfaceType = surfaceType,
                        Name = NwrwSurfaceTypeHelper.SurfaceTypeDictionary[surfaceType],
                    });
            }
            LoadNwrwDefaults(nwrwDefinitions);
            logHandler.LogReport();
            return nwrwDefinitions;
        }
        
        private static void LoadNwrwDefaults(EventedList<NwrwDefinition> nwrwDefinitions)
        {
            ILogHandler logHandler = new LogHandler("importing Default RR NWRW data");
            var line = RainfallRunoffModelFixedFiles.ReadFixedFileFromResource("PLUVIUS.ALG");
            ISobekRRNwrwSettings[] sobekRRNwrwSettings = new SobekRRNwrwSettingsReader().Parse(line).ToArray();
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwDefinitions, logHandler);
            logHandler.LogReport();
        }
    }
}