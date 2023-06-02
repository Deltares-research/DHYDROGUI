using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public interface IConceptModelController
    {
        IRRModelHybridFileWriter Writer { get; set; }
        IRainfallRunoffModelController RootController { get; set; }
        bool CanHandle(CatchmentModelData area);
        bool CanHandle(ElementSet elementSet);

        void AddArea(IRainfallRunoffModel model, CatchmentModelData area, IList<ModelLink> links, IList<IFeature> allRRNodes);
        void OnInitializeFeatureCoverage(EngineParameter modelParameter, IFeatureCoverage featureCoverage);

        void Reset();
    }
}