using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    //YAGNI (GvdO): What is the purpose of this (except for the base being abstract for whatever reason)?

    public class StructureProperty : ModelProperty
    {
        /// <summary>
        /// Create a new property for a structure.
        /// </summary>
        /// <param name="propertyDefinition">Property definition for this property.</param>
        /// <param name="valueAsString">String representing the initial value of this property.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="propertyDefinition"/> is null.</exception>
        /// <exception cref="System.FormatException">
        ///   When <paramref name="valueAsString"/> does not properly express the <see cref="ModelPropertyDefinition.DataType"/> 
        ///   specified in <paramref name="propertyDefinition"/>. Check <see cref="System.Exception.InnerException"/> for
        ///   underlying cause.
        /// </exception>
        public StructureProperty(ModelPropertyDefinition propertyDefinition, string valueAsString) : base(propertyDefinition, valueAsString)
        {
        }

        public override object Clone()
        {
            return new StructureProperty(PropertyDefinition, GetValueAsString());
        }
    }
}