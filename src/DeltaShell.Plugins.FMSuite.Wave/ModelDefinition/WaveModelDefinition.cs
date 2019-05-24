using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    public class WaveModelDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveModelDefinition));
        public IEventedList<WaveModelProperty> Properties { get; set; }
        public ModelSchema<WaveModelPropertyDefinition> ModelSchema { get; private set; }

        public bool BoundaryIsDefinedBySpecFile { get; set; }
        public string OverallSpecFile { get; set; }

        public WaveDomainData OuterDomain { get; set; }
        public IEventedList<WaveBoundaryCondition> BoundaryConditions { get; set; }
        public IEventedList<WaveObstacle> Obstacles { get; set; }
        public IEventedList<Feature2DPoint> ObservationPoints { get; set; }
        public IEventedList<Feature2D> ObservationCrossSections { get; set; }

        public string ObstaclePolylineFile { get; set; }

        // only for import, will be converted
        public IList<WaveBoundaryCondition> OrientedBoundaryConditions { get; set; }

        public WaveInputFieldData TimePointData { get; set; }

        // 

        /// <summary>
        /// Create default/empty model definition and set the correct default values depending on other properties.
        /// </summary>
        public WaveModelDefinition()
        {
            LoadSchema();

            Properties = new EventedList<WaveModelProperty>();
            foreach (WaveModelPropertyDefinition propertyDefinition in ModelSchema.PropertyDefinitions.Values)
            {
                if (MdwFile.ExcludedCategories.Contains(propertyDefinition.Category))
                {
                    continue;
                }

                if (propertyDefinition.MultipleDefaultValuesAvailable)
                {
                    string defaultValueDependentOn = propertyDefinition.DefaultValueDependentOn;
                    WaveModelProperty prop = Properties.FirstOrDefault(p =>
                                                                           p.PropertyDefinition.FilePropertyName.Equals(
                                                                               defaultValueDependentOn));
                    if (prop != null)
                    {
                        var propValue = (int) prop.Value;
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

                SetModelProperty(propertyDefinition.FileCategoryName, propertyDefinition.FilePropertyName,
                                 waveProp);
            }

            Dependencies.CompileEnabledDependencies(Properties);
            Dependencies.CompileVisibleDependencies(Properties);

            BoundaryConditions = new EventedList<WaveBoundaryCondition>();
            OrientedBoundaryConditions = new List<WaveBoundaryCondition>();
            Obstacles = new EventedList<WaveObstacle>();
            TimePointData = new WaveInputFieldData();
            ObservationPoints = new EventedList<Feature2DPoint>();
            ObservationCrossSections = new EventedList<Feature2D>();

            BoundaryIsDefinedBySpecFile = false;
        }

        public void SetModelProperty(string fileCategoryName, string filePropertyName, WaveModelProperty property)
        {
            WaveModelProperty prop = Properties
                                     .Where(p => p.PropertyDefinition.FileCategoryName.Equals(fileCategoryName))
                                     .FirstOrDefault(
                                         p => p.PropertyDefinition.FilePropertyName.Equals(filePropertyName));
            if (prop != null)
            {
                Properties[Properties.IndexOf(prop)] = property;
            }
            else
            {
                Properties.Add(property);
            }
        }

        public WaveModelProperty GetModelProperty(string fileCategoryName, string propertyName)
        {
            return Properties.Where(p => p.PropertyDefinition.FileCategoryName.Equals(fileCategoryName))
                             .FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.Equals(propertyName));
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
                ModelPropertyDefinition referenceDatePropertyDefinition = ModelSchema
                                                                          .ModelDefinitionCategory[
                                                                              KnownWaveCategories.GeneralCategory]
                                                                          ?.PropertyDefinitions
                                                                          .FirstOrDefault(
                                                                              p => p.FilePropertyName ==
                                                                                   KnownWaveProperties.ReferenceDate);
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
            get => (DateTime) GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ReferenceDate)
                .Value;
            set => GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ReferenceDate).Value =
                       value;
        }

        public WaveDirectionalSpaceType DefaultDirectionalSpaceType =>
            (WaveDirectionalSpaceType) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                        KnownWaveProperties.DirectionalSpaceType).Value;

        public int DefaultNumberOfDirections =>
            (int) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                   KnownWaveProperties.NumberOfDirections).Value;

        public double DefaultStartDirection =>
            (double) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                      KnownWaveProperties.StartDirection).Value;

        public double DefaultEndDirection =>
            (double) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                      KnownWaveProperties.EndDirection).Value;

        public int DefaultNumberOfFrequencies =>
            (int) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                   KnownWaveProperties.NumberOfFrequencies).Value;

        public double DefaultStartFrequency =>
            (double) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                      KnownWaveProperties.StartFrequency).Value;

        public double DefaultEndFrequency =>
            (double) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                      KnownWaveProperties.EndFrequency).Value;

        public UsageFromFlowType DefaultBedLevelUsage
        {
            get =>
                (UsageFromFlowType) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                     KnownWaveProperties.FlowBedLevelUsage).Value;
            set =>
                GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.FlowBedLevelUsage)
                    .SetValueAsString(((int) value).ToString());
        }

        public UsageFromFlowType DefaultWaterLevelUsage
        {
            get =>
                (UsageFromFlowType) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                     KnownWaveProperties.FlowWaterLevelUsage).Value;
            set =>
                GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.FlowWaterLevelUsage)
                    .SetValueAsString(((int) value).ToString());
        }

        public UsageFromFlowType DefaultVelocityUsage
        {
            get =>
                (UsageFromFlowType) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                     KnownWaveProperties.FlowVelocityUsage).Value;
            set =>
                GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.FlowVelocityUsage)
                    .SetValueAsString(((int) value).ToString());
        }

        public VelocityComputationType DefaultVelocityUsageType =>
            (VelocityComputationType) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                       KnownWaveProperties.FlowVelocityUsageType).Value;

        public UsageFromFlowType DefaultWindUsage
        {
            get =>
                (UsageFromFlowType) GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                     KnownWaveProperties.FlowWindUsage).Value;
            set =>
                GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.FlowWindUsage)
                    .SetValueAsString(((int) value).ToString());
        }

        public double WaveTimeStep
        {
            get
            {
                double dt = -1.0;
                WaveModelProperty prop =
                    GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeStep);
                if (prop != null)
                {
                    dt = (double) prop.Value;
                }

                return dt;
            }
        }

        public bool WaveSetup
        {
            get => (bool) GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup).Value;
            set => GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup).Value = value;
        }

        #endregion
    }
}