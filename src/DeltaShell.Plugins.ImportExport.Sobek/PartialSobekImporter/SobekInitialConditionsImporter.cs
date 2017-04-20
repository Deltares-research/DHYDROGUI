using System.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekInitialConditionsImporter: PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekInitialConditionsImporter));

        private string displayName = "Initial Conditions";
        public override string DisplayName
        {
            get { return displayName; }
        }

        protected override void PartialImport()
        {
            log.DebugFormat("Importing initial conditions ...");

            var waterFlowModel1D = GetModel<WaterFlowModel1D>();

            var initialPath = GetFilePath(SobekFileNames.SobekInitialConditionsFileName);
            if (!File.Exists(initialPath))
            {
                log.WarnFormat("Initial condition file [{0}] not found; skipping...", initialPath);
                return;
            }
            var initalFlowConditionsReader = new InitalFlowConditionsReader();
            var flowInitialConditions = initalFlowConditionsReader.Read(initialPath);

            var builder = new InitialConditionsBuilder(flowInitialConditions, HydroNetwork);
            //this is awkward...but putting it all in the constructor is as well
            builder.Build();

            SobekWaterFlowModel1DReaderHelper.CopyCoverageValuesAndDefault(builder.InitialDepth, waterFlowModel1D.InitialConditions);
            SobekWaterFlowModel1DReaderHelper.CopyCoverageValuesAndDefault(builder.InitialFlow, waterFlowModel1D.InitialFlow);

            // set global defaults 
            // if sobek 2.12 casesettings overrule initial.dat (DEFICN.2)
            waterFlowModel1D.DefaultInitialWaterLevel = builder.GlobalInitialWaterLevel;
            waterFlowModel1D.DefaultInitialDepth = builder.GlobalInitialDepth;

            if (SobekType == SobekType.Sobek212)
            {
                ReadInitialConditionsFromNetter(builder);
            }
        }

        private void ReadInitialConditionsFromNetter(InitialConditionsBuilder builder)
        {
            var path =GetFilePath(SobekFileNames.SobekCaseSettingFileName);
            var waterFlowModel1D = GetModel<WaterFlowModel1D>();

            if (!File.Exists(path))
            {
                log.WarnFormat("Import of case settings skipped, file {0} does not exist.", path);
                return;
            }
            //Simulation
            var sobekCaseSettings = SobekCaseSettingsReader.GetSobekCaseSettings(path);
            if (sobekCaseSettings == null)
            {
                return;
            }
            if (sobekCaseSettings.FromNetter)
            {
                waterFlowModel1D.DefaultInitialWaterLevel = builder.GlobalInitialWaterLevel;
                waterFlowModel1D.DefaultInitialDepth = builder.GlobalInitialDepth;
            }
            else
            {
                waterFlowModel1D.DefaultInitialWaterLevel = sobekCaseSettings.InitialLevelValue;
                waterFlowModel1D.DefaultInitialDepth = sobekCaseSettings.InitialDepthValue;
                
                if (sobekCaseSettings.InitialLevel)
                {
                    if (waterFlowModel1D.InitialConditions.Locations.Values.Count != 0)
                    {
                        //It turns out our data should be presented as Level data, and previously read Depth data
                        //should not be used (at all?).
                        //The problem is that when setting the InitialConditionsType (see below), the previously set
                        //Depth data is converted to Level data. We don't want to convert all Depth data, at least not 
                        //for empty branch. On empty branches the global default level should apply. What we probably 
                        //should be doing here is copy and convert per branch, with the specific rule 'no locations in 
                        //depth = no locations in level'. Since that normally doesn't apply, that would mean a bunch of 
                        //custom code..
                        log.Error("Initial Conditions Import: Simplified conversion from Initial Depth to Initial Level was performed.");
                    }

                    waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
                    waterFlowModel1D.InitialConditions.Clear(); //clear any 'depth-based' level data
                    return;
                }

                if (sobekCaseSettings.InitialDepth)
                {
                    waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
                    return;
                }

                if (!sobekCaseSettings.InitialDepth && !sobekCaseSettings.InitialLevel)
                {
                    // completely dry system
                    sobekCaseSettings.InitialDepth = true;
                    waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
                    waterFlowModel1D.DefaultInitialDepth = 0.0;
                }
            }

        }
    }
}
