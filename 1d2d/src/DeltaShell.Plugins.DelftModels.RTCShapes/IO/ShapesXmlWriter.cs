using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Serialization;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.DelftModels.RTCShapes.Xsd;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.IO
{
    /// <summary>
    /// Provides an XML writer for Real-Time Control (RTC) shape data.
    /// </summary>
    public sealed class ShapesXmlWriter : IRealTimeControlXmlWriter
    {
        private readonly IShapeAccessor shapeAccessor;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesXmlWriter"/> class.
        /// </summary>
        /// <param name="shapeAccessor">Provides access to the RTC shapes to write.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="shapeAccessor"/> is <c>null</c>.</exception>
        public ShapesXmlWriter(IShapeAccessor shapeAccessor)
            : this(shapeAccessor, new FileSystem())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesXmlWriter"/> class.
        /// </summary>
        /// <param name="shapeAccessor">Provides access to the RTC shapes to write.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// When <paramref name="shapeAccessor"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public ShapesXmlWriter(IShapeAccessor shapeAccessor, IFileSystem fileSystem)
        {
            Ensure.NotNull(shapeAccessor, nameof(shapeAccessor));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.shapeAccessor = shapeAccessor;
            this.fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public void WriteToXml(RealTimeControlModel model, string directory)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNullOrEmpty(directory, nameof(directory));

            if (!fileSystem.Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($@"Directory '{directory}' does not exist.");
            }

            string sourceXsdPath = ShapesFilePaths.GetDefaultShapesConfigXsdPath();
            string targetXsdPath = ShapesFilePaths.GetShapesConfigXsdPath(directory);
            string targetXmlPath = ShapesFilePaths.GetShapesConfigXmlPath(directory);

            CopyShapesConfigXsd(sourceXsdPath, targetXsdPath);

            ShapesConfigComplexType xmlData = ConvertToShapeXmlData(model.ControlGroups);
            if (CanSerializeShapesConfigXml(xmlData))
            {
                SerializeShapesConfigXml(xmlData, targetXmlPath);
            }
        }

        private void CopyShapesConfigXsd(string sourceXsdPath, string targetXsdPath) 
            => fileSystem.File.Copy(sourceXsdPath, targetXsdPath, true);

        private static bool CanSerializeShapesConfigXml(ShapesConfigComplexType xmlData)
            => xmlData.groups.Any() && xmlData.groups.Any(g => g.shapes.Any());
        
        private void SerializeShapesConfigXml(ShapesConfigComplexType xmlData, string xmlPath)
        {
            var serializer = new XmlSerializer(typeof(ShapesConfigComplexType));

            using (FileSystemStream stream = fileSystem.File.OpenWrite(xmlPath))
            {
                serializer.Serialize(stream, xmlData);
            }
        }

        private ShapesConfigComplexType ConvertToShapeXmlData(IEnumerable<ControlGroup> controlGroups)
        {
            return ShapesXmlConverter.ConvertToXmlData(
                controlGroups.Select(GetShapesGroup).ToArray());
        }

        private ShapesGroup GetShapesGroup(ControlGroup controlGroup)
        {
            IReadOnlyList<ShapeBase> shapes = shapeAccessor.GetShapes(controlGroup).ToArray();

            return new ShapesGroup(controlGroup.Name, shapes);
        }
    }
}