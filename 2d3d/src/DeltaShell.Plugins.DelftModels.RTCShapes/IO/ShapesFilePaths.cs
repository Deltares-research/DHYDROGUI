using System.IO;
using System.Reflection;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.IO
{
    /// <summary>
    /// Provides methods for retrieving RTC shape file paths.
    /// </summary>
    public static class ShapesFilePaths
    {
        private const string rtcShapesConfigXsd = "rtcShapesConfig.xsd";
        private const string rtcShapesConfigXml = "rtcShapesConfig.xml";

        /// <summary>
        /// Gets the path of the shapes configuration XML file.
        /// </summary>
        /// <param name="path">The base path where the XML file is located.</param>
        /// <returns>The full path of the shapes configuration XML file.</returns>
        public static string GetShapesConfigXmlPath(string path)
            => Path.Combine(path, rtcShapesConfigXml);

        /// <summary>
        /// Gets the path of the shapes configuration XSD file.
        /// </summary>
        /// <param name="path">The base path where the XSD file is located.</param>
        /// <returns>The full path of the shapes configuration XSD file.</returns>
        public static string GetShapesConfigXsdPath(string path)
            => Path.Combine(path, rtcShapesConfigXsd);

        /// <summary>
        /// Gets the default path of the shapes configuration XSD file.
        /// </summary>
        /// <returns>The full path of the default shapes configuration XSD file.</returns>
        public static string GetDefaultShapesConfigXsdPath()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(executingAssembly.Location) ?? string.Empty;

            return Path.Combine(assemblyDirectory, "Xsd", rtcShapesConfigXsd);
        }
    }
}