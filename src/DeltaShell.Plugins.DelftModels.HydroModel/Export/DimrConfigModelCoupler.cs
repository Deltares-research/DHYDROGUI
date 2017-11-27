using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public struct DimrCoupleInfo
    {
        public string Source;
        public string Target;
    }

    public class DimrConfigModelCoupler : IDimrConfigModelCoupler
    {
        public string Source { get; private set; }
        public string Target { get; private set; }
        public bool SourceIsMasterTimeStep { get; private set; }

        private IEnumerable<DimrCoupleInfo> coupleInfos;
        public IEnumerable<DimrCoupleInfo> CoupleInfos
        {
            get
            {
                return coupleInfos.Distinct();
            }
        }

        public string Name { get; set; }
        public Func<XElement, XNamespace, XElement> AddOptionalCouplerInfo { get; set; }

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
            var outputItems = HydroModel.GetDataItems(sourceModel, DataItemRole.Output).ToList();
            var inputItems = HydroModel.GetDataItems(targetModel, DataItemRole.Input).ToList();

            var coupledDataItems = new List<DimrCoupleInfo>();

            foreach (var outputItem in outputItems)
            {
                foreach (var inputItem in inputItems)
                {
                    if (inputItem.LinkedTo != null && inputItem.LinkedTo.Equals(outputItem))
                    {
                        var sourceString = ((IDimrModel)sourceModel).GetItemString(outputItem);

                        if (sourceString == null)
                        {
                            throw new ArgumentException("Cannot serialize data item " + outputItem.Value + " to d-hydro xml");
                        }
                        sourceString = AddModelName(sourceString, sourceModel);

                        var targetString = ( (IDimrModel) targetModel ).GetItemString(inputItem);
                        if (targetString == null)
                        {
                            throw new ArgumentException("Cannot serialize data item " + inputItem.Value + " to d-hydro xml");
                        }
                        targetString = AddModelName(targetString, targetModel);

                        coupledDataItems.Add(new DimrCoupleInfo {Source = sourceString, Target = targetString});
                    }
                }
            }

            return coupledDataItems;
        }

        private string AddModelName(string item, IModel model)
        {
            var modelOwner = model.Owner as ICompositeActivity;
            if (modelOwner == null) return item;
            var owner = GetOwner(model, modelOwner.CurrentWorkflow.Activities);
            if (model as IDimrModel != owner)
            {
                item = string.Concat(model.Name, "/", item);
            }
            return item;
        }

        private IDimrModel GetOwner(IActivity sourceActivity, IEnumerable<IActivity> ownerActivities)
        {

            foreach (var subActivity in ownerActivities)
            {
                var activity = subActivity is ActivityWrapper ? ((ActivityWrapper) subActivity).Activity : subActivity;
                if(activity == sourceActivity) return sourceActivity as IDimrModel;
                var compositeActivity = activity as ICompositeActivity;
                if (compositeActivity != null)
                {
                    if (compositeActivity.Activities.Any(compositeActivityActivity => (compositeActivityActivity is ActivityWrapper ? ((ActivityWrapper)compositeActivityActivity).Activity : compositeActivityActivity) == sourceActivity))
                    {
                        return compositeActivity as IDimrModel;
                    }
                }
            }
            return sourceActivity as IDimrModel;
        }
    }
    
}