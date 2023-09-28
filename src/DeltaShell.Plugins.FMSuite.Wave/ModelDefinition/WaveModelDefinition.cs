using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    public class WaveModelDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveModelDefinition));

        /// <summary>
        /// Create default/empty model definition and set the correct default values depending on other properties.
        /// </summary>
        public WaveModelDefinition()
        {
            LoadSchema();

            Properties = new EventedList<WaveModelProperty>();
            foreach (WaveModelPropertyDefinition propertyDefinition in ModelSchema.PropertyDefinitions.Values)
            {
                if (MdwFile.ExcludedSections.Contains(propertyDefinition.Category))
                {
                    continue;
                }

                if (propertyDefinition.MultipleDefaultValuesAvailable)
                {
                    string defaultValueDependentOn = propertyDefinition.DefaultValueDependentOn;
                    WaveModelProperty prop = Properties.FirstOrDefault(p =>
                                                                           p.PropertyDefinition.FilePropertyKey.Equals(
                                                                               defaultValueDependentOn));
                    if (prop != null)
                    {
                        var propValue = (int)prop.Value;
                        propertyDefinition.DefaultValueAsString = propertyDefinition.MultipleDefaultValues[propValue];
                    }
                    else
                    {
                        propertyDefinition.DefaultValueAsString = "0";
                        Log.WarnFormat(
                            "In file dwave-properties.csv multiple default values are defined dependent on the non-existing property {0}",
                            defaultValueDependentOn);
                    }
                }

                var waveProp = new WaveModelProperty(propertyDefinition, propertyDefinition.DefaultValueAsString);

                SetModelProperty(propertyDefinition.FileSectionName, propertyDefinition.FilePropertyKey,
                                 waveProp);
            }

            Dependencies.CompileEnabledDependencies(Properties);
            Dependencies.CompileVisibleDependencies(Properties);

            BoundaryContainer = new BoundaryContainer();
        }

        public IEventedList<WaveModelProperty> Properties { get; set; }
        public ModelPropertySchema<WaveModelPropertyDefinition> ModelSchema { get; private set; }
        public IWaveDomainData OuterDomain { get; set; }

        /// <summary>
        /// Gets the feature container.
        /// </summary>
        public IWaveFeatureContainer FeatureContainer { get; } = new WaveFeatureContainer();

        public string ObstaclePolylineFile { get; set; }

        public IBoundaryContainer BoundaryContainer { get; }

        public void SetModelProperty(string fileSectionName, string filePropertyKey, WaveModelProperty property)
        {
            WaveModelProperty prop = Properties
                                     .Where(p => p.PropertyDefinition.FileSectionName.Equals(fileSectionName))
                                     .FirstOrDefault(
                                         p => p.PropertyDefinition.FilePropertyKey.Equals(filePropertyKey));
            if (prop != null)
            {
                Properties[Properties.IndexOf(prop)] = property;
            }
            else
            {
                Properties.Add(property);
            }
        }

        public WaveModelProperty GetModelProperty(string fileSectionName, string propertyKey)
        {
            return Properties.Where(p => p.PropertyDefinition.FileSectionName.Equals(fileSectionName))
                             .FirstOrDefault(p => p.PropertyDefinition.FilePropertyKey.Equals(propertyKey));
        }

        private void LoadSchema()
        {
            const string dwavePropertiesCsvFileName = "dwave-properties.csv";
            Assembly assembly = typeof(WaveModelDefinition).Assembly;
            string assemblyLocation = assembly.Location;
            DirectoryInfo directoryInfo = new FileInfo(assemblyLocation).Directory;
            if (directoryInfo != null)
            {
                string path = directoryInfo.FullName;
                string propertiesDefinitionFile = Path.Combine(path, dwavePropertiesCsvFileName);
                ModelSchema =
                    new ModelSchemaCsvFile().ReadModelSchema<WaveModelPropertyDefinition>(
                        propertiesDefinitionFile, "MdwGroup");

                // override default reference date property value with current date
                ModelPropertyDefinition referenceDatePropertyDefinition =
                    ModelSchema.ModelDefinitionCategory[KnownWaveSections.GeneralSection]?
                               .PropertyDefinitions
                               .FirstOrDefault(p => p.FilePropertyKey == KnownWaveProperties.ReferenceDate);

                if (referenceDatePropertyDefinition != null)
                {
                    referenceDatePropertyDefinition.DefaultValueAsString = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                throw new Exception("Failed to load property definition file: " + dwavePropertiesCsvFileName);
            }
        }

        #region convenience getters for known modelproperties

        public DateTime ModelReferenceDateTime
        {
            get => GetReferenceDateAsDateTime();
            set => SetReferenceDateAsDateTime(value);
        }

        private DateTime GetReferenceDateAsDateTime()
        {
            object value = GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.ReferenceDate).Value;
            var refDate = (DateOnly)value;
            return refDate.ToDateTime(TimeOnly.MinValue);
        }

        private void SetReferenceDateAsDateTime(DateTime value)
        {
            DateOnly refDate = DateOnly.FromDateTime(value);
            if (refDate.ToDateTime(TimeOnly.MinValue) != value)
            {
                throw new ArgumentException($"Unexpected non-zero time in ReferenceTime value {value}");
            }
            GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.ReferenceDate).Value = refDate;

        }

        public WaveDirectionalSpaceType DefaultDirectionalSpaceType =>
            (WaveDirectionalSpaceType)GetModelProperty(KnownWaveSections.GeneralSection,
                                                        KnownWaveProperties.DirectionalSpaceType).Value;

        public int DefaultNumberOfDirections =>
            (int)GetModelProperty(KnownWaveSections.GeneralSection,
                                   KnownWaveProperties.NumberOfDirections).Value;

        public double DefaultStartDirection =>
            (double)GetModelProperty(KnownWaveSections.GeneralSection,
                                      KnownWaveProperties.StartDirection).Value;

        public double DefaultEndDirection =>
            (double)GetModelProperty(KnownWaveSections.GeneralSection,
                                      KnownWaveProperties.EndDirection).Value;

        public int DefaultNumberOfFrequencies =>
            (int)GetModelProperty(KnownWaveSections.GeneralSection,
                                   KnownWaveProperties.NumberOfFrequencies).Value;

        public double DefaultStartFrequency =>
            (double)GetModelProperty(KnownWaveSections.GeneralSection,
                                      KnownWaveProperties.StartFrequency).Value;

        public double DefaultEndFrequency =>
            (double)GetModelProperty(KnownWaveSections.GeneralSection,
                                      KnownWaveProperties.EndFrequency).Value;

        public UsageFromFlowType DefaultBedLevelUsage
        {
            get =>
                (UsageFromFlowType)GetModelProperty(KnownWaveSections.GeneralSection,
                                                     KnownWaveProperties.FlowBedLevelUsage).Value;
            set =>
                GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.FlowBedLevelUsage)
                    .SetValueAsString(((int)value).ToString());
        }

        public UsageFromFlowType DefaultWaterLevelUsage
        {
            get =>
                (UsageFromFlowType)GetModelProperty(KnownWaveSections.GeneralSection,
                                                     KnownWaveProperties.FlowWaterLevelUsage).Value;
            set =>
                GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.FlowWaterLevelUsage)
                    .SetValueAsString(((int)value).ToString());
        }

        public UsageFromFlowType DefaultVelocityUsage
        {
            get =>
                (UsageFromFlowType)GetModelProperty(KnownWaveSections.GeneralSection,
                                                     KnownWaveProperties.FlowVelocityUsage).Value;
            set =>
                GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.FlowVelocityUsage)
                    .SetValueAsString(((int)value).ToString());
        }

        public VelocityComputationType DefaultVelocityUsageType =>
            (VelocityComputationType)GetModelProperty(KnownWaveSections.GeneralSection,
                                                       KnownWaveProperties.FlowVelocityUsageType).Value;

        public UsageFromFlowType DefaultWindUsage
        {
            get =>
                (UsageFromFlowType)GetModelProperty(KnownWaveSections.GeneralSection,
                                                     KnownWaveProperties.FlowWindUsage).Value;
            set =>
                GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.FlowWindUsage)
                    .SetValueAsString(((int)value).ToString());
        }

        public string CommunicationsFilePath
        {
            get => GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.COMFile).GetValueAsString();
            set => GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.COMFile).SetValueAsString(value);
        }

        /// <summary>
        /// Gets or sets the value of the INPUTTemplateFile property of the model definition.
        /// The INPUTTemplateFile property defines a path to a pre-existing SWAN input file. 
        /// </summary>
        public string InputTemplateFilePath
        {
            get => InputTemplateFileProperty.GetValueAsString();
            set => InputTemplateFileProperty.SetValueAsString(value);
        }

        public bool WaveSetup
        {
            get => (bool)GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup).Value;
            set => GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup).Value = value;
        }

        private WaveModelProperty InputTemplateFileProperty => GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.InputTemplateFile);

        #endregion
    }
}