using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureLeveeBreach2D : DefinitionGeneratorTimeSeriesStructure2D
    {
        private DateTime? ReferenceDateTime { get; }

        public DefinitionGeneratorStructureLeveeBreach2D(DateTime? referenceDateTime)
        {
            ReferenceDateTime = referenceDateTime;
        }

        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            var leveeBreach = hydroObject as ILeveeBreach;
            if (leveeBreach == null) return IniSection;

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.LeveeBreach);

            
            AddPropertyToIniSection(leveeBreach.BreachLocationX, StructureRegion.BreachLocationX);
            AddPropertyToIniSection(leveeBreach.BreachLocationY, StructureRegion.BreachLocationY);

            var leveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings();

            if (ReferenceDateTime != null)
            {
                var secondsSinceRefDate = (int) (leveeBreachSettings.StartTimeBreachGrowth - (DateTime) ReferenceDateTime).TotalSeconds;
                IniSection.AddProperty(StructureRegion.StartTimeBreachGrowth.Key, secondsSinceRefDate, StructureRegion.StartTimeBreachGrowth.Description);
            }

            if (leveeBreach.WaterLevelFlowLocationsActive)
            {
                AddPropertyToIniSection(leveeBreach.WaterLevelUpstreamLocationX,
                    StructureRegion.WaterLevelUpstreamLocationX);
                AddPropertyToIniSection(leveeBreach.WaterLevelUpstreamLocationY,
                    StructureRegion.WaterLevelUpstreamLocationY);
                AddPropertyToIniSection(leveeBreach.WaterLevelDownstreamLocationX,
                    StructureRegion.WaterLevelDownstreamLocationX);
                AddPropertyToIniSection(leveeBreach.WaterLevelDownstreamLocationY,
                    StructureRegion.WaterLevelDownstreamLocationY);
            }

            IniSection.AddPropertyWithOptionalComment(StructureRegion.BreachGrowthActivated.Key, leveeBreachSettings.BreachGrowthActive ? "1" : "0", StructureRegion.BreachGrowthActivated.Description);

            if (!leveeBreachSettings.BreachGrowthActive) return IniSection;

            IniSection.AddProperty(StructureRegion.Algorithm.Key, (int)leveeBreach.LeveeBreachFormula);

            var verheijLeveeBreachSettings = leveeBreachSettings as VerheijVdKnaap2002BreachSettings;
            if (verheijLeveeBreachSettings != null)
            {
                AddPropertyToIniSection(verheijLeveeBreachSettings.InitialCrestLevel, StructureRegion.InitialCrestLevel);
                AddPropertyToIniSection(verheijLeveeBreachSettings.MinimumCrestLevel, StructureRegion.MinimumCrestLevel);
                AddPropertyToIniSection(verheijLeveeBreachSettings.InitialBreachWidth, StructureRegion.InitalBreachWidth);

                var value = DataTypeValueParser.ToString(verheijLeveeBreachSettings.PeriodToReachZmin, typeof(TimeSpan));
                IniSection.AddPropertyWithOptionalComment(StructureRegion.TimeToReachMinimumCrestLevel.Key, value);

                AddPropertyToIniSection(verheijLeveeBreachSettings.Factor1Alfa, StructureRegion.Factor1);
                AddPropertyToIniSection(verheijLeveeBreachSettings.Factor2Beta, StructureRegion.Factor2);
                AddPropertyToIniSection(verheijLeveeBreachSettings.CriticalFlowVelocity, StructureRegion.CriticalFlowVelocity);
            }

            var userDefinedBreachSettings = leveeBreachSettings as UserDefinedBreachSettings;
            if (userDefinedBreachSettings != null)
            {
                var timeSeriesFileName = $"{leveeBreach.Name}{FileSuffices.TimFile}";
                IniSection.AddPropertyWithOptionalComment(StructureRegion.TimeFileName.Key, timeSeriesFileName);
            }

            return IniSection;
        }
    }
}
