using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    public class RainfallRunoffModelExporter : IDimrModelFileExporter
    {
        private readonly IBasinGeometrySerializer serializer;
        private readonly IEvaporationExporter evaporationExporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffModelExporter"/> class.
        /// </summary>
        /// <remarks>
        /// A parameterless constructor is required to be able to use <see cref="Activator.CreateInstance(Type)"/> on this class.
        /// </remarks>
        public RainfallRunoffModelExporter(): this(new BasinGeometryShapeFileSerializer(), 
                                                   new EvaporationExporter(new EvaporationFileWriter(),
                                                                           new EvaporationFileCreator(),
                                                                           new EvaporationFileNameConverter(),
                                                                           new IOEvaporationMeteoDataSourceConverter())) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffModelExporter"/> class.
        /// </summary>
        /// <param name="serializer"> The basin geometry serializer. </param>
        /// <param name="evaporationExporter"> The evaporation exporter. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="serializer"/> or <paramref name="evaporationExporter"/> is <c>null</c>.
        /// </exception>
        public RainfallRunoffModelExporter(IBasinGeometrySerializer serializer, IEvaporationExporter evaporationExporter)
        {
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNull(evaporationExporter, nameof(evaporationExporter));

            this.serializer = serializer;
            this.evaporationExporter = evaporationExporter;
        }

        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return "Rainfall Runoff Model exporter"; }
        }

        [ExcludeFromCodeCoverage]
        public string Category
        {
            get { return ""; }
        }

        public string Description
        {
            get { return Name; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(RainfallRunoffModel);
        }
        
        public string FileFilter
        {
            get { return "RR file folder name export|*."; }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }

        [ExcludeFromCodeCoverage]
        public bool CanExportFor(object item)
        {
            return true;
        }

        public bool Export(object item, string path)
        {
            var model = item as RainfallRunoffModel;
            if (model == null)
            {
                return false;
            }
            
            ExportBcFile(path, model);
            ExportNwrwFiles(path, model);
            ExportModelController(path, model);
            ExportMeteoFile(path, model);
            ExportBasinGeometry(path, model);
            
            return true;
        }

        private void ExportBasinGeometry(string path, RainfallRunoffModel model)
        {
            WriteToFile(path, "basinGeometry.shp", p => serializer.WriteCatchmentGeometry(model.Basin, p));
        }

        private void ExportMeteoFile(string path, RainfallRunoffModel model)
        {
            var meteoWriter = new MeteoDataExporter(evaporationExporter);
            evaporationExporter.Export(model.Evaporation, new DirectoryInfo(path));
            WriteToFile(path, "default.bui", p => meteoWriter.Export(model.Precipitation, p));
            WriteToFile(path, "default.tmp", p => meteoWriter.Export(model.Temperature, p));
        }

        private static void ExportModelController(string path, RainfallRunoffModel model)
        {
            model.ModelController.GetWorkingDirectoryDelegate = () => Path.GetFullPath(path);
            model.ModelController.WriteFiles();
        }

        private static void ExportNwrwFiles(string path, RainfallRunoffModel model)
        {
            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new Nwrw3BComponentFileWriter(model),
                new NwrwAlgComponentFileWriter(model),
                new NwrwDwfComponentFileWriter(model),
                new NwrwTpComponentFileWriter(model),
            });
            nwrwWriter.WriteNwrwFiles(path);
        }

        private static void ExportBcFile(string path, RainfallRunoffModel model)
        {
            var bcWriter = new RainfallRunoffBoundaryDataFileWriter(new BcWriter(new FileSystem()));
            bcWriter.WriteFile(Path.Combine(Path.GetFullPath(path), "BoundaryConditions.bc"), model);
        }

        private static void WriteToFile(string path, string fileName, Action<string> writeAction)
        {
            var filePath = Path.Combine(Path.GetFullPath(path), fileName);
            FileUtils.DeleteIfExists(filePath);
            writeAction(filePath);
        }
    }
}