using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureLeveeBreach2D : DefinitionGeneratorStructure2D
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.LeveeBreach);

            var leveeBreach = (LeveeBreach) hydroObject;
            IniCategory.AddProperty(StructureRegion.BreachLocationX.Key, leveeBreach.BreachLocationX, StructureRegion.BreachLocationX.Description);
            IniCategory.AddProperty(StructureRegion.BreachLocationY.Key, leveeBreach.BreachLocationY, StructureRegion.BreachLocationY.Description);

            var leveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings();

            if (ReferenceDateTime != null)
            {
                var secondsSinceRefDate = (int) (leveeBreachSettings.StartTimeBreachGrowth - (DateTime) ReferenceDateTime).TotalSeconds;
                IniCategory.AddProperty(StructureRegion.StartTimeBreachGrowth.Key, secondsSinceRefDate, StructureRegion.StartTimeBreachGrowth.Description);
            }

            IniCategory.AddProperty(StructureRegion.BreachGrowthActivated.Key, leveeBreachSettings.BreachGrowthActive ? "1" : "0", StructureRegion.BreachGrowthActivated.Description);

            if (!leveeBreachSettings.BreachGrowthActive) return IniCategory;

            IniCategory.AddProperty(StructureRegion.Algorithm.Key, (int)leveeBreach.LeveeBreachFormula);

            var verheijLeveeBreachSettings = leveeBreachSettings as VerheijVdKnaap2002BreachSettings;
            if (verheijLeveeBreachSettings != null)
            {
                IniCategory.AddProperty(StructureRegion.InitialCrestLevel.Key, verheijLeveeBreachSettings.InitialCrestLevel);
                IniCategory.AddProperty(StructureRegion.MinimumCrestLevel.Key, verheijLeveeBreachSettings.MinimumCrestLevel);
                IniCategory.AddProperty(StructureRegion.InitalBreachWidth.Key, verheijLeveeBreachSettings.InitialBreachWidth);

                var value = DataTypeValueParser.ToString(verheijLeveeBreachSettings.PeriodToReachZmin, typeof(TimeSpan));
                IniCategory.AddProperty(StructureRegion.TimeToReachMinimumCrestLevel.Key, value);

                IniCategory.AddProperty(StructureRegion.Factor1.Key, verheijLeveeBreachSettings.Factor1Alfa);
                IniCategory.AddProperty(StructureRegion.Factor2.Key, verheijLeveeBreachSettings.Factor2Beta);
                IniCategory.AddProperty(StructureRegion.CriticalFlowVelocity.Key, verheijLeveeBreachSettings.CriticalFlowVelocity);
            }

            var userDefinedBreachSettings = leveeBreachSettings as UserDefinedBreachSettings;
            if (userDefinedBreachSettings != null)
            {
                var timeSeriesFileName = $"{leveeBreach.Name}_{KnownStructureProperties.TimeFilePath}.tim";
                IniCategory.AddProperty(StructureRegion.TimeFilePath.Key, timeSeriesFileName);
            }

            return IniCategory;
        }

        public DefinitionGeneratorStructureLeveeBreach2D(DateTime? referenceDateTime) : base(referenceDateTime)
        {
        }
    }
}
