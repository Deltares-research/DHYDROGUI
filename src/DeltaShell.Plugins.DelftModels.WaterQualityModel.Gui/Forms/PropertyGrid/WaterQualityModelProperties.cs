using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.UI.Forms;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Water quality model")]
    public class WaterQualityModelProperties : ObjectProperties<WaterQualityModel>
    {
        [PropertyOrder(1)]
        [Category("\t\t\t\t\t\tGeneral")]
        [Description("Name of model")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [PropertyOrder(2)]
        [Category("\t\t\t\t\t\tGeneral")]
        [Description("Status of model run")]
        public ActivityStatus Status
        {
            get { return data.Status; }
        }

        [PropertyOrder(3)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Hydrodynamics source")]
        [Description("Source of the hydro dynamica")]
        public string HydroDataImporter
        {
            get { return data.HydroData != null ? data.HydroData.ToString() : ""; }
        }

        [Category("\t\t\t\t\t\tGeneral")]
        [PropertyOrder(3)]
        [DisplayName("Coordinate system")]
        [Description("Coordinate system (geographic or projected) used for drawing.")]
        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Editor(typeof(CoordinateSystemTypeEditor), typeof(UITypeEditor))]
        public ICoordinateSystem CoordinateSystem
        {
            get { return data.CoordinateSystem; }
            set
            {
                try
                {
                    data.CoordinateSystem = value;
                }
                catch (CoordinateTransformException e)
                {
                    MessageBox.Show("Cannot convert map to coordinate system: " + e.Message,
                        "Coordinate transformation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        [PropertyOrder(6)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Working directory")]
        [Description("Directory where model runs")]
        [ReadOnly(true)]
        public string WorkDirectory
        {
            get { return data.ModelSettings.WorkDirectory; }
        }

        [PropertyOrder(7)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Correct for evaporation")]
        [Description("Toggles the correction in calculation for evaporation")]
        public bool CorrectForEvaporation
        {
            get { return data.ModelSettings.CorrectForEvaporation; }
            set { data.ModelSettings.CorrectForEvaporation = value; }
        }

        [PropertyOrder(8)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Processes active")]
        [Description("Processes active")]
        public bool ProcessesActive
        {
            get { return data.ModelSettings.ProcessesActive; }
            set { data.ModelSettings.ProcessesActive = value; }
        }

        [PropertyOrder(9)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Monitoring output level")]
        [Description("Monitoring output level to use")]
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public MonitoringOutputLevel MonitoringOutputLevel
        {
            get { return data.ModelSettings.MonitoringOutputLevel; }
            set { data.ModelSettings.MonitoringOutputLevel = value; }
        }

        [PropertyOrder(13)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Vertical schematization type")]
        public LayerType LayerType
        {
            get { return data.LayerType; }
        }

        [PropertyOrder(14)]
        [Category("\t\t\t\t\t\tGeneral")]
        [DisplayName("Water quality layers")]
        [Description("Relative thickness of the water quality layers defined in the model")]
        [TypeConverter(typeof(ExpandableArrayConverter))]
        public string[] WaterQualityLayerThicknesses
        {
            get
            {
                if (data.HydroData == null)
                {
                    return new string[0];
                }

                var hydroLayersPerWaqLayer = data.HydroData.NumberOfHydrodynamicLayersPerWaqSegmentLayer;
                var hydroLayerThicknesses = data.HydroData.HydrodynamicLayerThicknesses;
                var result = new double[hydroLayersPerWaqLayer.Length];

                for (int i = 0, layerCount = 0; i < result.Length; i++)
                {
                    result[i] = 0;
                    for (int j = 0; j < hydroLayersPerWaqLayer[i]; j++)
                    {
                        result[i] += hydroLayerThicknesses[layerCount + j];
                    }

                    layerCount += hydroLayersPerWaqLayer[i];
                }
                return result.Select(d=> d.ToString("F3", CultureInfo.InvariantCulture)).ToArray();
            }
        }

        [PropertyOrder(1)]
        [Category("\t\t\t\t\tSimulation timers")]
        [DisplayName("Start time")]
        [Description("Start time for model run")]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        public DateTime StartTime
        {
            get { return data.StartTime; }
            set { data.StartTime = value; }
        }

        [PropertyOrder(2)]
        [Category("\t\t\t\t\tSimulation timers")]
        [DisplayName("Stop time")]
        [Description("Stop time for model run")]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        public DateTime StopTime
        {
            get { return data.StopTime; }
            set { data.StopTime = value; }
        }

        [PropertyOrder(3)]
        [Category("\t\t\t\t\tSimulation timers")]
        [DisplayName("Time step")]
        [Description("Timestep interval for model run")]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        public TimeSpan TimeStep
        {
            get { return data.TimeStep; }
            set { data.TimeStep = value; }
        }

        [PropertyOrder(1)]
        [Category("\t\t\t\tOutput timers")]
        [DisplayName("Observation points and areas (his)")]
        [Description("Timers for output on observation points and areas")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterQualityTimeSettingsProperties HisOutput
        {
            get { return new WaterQualityTimeSettingsProperties(new WaterQualityTimeSettings(data.ModelSettings, WaterQualityTimeSettingsType.His)); }
        }

        [PropertyOrder(2)]
        [Category("\t\t\t\tOutput timers")]
        [DisplayName("Model wide (map)")]
        [Description("Timers for output on model wide (map)")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterQualityTimeSettingsProperties MapOutput
        {
            get { return new WaterQualityTimeSettingsProperties(new WaterQualityTimeSettings(data.ModelSettings, WaterQualityTimeSettingsType.Map)); }
        }

        [PropertyOrder(3)]
        [Category("\t\t\t\tOutput timers")]
        [DisplayName("Balances (mon)")]
        [Description("Timers for balance output")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterQualityTimeSettingsProperties BalanceOutput
        {
            get { return new WaterQualityTimeSettingsProperties(new WaterQualityTimeSettings(data.ModelSettings, WaterQualityTimeSettingsType.Balance)); }
        }

        [PropertyOrder(1)]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Integration method")]
        [Description("Integration method\n" +
        " Scheme  1 - 1st order upwind in time and space\n" +
        " Scheme  5 - 2nd order Flux Corrected Transport (Boris and Book)\n" +
        " Scheme 10 - Implicit, direct method, 1st order upwind\n" +
        " Scheme 11 - Horizontally method 1, vertically implicit 2nd order\n" +
        " Scheme 12 - Horizontally method 5, vertically implicit 2nd order\n" +
        " Scheme 13 - Horizontally method 1, vertically implicit 1st order\n" +
        " Scheme 14 - Horizontally method 5, vertically implicit 1st order\n" +
        " Scheme 15 - Implicit iterative Method, 1st order upwind in space and time\n" +
        " Scheme 16 - Implicit Iterative Method, 1st order upwind in horizontal and time, 2nd order vertically\n" +
        " Scheme 21 - Self adapting Theta Method, implicit vertically, FCT (Zalezac)\n" +
        " Scheme 22 - Self adapting Theta Method, implicit vertically, FCT (Boris and Book)")]
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public NumericalScheme IntegrationMethodNumber
        {
            get { return data.ModelSettings.NumericalScheme; }
            set { data.ModelSettings.NumericalScheme = value; }
        }

        [PropertyOrder(2)]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Use first order open boundary transport")]
        [Description("Use first order(true) or second order (false) approximation over open boundaries")]
        public bool UseFirstOrder
        {
            get { return data.ModelSettings.UseFirstOrder; }
            set { data.ModelSettings.UseFirstOrder = value; }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Max nr of iterations")]
        [Description("Maximum number of iterations.")]
        public int IterationMaximum
        {
            get { return data.ModelSettings.IterationMaximum; }
            set { data.ModelSettings.IterationMaximum = value; }
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Iteration convergence criterion")]
        [Description("Iteration tolerance used for convergence.")]
        public double Tolerance
        {
            get { return data.ModelSettings.Tolerance; }
            set { data.ModelSettings.Tolerance = value; }
        }

        [PropertyOrder(5)]
        [DynamicReadOnly]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Iteration report")]
        [Description("Write the iteration report or not.")]
        public bool WriteIterationReport
        {
            get { return data.ModelSettings.WriteIterationReport; }
            set { data.ModelSettings.WriteIterationReport = value; }
        }

        [PropertyOrder(6)]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Closure error correction")]
        [Description("Indicates whether Delwaq should correct water volumes to guarantee continuous concentrations (true) when wrapping hydro dynamic data or use the data as it is (causing discontinuities in concentrations)")]
        public bool ClosureErrorCorrection
        {
            get { return data.ModelSettings.ClosureErrorCorrection; }
            set { data.ModelSettings.ClosureErrorCorrection = value; }
        }

        [PropertyOrder(7)]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Dry cell threshold (m)")]
        [Description("Cells are considered to contain no water if their water level falls below this threshold.")]
        public double DryCellThreshold
        {
            get { return data.ModelSettings.DryCellThreshold; }
            set { data.ModelSettings.DryCellThreshold = value; }
        }

        [PropertyOrder(8)]
        [Category("\t\t\tNumerical options")]
        [DisplayName("Nr of cores to use (0 = max)")]
        [Description("Number of threads used during the calculation. A value of 0 indicates that all available threads should be used.")]
        public int NrOfThreads
        {
            get { return data.ModelSettings.NrOfThreads; }
            set { data.ModelSettings.NrOfThreads = value; }
        }

        [PropertyOrder(1)]
        [Category("\t\tDispersion options")]
        [DisplayName("Horizontal dispersion")]
        [Description("Uniform dispersion occurring in horizontal direction ([m^2/s]).")]
        [DynamicReadOnly]
        public double HorizontalDispersion
        {
            get { return data.HorizontalDispersion; }
            set { data.HorizontalDispersion = value; }
        }

        [PropertyOrder(2)]
        [Category("\t\tDispersion options")]
        [DisplayName("(Additional) vertical dispersion")]
        [Description("Uniform dispersion occurring in vertical direction ([m^2/s]).")]
        public double VerticalDispersion
        {
            get { return data.VerticalDispersion; }
            set { data.VerticalDispersion = value; }
        }

        [PropertyOrder(3)]
        [Category("\t\tDispersion options")]
        [DisplayName("Vertical dispersion from hydrodynamics")]
        [Description("Use additional vertical dispersion from hydro dynamics data.")]
        public bool UseAdditionalHydrodynamicVerticalDiffusion
        {
            get { return data.UseAdditionalHydrodynamicVerticalDiffusion; }
            set { data.UseAdditionalHydrodynamicVerticalDiffusion = value; }
        }

        [PropertyOrder(4)]
        [Category("\t\tDispersion options")]
        [DisplayName("No dispersion if the flow is zero")]
        [Description("No dispersion if the flow rate is zero")]
        public bool NoDispersionIfFlowIsZero
        {
            get { return data.ModelSettings.NoDispersionIfFlowIsZero; }
            set { data.ModelSettings.NoDispersionIfFlowIsZero = value; }
        }

        [PropertyOrder(5)]
        [Category("\t\tDispersion options")]
        [DisplayName("No dispersion over open boundaries")]
        [Description("Use no dispersion over open boundaries")]
        public bool NoDispersionOverOpenBoundaries
        {
            get { return data.ModelSettings.NoDispersionOverOpenBoundaries; }
            set { data.ModelSettings.NoDispersionOverOpenBoundaries = value; }
        }

        [PropertyOrder(1)]
        [Description("Use (restart) state as initial condition")]
        [DisplayName("Use restart")]
        [Category("\tRestart parameters")]
        public bool UseRestart
        {
            get { return data.UseRestart; }
            set { data.UseRestart = value; }
        }

        [PropertyOrder(2)]
        [Description("Write final state (can be used to restart from)")]
        [DisplayName("Write restart")]
        [Category("\tRestart parameters")]
        public bool WriteRestart
        {
            get { return data.WriteRestart; }
            set { data.WriteRestart = value; }
        }

        [PropertyOrder(3)]
        [Description("Write states on specified time instances")]
        [DisplayName("Use save state time range")]
        [Category("\tRestart parameters")]
        public bool UseSaveStateTimeRange
        {
            get { return data.UseSaveStateTimeRange; }
            set { data.UseSaveStateTimeRange = value; }
        }

        [PropertyOrder(4)]
        [Description("Start writing states when simulation time is equals or larger than this time")]
        [DisplayName("Save state start time")]
        [Category("\tRestart parameters")]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        public DateTime SaveStateStartTime
        {
            get { return data.SaveStateStartTime; }
            set { data.SaveStateStartTime = value; }
        }

        [PropertyOrder(5)]
        [Description("Stop writing states when simulation time is beyond this time")]
        [DisplayName("Save state stop time")]
        [Category("\tRestart parameters")]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        public DateTime SaveStateStopTime
        {
            get { return data.SaveStateStopTime; }
            set { data.SaveStateStopTime = value; }
        }

        [PropertyOrder(6)]
        [Description("Write state at each multiple of this time step after the 'Save state start time'")]
        [DisplayName("Save state time step")]
        [Category("\tRestart parameters")]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        [DynamicReadOnly]
        public TimeSpan SaveStateTimeStep
        {
            get { return data.SaveStateTimeStep; }
            set { data.SaveStateTimeStep = value; }
        }


        [PropertyOrder(1)]
        [Category("Balance options")]
        [DisplayName("Balance active")]
        [Description("Determines if balance output should be calculated")]
        public bool BalanceOutputLevel
        {
            get { return data.ModelSettings.Balance; }
            set { data.ModelSettings.Balance = value; }
        }

        [PropertyOrder(2)]
        [Category("Balance options")]
        [DisplayName("Balance output unit")]
        [Description("Determines the unit for the balance output")]
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public BalanceUnit BalanceUnit
        {
            get { return data.ModelSettings.BalanceUnit; }
            set { data.ModelSettings.BalanceUnit = value; }
        }

        [PropertyOrder(3)]
        [Category("Balance options")]
        [DisplayName("Lump processes")]
        [Description("Report contribution of all individual processes (false) or " +
                     "have them 'lumped' into a single term (true)")]
        public bool LumpProcesses
        {
            get { return data.ModelSettings.LumpProcesses; }
            set { data.ModelSettings.LumpProcesses = value; }
        }

        [PropertyOrder(4)]
        [Category("Balance options")]
        [DisplayName("Lump boundaries and loads")]
        [Description("Report contribution of all individual loads (false) or " +
                     "have them 'lumped' into a single term (true)")]
        public bool LumpLoads
        {
            get { return data.ModelSettings.LumpLoads; }
            set { data.ModelSettings.LumpLoads = value; }
        }

        [PropertyOrder(5)]
        [Category("Balance options")]
        [DisplayName("Lump transport")]
        [Description("Report contribution of all individual transports (false) or " +
                     "have them 'lumped' into a single term (true)")]
        public bool LumpTransport
        {
            get { return data.ModelSettings.LumpTransport; }
            set { data.ModelSettings.LumpTransport = value; }
        }

        [PropertyOrder(6)]
        [Category("Balance options")]
        [DisplayName("Suppress time dependent output")]
        [Description("Suppress output for the monitoring timesteps (leaving only the " +
                     "terms accumulated over time)")]
        public bool SuppressTime
        {
            get { return data.ModelSettings.SuppressTime; }
            set { data.ModelSettings.SuppressTime = value; }
        }

        [PropertyOrder(7)]
        [Category("Balance options")]
        [DisplayName("Suppress space")]
        [Description("Suppress output for individual monitoring points (segments) and " +
                     "monitoring areas (leaving only the overall mass balance term)")]
        public bool SuppressSpace
        {
            get { return data.ModelSettings.SuppressSpace; }
            set { data.ModelSettings.SuppressSpace = value; }
        }

        [PropertyOrder(8)]
        [Category("Balance options")]
        [DisplayName("No balance (monitoring points)")]
        [Description("Determines if balance output is generated for monitoring points")]
        public bool NoBalanceMonitoringPoints
        {
            get { return data.ModelSettings.NoBalanceMonitoringPoints; }
            set { data.ModelSettings.NoBalanceMonitoringPoints = value; }
        }

        [PropertyOrder(9)]
        [Category("Balance options")]
        [DisplayName("No balance (monitoring areas)")]
        [Description("Determines if balance output is generated for monitoring areas")]
        public bool NoBalanceMonitoringAreas
        {
            get { return data.ModelSettings.NoBalanceMonitoringAreas; }
            set { data.ModelSettings.NoBalanceMonitoringAreas = value; }
        }

        [PropertyOrder(10)]
        [Category("Balance options")]
        [DisplayName("No balance (model wide)")]
        [Description("Determines if balance output is generated model wide")]
        public bool NoBalanceMonitoringModelWide
        {
            get { return data.ModelSettings.NoBalanceMonitoringModelWide; }
            set { data.ModelSettings.NoBalanceMonitoringModelWide = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName == "SaveStateStartTime" ||
                propertyName == "SaveStateStopTime" ||
                propertyName == "SaveStateTimeStep")
            {
                return !data.UseSaveStateTimeRange;
            }
            else if (IsIterativeSchemeRelatedProperty(propertyName))
            {
                return !data.ModelSettings.NumericalScheme.IsIterativeCalculationScheme();
            }
            else if (propertyName == TypeUtils.GetMemberName(() => data.HorizontalDispersion))
            {
                return data.Dispersion[0].IsUnstructuredGridCellCoverage();
            }

            return false;
        }

        private static bool IsIterativeSchemeRelatedProperty(string propertyName)
        {
            return propertyName == TypeUtils.GetMemberName<WaterQualityModelProperties>(s => s.IterationMaximum) ||
                   propertyName == TypeUtils.GetMemberName<WaterQualityModelProperties>(s => s.Tolerance) ||
                   propertyName == TypeUtils.GetMemberName<WaterQualityModelProperties>(s => s.WriteIterationReport);
        }
    }
}