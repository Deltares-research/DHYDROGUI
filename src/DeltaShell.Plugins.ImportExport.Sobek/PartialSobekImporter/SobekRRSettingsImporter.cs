using System;
using System.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    /// <summary>
    /// Sobek RR settings importer
    /// </summary>
    public class SobekRRSettingsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRSettingsImporter));

        private const string displayName = "Rainfall Runoff settings";
        private RainfallRunoffModel model;
        private string path;
        private SobekRRIniSettings settings;

        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            model = GetModel<RainfallRunoffModel>();
            path = GetFilePath(SobekFileNames.SobekRRIniFileName);
            log.DebugFormat("Importing RR Settings ...");

            if (!File.Exists(path))
            {
                log.ErrorFormat("Could not find ini file {0}.", path);
                return;
            }

            settings = new SobekRRIniSettingsReader().GetSobekRRIniSettings(path);
            readModelGeneralSettings();
            readModelOutputSettings();
        }

        private void readModelOutputSettings()
        {
            ToggleOutputSetting(settings.OutputRRPaved, ElementSet.PavedElmSet); 
            ToggleOutputSetting(settings.OutputRRUnpaved, ElementSet.UnpavedElmSet); 
            ToggleOutputSetting(settings.OutputRRGreenhouse, ElementSet.GreenhouseElmSet); 
            ToggleOutputSetting(settings.OutputRROpenWater, ElementSet.OpenWaterElmSet); 
            ToggleOutputSetting(settings.OutputRRBoundary, ElementSet.BoundaryElmSet); 
            ToggleOutputSetting(settings.OutputRRNWRW, ElementSet.NWRWElmSet); 
            ToggleOutputSetting(settings.OutputRRWWTP, ElementSet.WWTPElmSet); 
            ToggleOutputSetting(settings.OutputRRSacramento, ElementSet.SacramentoElmSet); 
            ToggleOutputSetting(settings.OutputRRLinkFlows, ElementSet.LinkElmSet); 
            ToggleOutputSetting(settings.OutputRRBalance, ElementSet.BalanceModelElmSet); 
            ToggleOutputSetting(settings.OutputRRBalance, ElementSet.BalanceNodeElmSet);

            SetAggregationOptions();
        }

        private void SetAggregationOptions()
        {
            switch (settings.AggregationOptions)
            {
                case 1:
                    model.OutputSettings.AggregationOption = AggregationOptions.Current;
                    break;
                case 2:
                    model.OutputSettings.AggregationOption = AggregationOptions.Average;
                    break;
                case 3:
                    model.OutputSettings.AggregationOption = AggregationOptions.Maximum;
                    break;
                default:
                    model.OutputSettings.AggregationOption = AggregationOptions.None;
                    break;
            }
        }

        private void ToggleOutputSetting(bool add, ElementSet e)
        {
            model.OutputSettings.ToggleEngineParametersForElementSet(e, add);
        }

        private void readModelGeneralSettings() 
        {
            model.TimeStep = settings.TimestepSize;
            model.OutputTimeStep = new TimeSpan(0, 0,
                (int)Math.Round(settings.TimestepSize.TotalSeconds * settings.OutputTimestepMultiplier, 0, MidpointRounding.AwayFromZero));
            if (settings.StartTime > DateTime.MinValue)
                model.StartTime = settings.StartTime;
            if (settings.EndTime > DateTime.MinValue)
                model.StopTime = settings.EndTime;

            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            model.CapSim = settings.UnsaturatedZone != 0;

            if (settings.CapsimPerCropAreaIsDefined)
            {
                if (Enum.IsDefined(typeof (RainfallRunoffEnums.CapsimCropAreaOptions), settings.CapsimPerCropArea))
                {
                    model.CapSimCropAreaOption = (RainfallRunoffEnums.CapsimCropAreaOptions) settings.CapsimPerCropArea;
                }
                else
                {
                    log.ErrorFormat("CapSim crop area option {0} is not known.", settings.CapsimPerCropArea);
                }
            }

            if (Enum.IsDefined(typeof(RainfallRunoffEnums.CapsimInitOptions), settings.InitCapsimOption))
            {
                model.CapSimInitOption = (RainfallRunoffEnums.CapsimInitOptions) settings.InitCapsimOption;
            }
            else
            {
                log.ErrorFormat("CapSim init option {0} is not known.", settings.CapsimPerCropArea);
            }
        } 
    }
}