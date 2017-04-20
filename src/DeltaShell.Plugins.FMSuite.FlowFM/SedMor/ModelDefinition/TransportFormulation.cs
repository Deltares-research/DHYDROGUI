using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition
{
    public class TransportFormulation
    {
        public int IFormNumber { get; private set; }
        public string Name { get; private set; }
        public Dictionary<string, SedMorProperty> Properties { get; private set; }

        public TransportFormulation(int iformNumber)
        {
            IFormNumber = iformNumber;
            Name = GetNameByIFormNumber(iformNumber);
            Properties = new Dictionary<string, SedMorProperty>();
        }
        
        # region Static

        private static ModelSchema<SedMorPropertyDefinition> transportFormulationsSchema;

        public static ModelSchema<SedMorPropertyDefinition> TransportFormulationsSchema
        {
            get { return transportFormulationsSchema ?? (transportFormulationsSchema = LoadSchema()); }
        }

        public static IEnumerable<string> AvailableTransportFormulations
        {
            get { return TransportFormulationsSchema.GuiPropertyGroups.Select(g => g.Value.Name); }
        }

        private static ModelSchema<SedMorPropertyDefinition> LoadSchema()
        {
            const string traPropertiesCsvFileName = "tra-properties.csv";
            var assemblyLocation = typeof(TransportFormulation).Assembly.Location;
            var directoryInfo = new FileInfo(assemblyLocation).Directory;
            if (directoryInfo != null)
            {
                var path = directoryInfo.FullName;
                var propertiesDefinitionFile = Path.Combine(path, traPropertiesCsvFileName);
                return new ModelSchemaCsvFile().ReadModelSchema<SedMorPropertyDefinition>(propertiesDefinitionFile, "TraGroup");
            }
            throw new Exception("Failed to load property definition file: " + traPropertiesCsvFileName);
        }

        private static string GetNameByIFormNumber(int iformNumber)
        {
            return TransportFormulationsSchema.GuiPropertyGroups[iformNumber.ToString()].Name;
        }

        #endregion
    }
}