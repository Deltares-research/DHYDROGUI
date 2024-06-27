using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    /// <summary>
    /// <see cref="LateralSourceFileImporter"/> defines the <see cref="IFileImporter"/> used to
    /// import a lateral source csv file.
    /// </summary>
    /// <seealso cref="TimeSeriesCsvFileImporter"/>
    /// <remarks>
    /// This class is based upon the FlowTimeSeriesCsvFileImporter in SOBEK.
    /// </remarks>
    public class LateralSourceFileImporter : TimeSeriesCsvFileImporter
    {
        public BoundaryRelationType BoundaryRelationType { get; set; }
        private readonly ILog log;
        private readonly ICsvImporter csvImporter;
        private const string waterLevel = "Water Level";
        private const string discharge = "Discharge";
        private const string time = "Time";

        /// <summary>
        /// Creates a new default <see cref="LateralSourceFileImporter"/>.
        /// </summary>
        public LateralSourceFileImporter()
            : this(new CsvImporter(), LogManager.GetLogger(typeof(LateralSourceFileImporter))) { }
        
        /// <summary>
        /// Creates a new <see cref="LateralSourceFileImporter"/> with the given parameters.
        /// </summary>
        /// <param name="importer">The underlying CSV importer used.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either argument is <c>null</c>.
        /// </exception>
        public LateralSourceFileImporter(ICsvImporter importer, ILog logger)
        {
            Ensure.NotNull(importer, nameof(importer));
            Ensure.NotNull(logger, nameof(logger));

            BoundaryRelationType = BoundaryRelationType.Qt;
            log = logger;
            csvImporter = importer;
        }

        /// <summary>
        /// Imports the functions that are specified in the CSV Data and processes this to a certain data structure
        /// </summary>
        /// <param name="path">A custom path value passed to the base.ImportItem() call. Note: if no <see cref="path"/> is provided,
        /// the FilePath property of the base class is used</param>
        /// <param name="target">not used</param>
        /// <returns>
        /// <para>- CsvImporterMode.<see cref="CsvImporterMode.OneFunction"/>: A single <see cref="DataItem"/> with an <see cref="IFunction"/>;</para>
        /// <para>- CsvImporterMode.<see cref="CsvImporterMode.SeveralFunctionsBasedOnColumns"/>: A <see cref="Folder"/> with multiple <see cref="IFunction"/> Items;</para>
        /// <para>- CsvImporterMode.<see cref="CsvImporterMode.SeveralFunctionsBasedOnDiscriminator"/>: An Error log stating that this import mode is not supported;</para>
        /// </returns>
        public override object ImportItem(string path, object target = null)
        {
            switch (CsvImporterMode)
            {
                case CsvImporterMode.OneFunction:
                {
                    IEnumerable<IFunction> importedSeries = ImportFunctions(path ?? FilePath);
                    return new DataItem(importedSeries.FirstOrDefault())
                    {
                        Name = Path.GetFileNameWithoutExtension(path ?? FilePath)
                    };
                }
                case CsvImporterMode.SeveralFunctionsBasedOnColumns:
                {
                    IEnumerable<IFunction> importedSeries = ImportFunctions(path ?? FilePath);
                    return new Folder
                    {
                        Name = Path.GetFileNameWithoutExtension(path ?? FilePath),
                        Items = new EventedList<IProjectItem>(importedSeries.Select(ts => new DataItem(ts)))
                    };
                }
                case CsvImporterMode.SeveralFunctionsBasedOnDiscriminator:
                default:
                {
                    log.ErrorFormat(Resources.LateralSourceFileImporter_Not_Supported_CsvFileImporterMode_SeveralFunctionsBasedOnDiscriminator);
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Processes the data from the CSV file in to <see cref="IFunction"/> objects
        /// </summary>
        /// <param name="path">A custom path value passed to the base.ImportItem() call. Note: if no <see cref="path"/> is provided,
        /// the FilePath property of the base class is used</param>
        /// <returns>
        /// An <see cref="IEnumerable{IFunction}"/>, which may be empty when CsvImporterMode <see cref="CsvImporterMode.OneFunction"/>
        /// or <see cref="CsvImporterMode.SeveralFunctionsBasedOnColumns"/> is used
        /// </returns>
        public override IEnumerable<IFunction> ImportFunctions(string path = null)
        {
            DataTable dataTable = csvImporter.ImportCsv(path ?? FilePath, CsvMappingData);
            if (ShouldCancel || dataTable.HasErrors)
            {
                log.ErrorFormat(Resources.LateralSourceFileImporter_Import_Cancelled_Or_Error_Occured_On_Import);
                return Enumerable.Empty<IFunction>();
            }

            switch (CsvImporterMode)
            {
                case CsvImporterMode.SeveralFunctionsBasedOnDiscriminator:
                    return ImportSeriesSeveralFunctionsBasedOnDiscriminator(dataTable);
                case CsvImporterMode.OneFunction:
                    return ImportSeriesOneFunction(dataTable);
                case CsvImporterMode.SeveralFunctionsBasedOnColumns:
                    return ImportSeriesSeveralFunctionsBasedOnColumns(dataTable);
                default:
                    throw new NotSupportedException($"Not implemented CSV importer mode: {CsvImporterMode}");
            }
        }

        private IEnumerable<IFunction> ImportSeriesOneFunction(DataTable dataTable)
        {
            string argumentName = GetArgumentName();
            int argumentColumnIndex = dataTable.Columns.IndexOf(argumentName);

            IFunction function = CreateFunction();
            function.Components.Clear();  // Will be added later again. 

            SetArgumentValues(function.Arguments[0], dataTable);

            // Make components. 
            var functionAttributeRowOffset = 0;
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                if (i == argumentColumnIndex)
                {
                    functionAttributeRowOffset--;
                    continue;
                }

                // Assumption: all components are doubles. 
                var newVariable = new Variable<double>(dataTable.Columns[i].ColumnName);
                function.Components.Add(newVariable);
                
                SetComponentValues(newVariable, i, dataTable);
                ConfigureFunctionAttributes(function, i + functionAttributeRowOffset);
            }

            // Yield: one function with one argument and several components. 
            yield return function;
        }

        private IEnumerable<IFunction> ImportSeriesSeveralFunctionsBasedOnColumns(DataTable dataTable)
        {
            string argumentName = GetArgumentName();
            int argumentColumnIndex = dataTable.Columns.IndexOf(argumentName);

            var functionAttributeRowOffset = 0;
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                if (i == argumentColumnIndex)
                {
                    functionAttributeRowOffset--;
                    continue;
                }

                IFunction function = CreateFunction();

                SetArgumentValues(function.Arguments[0], dataTable);
                SetComponentValues(function.Components[0], i, dataTable);
                ConfigureFunctionAttributes(function, i + functionAttributeRowOffset);
                // Yield: one function with one argument and one component. 
                yield return function;
            }
        }

        private void SetArgumentValues(IVariable argument, DataTable table)
        {
            switch (BoundaryRelationType)
            {
                // Make argument
                case BoundaryRelationType.Ht:
                case BoundaryRelationType.Qt:
                    argument.SetValues(GetArgumentValues<DateTime>(time, table));
                    break;
                case BoundaryRelationType.Qh:
                    argument.SetValues(GetArgumentValues<double>(waterLevel, table));
                    break;
                case BoundaryRelationType.Q:
                case BoundaryRelationType.H:
                    // No arguments needed for constant functions. 
                    break;
                default:
                    throw new NotSupportedException("The boundary relation type is not supported: {BoundaryRelationType}");
            }
        }

        private static void SetComponentValues(IVariable component, int i, DataTable table) => 
            // Assumption: all components are doubles. 
            component.SetValues(table.AsEnumerable().Select(row => (double)row[i]));

        private void ConfigureFunctionAttributes(IFunction function, int iFunctionAttributeRow)
        {
            if (function is TimeSeries && 
                FunctionAttributeRows != null && 
                iFunctionAttributeRow < FunctionAttributeRows.Count && 
                FunctionAttributeRows[iFunctionAttributeRow] is IFunctionAttributes attributes)
            {
                attributes.SetToFunction(function);
            }
        }

        private IEnumerable<IFunction> ImportSeriesSeveralFunctionsBasedOnDiscriminator(DataTable dataTable)
        {
            const string featureIdColumn = "Feature ID";
            int discriminatorColumnIndex = dataTable.Columns.IndexOf(featureIdColumn);
            IEnumerable<string> discriminatorValues = dataTable.AsEnumerable().Select(row => row[discriminatorColumnIndex]).Cast<string>().Distinct();

            foreach (string discriminator in discriminatorValues)
            {
                var str = (string)discriminator.Clone();
                string componentName = GetComponentName();
                IFunction function = CreateFunction();
                string argumentName;

                switch (BoundaryRelationType)
                {
                    case BoundaryRelationType.Ht:
                    case BoundaryRelationType.Qt:
                    {
                        argumentName = time;
                        EnumerableRowCollection<Tuple<DateTime, double>> res = from row in dataTable.AsEnumerable()
                                                                               where row.Field<string>(featureIdColumn) == str
                                                                               orderby row.Field<DateTime>(argumentName)
                                                                               select new Tuple<DateTime, double>(row.Field<DateTime>(argumentName), row.Field<double>(componentName));
                        function.Arguments[0].SetValues(res.Select(row => row.Item1));
                        function.Components[0].SetValues(res.Select(row => row.Item2));
                        break;
                    }
                    case BoundaryRelationType.Qh:
                    {
                        argumentName = waterLevel;
                        EnumerableRowCollection<Tuple<double, double>> res = from row in dataTable.AsEnumerable()
                                                                             where row.Field<string>(featureIdColumn) == str
                                                                             orderby row.Field<double>(argumentName)
                                                                             select new Tuple<double, double>(row.Field<double>(argumentName), row.Field<double>(componentName));
                        function.Arguments[0].SetValues(res.Select(row => row.Item1));
                        function.Components[0].SetValues(res.Select(row => row.Item2));
                        break;
                    }
                    case BoundaryRelationType.Q:
                    case BoundaryRelationType.H:
                    default:
                    {
                        EnumerableRowCollection<double> res = from row in dataTable.AsEnumerable()
                                                              where row.Field<string>(featureIdColumn) == str
                                                              select row.Field<double>(componentName);
                        function.Components[0].SetValues(res);
                        break;
                    }
                }
                function.Name = discriminator;   // In order for the receiver of this importer to known which time series belongs to which feature. 

                yield return function;
            }
        }

        private static IEnumerable<T> GetArgumentValues<T>(string argumentName, DataTable dataTable) =>
            from row in dataTable.AsEnumerable()
            orderby row.Field<T>(argumentName)
            select row.Field<T>(argumentName);

        
        // Creates a new Function based on the BoundaryRelationType property
        private IFunction CreateFunction()
        {
            switch (BoundaryRelationType)
            {
                case BoundaryRelationType.Ht:
                {
                    var f = new TimeSeries();
                    f.Components.Add(new Variable<double>(waterLevel, new Unit("m", "m")));
                    f.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevel;
                    f.Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Ht;
                    return f;
                }
                case BoundaryRelationType.Qt:
                {
                    var f = new TimeSeries();
                    f.Components.Add(new Variable<double>(discharge, new Unit("m³/s", "m³/s")));
                    f.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterDischarge;
                    f.Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Qt;
                    return f;
                }
                case BoundaryRelationType.Qh:
                {
                    var f = new Function
                    {
                        Arguments = { new Variable<double>(waterLevel, new Unit("m", "m")) },
                        Components = { new Variable<double>(discharge, new Unit("m³/s", "m³/s")) }
                    };
                    f.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevelTable;
                    f.Attributes[FunctionAttributes.LocationType] = FunctionAttributes.QhBoundary;
                    return f;
                }
                case BoundaryRelationType.H:
                case BoundaryRelationType.Q:
                default:
                {
                    return new Function
                    {
                        Components = { new Variable<double>() }
                    };
                }
            }
        }

        // Creates a Component Name based on the BoundaryRelationType property
        private string GetComponentName()
        {
            switch (BoundaryRelationType)
            {
                case BoundaryRelationType.H:
                case BoundaryRelationType.Ht:
                {
                    return waterLevel;
                }
                case BoundaryRelationType.Q:
                case BoundaryRelationType.Qh:
                case BoundaryRelationType.Qt:
                default:
                {
                    return discharge;
                }
            }
        }
        
        private string GetArgumentName()
        {
            switch (BoundaryRelationType)
            {
                case BoundaryRelationType.Ht:
                case BoundaryRelationType.Qt:
                    return time;
                case BoundaryRelationType.Qh:
                    return waterLevel;
                case BoundaryRelationType.Q:
                case BoundaryRelationType.H:
                default:
                    // Constant H or Q
                    return null;
            }
        }
    }
}