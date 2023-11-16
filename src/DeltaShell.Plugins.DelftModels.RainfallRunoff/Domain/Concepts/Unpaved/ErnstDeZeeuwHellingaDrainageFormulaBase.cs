using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved
{
    [Entity(FireOnCollectionChange = false)]
    public abstract class ErnstDeZeeuwHellingaDrainageFormulaBase : Unique<long>, IDrainageFormula
    {
        private double levelOneTo;
        private double levelTwoTo;

        protected ErnstDeZeeuwHellingaDrainageFormulaBase()
        {
            LevelOneTo = 0.5;
        }

        public abstract bool IsErnst { get; }
        public double SurfaceRunoff { get; set; } // 1/day or day
        public double HorizontalInflow { get; set; } // 1/day or day
        public double InfiniteDrainageLevelRunoff { get; set; } // 1/day or day

        public bool LevelOneEnabled { get; set; }
        public bool LevelTwoEnabled { get; set; }
        public bool LevelThreeEnabled { get; set; }

        public double LevelOneTo
        {
            get { return levelOneTo; }
            set
            {
                levelOneTo = value;
                OnAfterLevelOneToSet(value);
            }
        }

        private void OnAfterLevelOneToSet(double value)
        {
            if (value >= levelTwoTo)
            {
                LevelTwoTo = value + 0.5;
            }
        }

        public double LevelTwoTo
        {
            get { return levelTwoTo; }
            set
            {
                levelTwoTo = value;
                OnAfterLevelTwoToSet(value);
            }
        }
        
        private void OnAfterLevelTwoToSet(double value)
        {
            if (value >= LevelThreeTo)
            {
                LevelThreeTo = value + 0.5;
            }
        }

        public double LevelThreeTo { get; set; }

        public double LevelOneValue { get; set; }
        public double LevelTwoValue { get; set; }
        public double LevelThreeValue { get; set; }

        #region IDrainageFormula Members

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }

        #endregion
    }
}