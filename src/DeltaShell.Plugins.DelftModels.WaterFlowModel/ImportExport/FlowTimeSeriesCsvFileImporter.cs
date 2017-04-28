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
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public enum BoundaryRelationType
    {
        Q, QH, QT, H, HT
    }

    // TODO: Better usage of ShouldCancel and ProgressChanged
    public class FlowTimeSeriesCsvFileImporter : TimeSeriesCsvFileImporter
    {
        public FlowTimeSeriesCsvFileImporter()
        {
            BoundaryRelationType = BoundaryRelationType.QT;
        }

        public override object ImportItem(string path, object target = null)
        {
            if (CsvImporterMode == CsvImporterMode.OneFunction)
            {
                var importedSeries = ImportFunctions(path ?? FilePath);
                return new DataItem(importedSeries.FirstOrDefault())
                {
                    Name = Path.GetFileNameWithoutExtension(path ?? FilePath)
                };
            }
            if (CsvImporterMode == CsvImporterMode.SeveralFunctionsBasedOnColumns)
            {
                var importedSeries = ImportFunctions(path ?? FilePath);
                return new Folder
                {
                    Name = Path.GetFileNameWithoutExtension(path ?? FilePath),
                    Items = new EventedList<IProjectItem>(importedSeries.Select(ts => new DataItem(ts)))
                };
            }
            throw new NotSupportedException(String.Format("SeriesCsvFileImporter does not support importer model {0}", CsvImporterMode));
        }

        public BoundaryRelationType BoundaryRelationType { get; set; }

        public override IEnumerable<IFunction> ImportFunctions(string path = null)
        {
            var dataTable = new CsvImporter().ImportCsv(path ?? FilePath, CsvMappingData);
            if (ShouldCancel || dataTable.HasErrors)
            {
                yield break;
            }

            switch (CsvImporterMode)
            {
                case CsvImporterMode.OneFunction:
                    foreach (var f in ImportSeriesOneFunction(dataTable))
                        yield return f;
                    break;
                case CsvImporterMode.SeveralFunctionsBasedOnColumns:
                    foreach (var f in ImportSeriesSeveralFunctionsBasedOnColumns(dataTable))
                        yield return f;
                    break;
                case CsvImporterMode.SeveralFunctionsBasedOnDiscriminator:
                    foreach (var f in ImportSeriesSeveralFunctionsBasedOnDiscriminator(dataTable))
                        yield return f;
                    break;
                default:
                    throw new NotImplementedException(String.Format("Not implemented CSV importer model: {0}", CsvImporterMode));
            }

        }

        private IEnumerable<IFunction> ImportSeriesOneFunction(DataTable dataTable)
        {
            var argumentName = GetArgumentName();
            int argumentColumnIndex = dataTable.Columns.IndexOf(argumentName);

            IFunction function = CreateFunction();
            function.Components.Clear();  // Will be added later again. 

            // Make argument
            if (BoundaryRelationType == BoundaryRelationType.HT || BoundaryRelationType == BoundaryRelationType.QT)
            {
                var res = from row in dataTable.AsEnumerable()
                          orderby row.Field<DateTime>(argumentName)
                          select row.Field<DateTime>(argumentName);
                function.Arguments[0].SetValues(res);
            }
            if (BoundaryRelationType == BoundaryRelationType.QH)
            {
                var res = from row in dataTable.AsEnumerable()
                          orderby row.Field<double>(argumentName)
                          select row.Field<double>(argumentName);
                function.Arguments[0].SetValues(res);
            }

            // No arguments needed for constant functions. 

            // Make components. 
            int functionAttributeRowOffset = 0;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (i == argumentColumnIndex)
                {
                    functionAttributeRowOffset--;
                    continue;
                }

                // Assumption: all components are doubles. 
                var newVariable = new Variable<double>(dataTable.Columns[i].ColumnName);
                function.Components.Add(newVariable);
                newVariable.SetValues(dataTable.AsEnumerable().Select(row => (double)row[i]));

                int iFunctionAttributeRow = i + functionAttributeRowOffset;
                if (function is TimeSeries && FunctionAttributeRows != null && iFunctionAttributeRow < FunctionAttributeRows.Count)
                {
                    var attributes = FunctionAttributeRows[iFunctionAttributeRow] as IFunctionAttributes;
                    if (attributes != null)
                    {
                        attributes.SetToFunction(function);
                    }
                }
            }

            // Yield: one function with one argument and several components. 
            yield return function;
        }

        private IEnumerable<IFunction> ImportSeriesSeveralFunctionsBasedOnColumns(DataTable dataTable)
        {
            var argumentName = GetArgumentName();
            int argumentColumnIndex = dataTable.Columns.IndexOf(argumentName);

            int functionAttributeRowOffset = 0;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (i == argumentColumnIndex)
                {
                    functionAttributeRowOffset--;
                    continue;
                }

                IFunction function = CreateFunction();

                // Make argument
                if (BoundaryRelationType == BoundaryRelationType.HT || BoundaryRelationType == BoundaryRelationType.QT)
                {
                    var res = from row in dataTable.AsEnumerable()
                              orderby row.Field<DateTime>(argumentName)
                              select row.Field<DateTime>(argumentName);
                    function.Arguments[0].SetValues(res);
                }
                else if (BoundaryRelationType == BoundaryRelationType.QH)
                {
                    var res = from row in dataTable.AsEnumerable()
                              orderby row.Field<double>(argumentName)
                              select row.Field<double>(argumentName);
                    function.Arguments[0].SetValues(res);
                }
                // No arguments needed for constant functions. 

                // Assumption: all components are doubles. 
                function.Components[0].SetValues(dataTable.AsEnumerable().Select(row => (double)row[i]));

                int iFunctionAttributeRow = i + functionAttributeRowOffset;
                if (function is TimeSeries && FunctionAttributeRows != null && iFunctionAttributeRow < FunctionAttributeRows.Count)
                {
                    var attributes = FunctionAttributeRows[iFunctionAttributeRow] as IFunctionAttributes;
                    if (attributes != null)
                    {
                        attributes.SetToFunction(function);
                    }
                }

                // Yield: one function with one argument and one component. 
                yield return function;
            }

        }

        private IEnumerable<IFunction> ImportSeriesSeveralFunctionsBasedOnDiscriminator(DataTable dataTable)
        {
            var discriminatorColumnIndex = dataTable.Columns.IndexOf("Feature ID");
            var discriminatorValues = dataTable.AsEnumerable().Select(row => row[discriminatorColumnIndex]).Cast<string>().Distinct();

            foreach (var discriminator in discriminatorValues)
            {
                var str = (string)discriminator.Clone();
                var argumentName = GetArgumentName();
                var componentName = GetComponentName();
                var function = CreateFunction();

                if (BoundaryRelationType == BoundaryRelationType.HT || BoundaryRelationType == BoundaryRelationType.QT)
                {
                    var res = from row in dataTable.AsEnumerable()
                              where row.Field<string>("Feature ID") == str
                              orderby row.Field<DateTime>(argumentName)
                              select new Tuple<DateTime, double>(row.Field<DateTime>(argumentName), row.Field<double>(componentName));
                    function.Arguments[0].SetValues(res.Select(row => row.Item1));
                    function.Components[0].SetValues(res.Select(row => row.Item2));
                }
                else if (BoundaryRelationType == BoundaryRelationType.QH)
                {
                    var res = from row in dataTable.AsEnumerable()
                              where row.Field<string>("Feature ID") == str
                              orderby row.Field<double>(argumentName)
                              select new Tuple<double, double>(row.Field<double>(argumentName), row.Field<double>(componentName));
                    function.Arguments[0].SetValues(res.Select(row => row.Item1));
                    function.Components[0].SetValues(res.Select(row => row.Item2));
                }
                else
                {
                    // Constant Q or H
                    var res = from row in dataTable.AsEnumerable()
                              where row.Field<string>("Feature ID") == str
                              select row.Field<double>(componentName);
                    function.Components[0].SetValues(res);
                }
                function.Name = discriminator;   // In order for the receiver of this importer to known which time series belongs to which feature. 

                yield return function;
            }
        }

        /// <summary>
        /// Creates function based on Boundary relation type. 
        /// </summary>
        /// <returns></returns>
        private IFunction CreateFunction()
        {
            if (BoundaryRelationType == BoundaryRelationType.HT)
            {
                var f = new TimeSeries();
                f.Components.Add(new Variable<double>("Water Level", new Unit("m", "m")));
                f.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevel;
                f.Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Ht;
                return f;
            }
            if (BoundaryRelationType == BoundaryRelationType.QT)
            {
                var f = new TimeSeries();
                f.Components.Add(new Variable<double>("Discharge", new Unit("m³/s", "m³/s")));
                f.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterDischarge;
                f.Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Qt;
                return f;
            }
            if (BoundaryRelationType == BoundaryRelationType.QH)
            {
                var f = new Function
                {
                    Arguments = { new Variable<double>("Water Level", new Unit("m", "m")) },
                    Components = { new Variable<double>("Discharge", new Unit("m³/s", "m³/s")) }
                };
                f.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevelTable;
                f.Attributes[FunctionAttributes.LocationType] = FunctionAttributes.QhBoundary;
                return f;
            }
            if (BoundaryRelationType == BoundaryRelationType.H || BoundaryRelationType == BoundaryRelationType.Q)
            {
                // Independent function. 
                return new Function
                {
                    Components = { new Variable<double>() }
                };
            }

            throw new InvalidOperationException("No suitable boundary relation type found.");
        }

        private string GetArgumentName()
        {
            if (BoundaryRelationType == BoundaryRelationType.HT || BoundaryRelationType == BoundaryRelationType.QT)
            {
                return "Time";
            }

            if (BoundaryRelationType == BoundaryRelationType.QH)
            {
                return "Water Level";
            }

            // Constant H or Q
            return null;
        }

        private string GetComponentName()
        {
            if (BoundaryRelationType == BoundaryRelationType.H || BoundaryRelationType == BoundaryRelationType.HT)
            {
                return "Water Level";
            }

            // Q, Q(t), Q(h)
            return "Discharge";
        }
    }
}
