using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Implementation of IDimrModel

        public const string CellsToFeaturesName = "CellsToFeatures";
        public const string GridPropertyName = "Grid";
        public const string IsPartOf1D2DModelPropertyName = "IsPartOf1D2DModel";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";

        public virtual bool IsMasterTimeStep => true;
        public virtual string ShortName => "flow";

        public virtual string GetItemString(IDataItem dataItem)
        {
            string feature = GetFeatureCategory(dataItem.GetFeature());

            string dataItemName = dataItem.Name;

            string parameterName = dataItem.GetParameterName();

            var concatNames = new List<string>(new[]
            {
                feature,
                dataItemName,
                parameterName
            });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        public virtual IDataItem GetDataItemByItemString(string itemString)
        {
            throw new NotImplementedException();
        }

        public virtual string MpiCommunicatorString => "DFM_COMM_DFMWORLD";

        public virtual ValidationReport Validate()
        {
            return ValidateBeforeRun || Status != ActivityStatus.Initializing
                       ? WaterFlowFmModelValidationExtensions.Validate(this)
                       : new ValidationReport("", new List<ValidationIssue>());
        }

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get => base.CurrentTime;
            set => base.CurrentTime = value;
        }

        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            if (category == CellsToFeaturesName)
            {
                if (OutputMapFileStore != null && OutputMapFileStore.BoundaryCellValues != null)
                {
                    return OutputMapFileStore.BoundaryCellValues.ToArray();
                }

                return null;
            }

            if (category == GridPropertyName)
            {
                return new[]
                {
                    grid
                };
            }

            return !string.IsNullOrEmpty(itemName)
                       ? !string.IsNullOrEmpty(parameter)
                             ? runner.GetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter))
                             : runner.GetVar(string.Format("{0}/{1}/{2}", Name, category, itemName))
                       : runner.GetVar(string.Format("{0}/{1}", Name, category));
        }

        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            if (category == IsPartOf1D2DModelPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                {
                    bool isPartOf1D2DModel = boolArray[0];
                    // This property is made because 1D2D integrated models do not support UGrid format.
                    // Remove when this dependency has vanished (DELFT3DFM-989)
                    WaterFlowFMProperty isPartOf1D2DModelGuiProperty =
                        ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel);
                    if ((bool) isPartOf1D2DModelGuiProperty.Value != isPartOf1D2DModel)
                    {
                        isPartOf1D2DModelGuiProperty.Value = isPartOf1D2DModel;
                        if (isPartOf1D2DModel && UseMorSed)
                        {
                            ModelDefinition.UseMorphologySediment = false;
                            Log.InfoFormat(
                                Resources
                                    .WaterFlowFMModel_SetVar_FM_Model__0__is_part_of_a_1D2D_model_and_can_t_have_morphology_properties_and___or_sediments__Removing_these_properties_from_the_model,
                                Name);
                        }

                        ModelDefinition.SetMapFormatPropertyValue();
                    }
                }

                return;
            }

            if (category == DisableFlowNodeRenumberingPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                {
                    DisableFlowNodeRenumbering = boolArray[0];
                }

                return;
            }

            if (!string.IsNullOrEmpty(itemName))
            {
                if (!string.IsNullOrEmpty(parameter))
                {
                    runner.SetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter), values);
                    return;
                }

                runner.SetVar(string.Format("{0}/{1}/{2}", Name, category, itemName), values);
                return;
            }

            runner.SetVar(string.Format("{0}/{1}", Name, category), values);
        }

        #endregion
    }
}