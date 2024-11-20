using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public static class ISedimentFractionExtensions
    {
        public static void UpdateSpatiallyVaryingNames(this ISedimentFraction sedimentFraction)
        {
            var spatiallyVaryingSedimentTypeProperties = sedimentFraction.AvailableSedimentTypes.SelectMany(
                st => st.Properties.OfType<ISpatiallyVaryingSedimentProperty>());
            var spatiallyVaryingSedimentFormulaTypeProperties = sedimentFraction.SupportedFormulaTypes.SelectMany(
                sft => sft.Properties.OfType<ISpatiallyVaryingSedimentProperty>());
            
            foreach (var spatiallyVaryingSedimentProperty in spatiallyVaryingSedimentTypeProperties
                .Plus(spatiallyVaryingSedimentFormulaTypeProperties.ToArray()))
            {
                spatiallyVaryingSedimentProperty.SpatiallyVaryingName = string.Concat(sedimentFraction.Name, "_", spatiallyVaryingSedimentProperty.Name);
            }
        }

        public static void CompileAndSetVisibilityAndIfEnabled(this ISedimentFraction sedimentFraction)
        {
            // 'compile' and set visibility and if enabled
            var sedimentProperties = sedimentFraction.CurrentFormulaType == null 
                ? sedimentFraction.CurrentSedimentType.Properties 
                : sedimentFraction.CurrentSedimentType.Properties.Plus(sedimentFraction.CurrentFormulaType.Properties.ToArray());

            foreach (var sedimentProperty in sedimentProperties)
            {
                sedimentProperty.IsEnabled =
                    sedimentProperty.Enabled(new List<ISediment>
                    {
                        sedimentFraction.CurrentFormulaType,
                        sedimentFraction.CurrentSedimentType
                    });
                sedimentProperty.IsVisible =
                    sedimentProperty.Visible(new List<ISediment>
                    {
                        sedimentFraction.CurrentFormulaType,
                        sedimentFraction.CurrentSedimentType
                    });
            }
        }

        public static void SetTransportFormulaInCurrentSedimentType(this ISedimentFraction sedimentFraction)
        {
            var traFrm =
                sedimentFraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "TraFrm") as
                    ISedimentProperty<int>;
            if (traFrm != null)
            {
                traFrm.Value = sedimentFraction.CurrentFormulaType.TraFrm;
            }
        }
    }
}