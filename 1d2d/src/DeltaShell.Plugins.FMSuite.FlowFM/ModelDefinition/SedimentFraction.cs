using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    [Entity]
    public class SedimentFraction : ISedimentFraction
    {
        private ISedimentFormulaType currentFormulaType;
        private readonly List<ISedimentFormulaType> AvailableFormulaTypes;
        

        public SedimentFraction()
        {
            AvailableSedimentTypes = SedimentFractionHelper.GetSedimentationTypes();
            AvailableFormulaTypes = SedimentFractionHelper.GetSedimentationFormulas();
            CurrentSedimentType = AvailableSedimentTypes.FirstOrDefault();
        }

        public string Name { get; set; }

        public ISedimentType CurrentSedimentType { get; set; }

        public ISedimentFormulaType CurrentFormulaType
        {
            get
            {
                if (currentFormulaType == null || !SupportedFormulaTypes.Contains(currentFormulaType))
                {
                    var traFrm = CurrentSedimentType.Properties.OfType<ISedimentProperty<int>>().FirstOrDefault(p => p.Name == "TraFrm");
                    if (traFrm != null)
                    {
                        // Always use default if possible
                        currentFormulaType = SupportedFormulaTypes.FirstOrDefault(sft => sft.TraFrm == traFrm.DefaultValue) ??
                                             SupportedFormulaTypes.FirstOrDefault();
                    }
                }
                return currentFormulaType;
            }
            set
            {
                currentFormulaType = value;
            }
        }
        
        public List<ISedimentType> AvailableSedimentTypes { get; set; }

        public List<ISedimentFormulaType> SupportedFormulaTypes
        {
            get
            {
                var currentFormulaRange = CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "TraFrm") as ISedimentProperty<int>;
                return currentFormulaRange != null
                    ? AvailableFormulaTypes.Where(f => f.TraFrm >= currentFormulaRange.MinValue && f.TraFrm <= currentFormulaRange.MaxValue).ToList()
                    : new List<ISedimentFormulaType>();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public List<string> GetAllSpatiallyVaryingPropertyNames()
        {
            var setOfLayers = new List<string>();
            if (AvailableSedimentTypes != null)
            {
                setOfLayers.AddRange(AvailableSedimentTypes.SelectMany(st => st.Properties.OfType<ISpatiallyVaryingSedimentProperty>())
                    .Select(p => p.SpatiallyVaryingName));
                    
            }
            if (AvailableFormulaTypes != null)
            {
                setOfLayers.AddRange(AvailableFormulaTypes.SelectMany(sft => sft.Properties.OfType<ISpatiallyVaryingSedimentProperty>())
                    .Select(p=> p.SpatiallyVaryingName));
            }

            return setOfLayers;
        }

        public List<string> GetAllActiveSpatiallyVaryingPropertyNames()
        {
            var setOfActiveLayers = new List<string>();
            if (CurrentSedimentType != null)
            {
                setOfActiveLayers.AddRange(CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                    .Where(p => p.IsSpatiallyVarying)
                    .Select(p => p.SpatiallyVaryingName)
                    .Where(n => !string.IsNullOrEmpty(n)));

            }
            if (CurrentFormulaType != null)
            {
                setOfActiveLayers.AddRange(CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                    .Where(p => p.IsSpatiallyVarying)
                    .Select(p => p.SpatiallyVaryingName)
                    .Where(n => !string.IsNullOrEmpty(n)));
            }

            return setOfActiveLayers;

        }
    }

    [Entity]
    public class SedimentType : ISedimentType
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public IEventedList<ISedimentProperty> Properties { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    [Entity]
    public class SedimentFormulaType : ISedimentFormulaType
    {
        public string Name { get; set; }
        public int TraFrm { get; set; }
        public IEventedList<ISedimentProperty> Properties { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
    
}