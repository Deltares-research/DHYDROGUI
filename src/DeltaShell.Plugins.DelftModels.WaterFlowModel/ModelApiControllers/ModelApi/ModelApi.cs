using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi
{
    public enum FrictionFunctionType
    {
        Constant = 1,
        TabbedDischarge = 2,
        TabbedLevel = 3,
    }

    public enum BoundaryType
    {
        None = 0,
        Level = 1,
        Discharge,
        Salinity,
        WindVelocity,
        WindDirection,
        AirTemperature,
        RelativeHumidity,
        Cloudiness
    }

    // The parameters supported by ModelApi; always check with availablity of QuantityType per ElementSet!
    public enum QuantityType
    {
        WaterLevel = 1, // BranchNodes, GridpointsOnBranches, HBoundaries, QBoundaries, Measurements
        WaterDepth = 2, // BranchNodes, GridpointsOnBranches, Measurements
        BottomLevel = 3, // BranchNodes, GridpointsOnBranches
        SurfaceArea = 4, // BranchNodes, GridpointsOnBranches, Measurements
        Volume = 5, // BranchNodes, GridpointsOnBranches
        Salinity = 6, // BranchNodes, GridpointsOnBranches
        Dispersion = 7, // ?
        Discharge = 8, // BranchLinks, ReachSegElmSet, Structures, HBoundaries, QBoundaries, Measurements
        Velocity = 9, // BranchLinks, ReachSegElmSet, Structures
        FlowArea = 10, // BranchLinks, ReachSegElmSet, Structures
        FlowPerimeter = 11, // BranchLinks
        FlowHydrad = 12, // BranchLinks
        FlowConv = 13, // BranchLinks
        FlowChezy = 14, // BranchLinks
        TotalArea = 15, // ?
        TotalWidth = 16, // BranchLinks
        Hyddepth = 17, // BranchLinks
        CrestLevel = 18, // Structures
        CrestWidth = 19, // Structures
        GateLowerEdgeLevel = 20, // Structures
        GateOpeningHeight = 21, // Structures
        ValveOpening = 22, // ?
        WaterlevelUp = 23, // Structures, Pumps
        WaterlevelDown = 24, // Structures, Pumps
        Head = 25, // Structures, Pumps
        PressureDifference = 26, // Structures
        PumpCapacity = 27, // Pumps
        Flux = 28, // 
        Setpoint = 29, // 
        Xcor = 30, // 
        Ycor = 31, // 
        WindShield = 32, // 
        // Flow analysis quantities
        NoIteration = 33, // BranchNodes, GridpointsOnBranches
        NegativeDepth = 34, // BranchNodes, GridpointsOnBranches
        TimeStepEstimation = 35, // ReachSegElmSet
        // New entities 
        WaterLevelAtCrest = 36, // 
        WaterLevelGradient = 37,
        Froude = 38,
        DischargeMain = 39,
        DischargeFP1 = 40,
        DischargeFP2 = 41,
        ChezyMain = 42,
        ChezyFP1 = 43,
        ChezyFP2 = 44,
        AreaMain = 45,
        AreaFP1 = 46,
        AreaFP2 = 47,
        WidthMain = 48,
        WidthFP1 = 49,
        WidthFP2 = 50,
        HydradMain = 51,
        HydradFP1 = 52,
        HydradFP2 = 53,
        Length = 54, //length of segments
        QLat = 55, //QLat (fluxes: 0 -> many) per branch 
        FiniteVolumeGridIndex = 56, //delwaq segment id
        LateralIndex = 57, //lateral source index
        FiniteGridType = 58, //Not supported in Model Api, but needed for the GUI
        DischargeDemanded = 59, // Discharge demanded on lateral source
        TH_F1 = 60, // Thatcher-Harleman coefficient F1
        TH_F3 = 61, // Thatcher-Harleman coefficient F3
        TH_F4 = 62, // Thatcher-Harleman coefficient F4
        Density = 63, // Density salt
        LateralDefined = 64, // Defined lateral discharge
        LateralDifference = 65, // difference between realised and defined lateral discharge
        EnergyLevels = 66, // energy levels on reach segments
        BalBoundariesIn = 67,
        BalBoundariesOut = 68,
        BalBoundariesTot = 69,
        BalError = 70,
        BalLatIn = 71,
        BalLatOut = 72,
        BalLatTot = 73,
        BalStorage = 74,
        BalVolume = 75,
        CrossLevels = 76,
        CrossFlowWidths = 77,
        CrossTotalWidths = 78,
        QZeta_1D2D = 79, // input coefficient for water level dependend flow
        QLat_1D2D = 80, // input coefficient for 1d2d lateral flow
        QTotal_1d2d = 81, // Result of Qzeta_1d2d * s1 - Qlat_1d2d
        Bal2d1dIn = 82,
        Bal2d1dOut = 83,
        Bal2d1dTot = 84,
        LateralAtNodes = 85,
        PumpDischarge = 86,
        SuctionSideLevel = 87,
        DeliverySideLevel = 88,
        PumpHead = 89,
        ActualPumpStage = 90,
        ReductionFactor = 91,
        Temperature = 92,
        TotalHeatFlux = 93,
        RadFluxClearSky = 94,
        HeatLossConv = 95,
        NetSolarRad = 96,
        EffectiveBackRad = 97,
        HeatLossEvap = 98,
        HeatLossForcedEvap = 99,
        HeatLossFreeEvap = 100,
        HeatLossForcedConv = 101,
        HeatLossFreeConv = 102
    }

    // The ElementSets supported by ModelApi; always chaeck with availablity of QuantityType per ElementSet!
    public enum ElementSet
    {
        BranchNodes = 1,
        //BranchLinks = 2,
        //UniqueNodesElementSet = 3,
        GridpointsOnBranches = 4,
        ReachSegElmSet = 5,
        //Structures = 6, // (no pumps)
        Pumps = 7,
        //Controllers = 8,
        HBoundaries = 9,
        QBoundaries = 10,
        Laterals = 11,
        Branches = 13,
        Observations = 14,
        Structures = 15,
        Retentions = 16,
        FiniteVolumeGridOnReachSegments = 17,
        FiniteVolumeGridOnGridPoints = 18,
        LateralsOnReachSegments = 19,
        LateralsOnGridPoints = 20,
        ModelWide = 21,
        CrossSection = 22
    }

    // availablity of QuantityType per ElementSet from ExchangeItems.f90
    // ElementSet::BranchNodes
    //    Waterlevel
    //    WaterDepth
    //    BottomLevel
    //    SurfaceArea
    //    Volume
    //    Salinity
    //    NoIteration
    //    NegativeDepth
    // ElementSet::BranchLinks
    //    Discharge
    //    Hyddepth
    //    Velocity
    //    Flowarea
    //    Flowperi
    //    Flowhydrad
    //    Flowconv
    //    Flowchezy
    //    Totalwidth
    // ElementSet::UniqueNodesElementSet
    // ElementSet::GridpointsOnBranches
    //    Waterlevel
    //    WaterDepth
    //    BottomLevel
    //    SurfaceArea
    //    Volume
    //    Salinity
    //    NoIteration
    //    NegativeDepth
    // ElementSet::ReachSegElmSet
    //    Discharge
    //    Velocity
    //    TimeStepEstimation
    //    Flowarea
    // ElementSet::Structures
    //    CrestLevel
    //    Crestwidth
    //    GateLowerEdgeLevel
    //    GateOpeningHeight
    //    Flowarea
    //    Discharge
    //    Velocity
    //    Head
    //    Pressuredifference
    //    Waterlevelup
    //    Waterleveldown
    // ElementSet::Pumps
    //    PumpCapacity
    //    Head
    //    Waterlevelup
    //    Waterleveldown
    // ElementSet::Controllers
    // ElementSet::HBoundaries
    //    Discharge
    //    Waterlevel
    // ElementSet::QBoundaries 
    //    Discharge
    //    Waterlevel
    // ElementSet::Laterals
    //    Discharge
    // ElementSet::Measurements
    //    Waterlevel
    //    Discharge
    //    WaterDepth
    //    SurfaceArea
    // ElementSet::Branches

    public enum ErrorLevels
    {
        Debug = 1,
        Info,
        Warning,
        Error,
        Fatal
    }

    public enum AggregationOptions // NOTE: this is flow model output type + aggregation option in one!
    {
        None = 0,
        Maximum = 1,
        Minimum = 2,
        Average = 3,
        Current = 4
    }

    public enum WaqVolType
    {
        VolumesOnReachSegments = 1,
        VolumesOnGridPoints = 2,
    }

    public class ModelApi : MarshalByRefObject, IModelApi
    {
        private IList<ModelApiParameter> Parameters;
        //logging
        private static readonly ILog Log = LogManager.GetLogger(typeof (ModelApi));
        private bool loggingEnabled = true;

        
        private const int MAXSTRLEN = 1024;
        private const int MAXDIMS = 6;

        static ModelApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(Flow1DApiDll.CF_DLL_NAME, DimrApiDataSet.CfDllPath);
        }

        public ModelApi()
        {
            Parameters = new List<ModelApiParameter>();
            LogMessages();
        }

        #region Logging

        public bool LoggingEnabled
        {
            get { return loggingEnabled; }
            set { loggingEnabled = value; }
        }

        /// <summary>
        /// Gets all log messages from the model engine.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllLogMessages()
        {
            List<string> messages = new List<string>();

            if (Flow1DApiDll.MessageCnt() > 0)
            {
                int _length = 1;
                StringBuilder message = new StringBuilder("") { Length = _length };
                message.Length = 200;
                int ilevel;
                for (int i = 1; i <= Flow1DApiDll.MessageCnt(); i++)
                {
                    int length = message.Length;
                    ilevel = Flow1DApiDll.getMessage(ref i, message, ref length);
                    messages.Add(message.ToString());
                }
            }

            return messages.ToArray();
        }

        public int MessageCount()
        {
            return Flow1DApiDll.MessageCnt();
        }

        public string GetMessage(int messageId, out ErrorLevels errorLevel)
        {
            int _messageId = messageId;
            int _length = 200;
            StringBuilder message = new StringBuilder("") { Length = _length };
            errorLevel = (ErrorLevels)Flow1DApiDll.getMessage(ref _messageId, message, ref _length);
            return message.ToString();
        }

        public void LogMessages()
        {
            if (!LoggingEnabled)
            {
                return;
            }
            if (Flow1DApiDll.MessageCnt() > 0)
            {
                var message = new StringBuilder("") { Length = 200 };

                for (int i = 1; i <= Flow1DApiDll.MessageCnt(); i++)
                {
                    int length = message.Length;
                    int ilevel = Flow1DApiDll.getMessage(ref i, message, ref length);

                    var errorLevel = (ErrorLevels)ilevel;

                    string mess = message.ToString();
                    char[] kars = { ' ' };
                    mess = mess.TrimEnd(kars);

                    switch (errorLevel)
                    {
                        case ErrorLevels.Debug:
                            Log.Debug(mess);
                            break;
                        case ErrorLevels.Info:
                            Log.Debug(mess); // info messages of modelAPI are debug messages for users
                            break;
                        case ErrorLevels.Warning:
                            Log.Warn(mess);
                            break;
                        case ErrorLevels.Error:
                            Log.Error(mess);
                            break;
                        case ErrorLevels.Fatal:
                            Log.Fatal(mess);
                            break;
                        default:
                            Log.Debug(message);
                            break;
                    }
                }
            }
            ResetMessageCount();
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "resetMessageCount", CallingConvention = CallingConvention.Cdecl)]
        private static extern int resetMessageCount_();

        public int ResetMessageCount()
        {
            return resetMessageCount_();
        }

        #endregion

        #region Other from interface

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "SetMissingValue", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SetMissingValue([In] ref double missingValue);

        public bool SetMissingValue(double missingValue)
        {
            // Turn off.
            return true; 
            
            return SetMissingValue(ref missingValue);
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "WriteWaqOutput", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool WriteWaqOutput([In] ref int waqVolType);

        public void WriteWaqOutput(WaqVolType volType)
        {
            // Turn off. 
            return; 
            
            var volTypeInt = (int) volType;
            WriteWaqOutput(ref volTypeInt);
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "ReadFiles", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReadFiles_([In] String filename);

        public void ReadFiles(string filename)
        {
            ReadFiles_(filename);
            LogMessages();
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "SE_GETVALUESBYINTID", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SE_GetValuesByIntId(
            [In] string componentId,
            [In] string schemId,
            [In] ref int valueId,
            [In] ref int elementsetId,
            [In] ref int ivalues,
            [In, Out] double[] values,
            [In] ref int location,
            int a, int b);

        public int SetStatisticalOutput(ElementSet elementsetId, QuantityType quantityId,
            AggregationOptions operation, double outputInterval)
        {
            int elementsetId_ = (int)elementsetId;
            int quantityId_ = (int)quantityId;
            int operation_ = (int)operation;
            double outputInterval_ = outputInterval;
            int retval = 0;
            if (quantityId != QuantityType.FiniteGridType)
            {
                retval = Flow1DApiDll.SetStatisticalOutput(ref elementsetId_, ref quantityId_, ref operation_, ref outputInterval_);
            }
            return retval;
        }

        public double[] GetStatisticalOutput(ElementSet elementsetId, QuantityType quantityId,
            AggregationOptions operation, double outputTimeStep)
        {
            int elementsetId_ = (int)elementsetId;
            int quantityId_ = (int)quantityId;
            int operation_ = (int)operation;
            double outputInterval_ = outputTimeStep;
            int index = Flow1DApiDll.GetStatisticalOutputIndex(ref elementsetId_, ref quantityId_, ref operation_,
                ref outputInterval_);
            int count = Flow1DApiDll.GetStatisticalOutputSize(ref index);
            double[] values = new double[count];
            int retval = Flow1DApiDll.GetStatisticalOutput(ref index, values, ref count);
            return values;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetSizeBranch", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSizeBranch([In] ref int ElementSetId, [In] ref int location);

        public int GetSizeBranch(ElementSet elmSet, int location)
        {
            int elementSet = (int)elmSet;
            var count = GetSizeBranch(ref elementSet, ref location);
            return count;
        }

        // dedicated function for retrieving data on a  selected branch, location index, alwasy works on BranchNodes element set
        public double[] GetValues(QuantityType quantity, ElementSet elmSet, int gridlocation)
        {

            int elementSet = (int)elmSet;
            var levelCount = GetSizeBranch(ref elementSet, ref gridlocation);
            if (levelCount < 0)
                throw new Exception("GetSizeBranch of elementset " + elementSet + " returned " + levelCount);
            if (levelCount == 0)
                return null;

            var values = new double[levelCount];
            const string componentId = "CF";
            const string schemId = "sobeksim.fnm";

            int quant = (int)quantity;
            int numValues = values.Length;
            int loc_ = gridlocation;

            //TODO add extra getter / setter
            SE_GetValuesByIntId(componentId, schemId, ref quant, ref elementSet, ref numValues, values, ref loc_,
                componentId.Length, schemId.Length);
            return values;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "SetValuesOnGridPoint", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetValues_([In] ref int quantityId, [In] ref int ElementSetId, [In] ref int location,
            [In] double[] values, [In] ref int valuesCount);

        // TODO to be replaced by BMI get_Var
        public double[] GetValues(QuantityType quantity, ElementSet elmSet) // outputType (min/max/avg/current)
        {
            //Default is the elementset containing the gridpoints
            const string componentId = "CF";
            const string schemId = "sobeksim.fnm";

            var pointsCount = GetSize(elmSet);
            if (pointsCount < 0)
                throw new Exception("GetSize of elementset " + elmSet + " returned " + pointsCount);
            if (pointsCount == 0)
                return null;

            var values = new double[pointsCount];
            int elementSet = (int)elmSet;
            int quant = (int)quantity;
            int numValues = values.Length;
            int loc_ = -1;
            SE_GetValuesByIntId(componentId, schemId, ref quant, ref elementSet, ref numValues, values, ref loc_,
                componentId.Length, schemId.Length);
            LogMessages();
            return values;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetModelApiValue", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetValue([In] ref int quantityId, [In] ref int ElementSetId, [In] ref int location);

        // TODO to be replaced by BMI get_Var
        public double GetValue(QuantityType quantity, ElementSet elmSet, int location)
        {
            int quantityId = (int)quantity;
            int elmSetId = (int)elmSet;
            int loc_ = location;
            double val = GetValue(ref quantityId, ref elmSetId, ref loc_);
            return val;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSize([In] ref int ielmSet);

        // TODO to be replaced by BMI get_Var
        public int GetSize(ElementSet ielmSet)
        {
            LogMessages();
            int elmSet = (int)ielmSet;
            return GetSize(ref elmSet);
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "SetValues", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetValues_([In] ref int type, [In] double[] values, int a);

        public void SetValues(QuantityType type, double[] values)
        {
            var iType = (int)type;
            SetValues_(ref iType, values, type.ToString().Length);
        }

        public void setParameter(ModelApiParameter parameter)
        {
            //remove existing ..if any
            var existingParameter =
                Parameters.FirstOrDefault(p => p.Name == parameter.Name && p.Category == parameter.Category);
            if (existingParameter != null)
            {
                Parameters.Remove(existingParameter);
            }

            Parameters.Add(parameter);
        }

        public void get_var(string variable, ref Array array)
        {
            IntPtr ptr = IntPtr.Zero;
            Flow1DApiDll.get_var(variable, ref ptr);

            if (ptr == IntPtr.Zero)
            {
                return;
            }

            // get rank
            int rank;
            get_var_rank(variable, out rank);

            // get shape
            int[] shape = new int[MAXDIMS];
            get_var_shape(variable, ref shape);
            shape = shape.Take(rank).ToArray();

            // get value type
            string typeName;
            get_var_type(variable, out typeName);

            // copy to 1D array
            var totalLength = GetTotalLength(shape);

            var values1D = ToArray1D(ptr, typeName, totalLength);

            if (rank == 1)
            {
                array = values1D;
            }
            else
            {
                throw new NotImplementedException("only rank 1");
            }
        }

        private static int GetTotalLength(int[] shape)
        {
            return shape.Aggregate(1, (current, t) => current * t);
        }

        private Array ToArray1D(IntPtr ptr, string valueType, int totalLength)
        {
            if (valueType == "double")
            {
                var values = new double[totalLength];
                Marshal.Copy(ptr, values, 0, totalLength);
                return values;
            }

            if (valueType == "int")
            {
                var values = new int[totalLength];
                Marshal.Copy(ptr, values, 0, totalLength);
                return values;
            }

            throw new NotSupportedException("Unsupported type: " + valueType);
        }

        public void get_var_shape(string variable, ref int[] shape)
        {
            Flow1DApiDll.get_var_shape(variable, shape);
        }

        public void get_var_rank(string variable, out int rank)
        {
            Flow1DApiDll.get_var_rank(variable, out rank);
        }

        public void get_var_type(string variable, out string value)
        {
            StringBuilder builder = new StringBuilder(MAXSTRLEN);
            Flow1DApiDll.get_var_type(variable, builder);

            value = builder.ToString();
        }

        public void set_var(string variable, double[] values)
        {
            Flow1DApiDll.set_var(variable, values);
        }

        public void initialize(string path)
        {
            Flow1DApiDll.initialize(path);
        }
        #endregion

        #region Other not interface

        public static StringBuilder Str2Builder(string[] names, int len)
        {
            var ids = new StringBuilder(names[0].PadRight(len));
            for (int i = 1; i < names.Length; i++)
            {
                ids.Append(names[i].PadRight(len));
            }
            return ids;
        }

        #endregion

        #region Network

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkSetTabCrossSection", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NetworkSetTabCrossSection([In] ref int levelCount, [In] double[] levels,
            [In] double[] flowWidth, [In] double[] totalWidth, [In] double[] plains, [In] ref double levelCrest,
            [In] ref double levelBottom, [In] ref double flowArea, [In] ref double totalArea, [In] ref bool closed,
            [In] ref bool groundlayerUsed, [In] ref double groundlayer);

        public int NetworkSetTabCrossSection([In] double[] levels, [In] double[] flowWidth,
            [In] double[] totalWidth, [In] double[] plains, [In] bool closed,
            [In] bool groundlayerUsed, [In] double groundlayer)
        {
            int levelCount = levels.Length;
            bool closed_ = closed;
            double dummy = 0.0;
            bool groundlayerUsed_ = groundlayerUsed;
            double groundlayer_ = groundlayer;
            int res = NetworkSetTabCrossSection(ref levelCount, levels, flowWidth, totalWidth, plains, ref dummy,
                ref dummy, ref dummy, ref dummy, ref closed_, ref groundlayerUsed_, ref groundlayer_);
            LogMessages();
            return res;
        }

        public int NetworkSetTabCrossSection([In] double[] levels, [In] double[] flowWidth,
            [In] double[] totalWidth, [In] double[] plains, [In] double levelCrest,
            [In] double levelBottom, [In] double flowArea, [In] double totalArea,
            [In] bool closed, [In] bool groundlayerUsed, [In] double groundlayer)
        {
            int levelCount = levels.Length;
            bool closed_ = closed;
            double levelCrest_ = levelCrest;
            double levelBottom_ = levelBottom;
            double flowArea_ = flowArea;
            double totalArea_ = totalArea;
            double groundlayer_ = groundlayer;
            bool groundlayerUsed_ = groundlayerUsed;
            int res = NetworkSetTabCrossSection(ref levelCount, levels, flowWidth, totalWidth, plains, ref levelCrest_,
                ref levelBottom_, ref flowArea_, ref totalArea_, ref closed_, ref groundlayerUsed_, ref groundlayer_);
            LogMessages();
            return res;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkSetYZCrossSection", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NetworkSetYZCrossSection([In] ref int count, [In] double[] y, [In] double[] z,
            [In] ref int frictionCount, [In] double[] frictionSectionFrom, [In] double[] frictionSectionTo,
            [In] int[] frictionTypePos, [In] double[] frictionValuePos, [In] int[] frictionTypeNeg,
            [In] double[] frictionValueNeg, [In] ref int levelsCount, [In] double[] storageLevels,
            [In] double[] storage);

        public int NetworkSetYZCrossSection(double[] y, double[] z, double[] frictionSectionFrom,
            double[] frictionSectionTo,
            int[] frictionTypePos, double[] frictionValuePos, int[] frictionTypeNeg, double[] frictionValueNeg,
            double[] storageLevels, double[] storage)
        {
            int count = y.Length;
            int frictionCount = frictionSectionFrom.Length;
            int levelsCount = storage.Length;
            int res = NetworkSetYZCrossSection(ref count, y, z, ref frictionCount, frictionSectionFrom,
                frictionSectionTo,
                frictionTypePos, frictionValuePos, frictionTypeNeg, frictionValueNeg, ref levelsCount,
                storageLevels, storage);
            LogMessages();
            return res;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkAddStorage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NetworkAddStorage([In] string id, int len);

        public int NetworkAddStorage(string id)
        {
            int retval = NetworkAddStorage(id, id.Length);
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkSetBoundary", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NetworkSetBoundary([In] ref int nodeId, [In] ref int interpolationType,
            [In] ref int type, [In] ref double value, [In] ref double returnTime);

        public int NetworkSetBoundary(int nodeId, int interpolationType, BoundaryType type, double value)
        {
            return NetworkSetBoundary(nodeId, interpolationType, type, value, 0.0);
        }

        public int NetworkSetBoundary(int nodeId, int interpolationType, BoundaryType type, double value,
            double returnTime)
        {
            //Trace.WriteLine(string.Format("nodeId = {0}, value = {1}, init = {2}", nodeId, value, init));
            int nodeId_ = nodeId;
            double value_ = value;
            int type_ = (int) type;
            double returnTime_ = returnTime;
            int interpolationType_ = interpolationType;
            int iret = NetworkSetBoundary(ref nodeId_, ref interpolationType_, ref type_, ref value_, ref returnTime_);
            LogMessages();
            return iret;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkSetBoundaryQH", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NetworkSetBoundaryQH([In] ref int nodeId, [In] double[] discharge,
            [In] double[] waterLevel, [In] ref int length);

        public int NetworkSetBoundary(int nodeId, double[] discharge, double[] waterLevel)
        {
            int nodeId_ = nodeId;
            int length = discharge.Length;

            int iret = NetworkSetBoundaryQH(ref nodeId_, discharge, waterLevel, ref length);
            LogMessages();
            return iret;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkSetBoundaryValue", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NetworkSetBoundaryValue([In] ref int iref, [In] ref double time,
            [In] ref double value);

        public void NetworkSetBoundaryValue(int iref, double time, double value)
        {
            //Trace.WriteLine(string.Format("nodeId = {0}, value = {1}, init = {2}", nodeId, value, init));
            int iref_ = iref;
            double time_ = time;
            double value_ = value;
            //int type_ = (int)type;
            NetworkSetBoundaryValue(ref iref_, ref time_, ref value_);
            LogMessages();
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "NetworkAddObservationPoint", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NetworkAddObservationPoint_([In] String id, [In] ref int branchId,
            [In] ref double offset, int a);

        public int NetworkAddObservationPoint(string id, int branchId, double offset)
        {
            double offset_ = offset;
            int branchId_ = branchId;
            int intId = NetworkAddObservationPoint_(id, ref branchId_, ref offset_, id.Length);
            LogMessages();
            return intId;
        }

        #endregion
        #region coupling on computational timestep

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetIstep", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetIstep();

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetTimeStep", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetTimeStep();

        public bool ModelPerformTimeStep()
        {
            var result = true;
            try
            {
                var dtPluvMin = 0.001;
                var istep = GetIstep();
                var dt = GetTimeStep();
                var currentTime = (istep - 1)*dt;
                var newTime = istep*dt;
                ModelInitializeUserTimeStep();
                while (newTime - currentTime > 0.001)
                {

                    dt = ModelInitializeComputationalTimeStep(newTime, dt);
                    var successfullTimeStep = false;
                    while ((!successfullTimeStep) && (dt > dtPluvMin))
                    {
                        var dtNew = ModelRunComputationalTimeStep(dt);
                        successfullTimeStep = (dtNew == dt);
                        dt = dtNew;
                    }
                    if (!successfullTimeStep)
                    {
                        Log.Error(String.Format("Estimated required time step smaller then mininum possible time step {0}", dtPluvMin));
                        return false;
                    }
                    result = result && ModelFinalizeComputationalTimeStep();

                    currentTime += dt;
                }
                result = result && ModelFinalizeUserTimeStep();

            }
            catch (Exception)
            {
                result = false;
            }
            LogMessages();
            return result;
        }

        public bool ModelInitializeUserTimeStep()
        {
            return Flow1DApiDll.ModelInitializeUserTimeStep();
        }

        public bool ModelFinalizeUserTimeStep()
        {
            return Flow1DApiDll.ModelFinalizeUserTimeStep();
        }

        public double ModelInitializeComputationalTimeStep(double newTime, double dt)
        {
            Flow1DApiDll.ModelInitializeComputationalTimeStep_(ref newTime, ref dt);
            return dt;
        }

        public double ModelRunComputationalTimeStep(double dt)
        {
            Flow1DApiDll.ModelRunComputationalTimeStep(ref dt);
            return dt;
        }

        public bool ModelFinalizeComputationalTimeStep()
        {
            return Flow1DApiDll.ModelFinalizeComputationalTimeStep();
        }

        public bool Finalize()
        {
            LogMessages();
            Parameters.Clear();
            bool res = Flow1DApiDll.Finalize_();
            LogMessages();
            return res;
        }

        #endregion


        #region Structures

        public int setStrucWeir(string id, int ibranch2, double dist, int icompound,
            double crestlevel,
            double crestwidth,
            double dischargecoef,
            double latdiscoef,
            int allowflowdir)
        {
            int icompound_ = icompound;
            int ibranch2_ = ibranch2;
            double dist_ = dist;
            double crestlevel_ = crestlevel;
            double crestwidth_ = crestwidth;
            double dischargecoef_ = dischargecoef;
            double latdiscoef_ = latdiscoef;
            int allowflowdir_ = allowflowdir;
            int retval = setStrucWeir(ref ibranch2_, ref dist_, ref icompound_,
                ref crestlevel_,
                ref crestwidth_,
                ref dischargecoef_,
                ref latdiscoef_,
                ref allowflowdir_, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucWeir", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucWeir(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double crestlevel,
            [In] ref double crestwidth,
            [In] ref double dischargecoef,
            [In] ref double latdiscoef,
            [In] ref int allowflowdir,
            [In] string id,
            int len);

        public int setStrucRiverWeir(string id, int ibranch2, double dist, int icompound,
            double crestlevel,
            double crestwidth,
            double pos_cwcoef,
            double pos_slimlimit,
            double[] pos_sf,
            double[] pos_red,
            double neg_cwcoef,
            double neg_slimlimit,
            double[] neg_sf,
            double[] neg_red)
        {
            int pos_sf_count = pos_sf.Length;
            int neg_sf_count = neg_sf.Length;
            int retval = setStrucRiverWeir(ref ibranch2, ref dist, ref icompound,
                ref crestlevel,
                ref crestwidth,
                ref pos_cwcoef,
                ref pos_slimlimit,
                pos_sf,
                pos_red,
                ref pos_sf_count,
                ref neg_cwcoef,
                ref neg_slimlimit,
                neg_sf,
                neg_red,
                ref neg_sf_count, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucRiverWeir", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucRiverWeir(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double crestlevel,
            [In] ref double crestwidth,
            [In] ref double pos_cwcoef,
            [In] ref double pos_slimlimit,
            [In] double[] pos_sf,
            [In] double[] pos_red,
            [In] ref int pos_sf_count,
            [In] ref double neg_cwcoef,
            [In] ref double neg_slimlimit,
            [In] double[] neg_sf,
            [In] double[] neg_red,
            [In] ref int neg_sf_count,
            [In] string id,
            int len);

        public int setStrucAdvWeir(string id, int ibranch2, double dist, int icompound,
            double crestlevel,
            double totwidth,
            int npiers,
            double pos_height,
            double pos_designhead,
            double pos_piercontractcoef,
            double pos_abutcontractcoef,
            double neg_height,
            double neg_designhead,
            double neg_piercontractcoef,
            double neg_abutcontractcoef)
        {
            int retval = setStrucAdvWeir(ref ibranch2, ref dist, ref icompound,
                ref crestlevel,
                ref totwidth,
                ref npiers,
                ref pos_height,
                ref pos_designhead,
                ref pos_piercontractcoef,
                ref pos_abutcontractcoef,
                ref neg_height,
                ref neg_designhead,
                ref neg_piercontractcoef,
                ref neg_abutcontractcoef, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucAdvWeir", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucAdvWeir(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double crestlevel,
            [In] ref double totwidth,
            [In] ref int npiers,
            [In] ref double pos_height,
            [In] ref double pos_designhead,
            [In] ref double pos_piercontractcoef,
            [In] ref double pos_abutcontractcoef,
            [In] ref double neg_height,
            [In] ref double neg_designhead,
            [In] ref double neg_piercontractcoef,
            [In] ref double neg_abutcontractcoef,
            [In] string id,
            int len);

        public int setStrucOrifice(string id, int ibranch2, double dist, int icompound,
            double crestlevel,
            double crestwidth,
            double contrcoef,
            double latcontrcoef,
            int allowedflowdir,
            double openlevel,
            int uselimitflowpos,
            double limitflowpos,
            int uselimitflowneg,
            double limitflowneg)
        {
            int retval = setStrucOrifice(ref ibranch2, ref dist, ref icompound,
                ref crestlevel,
                ref crestwidth,
                ref contrcoef,
                ref latcontrcoef,
                ref allowedflowdir,
                ref openlevel,
                ref uselimitflowpos,
                ref limitflowpos,
                ref uselimitflowneg,
                ref limitflowneg, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucOrifice", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucOrifice(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double crestlevel,
            [In] ref double crestwidth,
            [In] ref double contrcoef,
            [In] ref double latcontrcoef,
            [In] ref int allowedflowdir,
            [In] ref double openlevel,
            [In] ref int uselimitflowpos,
            [In] ref double limitflowpos,
            [In] ref int uselimitflowneg,
            [In] ref double limitflowneg,
            [In] string id,
            int len);

        public int setStrucCulvert(string id, int ibranch2, double dist, int icompound,
            double leftlevel,
            double rightlevel,
            int crosssectionnr,
            int allowedflowdir,
            double length,
            double inletlosscoef,
            double outletlosscoef,
            int valve_onoff,
            double inivalveopen,
            int bedFrictionType,
            double bedFriction,
            double groundFriction,
            double[] relativeOpening,
            double[] lossCoefficient)
        {
            int lossCoefflength = lossCoefficient.Length;
            int retval = setStrucCulvert(ref ibranch2, ref dist, ref icompound,
                ref leftlevel,
                ref rightlevel,
                ref crosssectionnr,
                ref allowedflowdir,
                ref length,
                ref inletlosscoef,
                ref outletlosscoef,
                ref valve_onoff,
                ref inivalveopen,
                relativeOpening,
                lossCoefficient,
                ref lossCoefflength,
                ref bedFrictionType,
                ref bedFriction,
                ref bedFrictionType,
                ref groundFriction,
                id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucCulvert", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucCulvert(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double leftlevel,
            [In] ref double rightlevel,
            [In] ref int crosssectionnr,
            [In] ref int allowedflowdir,
            [In] ref double length,
            [In] ref double inletlosscoef,
            [In] ref double outletlosscoef,
            [In] ref int valve_onoff,
            [In] ref double inivalveopen,
            [In] double[] relativeOpening,
            [In] double[] lossCoefficient,
            [In] ref int lossCoefflength,
            [In] ref int bedFrictionType,
            [In] ref double bedFriction,
            [In] ref int groundFrictionType,
            [In] ref double groundFriction,
            [In] string id,
            int len);

        public int setStrucSiphon(string id, int ibranch2, double dist, int icompound,
            double leftlevel,
            double rightlevel,
            int crosssectionnr,
            int allowedflowdir,
            double length,
            double inletlosscoef,
            double outletlosscoef,
            double bendlosscoef,
            int valve_onoff,
            double inivalveopen,
            int bedFrictionType,
            double bedFriction,
            double groundFriction,
            double[] relativeOpening,
            double[] lossCoefficient,
            double turnonlevel,
            double turnofflevel,
            int siphon_onoff)
        {
            int lossCoefflength = lossCoefficient.Length;
            int retval = setStrucSiphon(ref ibranch2, ref dist, ref icompound,
                ref leftlevel,
                ref rightlevel,
                ref crosssectionnr,
                ref allowedflowdir,
                ref length,
                ref inletlosscoef,
                ref outletlosscoef,
                ref bendlosscoef,
                ref valve_onoff,
                ref inivalveopen,
                ref bedFrictionType,
                ref bedFriction,
                ref bedFrictionType,
                ref groundFriction,
                relativeOpening,
                lossCoefficient,
                ref lossCoefflength,
                ref turnonlevel,
                ref turnofflevel,
                ref siphon_onoff,
                id, id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucSiphon", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucSiphon(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double leftlevel,
            [In] ref double rightlevel,
            [In] ref int crosssectionnr,
            [In] ref int allowedflowdir,
            [In] ref double length,
            [In] ref double inletlosscoef,
            [In] ref double outletlosscoef,
            [In] ref double bendlosscoef,
            [In] ref int valve_onoff,
            [In] ref double inivalveopen,
            [In] ref int bedFrictionType,
            [In] ref double bedFriction,
            [In] ref int groundFrictionType,
            [In] ref double groundFriction,
            [In] double[] relativeOpening,
            [In] double[] lossCoefficient,
            [In] ref int lossCoefflength,
            [In] ref double turnonlevel,
            [In] ref double turnofflevel,
            [In] ref int siphon_onoff,
            [In] string id,
            int len);

        public int setStrucInvSiphon(string id, int ibranch2, double dist, int icompound,
            double leftlevel,
            double rightlevel,
            int crosssectionnr,
            int allowedflowdir,
            double length,
            double inletlosscoef,
            double outletlosscoef,
            double bendlosscoef,
            int valve_onoff,
            double inivalveopen,
            int bedFrictionType,
            double bedFriction,
            double groundFriction,
            double[] relativeOpening,
            double[] lossCoefficient)
        {
            int lossCoefflength = relativeOpening.Length;
            int retval = setStrucInvSiphon(ref ibranch2, ref dist, ref icompound,
                ref leftlevel,
                ref rightlevel,
                ref crosssectionnr,
                ref allowedflowdir,
                ref length,
                ref inletlosscoef,
                ref outletlosscoef,
                ref bendlosscoef,
                ref valve_onoff,
                ref inivalveopen,
                ref bedFrictionType,
                ref bedFriction,
                ref bedFrictionType,
                ref groundFriction,
                relativeOpening,
                lossCoefficient, ref lossCoefflength, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucInvSiphon", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucInvSiphon(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double leftlevel,
            [In] ref double rightlevel,
            [In] ref int crosssectionnr,
            [In] ref int allowedflowdir,
            [In] ref double length,
            [In] ref double inletlosscoef,
            [In] ref double outletlosscoef,
            [In] ref double bendlosscoef,
            [In] ref int valve_onoff,
            [In] ref double inivalveopen,
            [In] ref int bedFrictionType,
            [In] ref double bedFriction,
            [In] ref int groundFrictionType,
            [In] ref double groundFriction,
            [In] double[] relativeOpening,
            [In] double[] lossCoefficient,
            [In] ref int lossCoefflength,
            [In] string id,
            int len);

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucUniWeir", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucUniWeir(
            [In] ref int ibranch2,
            [In] ref double dist,
            [In] ref int icompound,
            [In] ref int yzcount,
            [In] double[] y,
            [In] double[] z,
            [In] ref double crestlevel,
            [In] ref double dischargecoef,
            [In] ref int allowedflowdir,
            [In] ref double freesubmergedfactor,
            [In] string id,
            int len);

        public int setStrucUniWeir(
            string id,
            int ibranch2,
            double dist,
            int icompound,
            double[] y,
            double[] z,
            double crestlevel,
            double dischargecoef,
            int allowedflowdir,
            double freesubmergedfactor)
        {
            int yzcount = y.Length;

            int retval = setStrucUniWeir(
                ref ibranch2,
                ref dist,
                ref icompound,
                ref yzcount,
                y,
                z,
                ref crestlevel,
                ref dischargecoef,
                ref allowedflowdir,
                ref freesubmergedfactor, id,
                id.Length);
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucBridgePillars", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucBridge(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double pillarwidth,
            [In] ref double formfactor,
            [In] ref int allowedFlowDir,
            [In] string id,
            int len);

        public int setStrucBridge(string id, int ibranch2, double dist, int icompound,
            double pillarwidth, double formfactor, int allowedFlowDir)
        {
            double pillarwidth_ = pillarwidth;
            int ibranch2_ = ibranch2;
            double dist_ = dist;
            int icompound_ = icompound;
            double formfactor_ = formfactor;
            int allowedFlowDir_ = allowedFlowDir;
            int retval = setStrucBridge(ref ibranch2_, ref dist_, ref icompound_,
                ref pillarwidth_, ref formfactor_, ref allowedFlowDir_, id, id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucBridge", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucBridge(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double bottomlevel,
            [In] ref double pillarwidth,
            [In] ref double formfactor,
            [In] ref int crosssectionnr,
            [In] ref double length,
            [In] ref double inletlosscoef,
            [In] ref double outletlosscoef,
            [In] ref int allowedFlowDir,
            [In] ref int bedFrictionType,
            [In] ref double bedFriction,
            [In] ref int groundFrictionType,
            [In] ref double groundFriction,
            [In] string id,
            int len);

        public int setStrucBridge(string id, int ibranch2, double dist, int icompound,
            double bottomlevel, double pillarwidth, double formfactor, int crosssectionnr,
            double length, double inletlosscoef, double outletlosscoef, int allowedFlowDir,
            int bedFrictionType, double bedFriction, double groundFriction)
        {
            int ibranch2_ = ibranch2;
            double dist_ = dist;
            int icompound_ = icompound;
            double pillarwidth_ = pillarwidth;
            double formfactor_ = formfactor;
            int allowedFlowDir_ = allowedFlowDir;
            int crosssectionnr_ = crosssectionnr;
            double length_ = length;
            double inletlosscoef_ = inletlosscoef;
            double outletlosscoef_ = outletlosscoef;
            int bedFrictionType_ = bedFrictionType;
            double bedFriction_ = bedFriction;
            int groundFrictionType_ = bedFrictionType;
            double groundFriction_ = groundFriction;
            double bottomlevel_ = bottomlevel;
            int retval = setStrucBridge(ref ibranch2_, ref dist_, ref icompound_,
                ref bottomlevel_, ref pillarwidth_, ref formfactor_, ref crosssectionnr_, ref length_,
                ref inletlosscoef_, ref outletlosscoef_, ref allowedFlowDir_,
                ref bedFrictionType_, ref bedFriction_, ref groundFrictionType_,
                ref groundFriction_, id, id.Length);
            LogMessages();
            return retval;
        }

        public int setStrucGeneralst(string id, int ibranch2, double dist, int icompound,
            double widthleftW1,
            double levelleftZb1,
            double widthleftWsdl,
            double levelleftZbsl,
            double widthcenter,
            double levelcenter,
            double widthrightWsdr,
            double levelrightZbsr,
            double widthrightW2,
            double levelrightZb2,
            double gateheight,
            double pos_freegateflowcoef,
            double pos_drowngateflowcoef,
            double pos_freeweirflowcoef,
            double pos_drownweirflowcoef,
            double pos_contrcoeffreegate,
            double neg_freegateflowcoef,
            double neg_drowngateflowcoef,
            double neg_freeweirflowcoef,
            double neg_drownweirflowcoef,
            double neg_contrcoeffreegate,
            double extraresistance)
        {
            int retval = setStrucGeneralst(ref ibranch2, ref dist, ref icompound,
                ref widthleftW1,
                ref levelleftZb1,
                ref widthleftWsdl,
                ref levelleftZbsl,
                ref widthcenter,
                ref levelcenter,
                ref widthrightWsdr,
                ref levelrightZbsr,
                ref widthrightW2,
                ref levelrightZb2,
                ref gateheight,
                ref pos_freegateflowcoef,
                ref pos_drowngateflowcoef,
                ref pos_freeweirflowcoef,
                ref pos_drownweirflowcoef,
                ref pos_contrcoeffreegate,
                ref neg_freegateflowcoef,
                ref neg_drowngateflowcoef,
                ref neg_freeweirflowcoef,
                ref neg_drownweirflowcoef,
                ref neg_contrcoeffreegate,
                ref extraresistance, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucGeneralst", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucGeneralst(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref double widthleftW1,
            [In] ref double levelleftZb1,
            [In] ref double widthleftWsdl,
            [In] ref double levelleftZbsl,
            [In] ref double widthcenter,
            [In] ref double levelcenter,
            [In] ref double widthrightWsdr,
            [In] ref double levelrightZbsr,
            [In] ref double widthrightW2,
            [In] ref double levelrightZb2,
            [In] ref double gateheight,
            [In] ref double pos_freegateflowcoef,
            [In] ref double pos_drowngateflowcoef,
            [In] ref double pos_freeweirflowcoef,
            [In] ref double pos_drownweirflowcoef,
            [In] ref double pos_contrcoeffreegate,
            [In] ref double neg_freegateflowcoef,
            [In] ref double neg_drowngateflowcoef,
            [In] ref double neg_freeweirflowcoef,
            [In] ref double neg_drownweirflowcoef,
            [In] ref double neg_contrcoeffreegate,
            [In] ref double extraresistance,
            [In] string id,
            int len);

        public int setStrucPump(string id, int ibranch2, double dist, int icompound,
            int direction,
            int nrstages,
            int icapo,
            double computed_capacity,
            double oldcapacity,
            double capacitySetpoint,
            bool isControlled,
            double[] capacity,
            double[] onlevelup,
            double[] offlevelup,
            double[] onleveldown,
            double[] offleveldown,
            //type t_table
            int tabsize,
            int interpoltype,
            int storedcounter,
            double[] leveldiff,
            double[] redfact)

        {
            int ibranch2_ = ibranch2;
            double dist_ = dist;
            int icompound_ = icompound;
            int direction_ = direction;
            int nrstages_ = nrstages;
            int icapo_ = icapo;
            double computed_capacity_ = computed_capacity;
            double oldcapacity_ = oldcapacity;
            double capacitySetpoint_ = capacitySetpoint;
            bool isControlled_ = isControlled;
            int tabsize_ = tabsize;
            int interpoltype_ = interpoltype;
            int storedcounter_ = storedcounter;
            int retval = setStrucPump(ref ibranch2_, ref dist_, ref icompound_,
                ref direction_,
                ref nrstages_,
                ref icapo_,
                ref computed_capacity_,
                ref oldcapacity_,
                ref capacitySetpoint_,
                ref isControlled_,
                capacity,
                onlevelup,
                offlevelup,
                onleveldown,
                offleveldown,
                ref tabsize_,
                ref interpoltype_,
                ref storedcounter_,
                leveldiff,
                redfact, id,
                id.Length);
            LogMessages();
            return retval;
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "setStrucPump", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setStrucPump(
            [In] ref int ibranch2, [In] ref double dist, [In] ref int icompound,
            [In] ref int direction,
            [In] ref int nrstages,
            [In] ref int icapo,
            [In] ref double computed_capacity,
            [In] ref double oldcapacity,
            [In] ref double capacitySetpoint,
            [In] ref bool isControlled,
            [In] double[] capacity,
            [In] double[] onlevelup,
            [In] double[] offlevelup,
            [In] double[] onleveldown,
            [In] double[] offleveldown,
            //type t_table
            [In] ref int tabsize,
            [In] ref int interpoltype,
            [In] ref int storedcounter,
            [In] double[] leveldiff,
            [In] double[] redfact,
            [In] string id,
            int len
            );

        public void SetStrucControlValue(int istru, QuantityType type, double numValue)
        {
            int istru_ = istru;
            int type_ = (int) type;
            double numValue_ = numValue;
            bool success = Flow1DApiDll.SetStrucControlValue(ref istru_, ref type_, ref numValue_);
            if (!success)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Attempt to set value for quantity '{0}' for structure ({1}); however this quantity is not valid for this structure type",
                        type, istru));
            }
        }

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "InitialiseStrucMappingArray", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitialiseStrucMappingArray([In] ref int count);

        public void InitialiseStrucMappingArray(int count)
        {
            int count_ = count;
            InitialiseStrucMappingArray(ref count_);
        }

        #endregion

        #region Conveyance tables and interpolation

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetConveyanceTable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetConveyanceTable([In] ref int csInterpolated,
            [In, Out] double[] Levels,
            [In, Out] double[] FlowArea,
            [In, Out] double[] FlowWidth,
            [In, Out] double[] Perimeter,
            [In, Out] double[] HydraulicRadius,
            [In, Out] double[] TotalWidth,
            [In, Out] double[] ConveyancePos,
            [In, Out] double[] ConveyanceNeg,
            [In] ref int length);

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "InterpolateCrossSections", CallingConvention = CallingConvention.Cdecl)]
        private static extern int InterpolateCrossSections([In] ref int crossSectionNr1, [In] ref int crossSectionNr2,
            [In] ref double distanceBetweenCrossSections, [In] ref double distanceToCrossSectionNr1);

        [DllImport(Flow1DApiDll.CF_DLL_NAME, EntryPoint = "GetNumberOfConveyanceLevels", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNumberOfConveyanceLevels([In] ref int csInterpolated);

        public void GetConveyanceTable(int crossSectionNr, ref double[] levels, ref double[] flowArea,
            ref double[] flowWidth, ref double[] perimeter, ref double[] hydraulicRadius, ref double[] totalWidth,
            ref double[] conveyancePos, ref double[] conveyanceNeg)
        {
            int count = GetNumberOfConveyanceLevels(ref crossSectionNr);

            levels = new double[count];
            flowArea = new double[count];
            flowWidth = new double[count];
            perimeter = new double[count];
            hydraulicRadius = new double[count];
            totalWidth = new double[count];
            conveyancePos = new double[count];
            conveyanceNeg = new double[count];
            GetConveyanceTable(ref crossSectionNr, levels, flowArea, flowWidth,
                perimeter, hydraulicRadius, totalWidth, conveyancePos,
                conveyanceNeg, ref count);

            LogMessages();
        }

        #endregion
        
    }
}