using System;
using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    public class RainfallRunoffModelProperties : ObjectProperties<RainfallRunoffModel>
    {
        [Description("Name of model")]
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }


        [Description("Status")]
        [Category("General")]
        [PropertyOrder(2)]
        public ActivityStatus Status
        {
            get { return data.Status; }
        }

        [TypeConverter(typeof (DeltaShellDateTimeConverter))]
        [PropertyOrder(3)]
        [Description("Start time for model run")]
        [Category("Run parameters")]
        [DisplayName("Start time")]
        public DateTime StartTime
        {
            get { return data.StartTime; }
            set { data.StartTime = value; }
        }

        [PropertyOrder(4)]
        [Description("Timestep interval")]
        [Category("Run parameters")]
        [DisplayName("Timestep")]
        [TypeConverter(typeof (DeltaShellTimeSpanConverter))]
        public TimeSpan TimeStep
        {
            get { return data.TimeStep; }
            set { data.TimeStep = value; }
        }


        [TypeConverter(typeof (DeltaShellDateTimeConverter))]
        [PropertyOrder(5)]
        [Description("Stop time for model run")]
        [DisplayName("Stop time")]
        [Category("Run parameters")]
        public DateTime StopTime
        {
            get { return data.StopTime; }
            set { data.StopTime = value; }
        }

        [TypeConverter(typeof (DeltaShellDateTimeConverter))]
        [PropertyOrder(8)]
        [DisplayName("Current time")]
        public DateTime CurrentTime
        {
            get { return data.CurrentTime; }
        }

        [PropertyOrder(7)]
        [Description("Use (restart) state as initial condition")]
        [DisplayName("Use restart")]
        [Category("Run parameters")]
        public bool UseRestart
        {
            get { return data.UseRestart; }
            set { data.UseRestart = value; }
        }

        [PropertyOrder(7)]
        [Description("Write final state (can be used to restart from)")]
        [DisplayName("Write restart")]
        [Category("Run parameters")]
        public bool WriteRestart
        {
            get { return data.WriteRestart; }
            set { data.WriteRestart = value; }
        }

        [PropertyOrder(8)]
        [Description("Write states on specified time instances")]
        [DisplayName("Use save state time range")]
        [Category("Run parameters")]
        public bool UseSaveStateTimeRange
        {
            get { return data.UseSaveStateTimeRange; }
            set { data.UseSaveStateTimeRange = value; } //RR does not support this feature yet
        }

        [PropertyOrder(9)]
        [Description("Start writing states when simulation time is equals or larger than this time")]
        [DisplayName("Save state start time")]
        [Category("Run parameters")]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        public DateTime SaveStateStartTime
        {
            get { return data.SaveStateStartTime; }
            set { data.SaveStateStartTime = value; }
        }

        [PropertyOrder(10)]
        [Description("Stop writing states when simulation time is beyond this time")]
        [DisplayName("Save state stop time")]
        [Category("Run parameters")]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        public DateTime SaveStateStopTime
        {
            get { return data.SaveStateStopTime; }
            set { data.SaveStateStopTime = value; }
        }

        [PropertyOrder(11)]
        [Description("Write state at each multiple of this time step after the 'Save state start time'")]
        [DisplayName("Save state time step")]
        [Category("Run parameters")]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        [DynamicReadOnly]
        public TimeSpan SaveStateTimeStep
        {
            get { return data.SaveStateTimeStep; }
            set { data.SaveStateTimeStep = value; }
        }

        [PropertyOrder(12)]
        [Category("General")]
        [DisplayName("Area unit")]
        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return data.AreaUnit; }
            set { data.AreaUnit = value; }
        }

        [PropertyOrder(13)]
        [Category("Fixed files")]
        [DisplayName(RainfallRunoffModelFixedFiles.UnpavedCropFactors)]
        [Editor(typeof (FixedFileTextDocumentPropertyGridEditor), typeof (UITypeEditor))]
        public TextDocument CropFactorsFile
        {
            get { return data.FixedFiles.UnpavedCropFactorsFile; }
            set { data.FixedFiles.UnpavedCropFactorsFile = value; }
        }

        [PropertyOrder(14)]
        [Category("Fixed files")]
        [DisplayName(RainfallRunoffModelFixedFiles.UnpavedStorageCoefficient)]
        [Editor(typeof (FixedFileTextDocumentPropertyGridEditor), typeof (UITypeEditor))]
        public TextDocument StorageCoeffFile
        {
            get { return data.FixedFiles.UnpavedStorageCoeffFile; }
            set { data.FixedFiles.UnpavedCropFactorsFile = value; }
        }

        [PropertyOrder(15)]
        [Category("Fixed files")]
        [DisplayName(RainfallRunoffModelFixedFiles.GreenhouseUsage)]
        [Editor(typeof (FixedFileTextDocumentPropertyGridEditor), typeof (UITypeEditor))]
        public TextDocument GreenhouseUsageFile
        {
            get { return data.FixedFiles.GreenhouseUsageFile; }
            set { data.FixedFiles.GreenhouseUsageFile = value; }
        }

        [PropertyOrder(16)]
        [Category("Fixed files")]
        [DisplayName(RainfallRunoffModelFixedFiles.GreenhouseStorage)]
        [Editor(typeof (FixedFileTextDocumentPropertyGridEditor), typeof (UITypeEditor))]
        public TextDocument GreenhouseStorageFile
        {
            get { return data.FixedFiles.GreenhouseStorageFile; }
            set { data.FixedFiles.GreenhouseStorageFile = value; }
        }

        [PropertyOrder(17)]
        [Category("Fixed files")]
        [DisplayName(RainfallRunoffModelFixedFiles.GreenhouseClasses)]
        [Editor(typeof (FixedFileTextDocumentPropertyGridEditor), typeof (UITypeEditor))]
        public TextDocument GreenhouseClassesFile
        {
            get { return data.FixedFiles.GreenhouseClassesFile; }
            set { data.FixedFiles.GreenhouseClassesFile = value; }
        }

        [PropertyOrder(18)]
        [Category("Fixed files")]
        [DisplayName(RainfallRunoffModelFixedFiles.OpenWaterCropFactor)]
        [Editor(typeof (FixedFileTextDocumentPropertyGridEditor), typeof (UITypeEditor))]
        public TextDocument OpenwaterCropFactorFile
        {
            get { return data.FixedFiles.OpenWaterCropFactorFile; }
            set { data.FixedFiles.OpenWaterCropFactorFile = value; }
        }

        [PropertyOrder(19)]
        [Category("Greenhouse")]
        [DisplayName("Minimum filling/storage percentage")]
        public double MinimumFillingStoragePercentage
        {
            get { return data.MinimumFillingStoragePercentage; }
            set { data.MinimumFillingStoragePercentage = value; }
        }

        [PropertyOrder(20)]
        [Category("Greenhouse")]
        [DisplayName("Greenhouse year")]
        [Description("Fill in historic year for data KasInit and Kasgebr (1951-1994)")]
        public short GreenhouseYear
        {
            get { return data.GreenhouseYear; }
            set
            {
                if (!value.IsInRange(RainfallRunoffModel.MinGreenhouseYear, RainfallRunoffModel.MaxGreenhouseYear))
                {
                    throw new ArgumentOutOfRangeException(nameof(GreenhouseYear), "Greenhouse year must be in the period between 1951 and 1994 (1994 is the default year).");
                }

                data.GreenhouseYear = value;
            }
        }

        [PropertyOrder(21)]
        [Category("Evaporation")]
        [DisplayName("Start active period")]
        public int EvaporationStartActivePeriod
        {
            get { return data.EvaporationStartActivePeriod; }
            set { data.EvaporationStartActivePeriod = value; }
        }

        [PropertyOrder(22)]
        [Category("Evaporation")]
        [DisplayName("End active period")]
        public int EvaporationEndActivePeriod
        {
            get { return data.EvaporationEndActivePeriod; }
            set { data.EvaporationEndActivePeriod = value; }
        }

        [PropertyOrder(23)]
        [Category("CapSim")]
        [DisplayName("CapSim calculation")]
        public bool Capsim
        {
            get { return data.CapSim; }
            set { data.CapSim = value; }
        }

        [PropertyOrder(24)]
        [Category("CapSim")]
        [DisplayName("Initial option")]
        [DynamicReadOnly]
        public RainfallRunoffEnums.CapsimInitOptions CapsimInitOption
        {
            get { return data.CapSimInitOption; }
            set { data.CapSimInitOption = value; }
        }

        [PropertyOrder(25)]
        [Category("CapSim")]
        [DisplayName("Crop area option")]
        [DynamicReadOnly]
        public RainfallRunoffEnums.CapsimCropAreaOptions CapsimCropAreaOption
        {
            get { return data.CapSimCropAreaOption; }
            set { data.CapSimCropAreaOption = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == nameof(CapsimInitOption) ||
                propertyName == nameof(CapsimCropAreaOption))
            {
                return !data.CapSim;
            }

            if (propertyName == nameof(SaveStateStartTime) ||
                propertyName == nameof(SaveStateStopTime) ||
                propertyName == nameof(SaveStateTimeStep))
            {
                return !data.UseSaveStateTimeRange;
            }

            return false;
        }
    }
}