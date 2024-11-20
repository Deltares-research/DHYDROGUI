using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public struct DimrCoupleInfo
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }

    public class DimrConfigModelCoupler : IDimrConfigModelCoupler
    {
        private IEnumerable<DimrCoupleInfo> coupleInfos;

        public DimrConfigModelCoupler(IModel sourceModel, IModel targetModel, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler)
        {
            coupleInfos = GetCoupleInfos(sourceModel, targetModel);
            var sourceDimrCoupler = sourceCoupler as IDimrModel;
            SourceIsMasterTimeStep = false;
            if (sourceDimrCoupler != null)
            {
                SourceIsMasterTimeStep = sourceDimrCoupler.IsMasterTimeStep;
            }
            else
            {
                sourceDimrCoupler = sourceModel as IDimrModel;
                if (sourceDimrCoupler != null)
                {
                    SourceIsMasterTimeStep = sourceDimrCoupler.IsMasterTimeStep;
                }
            }

            /*Set name*/
            UpdateNames(sourceModel as IDimrModel, targetModel as IDimrModel, sourceCoupler as IDimrModel, targetCoupler as IDimrModel);
        }

        public string Source { get; private set; }
        public string Target { get; private set; }
        public bool SourceIsMasterTimeStep { get; private set; }

        public IEnumerable<DimrCoupleInfo> CoupleInfos
        {
            get
            {
                return coupleInfos.Distinct();
            }
        }

        public string Name { get; set; }
        public bool AddCouplerLoggerInfo { get; set; }

        public void UpdateModel(IModel sourceModel, IModel targetModel, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler)
        {
            coupleInfos = coupleInfos.Concat(GetCoupleInfos(sourceModel, targetModel));
            UpdateNames(sourceModel as IDimrModel, targetModel as IDimrModel, sourceCoupler as IDimrModel, targetCoupler as IDimrModel);
        }

        private void UpdateNames(IDimrModel sourceModel, IDimrModel targetModel, IDimrModel sourceCoupler, IDimrModel targetCoupler)
        {
            Name = string.Join(DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER, GetModelShortName(sourceModel, sourceCoupler), GetModelShortName(targetModel, targetCoupler));
            Source = GetModelName(sourceModel, sourceCoupler);
            Target = GetModelName(targetModel, targetCoupler);
        }

        private static string GetModelName(IDimrModel model, IDimrModel coupler)
        {
            return coupler != null ? coupler.Name : model != null ? model.Name : string.Empty;
        }

        private static string GetModelShortName(IDimrModel model, IDimrModel coupler)
        {
            return coupler != null ? coupler.ShortName : model != null ? model.ShortName : string.Empty;
        }

        private IEnumerable<DimrCoupleInfo> GetCoupleInfos(IModel sourceModel, IModel targetModel)
        {
            List<IDataItem> outputItems = HydroModel.GetDataItemsUsedForCouplingModel(sourceModel, DataItemRole.Output).ToList();
            List<IDataItem> inputItems = HydroModel.GetDataItemsUsedForCouplingModel(targetModel, DataItemRole.Input).ToList();

            var coupledDataItems = new List<DimrCoupleInfo>();

            foreach (IDataItem outputItem in outputItems)
            {
                foreach (IDataItem inputItem in inputItems)
                {
                    if (inputItem.LinkedTo != null && inputItem.LinkedTo.Equals(outputItem))
                    {
                        string sourceString = ((IDimrModel) sourceModel).GetItemString(outputItem);

                        if (sourceString == null)
                        {
                            throw new ArgumentException("Cannot serialize data item " + outputItem.Value + " to d-hydro xml");
                        }

                        sourceString = AddModelName(sourceString, sourceModel);

                        string targetString = ((IDimrModel) targetModel).GetItemString(inputItem);
                        if (targetString == null)
                        {
                            throw new ArgumentException("Cannot serialize data item " + inputItem.Value + " to d-hydro xml");
                        }

                        targetString = AddModelName(targetString, targetModel);

                        coupledDataItems.Add(new DimrCoupleInfo
                        {
                            Source = sourceString,
                            Target = targetString
                        });
                    }
                }
            }

            return coupledDataItems;
        }

        private string AddModelName(string item, IModel model)
        {
            var modelOwner = model.Owner as ICompositeActivity;
            if (modelOwner == null)
            {
                return item;
            }

            IDimrModel owner = GetOwner(model, modelOwner.CurrentWorkflow.Activities);
            if (model as IDimrModel != owner)
            {
                item = string.Concat(model.Name, "/", item);
            }

            return item;
        }

        private static IDimrModel GetOwner(IActivity sourceActivity, IEnumerable<IActivity> ownerActivities)
        {
            foreach (IActivity subActivity in ownerActivities)
            {
                IActivity activity = subActivity is ActivityWrapper wrapper ? wrapper.Activity : subActivity;
                if (activity == sourceActivity)
                {
                    return sourceActivity as IDimrModel;
                }

                if (activity is ICompositeActivity compositeActivity && compositeActivity.Activities.Any(compositeActivityActivity => GetActivity(compositeActivityActivity) == sourceActivity))
                {
                    return compositeActivity as IDimrModel;
                }
            }

            return sourceActivity as IDimrModel;
        }

        private static IActivity GetActivity(IActivity activity)
        {
            return activity is ActivityWrapper wrapper ? wrapper.Activity : activity;
        }
    }
}