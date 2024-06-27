using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing nwrw definitions from nwrw.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    [Entity]
    public class NwrwDefinition : INwrwFeature
    {
        public string Name { get; set; } // AFV_IDE
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
        public IGeometry Geometry { get; set; }

        public void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper, ILogHandler logHandler)
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

        public void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            //nothing to initialize
        }

        public static IEventedList<NwrwDefinition> CreateDefaultNwrwDefinitions()
        {
            var nwrwDefinitions = new EventedList<NwrwDefinition>();
            foreach (var surfaceType in NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder)
            {
                nwrwDefinitions.Add(
                    new NwrwDefinition
                    {
                        SurfaceType = surfaceType,
                        Name = NwrwSurfaceTypeHelper.SurfaceTypeDictionary[surfaceType],
                    });
            }
            LoadNwrwDefaults(nwrwDefinitions);
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