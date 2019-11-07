using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition
{
    public interface IDefinitionReader<T>
    {
        /// <summary>
        /// Creates an object of type T for an <see cref="IDelftIniCategory"/>
        /// </summary>
        /// <param name="category">Category to parse</param>
        T ReadDefinition(IDelftIniCategory category);
    }
}