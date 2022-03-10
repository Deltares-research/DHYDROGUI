namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class FlowInitialCondition
    {
        public enum FlowConditionType
        {
            WaterDepth = 0,
            WaterLevel = 1
        }

        public FlowInitialCondition()
        {
            Discharge = new InitialCondition();
            Level = new InitialCondition();
        }

        /// <summary>
        /// ID of the branch at which the initial/boundary condition is specified
        /// </summary>
        public string BranchID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }

        public bool IsGlobalDefinition { get; set; }
        public bool IsQBoundary { get; set; }
        public bool IsLevelBoundary { get; set; }

        /// <summary>
        /// ty
        /// 0 = water level
        /// 1 = water depth
        /// </summary>
        public FlowConditionType WaterLevelType { get; set; }

        public InitialCondition Discharge { get; private set; }
        public InitialCondition Level { get; private set; }
    }
}