using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RRInitialConditionsWrapper : INameable
    {
        #region InitialConditionsType enum

        public enum InitialConditionsType
        {
            Unpaved,
            Paved,
            Greenhouse,
            OpenWater
        }

        #endregion

        public RainfallRunoffModel Model { get; set; }
        public InitialConditionsType Type { get; set; }
        public string Name { get; set; }
    }
}