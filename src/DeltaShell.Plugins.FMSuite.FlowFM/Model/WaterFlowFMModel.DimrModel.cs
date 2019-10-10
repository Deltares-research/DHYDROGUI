using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Implementation of IDimrModel

        public const string CellsToFeaturesName = "CellsToFeatures";
        public const string GridPropertyName = "Grid";
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

        /// <summary>
        /// Gets the data item by item string.
        /// </summary>
        /// <param name="itemString"> The item string. </param>
        /// <returns> The matching data item. </returns>
        /// <remarks> <param name="itemString"/> cannot be null. </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown in case,
        /// - <param name="itemString"/> does not contain 3 elements
        /// - category in <param name="itemString"/> is unknown
        /// - feature in <param name="itemString"/> is unknown
        /// - parameter name in <param name="itemString"/> is unknown. </exception>
        public virtual IDataItem GetDataItemByItemString(string itemString)
        {
            string[] stringParts = itemString.Split('/');

            if (stringParts.Length != 3)
            {
                throw new ArgumentException(
                    $"{itemString} should contain a category, feature name and a parameter name");
            }

            IFeature feature = GetAreaFeature(stringParts[0], stringParts[1]);

            if (feature == null)
            {
                throw new ArgumentException(
                    $"feature {stringParts[1]} in {itemString} cannot be found in the FM model");
            }

            IDataItem dataItem = GetChildDataItems(feature).FirstOrDefault(di =>
            {
                var parameterValueConverter = di.ValueConverter as ParameterValueConverter;
                return parameterValueConverter?.ParameterName == stringParts[2];
            });

            if (dataItem == null)
            {
                throw new ArgumentException(
                    $"parameter name {stringParts[2]} in {itemString} cannot be found in the FM model");
            }

            return dataItem;
        }

        private IFeature GetAreaFeature(string featureCategory, string featureName)
        {
            IEnumerable<INameable> featuresFromCategory =
                Area.GetFeaturesFromCategory(featureCategory).OfType<INameable>();

            return (IFeature) featuresFromCategory.FirstOrDefault(f => f.Name.Equals(featureName));
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