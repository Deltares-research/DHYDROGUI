using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Dimr.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <summary>
    /// RR dimr export config creater
    /// </summary>
    public class RRFlowDimrConfigModelCoupler : IDimrConfigModelCoupler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RRFlowDimrConfigModelCoupler));

        private readonly IList<DimrCoupleInfo> coupleInfos = new List<DimrCoupleInfo>();

        public RRFlowDimrConfigModelCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler)
        {
            // create mapping for rr => flow
            if (source is IRainfallRunoffModel sourceRainfallRunoffModel)
            {
                if (!(target is IDimrModel targetDimr))
                {
                    return;
                }

                IEnumerable<HydroLink> links = sourceRainfallRunoffModel.Basin.Catchments.SelectMany(c => c.Links)
                                                                        .Concat(sourceRainfallRunoffModel.Basin.WasteWaterTreatmentPlants.SelectMany(wwtp => wwtp.Links));
                SetCouplingInformation(sourceRainfallRunoffModel, targetDimr, links, FunctionAttributes.StandardNames.WaterDischarge);
            }

            // create mapping for flow => rr
            if (target is IRainfallRunoffModel targetRainfallRunoffModel && (source is IDimrModel sourceDimr))
            {
                IList<Catchment> catchments = GetPavedAndUnpavedCatchmentsToLink(targetRainfallRunoffModel);
                IEnumerable<HydroLink> links = catchments.SelectMany(c => c.Links);
                if(links.Any())
                    SetCouplingInformation(sourceDimr, targetRainfallRunoffModel, links, FunctionAttributes.StandardNames.WaterLevel, true);
            }
        }

        private static IList<Catchment> GetPavedAndUnpavedCatchmentsToLink(IRainfallRunoffModel targetRainfallRunoffModel)
        {
            var catchments = new List<Catchment>();
            foreach (Catchment catchment in targetRainfallRunoffModel.Basin.Catchments)
            {
                CatchmentType catchmentType = catchment.CatchmentType;
                if (Equals(catchmentType, CatchmentType.Paved))
                {
                    catchments.Add(catchment);
                }

                if (Equals(catchmentType, CatchmentType.Unpaved))
                {
                    var unpavedData = targetRainfallRunoffModel.ModelData.FirstOrDefault(md => catchment.Equals(md.Catchment)) as UnpavedData;
                    if (unpavedData != null && !unpavedData.BoundarySettings.UseLocalBoundaryData)
                    {
                        catchments.Add(catchment);
                    }
                }
            }

            return catchments;
        }

        public string Source { get; private set; }

        public string Target { get; private set; }

        public bool SourceIsMasterTimeStep { get; private set; }

        public string Name { get; set; }

        public bool AddCouplerLoggerInfo { get; set; }

        public IEnumerable<DimrCoupleInfo> CoupleInfos
        {
            get { return coupleInfos == null ? new List<DimrCoupleInfo>() : coupleInfos.Distinct(); }
        }

        public void UpdateModel(IModel sourceModel, IModel targetModel, ICompositeActivity sourceCoupler,
            ICompositeActivity targetCoupler)
        {
        }

        private void SetCouplingInformation(IDimrModel sourceModel, IDimrModel targetModel, IEnumerable<HydroLink> links, string quantity, bool invertLinkDirection = false)
        {
            Name = sourceModel.ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + targetModel.ShortName;
            Source = sourceModel.Name;
            Target = targetModel.Name;
            SourceIsMasterTimeStep = sourceModel.IsMasterTimeStep;

            foreach (HydroLink link in links)
            {
                IHydroObject sourceObject = invertLinkDirection ? link.Target : link.Source;
                IHydroObject targetObject = invertLinkDirection ? link.Source : link.Target;

                string sourceCategory = GetItemCategory(sourceObject);
                string targetCategory = GetItemCategory(targetObject);

                if (sourceCategory == targetCategory)
                {
                    continue;
                }

                string sourceObjectName = sourceObject.Name;
                string targetObjectName = targetObject.Name;
                if (invertLinkDirection)
                {
                    targetObjectName = sourceObjectName;
                }
                else
                {
                    sourceObjectName = targetObjectName;
                }

                string sourceString = sourceCategory != null
                                          ? $"{sourceCategory}/{sourceObjectName}/{quantity}"
                                          : null;

                string targetString = targetCategory != null
                                          ? $"{targetCategory}/{targetObjectName}/{quantity}"
                                          : null;

                if (sourceString == null || targetString == null)
                {
                    log.Warn($"Cannot serialize hydrolink {link} to d-hydro xml. This is probably because this is an internal link of the model");
                    continue;
                }

                coupleInfos.Add(new DimrCoupleInfo {Source = sourceString, Target = targetString});
            }
        }

        private static string GetItemCategory(IHydroObject hydroObject)
        {
            switch (hydroObject)
            {
                case Catchment _:
                case WasteWaterTreatmentPlant _:
                    return "catchments";
                case ILateralSource _:
                    return "laterals";
                case IHydroNode _:
                    return "boundaries";
                default:
                    return null;
            }
        }
    }
}