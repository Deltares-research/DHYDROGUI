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
            conditionalActivateOutput(settings.OutputRRPaved, ElementSet.PavedElmSet); 
            conditionalActivateOutput(settings.OutputRRUnpaved, ElementSet.UnpavedElmSet); 
            conditionalActivateOutput(settings.OutputRRGreenhouse, ElementSet.GreenhouseElmSet); 
            conditionalActivateOutput(settings.OutputRROpenWater, ElementSet.OpenWaterElmSet); 
            conditionalActivateOutput(settings.OutputRRBoundary, ElementSet.BoundaryElmSet); 
            conditionalActivateOutput(settings.OutputRRWWTP, ElementSet.WWTPElmSet); 
            conditionalActivateOutput(settings.OutputRRSacramento, ElementSet.SacramentoElmSet); 
            conditionalActivateOutput(settings.OutputRRLinkFlows, ElementSet.LinkElmSet); 
            conditionalActivateOutput(settings.OutputRRBalance, ElementSet.BalanceModelElmSet); 
            conditionalActivateOutput(settings.OutputRRBalance, ElementSet.BalanceNodeElmSet); 
        }

        private void conditionalActivateOutput(bool add, ElementSet e)
        {
            if (add)
            {
                model.OutputSettings.SetAggregationOptionForElementSet(AggregationOptions.Current, e);
            }
        }

        private void readModelGeneralSettings() 
        {
            model.TimeStep = settings.TimestepSize;
            model.OutputTimeStep = new TimeSpan(0, 0,
                                                (int)settings.TimestepSize.TotalSeconds *
                                                settings.OutputTimestepMultiplier);
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