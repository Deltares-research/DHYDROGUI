using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        public const string CellsToFeaturesName = "CellsToFeatures";
        public const string GridPropertyName = "Grid";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";

        /// <inheritdoc/>
        public virtual string LibraryName => "dflowfm";

        /// <inheritdoc/>
        public virtual string InputFile => $"{Name}{FileConstants.MduFileExtension}";

        /// <inheritdoc/>
        public virtual string DirectoryName
        {
            get
            {
                string directory = GetMduSubDirectoryFromModelDirectory();
                return string.IsNullOrWhiteSpace(directory) ? "dflowfm" : directory;
            }
        }

        /// <inheritdoc/>
        public virtual bool IsMasterTimeStep => true;

        /// <inheritdoc/>
        public virtual string ShortName => "flow";

        /// <inheritdoc/>
        public virtual string KernelDirectoryLocation => DimrApiDataSet.DFlowFmDllDirectory;

        /// <inheritdoc cref="IDimrModel.CurrentTime"/>
        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get => base.CurrentTime;
            set => base.CurrentTime = value;
        }

        /// <inheritdoc cref="IDimrModel.Status"/>
        public new virtual ActivityStatus Status
        {
            get => base.Status;
            set => base.Status = value;
        }

        /// <inheritdoc/>
        public virtual bool RunsInIntegratedModel { get; set; }
        
        /// <inheritdoc/>
        public virtual DimrRunner DimrRunner { get; }

        /// <inheritdoc/>
        public virtual string DimrExportDirectoryPath => WorkingDirectoryPath;

        /// <inheritdoc/>
        public virtual string DimrModelRelativeOutputDirectory
            => Path.Combine(DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        /// <inheritdoc/>
        public ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory =>
            CacheFile.UseCaching && CacheFile.Path.StartsWith(WorkingDirectoryPath)
                ? new HashSet<string> { CacheFile.Path }
                : new HashSet<string>();

        /// <inheritdoc/>
        public virtual string GetExporterPath(string directoryName) 
            => Path.Combine(directoryName, InputFile);

        /// <inheritdoc/>
        public virtual string GetItemString(IDataItem value)
        {
            string feature = GetFeatureCategory(value.GetFeature());

            string dataItemName = value.Name;

            string parameterName = value.GetParameterName();

            var concatNames = new List<string>(new[] { feature, dataItemName, parameterName });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        /// <summary>
        /// Gets the data item by item string.
        /// </summary>
        /// <param name="itemString"> The item string. </param>
        /// <returns> The matching data item. </returns>
        /// <remarks>
        /// <paramref name="itemString"/> cannot be null.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when
        /// - <paramref name="itemString"/> does not contain 3 elements
        /// - category in <paramref name="itemString"/> is unknown
        /// - feature in <paramref name="itemString"/> is unknown
        /// - parameter name in <paramref name="itemString"/> is unknown.
        /// </exception>
        public virtual IEnumerable<IDataItem> GetDataItemsByItemString(string itemString)
        {
            string[] stringParts = itemString.Split('/');

            if (stringParts.Length != 3)
            {
                throw new ArgumentException(string.Format(Resources.WaterFlowFMModel_DimrModel_GetDataItemByItemString__0__should_contain_a_category_feature_name_and_a_parameter_name,
                                                          itemString));
            }

            string category = stringParts[0];
            string featureName = stringParts[1];
            string parameterName = stringParts[2];

            IFeature feature = GetAreaFeature(category, featureName);

            if (feature == null)
            {
                throw new ArgumentException(string.Format(Resources.WaterFlowFMModel_DimrModel_GetDataItemByItemString_feature__0__in__1__cannot_be_found_in_the_FM_model,
                                                          featureName, itemString));
            }

            IDataItem dataItem = GetChildDataItems(feature).FirstOrDefault(di =>
            {
                var parameterValueConverter = di.ValueConverter as ParameterValueConverter;
                return parameterValueConverter?.ParameterName == parameterName;
            });

            if (dataItem == null)
            {
                throw new ArgumentException(string.Format(Resources.WaterFlowFMModel_DimrModel_GetDataItemByItemString_parameter_name__0__in__1__cannot_be_found_in_the_FM_model,
                                                          parameterName, itemString));
            }

            return new[] { dataItem };
        }

        private IFeature GetAreaFeature(string featureCategory, string featureName)
        {
            IEnumerable<INameable> featuresFromCategory = Area.GetFeaturesFromCategory(featureCategory).OfType<INameable>();

            return (IFeature)featuresFromCategory.FirstOrDefault(f => f.Name.Equals(featureName));
        }

        /// <summary>
        /// Moves all content in the source directory into the target directory.
        /// </summary>
        /// <param name="outputPath"> The path to the output directory. </param>
        public virtual void ConnectOutput(string outputPath)
        {
            currentOutputDirectoryPath = outputPath;
            ReconnectOutputFiles(outputPath);
            ReadDiaFile(outputPath);
            ClearWaqOutputDirProperty();
        }

        /// <summary>
        /// Disconnects the output.
        /// </summary>
        public virtual void DisconnectOutput()
        {
            if (HasOpenFunctionStores)
            {
                BeginEdit("Disconnecting from output files");

                if (OutputMapFileStore != null)
                {
                    OutputMapFileStore.Close();
                    OutputMapFileStore = null;
                }

                if (OutputHisFileStore != null)
                {
                    OutputHisFileStore.Close();
                    OutputHisFileStore = null;
                }

                if (OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Close();
                    OutputClassMapFileStore = null;
                }

                EndEdit();
            }

            OutputSnappedFeaturesPath = null;
        }

        /// <inheritdoc/>
        public virtual ValidationReport Validate()
        {
            return ValidateBeforeRun || Status != ActivityStatus.Initializing
                       ? WaterFlowFmModelValidationExtensions.Validate(this)
                       : new ValidationReport("", new List<ValidationIssue>());
        }

        /// <inheritdoc/>
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
                return new[] { grid };
            }

            return !string.IsNullOrEmpty(itemName)
                       ? !string.IsNullOrEmpty(parameter)
                             ? DimrRunner.GetVar($"{Name}/{category}/{itemName}/{parameter}")
                             : DimrRunner.GetVar($"{Name}/{category}/{itemName}")
                       : DimrRunner.GetVar($"{Name}/{category}");
        }

        /// <inheritdoc/>
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
                    DimrRunner.SetVar($"{Name}/{category}/{itemName}/{parameter}", values);
                    return;
                }

                DimrRunner.SetVar($"{Name}/{category}/{itemName}", values);
                return;
            }

            DimrRunner.SetVar($"{Name}/{category}", values);
        }

        /// <inheritdoc/>
        public virtual void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath)
        {
            if ((bool)ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value)
            {
                // Actions, which should be done in the IDimrModel after a successful integrated model
                // run.

                // We know the cache file will either exist at the runMduPath because it 
                // was copied here, or it will be generated by the kernel during the run.
                string runMduPath = Path.Combine(hydroModelWorkingDirectoryPath, DirectoryName, InputFile);

                CacheFile.UpdatePathToMduLocation(runMduPath);
            }

            LogWarningWriteRestartModelRun();
        }
    }
}