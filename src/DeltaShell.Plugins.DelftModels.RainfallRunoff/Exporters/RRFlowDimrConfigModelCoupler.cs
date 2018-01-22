using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <summary>
    /// RR dimr export config creater
    /// </summary>
    public class RRFlowDimrConfigModelCoupler : IDimrConfigModelCoupler
    {
        private readonly Dictionary<string, string> ParameterNamesToDimrNames;
        public RRFlowDimrConfigModelCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler)
        {
            ParameterNamesToDimrNames = CreateDHydroNamesDictionary();
            
            var outputItems = HydroModel.HydroModel.GetDataItems(source, DataItemRole.Output).ToList();
            var inputItems = HydroModel.HydroModel.GetDataItems(target, DataItemRole.Input).ToList();
            
            foreach (var outputItem in outputItems)
            {
                GetExchangeItem(source, target, inputItems, outputItem);
            }
        }

        private Dictionary<string, string> CreateDHydroNamesDictionary()
        {
            Dictionary<string, string> dHydroNamesDictionary = new Dictionary<string, string>();

            dHydroNamesDictionary.Add(RainfallRunoffModelParameterNames.BoundaryDischarge, FunctionAttributes.StandardNames.WaterDischarge);
            //see hydromodelbuilder, why this is not a const....
            dHydroNamesDictionary.Add("discharge (from rr 0d)", FunctionAttributes.StandardNames.WaterDischarge);

            //should be WaterFlowModelParameterNames.LocationWaterLevel
            dHydroNamesDictionary.Add("Water level", FunctionAttributes.StandardNames.WaterLevel);
            //TODO:this is a typo in Hydromodelbuilder but because of persistance not changed yet
            dHydroNamesDictionary.Add("water depth (from flow 1d)", FunctionAttributes.StandardNames.WaterLevel);
            return dHydroNamesDictionary;
        }

        private void GetExchangeItem(IModel sourceModel, IModel targetModel, List<IDataItem> inputItems, IDataItem outputItem)
        {
            foreach (var inputItem in inputItems)
            {
                if (inputItem.Children.Count > 0)
                    GetExchangeItem(sourceModel, targetModel, inputItem.Children.ToList(), outputItem);
                if (inputItem.LinkedTo != null && inputItem.LinkedTo.Equals(outputItem))
                {
                    var inDataItem = TypeUtils.Unproxy<IDataItem>(inputItem);
                    var outDataItem = TypeUtils.Unproxy<IDataItem>(outputItem);

                    var sourceRR = sourceModel as IRainfallRunoffModel;
                    if (sourceRR == null)
                    {
                        var targetRR = targetModel as IRainfallRunoffModel;
                        if (targetRR == null) return;
                        var basin = targetRR.Region as DrainageBasin;
                        if (basin == null) return;
                        if (!basin.Catchments.Any(c => Equals(c.CatchmentType, CatchmentType.Unpaved))) return;
                        var sourceDimr = sourceModel as IDimrModel;
                        if (sourceDimr == null) return;
                        var targetDimr = targetModel as IDimrModel;
                        if (targetDimr == null) return;
                        Name = sourceDimr.ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + targetDimr.ShortName;
                        Source = sourceDimr.Name;
                        Target = targetRR.Name;
                        SourceIsMasterTimeStep = true;
                        var coupledHydroLinks = new List<DimrCoupleInfo>();
                        foreach (
                            var unpavedCatchmentLink in
                            basin.Catchments.Where(c => c.CatchmentType == CatchmentType.Unpaved)
                                .SelectMany(c => c.Links)
                                .ToList())
                        {
                            var sourceString = GetItemString(unpavedCatchmentLink.Target, outputItem.Name);

                            if (sourceString == null)
                            {
                                throw new ArgumentException("Cannot serialize hydrolink source " + unpavedCatchmentLink.Target + " to d-hydro xml");
                            }
                            var targetString = GetItemString(unpavedCatchmentLink.Source, inputItem.Name);

                            if (targetString == null)
                            {
                                throw new ArgumentException("Cannot serialize hydrolink target " + unpavedCatchmentLink.Source + " to d-hydro xml");
                            }
                            coupledHydroLinks.Add(new DimrCoupleInfo { Source = sourceString, Target = targetString });

                        }
                        coupleInfos = coupledHydroLinks;
                    }
                    else
                    {
                        var sourceDimr = sourceModel as IDimrModel;
                        if (sourceDimr == null) return;
                        var targetDimr = targetModel as IDimrModel;
                        if (targetDimr == null) return;
                        Name = sourceDimr.ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + targetDimr.ShortName;
                        Source = sourceRR.Name;
                        Target = targetDimr.Name;
                        SourceIsMasterTimeStep = false;
                        var coupledHydroLinks = new List<DimrCoupleInfo>();
                        var basin = sourceRR.Region as DrainageBasin;
                        if (basin == null) return;
                        foreach (var catchmentLink in basin.Catchments.SelectMany(c => c.Links).ToList())
                        {
                            var sourceString = GetItemString(catchmentLink.Source, outputItem.Name);

                            if (sourceString == null)
                            {
                                throw new ArgumentException("Cannot serialize hydrolink source " + catchmentLink.Source + " to d-hydro xml");
                            }
                            var targetString = GetItemString(catchmentLink.Target, inputItem.Name);

                            if (targetString == null)
                            {
                                throw new ArgumentException("Cannot serialize hydrolink target " + catchmentLink.Target + " to d-hydro xml");
                            }
                            coupledHydroLinks.Add(new DimrCoupleInfo { Source = sourceString, Target = targetString });
                        }
                        coupleInfos = coupledHydroLinks;
                    }
                }
            }
        }

        private string GetItemString(IHydroObject hydroObject, string quantity)
        {
            var category = GetItemCategory(hydroObject);
            return category == null ? null : string.Concat(category,"/",hydroObject.Name, "/", ConvertedParameterNameToDimrName(quantity));
        }

        private string GetItemCategory(IHydroObject hydroObject)
        {
            if (hydroObject is Catchment)
            {
                return "catchments";
            }
            if (hydroObject is ILateralSource)
            {
                return "laterals";
            }
            if (hydroObject is IHydroNode)
            {
                return "boundaries";
            }
            return null;
        }

        #region Implementation of IDimrConfigModelCoupler

        public string Source { get; private set; }
        public string Target { get; private set; }
        public bool SourceIsMasterTimeStep { get; private set; }

        public string Name { get; set; }
        public bool AddCouplerLoggerInfo { get; set; }

        private IEnumerable<DimrCoupleInfo> coupleInfos;
        public IEnumerable<DimrCoupleInfo> CoupleInfos
        {
            get { return coupleInfos == null ? new List<DimrCoupleInfo>() : coupleInfos.Distinct(); }
        }

        public void UpdateModel(IModel sourceModel, IModel targetModel, ICompositeActivity sourceCoupler,
            ICompositeActivity targetCoupler)
        {
        }

        private string ConvertedParameterNameToDimrName(string parameterName)
        {
            string dimrName;
            return ParameterNamesToDimrNames.TryGetValue(parameterName, out dimrName) ? dimrName : parameterName;
        }
        #endregion
    }
}