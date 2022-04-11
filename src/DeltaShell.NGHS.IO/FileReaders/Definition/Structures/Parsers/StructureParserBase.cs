using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Base class for parsers of structures.
    /// </summary>
    public abstract class StructureParserBase : IStructureParser
    {
        private readonly List<string> errorMessages;
        private readonly string structuresFilename;
        private readonly string structureName;

        /// <summary>
        /// The <see cref="IDelftIniCategory"/> for a structure.
        /// </summary>
        protected IDelftIniCategory Category { get; }
        
        /// <summary>
        /// The branch the structure should be imported on.
        /// </summary>
        protected IBranch Branch { get; }

        /// <summary>
        /// The structure type.
        /// </summary>
        protected StructureType StructureType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="StructureParserBase"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The structure <see cref="IDelftIniCategory"/> to parse.</param>
        /// <param name="branch">The branch the structure should be imported to.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        protected StructureParserBase(StructureType structureType, 
                                      IDelftIniCategory category, 
                                      IBranch branch, 
                                      string structuresFilename)
        {
            Ensure.IsDefined(structureType, nameof(structureType));
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(structuresFilename, nameof(structuresFilename));

            StructureType = structureType;
            Category = category;
            Branch = branch;
            this.structuresFilename = structuresFilename;
            structureName = category.ReadProperty<string>(StructureRegion.Id.Key, false, string.Empty);

            errorMessages = new List<string>();
        }

        /// <summary>
        /// Parses a structure from the structure <see cref="IDelftIniCategory"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileReadingException">Thrown when any property in the DelftInitCategory is missing a value.</exception>
        public IStructure1D ParseStructure()
        {
            ValidateStructureProperties();

            if (errorMessages.Any())
            {
                throw new FileReadingException($"{string.Join(Environment.NewLine, errorMessages)}");
            }

            return Parse();
        }

        /// <summary>
        /// Parses a structure from the <see cref="IDelftIniCategory"/>.
        /// </summary>
        /// <returns>The parsed structure.</returns>
        protected abstract IStructure1D Parse();

        private void ValidateStructureProperties()
        {
            foreach (DelftIniProperty property in Category.Properties
                                                          .Where(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                errorMessages.Add(string.Format(Resources.StructureParserBase_Missing_structure_property,
                                                StructureType.ToString(), structureName, property.Name, structuresFilename,
                                                property.LineNumber));
            }
        }
    }
}