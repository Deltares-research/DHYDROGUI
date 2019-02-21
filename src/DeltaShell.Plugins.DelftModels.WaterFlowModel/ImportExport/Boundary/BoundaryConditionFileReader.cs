using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// BoundaryConditionFileReader is responsible for parsing, extracting and
    /// adding the meteo, wind, boundary conditions and lateral discharges to
    /// their respective parameters. Any errors encountered are reported
    /// through createAndAddErrorReport
    /// </summary>
    public class BoundaryConditionFileReader
    {
        /// <summary>
        /// Construct a new BoundaryConditionFileReader with an empty
        /// createAndAddErrorReport.
        /// </summary>
        public BoundaryConditionFileReader() : this(null) { }

        /// <summary>
        /// Construct a new BoundaryConditionFileReader with the specified
        /// craeteAndAddErrorReport.
        /// </summary>
        /// <param name="createAndAddErrorReport"> The action used to report any errors encountered. </param>
        public BoundaryConditionFileReader(Action<string, IList<string>> createAndAddErrorReport) : this(DelftBcFileParser.ReadFile,
                                                                                                         MeteoDataConverter.Convert,
                                                                                                         WindDataConverter.Convert,
                                                                                                         BoundaryConditionConverter.Convert,
                                                                                                         LateralDischargeConverter.Convert, 
                                                                                                         createAndAddErrorReport)
        { }

        /// <summary>
        /// Construct a new BoundaryConditionFileReader with the specified functions.
        /// </summary>
        /// <param name="parseFunc"> The function used for parsing the BoundaryCondition.bc file. </param>
        /// <param name="meteoConvertFunc"> The function used to extract the meteo function. </param>
        /// <param name="windConvertFunc"> The function used to extract the wind function. </param>
        /// <param name="boundaryConvertFunc"> The function used to extract the BoundaryConditions. </param>
        /// <param name="lateralConvertFunc"> The function used to extract the LateralConditions. </param>
        /// <param name="createAndAddErrorReport"> The action used to report any errors encountered. </param>
        /// <exception cref="ArgumentException"> parseFunc == null || meteoConvertFunc == null || windConvertFunc == null || boundaryConvertFunc == null || lateralConvertFunc == null </exception>
        public BoundaryConditionFileReader(Func<string, IList<IDelftBcCategory>> parseFunc, 
                                           Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> meteoConvertFunc,
                                           Func<IList<IDelftBcCategory>, IList<string>, WindFunction> windConvertFunc,
                                           Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> boundaryConvertFunc,
                                           Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> lateralConvertFunc,
                                           Action<string, IList<string>> createAndAddErrorReport)
        {
            if (parseFunc != null) this.parseFunc = parseFunc;
            else throw new ArgumentException("Parser cannot be null.");

            if (meteoConvertFunc == null || windConvertFunc == null || boundaryConvertFunc == null || lateralConvertFunc == null)
                throw new ArgumentException("Converter cannot be null.");

            this.meteoConvertFunc = meteoConvertFunc;
            this.windConvertFunc = windConvertFunc;
            this.boundaryConvertFunc = boundaryConvertFunc;
            this.lateralConvertFunc = lateralConvertFunc;
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        /// <summary>
        /// Read the BoundaryCondition file at <paramref name="filePath"/> and store it in the provided parameters.
        /// </summary>
        /// <param name="filePath"> Path to the BoundaryConditions file. </param>
        /// <param name="meteoFunction"> meteoFunction to store meteo data in. </param>
        /// <param name="windFunction"> windFunction to store wind data in. </param>
        /// <param name="boundaryNodes"> set of boundaryNodes to add the BoundaryConditions too. </param>
        /// <param name="lateralNodes"> set of lateralNodes to add LateralDischarge too </param>
        public void Read(string filePath,
                         MeteoFunction meteoFunction,
                         WindFunction windFunction,
                         IEventedList<WaterFlowModel1DBoundaryNodeData> boundaryNodes,
                         IEventedList<WaterFlowModel1DLateralSourceData> lateralNodes)
        {
            var errorMessagesParse = new List<string>();
            IList<IDelftBcCategory> bcCategories;
            try
            {
                bcCategories = parseFunc.Invoke(filePath);
            }
            catch (Exception e)
            {
                errorMessagesParse.Add(e.Message);
                createAndAddErrorReport?.Invoke("While reading the boundary locations from file, the following errors occured:", errorMessagesParse);
                return;
            }

            AddMeteoData(bcCategories, meteoFunction);
            AddWindData(bcCategories, windFunction);
            AddBoundaryConditionData(bcCategories, boundaryNodes);
            AddLateralDischargeData(bcCategories, lateralNodes);
        }

        private void AddMeteoData(IList<IDelftBcCategory> bcCategories, MeteoFunction modelMeteoData)
        {
            var errorMessagesMeteoData = new List<string>();
            var meteoData = meteoConvertFunc.Invoke(bcCategories, errorMessagesMeteoData);

            if (meteoData != null)
            {
                modelMeteoData.Arguments[0].SetValues(meteoData.Arguments[0].Values);
                modelMeteoData.AirTemperature.SetValues(meteoData.AirTemperature.Values);
                modelMeteoData.Cloudiness.SetValues(meteoData.Cloudiness.Values);
                modelMeteoData.RelativeHumidity.SetValues(meteoData.RelativeHumidity.Values);
            }

            if (errorMessagesMeteoData.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the model wide meteo data from file, the following errors occured:", errorMessagesMeteoData);
        }

        private void AddWindData(IList<IDelftBcCategory> bcCategories, WindFunction modelWind)
        {
            var errorMessagesMeteoData = new List<string>();
            var windData = windConvertFunc.Invoke(bcCategories, errorMessagesMeteoData);

            if (windData != null)
            {
                modelWind.Arguments[0].SetValues(windData.Arguments[0].Values);
                modelWind.Direction.SetValues(windData.Direction.Values);
                modelWind.Velocity.SetValues(windData.Velocity.Values);
            }

            if (errorMessagesMeteoData.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the model wide wind data from file, the following errors occured:", errorMessagesMeteoData);

        }

        private void AddBoundaryConditionData(IList<IDelftBcCategory> bcCategories,
                                              IEventedList<WaterFlowModel1DBoundaryNodeData> boundaryNodes)
        {
            var errorMessagesBoundary = new List<string>();
            var boundaryConditionData = boundaryConvertFunc.Invoke(bcCategories, errorMessagesBoundary);
            if (errorMessagesBoundary.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the boundary condition data from file, the following errors occured:", errorMessagesBoundary);


            foreach (var boundaryNode in boundaryNodes)
            {
                if (!boundaryConditionData.ContainsKey(boundaryNode.Feature.Name)) continue;

                var nodeData = boundaryConditionData[boundaryNode.Feature.Name];
                if (nodeData.WaterComponent != null)
                {
                    boundaryNode.DataType = nodeData.WaterComponent.BoundaryType;

                    switch (boundaryNode.DataType)
                    {
                        case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                            boundaryNode.Flow = nodeData.WaterComponent.ConstantBoundaryValue;
                            break;
                        case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                            boundaryNode.WaterLevel = nodeData.WaterComponent.ConstantBoundaryValue;
                            break;
                        case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                        case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                        case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                            CopyFunction(nodeData.WaterComponent.TimeDependentBoundaryValue, boundaryNode.Data);
                            break;
                    }
                }

                if (nodeData.SaltComponent != null)
                {
                    boundaryNode.SaltConditionType = nodeData.SaltComponent.BoundaryType;

                    switch (boundaryNode.SaltConditionType)
                    {
                        case SaltBoundaryConditionType.Constant:
                            boundaryNode.SaltConcentrationConstant = nodeData.SaltComponent.ConstantBoundaryValue;
                            break;
                        case SaltBoundaryConditionType.TimeDependent:
                            CopyFunction(nodeData.SaltComponent.TimeDependentBoundaryValue, boundaryNode.SaltConcentrationTimeSeries);
                            break;
                        case SaltBoundaryConditionType.None:
                            break;
                    }
                }

                if (nodeData.TemperatureComponent != null)
                {
                    boundaryNode.TemperatureConditionType = nodeData.TemperatureComponent.BoundaryType;

                    switch (boundaryNode.TemperatureConditionType)
                    {
                        case TemperatureBoundaryConditionType.Constant:
                            boundaryNode.TemperatureConstant = nodeData.TemperatureComponent.ConstantBoundaryValue;
                            break;
                        case TemperatureBoundaryConditionType.TimeDependent:
                            CopyFunction(nodeData.TemperatureComponent.TimeDependentBoundaryValue, boundaryNode.TemperatureTimeSeries);
                            break;
                        case TemperatureBoundaryConditionType.None:
                            break;
                    }
                }
            }
        }

        private void AddLateralDischargeData(IList<IDelftBcCategory> bcCategories,
                                             IEventedList<WaterFlowModel1DLateralSourceData> lateralNodes)
        {
            var errorMessagesBoundary = new List<string>();
            var lateralDischargeData = lateralConvertFunc.Invoke(bcCategories, errorMessagesBoundary);
            if (errorMessagesBoundary.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the lateral discharge data from file, the following errors occured:", errorMessagesBoundary);


            foreach (var lateralNode in lateralNodes)
            {
                if (!lateralDischargeData.ContainsKey(lateralNode.Feature.Name)) continue;

                var nodeData = lateralDischargeData[lateralNode.Feature.Name];
                if (nodeData.WaterComponent != null)
                {
                    lateralNode.DataType = nodeData.WaterComponent.BoundaryType;

                    switch (lateralNode.DataType)
                    {
                        case WaterFlowModel1DLateralDataType.FlowConstant:
                            lateralNode.Flow = nodeData.WaterComponent.ConstantBoundaryValue;
                            break;
                        case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                        case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                            CopyFunction(nodeData.WaterComponent.TimeDependentBoundaryValue, lateralNode.Data);
                            break;
                    }
                }

                if (nodeData.SaltComponent != null)
                {
                    lateralNode.SaltLateralDischargeType = nodeData.SaltComponent.BoundaryType;

                    switch (lateralNode.SaltLateralDischargeType)
                    {
                        case SaltLateralDischargeType.ConcentrationConstant:
                            lateralNode.SaltConcentrationDischargeConstant = nodeData.SaltComponent.ConstantBoundaryValue;
                            break;
                        case SaltLateralDischargeType.ConcentrationTimeSeries:
                            CopyFunction(nodeData.SaltComponent.TimeDependentBoundaryValue, lateralNode.SaltConcentrationTimeSeries);
                            break;
                        case SaltLateralDischargeType.MassConstant:
                            lateralNode.SaltMassDischargeConstant = nodeData.SaltComponent.ConstantBoundaryValue;
                            break;
                        case SaltLateralDischargeType.MassTimeSeries:
                            CopyFunction(nodeData.SaltComponent.TimeDependentBoundaryValue, lateralNode.SaltMassTimeSeries);
                            break;
                        case SaltLateralDischargeType.Default:
                            break;
                    }
                }

                if (nodeData.TemperatureComponent != null)
                {
                    lateralNode.TemperatureLateralDischargeType = nodeData.TemperatureComponent.BoundaryType;

                    switch (lateralNode.TemperatureLateralDischargeType)
                    {
                        case TemperatureLateralDischargeType.Constant:
                            lateralNode.TemperatureConstant = nodeData.TemperatureComponent.ConstantBoundaryValue;
                            break;
                        case TemperatureLateralDischargeType.TimeDependent:
                            CopyFunction(nodeData.TemperatureComponent.TimeDependentBoundaryValue, lateralNode.TemperatureTimeSeries);
                            break;
                        case TemperatureLateralDischargeType.None:
                            break;
                    }
                }
            }
        }

        private static void CopyFunction(IFunction from, IFunction to)
        {
            if (from.Arguments.Count != to.Arguments.Count || from.Components.Count != to.Components.Count)
                return;

            for (var i = 0; i < from.Arguments.Count; i++)
            {
                to.Arguments[i].SetValues(from.Arguments[i].Values);
                to.Arguments[i].ExtrapolationType = from.Arguments[i].ExtrapolationType;
                to.Arguments[i].InterpolationType = from.Arguments[i].InterpolationType;
            }

            for (var i = 0; i < from.Components.Count; i++)
            {
                to.Components[i].SetValues(from.Components[i].Values);
                to.Components[i].ExtrapolationType = from.Components[i].ExtrapolationType;
                to.Components[i].InterpolationType = from.Components[i].InterpolationType;
            }

            to.SetInterpolationType(from.GetInterpolationType());
            to.SetExtrapolationType(from.GetExtrapolationType());
            to.SetPeriodicity(from.HasPeriodicity());
        }

        private readonly Func<string, IList<IDelftBcCategory>> parseFunc;
        private readonly Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> meteoConvertFunc;
        private readonly Func<IList<IDelftBcCategory>, IList<string>, WindFunction> windConvertFunc;
        private readonly Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> boundaryConvertFunc;
        private readonly Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> lateralConvertFunc;
        private readonly Action<string, IList<string>> createAndAddErrorReport;
    }
}
