using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder
{
    [Entity(FireOnCollectionChange=false)]
    public class PolderConcept : CatchmentModelData
    {
        protected PolderConcept():base(null) { }

        public PolderConcept(Catchment catchment)
            : base(catchment)
        {
            CalculationArea = catchment.AreaSize;
        }
        
        public double PavedArea
        {
            get { return Paved != null ? Paved.CalculationArea : 0.0; }
            set { SetModelDataCalculationArea(value, Paved); }
        }

        public double UnpavedArea
        {
            get { return Unpaved != null ? Unpaved.CalculationArea : 0.0; }
            set { SetModelDataCalculationArea(value, Unpaved); }
        }

        public double GreenhouseArea
        {
            get { return Greenhouse != null ? Greenhouse.CalculationArea : 0.0; }
            set { SetModelDataCalculationArea(value, Greenhouse); }
        }

        public double OpenWaterArea
        {
            get { return OpenWater != null ? OpenWater.CalculationArea : 0.0; }
            set { SetModelDataCalculationArea(value, OpenWater); }
        }

        public PavedData Paved
        {
            get { return SubCatchmentModelData.OfType<PavedData>().FirstOrDefault(); }
        }

        public UnpavedData Unpaved
        {
            get { return SubCatchmentModelData.OfType<UnpavedData>().FirstOrDefault(); }
        }

        public GreenhouseData Greenhouse
        {
            get { return SubCatchmentModelData.OfType<GreenhouseData>().FirstOrDefault(); }
        }

        public OpenWaterData OpenWater
        {
            get { return SubCatchmentModelData.OfType<OpenWaterData>().FirstOrDefault(); }
        }

        /// <summary>
        /// todo: fix this
        /// </summary>
        public override double CalculationArea
        {
            get { return PavedArea + UnpavedArea + GreenhouseArea + OpenWaterArea; }
            set { base.CalculationArea = value; }
        }

        [EditAction]
        private void SetModelDataCalculationArea(double value, CatchmentModelData catchmentModelData)
        {
            if (catchmentModelData != null)
            {
                catchmentModelData.CalculationArea = value;
            }
        }
    }
}