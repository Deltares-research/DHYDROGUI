using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        public const string CellsToFeaturesName = "CellsToFeatures";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";

        private bool runsInIntegratedModel;

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
        public virtual string DimrModelRelativeOutputDirectory 
            => Path.Combine(DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        /// <inheritdoc/>
        public ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory =>
            CacheFile.UseCaching && CacheFile.Path.StartsWith(WorkingDirectory)
                ? new HashSet<string> { CacheFile.Path }
                : new HashSet<string>();
        
        /// <inheritdoc/>
        public virtual string GetItemString(IDataItem dataItem)
        {
            string category = GetFeatureCategory(dataItem.GetFeature());

            string dataItemName = dataItem.ValueConverter.OriginalValue is INetworkFeature networkFeature ? networkFeature.Name : dataItem.Name;

            string parameterName = GetConvertedParameterName(dataItem.GetParameterName(), category);
            string nameWithoutHashTags = dataItemName.Replace("##", "~~");

            var concatNames = new List<string>(new[] { category, nameWithoutHashTags, parameterName });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        private static string GetConvertedParameterName(string parameterName, string category, bool lookForValue = false)
        {
            Dictionary<string, string> namesLookup = WaterFlowFMModelDataSet.GetDictionaryForCategory(category);
            if (namesLookup == null)
            {
                return parameterName;
            }

            if (!lookForValue)
            {
                return namesLookup.TryGetValue(parameterName, out string dhydroParameterName)
                           ? dhydroParameterName
                           : parameterName;
            }

            return namesLookup.ContainsValue(parameterName)
                       ? namesLookup.First(kvp => kvp.Value == parameterName).Key
                       : parameterName;
        }

        /// <inheritdoc/>
        public virtual string GetExporterPath(string directoryName) 
            => Path.Combine(directoryName, InputFile);

        /// <inheritdoc/>
        public virtual bool CanRunParallel => true;

        /// <inheritdoc/>
        public virtual string MpiCommunicatorString => "DFM_COMM_DFMWORLD";

        /// <inheritdoc/>
        public virtual string KernelDirectoryLocation => DimrApiDataSet.DFlowFmDllDirectory;

        /// <inheritdoc/>
        public virtual void DisconnectOutput()
        {
            string[] storeNames = 
            {
                nameof(OutputMapFileStore),
                nameof(Output1DFileStore),
                nameof(OutputHisFileStore),
                nameof(OutputClassMapFileStore),
                nameof(OutputFouFileStore)
            };

            PropertyInfo[] properties = storeNames.Select(n => GetType().GetProperty(n)).ToArray();

            if (properties.Any(p => p.GetValue(this) != null))
            {
                using (this.InEditMode(DelftTools.Hydro.Properties.Resources.Disconnect_output_files_edit_action))
                {
                    properties.ForEach(ClearFunctionStore);
                }
            }

            OutputSnappedFeaturesPath = null;
            OutputIsEmpty = true;
        }

        /// <inheritdoc/>
        public virtual void ConnectOutput(string outputPath)
        {
            currentOutputDirectoryPath = outputPath;
            ReconnectOutputFiles(outputPath);
            ClearWaqOutputDirProperty();
        }

        /// <inheritdoc/>
        public virtual ValidationReport Validate()
        {
            if (Status == ActivityStatus.Initializing && !ValidateBeforeRun)
            {
                return null;
            }

            return WaterFlowFmModelValidationExtensions.Validate(this);
        }

        public virtual ValidationReport ValidationReport
            => report == null
                   ? report = Validate()
                   : report.Equals(Validate())
                       ? report
                       : report = Validate();

        /// <inheritdoc cref="IDimrModel.Status"/>
        public new virtual ActivityStatus Status
        {
            get => base.Status;
            set => base.Status = value;
        }

        /// <inheritdoc/>
        public virtual bool RunsInIntegratedModel
        {
            get => runsInIntegratedModel;
            set
            {
                runsInIntegratedModel = value;
                if (runsInIntegratedModel)
                {
                    CacheTimes();
                }
                else
                {
                    CleanCacheTimes();
                }
            }
        }

        /// <inheritdoc/>
        public virtual DimrRunner DimrRunner { get; }

        /// <inheritdoc/>
        public virtual string DimrExportDirectoryPath => WorkingDirectory;

        /// <inheritdoc cref="IDimrModel.CurrentTime"/>
        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get => base.CurrentTime;
            set
            {
                // prevent base class event bubbling
                EventingHelper.DoWithoutEvents(() => { base.CurrentTime = value; });

                OnProgressChanged();
            }
        }
        
        /// <summary>
        /// The dimr coupling for this <see cref="WaterFlowFMModel"/>.
        /// </summary>
        /// <remarks>
        /// Always returns an up-to-date <see cref="IHydroCoupling"/>.
        /// Does not return <c>null</c>.
        /// </remarks>
        public IHydroCoupling DimrCoupling
        {
            get
            {
                if (dimrCoupling == null || dimrCoupling.HasEnded)
                {
                    dimrCoupling = new WaterFlowFmDimrCoupling(Network);
                }

                return dimrCoupling;
            }
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

            if (DimrRunner.CanCommunicateWithDimrApi)
            {
                string itemText = string.IsNullOrEmpty(itemName) ? "" : $"/{itemName}";
                string parameterText = string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(parameter) ? "" : $"/{parameter}";

                return DimrRunner.GetVar($"{Name}/{category}{itemText}{parameterText}");
            }

            IFeature feature = null;
            switch (category)
            {
                case Model1DParametersCategories.Weirs:
                    feature = Network.GetBranchFeatureByName<IWeir>(itemName);
                    break;
                case Model1DParametersCategories.Culverts:
                    feature = Network.GetBranchFeatureByName<ICulvert>(itemName);
                    break;
                case Model1DParametersCategories.Pumps:
                    feature = Network.GetBranchFeatureByName<IPump>(itemName);
                    break;
                case Model1DParametersCategories.Laterals:
                    feature = Network.GetBranchFeatureByName<ILateralSource>(itemName);
                    break;
            }

            return new[] { EngineParameters.GetInitialValue(feature, parameter) };
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
        public bool IsActivityOfEnumType(ModelType type)
        {
            return type == ModelType.DFlowFM;
        }
        
        /// <inheritdoc/>
        public void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath)
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
        public virtual IEnumerable<IDataItem> GetDataItemsByItemString(string itemString, string itemString2)
        {
            string[] stringParts = itemString.Split('/');

            if (stringParts.Length != 3)
            {
                throw new ArgumentException($"{itemString} should contain a category, feature name and a parameter name.");
            }

            string category = stringParts[0];
            string featureName = stringParts[1];
            string parameterName = stringParts[2];

            IFeature feature = GetAreaFeature(category, featureName);

            if (feature == null)
            {
                throw new ArgumentException($"feature {featureName} in {itemString} cannot be found in the FM model.");
            }
            
            string parameterName2 = itemString2.Split('/').LastOrDefault() ?? string.Empty;
            IDataItem dataItem = GetChildDataItems(feature).FirstOrDefault(di =>
            {
                var parameterValueConverter = di.ValueConverter as ParameterValueConverter;
                return parameterValueConverter != null &&
                       (parameterValueConverter.ParameterName.EqualsCaseInsensitive(parameterName) || 
                       parameterValueConverter.ParameterName.EqualsCaseInsensitive(parameterName2));
            });
            
            if (dataItem == null)
            {
                return null;
            }

            return new[]
            {
                dataItem
            };
        }
        
        private IFeature GetAreaFeature(string featureCategory, string featureName)
        {

            IEnumerable<INameable> featuresFromCategory = Area.GetFeaturesFromCategory(featureCategory)
                                                              .Concat(Network.GetFeaturesFromCategory(featureCategory))
                                                              .Concat(featureCategory == Model1DParametersCategories.SourceSinks ? SourcesAndSinks.Select(sas => sas.Feature) : Enumerable.Empty<IFeature>())
                                                              .Concat(featureCategory == Model1DParametersCategories.BoundaryConditions ? BoundaryConditions1D : Enumerable.Empty<IFeature>())
                                                              .OfType<INameable>();

            return (IFeature)featuresFromCategory.FirstOrDefault(f => f.Name.Equals(featureName));
        }
    }
}