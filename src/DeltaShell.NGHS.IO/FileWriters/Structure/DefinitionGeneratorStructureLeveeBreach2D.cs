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
        public DefinitionGeneratorStructureLeveeBreach2D(DateTime? referenceDateTime) : base(referenceDateTime)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.LeveeBreach);

            var leveeBreach = hydroObject as ILeveeBreach;
            if (leveeBreach == null) return IniCategory;

            AddPropertyToIniCategory(leveeBreach.BreachLocationX, StructureRegion.BreachLocationX);
            AddPropertyToIniCategory(leveeBreach.BreachLocationY, StructureRegion.BreachLocationY);

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
                AddPropertyToIniCategory(verheijLeveeBreachSettings.InitialCrestLevel, StructureRegion.InitialCrestLevel);
                AddPropertyToIniCategory(verheijLeveeBreachSettings.MinimumCrestLevel, StructureRegion.MinimumCrestLevel);
                AddPropertyToIniCategory(verheijLeveeBreachSettings.InitialBreachWidth, StructureRegion.InitalBreachWidth);

                var value = DataTypeValueParser.ToString(verheijLeveeBreachSettings.PeriodToReachZmin, typeof(TimeSpan));
                IniCategory.AddProperty(StructureRegion.TimeToReachMinimumCrestLevel.Key, value);

                AddPropertyToIniCategory(verheijLeveeBreachSettings.Factor1Alfa, StructureRegion.Factor1);
                AddPropertyToIniCategory(verheijLeveeBreachSettings.Factor2Beta, StructureRegion.Factor2);
                AddPropertyToIniCategory(verheijLeveeBreachSettings.CriticalFlowVelocity, StructureRegion.CriticalFlowVelocity);
            }

            var userDefinedBreachSettings = leveeBreachSettings as UserDefinedBreachSettings;
            if (userDefinedBreachSettings != null)
            {
                var timeSeriesFileName = $"{leveeBreach.Name}.tim";
                IniCategory.AddProperty(StructureRegion.TimeFileName.Key, timeSeriesFileName);
            }

            return IniCategory;
        }
    }
}
