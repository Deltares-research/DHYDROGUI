using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using DeltaShell.Plugins.DelftModels.RTCShapes.Xsd;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.IO
{
    /// <summary>
    /// Provides an XML reader for Real-Time Control (RTC) shape data.
    /// </summary>
    public sealed class ShapesXmlReader : IRealTimeControlXmlReader
    {
        private readonly IShapeSetter shapeSetter;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesXmlReader"/> class.
        /// </summary>
        /// <param name="shapeSetter">Sets the RTC shapes read from the shapes XML file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="shapeSetter"/> is <c>null</c>.</exception>
        public ShapesXmlReader(IShapeSetter shapeSetter)
            : this(shapeSetter, new FileSystem())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesXmlReader"/> class.
        /// </summary>
        /// <param name="shapeSetter">Sets the RTC shapes read from the shapes XML file.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// When <paramref name="shapeSetter"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public ShapesXmlReader(IShapeSetter shapeSetter, IFileSystem fileSystem)
        {
            Ensure.NotNull(shapeSetter, nameof(shapeSetter));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.shapeSetter = shapeSetter;
            this.fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public void ReadFromXml(RealTimeControlModel model, string directory)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNullOrEmpty(directory, nameof(directory));

            if (!fileSystem.Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($@"Directory '{directory}' does not exist.");
            }

            string sourceXmlPath = ShapesFilePaths.GetShapesConfigXmlPath(directory);
            if (!fileSystem.File.Exists(sourceXmlPath))
            {
                return;
            }

            ValidateShapesConfigXml(sourceXmlPath);
                
            ShapesConfigComplexType xmlData = DeserializeShapesConfigXml(sourceXmlPath);
            IReadOnlyList<ShapesGroup> shapesGroups = ConvertFromShapeXmlData(xmlData);

            AssignControlGroupShapes(shapesGroups, model.ControlGroups);
        }

        private void ValidateShapesConfigXml(string xmlPath)
        {
            var xmlValidator = new Validator(new[] { ShapesFilePaths.GetDefaultShapesConfigXsdPath() });
            
            using (FileSystemStream stream = fileSystem.File.OpenRead(xmlPath))
            {
                xmlValidator.Validate(XDocument.Load(stream));
            }
        }

        private ShapesConfigComplexType DeserializeShapesConfigXml(string xmlPath)
        {
            var serializer = new XmlSerializer(typeof(ShapesConfigComplexType));

            using (FileSystemStream stream = fileSystem.File.OpenRead(xmlPath))
            {
                return (ShapesConfigComplexType)serializer.Deserialize(stream);
            }
        }

        private void AssignControlGroupShapes(IReadOnlyList<ShapesGroup> shapesGroups, IEnumerable<ControlGroup> controlGroups)
        {
            foreach (ControlGroup controlGroup in controlGroups)
            {
                ShapesGroup shapesGroup = shapesGroups.FirstOrDefault(x => x.GroupId == controlGroup.Name);
                
                if (shapesGroup != null)
                {
                    shapeSetter.SetShapes(controlGroup, shapesGroup.Shapes);
                }
            }
        }

        private static IReadOnlyList<ShapesGroup> ConvertFromShapeXmlData(ShapesConfigComplexType xmlData)
            => ShapesXmlConverter.ConvertFromXmlData(xmlData);
    }
}