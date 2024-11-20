using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows
{
    public static class RainfallRunoffDataRowProviderFactory
    {
        public static IDataRowProvider[] GetDataRowProviders(RainfallRunoffModel model, IEnumerable<Catchment> filter)
        {
            return new IDataRowProvider[]
                {
                    new ConceptDataRowProvider<NwrwData, NwrwDataRow>(model,"NWRW"){Filter=filter},
                    new ConceptDataRowProvider<UnpavedData, UnpavedDataRow>(model, "Unpaved"){Filter=filter},
                    new ConceptDataRowProvider<PavedData, PavedDataRow>(model, "Paved"){Filter=filter},
                    new ConceptDataRowProvider<GreenhouseData, GreenhouseDataRow>(model, "Greenhouse"){Filter=filter},
                    new ConceptDataRowProvider<OpenWaterData, OpenWaterDataRow>(model, "OpenWater"){Filter=filter},
                    new ConceptDataRowProvider<HbvData,HbvDataRow>(model,"HBV"){Filter=filter},
                    new ConceptDataRowProvider<SacramentoData,SacramentoDataRow>(model,"Sacramento"){Filter=filter},
                };
        }
    }
}