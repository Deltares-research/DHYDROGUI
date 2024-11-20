using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders.Definition
{
    public interface IDefinitionReader<T>
    {
        /// <summary>
        /// Creates an object of type T for an <see cref="IniSection"/>
        /// </summary>
        /// <param name="iniSection">INI section to parse</param>
        T ReadDefinition(IniSection iniSection);
    }
}