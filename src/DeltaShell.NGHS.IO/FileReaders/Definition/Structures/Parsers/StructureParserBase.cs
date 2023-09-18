using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
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
        /// The <see cref="IniSection"/> for a structure.
        /// </summary>
        protected IniSection IniSection { get; }
        
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
        /// <param name="iniSection">The structure <see cref="DHYDRO.Common.IO.Ini.IniSection"/> to parse.</param>
        /// <param name="branch">The branch the structure should be imported to.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        protected StructureParserBase(StructureType structureType, 
                                      IniSection iniSection, 
                                      IBranch branch, 
                                      string structuresFilename)
        {
            Ensure.IsDefined(structureType, nameof(structureType));
            Ensure.NotNull(iniSection, nameof(iniSection));
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(structuresFilename, nameof(structuresFilename));

            StructureType = structureType;
            IniSection = iniSection;
            Branch = branch;
            this.structuresFilename = structuresFilename;
            structureName = iniSection.ReadProperty<string>(StructureRegion.Id.Key, false, string.Empty);

            errorMessages = new List<string>();
        }

        /// <summary>
        /// Parses a structure from the structure <see cref="DHYDRO.Common.IO.Ini.IniSection"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileReadingException">Thrown when any property in the IniSection is missing a value.</exception>
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
        /// Parses a structure from the <see cref="DHYDRO.Common.IO.Ini.IniSection"/>.
        /// </summary>
        /// <returns>The parsed structure.</returns>
        protected abstract IStructure1D Parse();

        private void ValidateStructureProperties()
        {
            foreach (IniProperty property in IniSection.Properties
                                                          .Where(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                errorMessages.Add(string.Format(Resources.StructureParserBase_Missing_structure_property,
                                                StructureType.ToString(), structureName, property.Key, structuresFilename,
                                                property.LineNumber));
            }
        }
    }
}