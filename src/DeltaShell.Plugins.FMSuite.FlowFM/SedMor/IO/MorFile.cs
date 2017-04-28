using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.IO
{
    public class MorFile
    {
        public string MorFilePath { get; private set; }

        public MorphologyModelDefinition Load(string path)
        {
            MorFilePath = path;

            var modelDefinition = new MorphologyModelDefinition();
            var modelSchema = modelDefinition.ModelSchema;
            var boundarySchema = modelDefinition.BoundarySchema;
            var morCategories = new SedMorDelftIniReader().ReadDelftIniFile(path);
            foreach (var category in morCategories)
            {
                if (category.Name == "Boundary") // special treatment
                {
                    var boundary = new MorphologyBoundary();
                    SedMorFileHelper.LoadCategoryIntoProperties(category, boundary.Properties, boundarySchema, "Boundary");
                    modelDefinition.Boundaries.Add(boundary);
                }
                
                if (modelSchema.ModelDefinitionCategory.ContainsKey(category.Name))
                {
                    SedMorFileHelper.LoadCategoryIntoProperties(category,
                                               modelDefinition.Properties,
                                               modelSchema.PropertyDefinitions,
                                               modelSchema.ModelDefinitionCategory[category.Name].Name);
                }
            }
            return modelDefinition;
        }

        public void Save(string path, MorphologyModelDefinition definition)
        {
            var writer = new SedMorDelftIniWriter();

            var categories = SedMorFileHelper.PutPropertiesIntoCategories(definition.Properties);
            foreach (var sediment in definition.Boundaries)
            {
                var sedimentCategory = new DelftIniCategory("Boundary");

                foreach (var property in sediment.Properties.Values)
                {
                    var propDef = property.PropertyDefinition;
                    SedMorFileHelper.AddProperty(sedimentCategory, propDef.FilePropertyName, property);
                }

                categories.Add(sedimentCategory);
            }

            writer.WriteDelftIniFile(categories.ToList(), path);
        }
    }
}