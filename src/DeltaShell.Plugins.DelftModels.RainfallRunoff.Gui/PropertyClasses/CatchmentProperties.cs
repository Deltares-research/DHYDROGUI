using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    [DisplayName("Catchment")]
    public class CatchmentProperties : ObjectProperties<Catchment>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CatchmentProperties));

        [Browsable(false)]
        internal CatchmentModelData CatchmentData { get; set; }

        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.SetNameIfValid(value); }
        }

        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        public CatchmentType CatchmentType
        {
            get
            {
                return data.CatchmentType;
            }
        }

        [Category("General")]
        [DisplayName("Catchment type")]
        [PropertyOrder(2)]
        public virtual CatchmentTypes CatchmentTypes
        {
            get
            {
                return data.CatchmentType.Types;
            }
            set
            {
                data.CatchmentTypes = value;
            }
        }
        
        [Description("Catchment area based on input data used for computation.\n" +
                     "This can differ from the geometry area")]
        [Category("General")]
        [DisplayName("Computation area (m²)")]
        [PropertyOrder(3)]
        public double ComputationArea
        {
            get { return CatchmentData?.CalculationArea ?? double.NaN; }
            set
            {
                if (CatchmentData == null)
                {
                    log.Error($"Could not set {data.Name} computation area");
                    return;
                }

                CatchmentData.CalculationArea = value;
            }
        }

        [Description("Catchment area based on geometry.")]
        [Category("General")]
        [DisplayName("Geometry area (m²)")]
        [PropertyOrder(4)]
        public double GeometryArea
        {
            get { return data.Geometry?.Area ?? double.NaN; }
        }

        [Description("If TRUE, catchment geometry is derived from computation area")]
        [Category("General")]
        [DisplayName("Generated geometry")]
        [PropertyOrder(5)]
        public bool IsDefaultGeometry
        {
            get { return data.IsGeometryDerivedFromAreaSize; }
        }
    }
}