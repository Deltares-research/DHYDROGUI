using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <summary>
    /// RR dimr export config creater
    /// </summary>
    public class RRFlowDimrConfigModelCoupler : IDimrConfigModelCoupler
    {
        private readonly IList<DimrCoupleInfo> coupleInfos = new List<DimrCoupleInfo>();

        public RRFlowDimrConfigModelCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler)
        {
            // create mapping for rr => flow
            if (source is IRainfallRunoffModel sourceRainfallRunoffModel)
            {
                if (!(target is IDimrModel targetDimr)) return;

                var links = sourceRainfallRunoffModel.Basin.Catchments.SelectMany(c => c.Links);
                SetCouplingInformation(sourceRainfallRunoffModel, targetDimr, links, FunctionAttributes.StandardNames.WaterDischarge);
            }

            // create mapping for flow => rr
            if (target is IRainfallRunoffModel targetRainfallRunoffModel)
            {
                var unpavedCatchments = targetRainfallRunoffModel.Basin.Catchments.Where(c => Equals(c.CatchmentType, CatchmentType.Unpaved)).ToList();
                if (unpavedCatchments.Count == 0) return;

                if (!(source is IDimrModel sourceDimr)) return;

                SetCouplingInformation(sourceDimr, targetRainfallRunoffModel, unpavedCatchments.SelectMany(c => c.Links), FunctionAttributes.StandardNames.WaterLevel, true);
            }
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
            SourceIsMasterTimeStep = false;

            foreach (var link in links)
            {
                var sourceObject = invertLinkDirection? link.Target : link.Source;
                var targetObject = invertLinkDirection? link.Source : link.Target;

                var sourceString = GetItemString(sourceObject, quantity);
                var targetString = GetItemString(targetObject, quantity);

                if (sourceString == null || targetString == null)
                {
                    throw new ArgumentException($"Cannot serialize hydrolink {link} to d-hydro xml");
                }

                coupleInfos.Add(new DimrCoupleInfo {Source = sourceString, Target = targetString});
            }
        }

        private static string GetItemString(IHydroObject hydroObject, string quantity)
        {
            var category = GetItemCategory(hydroObject);
            var suffix = hydroObject is Catchment catchment &&
                         !Equals(catchment.CatchmentType, CatchmentType.NWRW)
                ? "_boundary"
                : "";

            return category != null
                ? $"{category}/{hydroObject.Name}{suffix}/{quantity}"
                : null;
        }

        private static string GetItemCategory(IHydroObject hydroObject)
        {
            switch (hydroObject)
            {
                case Catchment catchment:
                    return "catchments";
                case ILateralSource lateral:
                    return "laterals";
                case IHydroNode hydroNode:
                    return "boundaries";
                default:
                    return null;
            }
        }
    }
}