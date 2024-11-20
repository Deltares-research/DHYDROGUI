using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Sediment
{
    public static class ISedimentFractionExtensions
    {
        public static void UpdateSpatiallyVaryingNames(this ISedimentFraction sedimentFraction)
        {
            IEnumerable<ISpatiallyVaryingSedimentProperty> spatiallyVaryingSedimentTypeProperties =
                sedimentFraction.AvailableSedimentTypes.SelectMany(
                    st => st.Properties.OfType<ISpatiallyVaryingSedimentProperty>());
            IEnumerable<ISpatiallyVaryingSedimentProperty> spatiallyVaryingSedimentFormulaTypeProperties =
                sedimentFraction.SupportedFormulaTypes.SelectMany(
                    sft => sft.Properties.OfType<ISpatiallyVaryingSedimentProperty>());

            foreach (ISpatiallyVaryingSedimentProperty spatiallyVaryingSedimentProperty in
                spatiallyVaryingSedimentTypeProperties
                    .Plus(spatiallyVaryingSedimentFormulaTypeProperties.ToArray()))
            {
                spatiallyVaryingSedimentProperty.SpatiallyVaryingName =
                    string.Concat(sedimentFraction.Name, "_", spatiallyVaryingSedimentProperty.Name);
            }
        }

        public static void CompileAndSetVisibilityAndIfEnabled(this ISedimentFraction sedimentFraction)
        {
            // 'compile' and set visibility and if enabled
            IEnumerable<ISedimentProperty> sedimentProperties = sedimentFraction.CurrentFormulaType == null
                                                                    ? sedimentFraction.CurrentSedimentType.Properties
                                                                    : sedimentFraction
                                                                      .CurrentSedimentType.Properties
                                                                      .Plus(sedimentFraction
                                                                            .CurrentFormulaType.Properties.ToArray());

            foreach (ISedimentProperty sedimentProperty in sedimentProperties)
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