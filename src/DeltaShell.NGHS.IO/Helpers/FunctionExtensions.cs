using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Retention;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class FunctionExtensions
    {
        public static void AddStorageTable(this IFunction storageDataTable, DelftIniCategory definition, string name)
        {
            IVariable argument = storageDataTable?.Arguments.Count > 0 ? storageDataTable.Arguments[0] : null;
            IVariable component = storageDataTable?.Components.Count > 0 ? storageDataTable.Components[0] : null;

            if (argument == null || component == null)
            {
                return;
            }

            if (!(argument.Values is IList<double> levels))
            {
                var cannotWriteArea = $"Cannot write area with id : {name} because levels / heighes in table is not defined as a list of doubles.";
                throw new FileWritingException(cannotWriteArea);
            }

            if (!(component.Values is IList<double> storageAreas))
            {
                var cannotWriteArea = $"Cannot write area with id : {name} because storage areas in table is not defined as a list of doubles.";
                throw new FileWritingException(cannotWriteArea);
            }

            if (argument.InterpolationType == InterpolationType.None)
            {
                var cannotWriteArea = $"Cannot write area with id : {name} because interpolation type is set to 'None'. Core cannot handle this type";
                throw new FileWritingException(cannotWriteArea);
            }

            definition.AddProperty(RetentionRegion.NumLevels, levels.Count);
            definition.AddProperty(RetentionRegion.Levels, levels);
            definition.AddProperty(RetentionRegion.StorageArea, storageAreas);
            definition.AddProperty(RetentionRegion.Interpolate, argument.InterpolationType == InterpolationType.Linear ? "linear" : "block");
        }
    }
}