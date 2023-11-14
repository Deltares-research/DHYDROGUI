using System;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Utils.Validation.NameValidation;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Structures
{
    public interface ILeveeBreach : IStructure2D, IHasNameValidation
    {
        double BreachLocationX { get; set; }
        double BreachLocationY { get; set; }
        IPoint BreachLocation { get; }
        LeveeBreachGrowthFormula LeveeBreachFormula { get; set; }
        LeveeBreachSettings GetActiveLeveeBreachSettings();
        void SetBaseLeveeBreachSettings(DateTime startTime, bool breachGrowthActive);
        double WaterLevelUpstreamLocationX { get; set; }
        double WaterLevelUpstreamLocationY { get; set; }
        IPoint WaterLevelUpstreamLocation { get; }
        double WaterLevelDownstreamLocationX { get; set; }
        double WaterLevelDownstreamLocationY { get; set; }
        IPoint WaterLevelDownstreamLocation { get; }

        bool WaterLevelFlowLocationsActive { get; set; }
    }
}