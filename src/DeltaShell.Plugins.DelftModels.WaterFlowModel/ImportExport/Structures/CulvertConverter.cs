using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory" /> objects into <see cref="Culvert" /> objects.
    /// </summary>
    /// <seealso cref="StructureConverter" />
    public class CulvertConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Culvert
            {
                CulvertType = CulvertType.Culvert
            };
        }

        protected override void SetStructureProperties()
        {
            if (Structure is ICulvert culvert)
            {
                SetCommonCulvertProperties(culvert);
            }
        }

        protected static void SetCommonCulvertProperties(ICulvert culvert)
        {
            culvert.FlowDirection = (FlowDirection) Category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            culvert.InletLevel = Category.ReadProperty<double>(StructureRegion.LeftLevel.Key);
            culvert.OutletLevel = Category.ReadProperty<double>(StructureRegion.RightLevel.Key);

            culvert.Length = Category.ReadProperty<double>(StructureRegion.Length.Key);
            culvert.InletLossCoefficient = Category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key);
            culvert.OutletLossCoefficient = Category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key);
            culvert.GateInitialOpening = Category.ReadProperty<double>(StructureRegion.IniValveOpen.Key);

            culvert.IsGated = Convert.ToBoolean(Category.ReadProperty<int>(StructureRegion.ValveOnOff.Key));

            SetFrictionValues(culvert);
            SetGateOpeningLossCoefficientFunctionValues(culvert);
        }

        private static void SetFrictionValues(ICulvert culvert)
        {
            culvert.FrictionDataType = (Friction) Category.ReadProperty<int>(StructureRegion.BedFrictionType.Key);
            var bedFriction = Category.ReadProperty<double>(StructureRegion.BedFriction.Key);
            var groundFriction = Category.ReadProperty<double>(StructureRegion.GroundFriction.Key);
            culvert.Friction = bedFriction;
            if (Math.Abs(groundFriction - bedFriction) > double.Epsilon)
            {
                culvert.GroundLayerRoughness = groundFriction;
                culvert.GroundLayerEnabled = true;
            }
        }

        private static void SetGateOpeningLossCoefficientFunctionValues(ICulvert culvert)
        {
            try
            {
                var numberOfFunctionEntries = Category.ReadProperty<int>(StructureRegion.LossCoeffCount.Key);
                if (numberOfFunctionEntries <= 0) return;

                var relativeOpeningValues = TransformToDoubleArray(Category.ReadProperty<string>(StructureRegion.RelativeOpening.Key));
                var lossCoefficientValues = TransformToDoubleArray(Category.ReadProperty<string>(StructureRegion.LossCoefficient.Key));
                for (var i = 0; i < numberOfFunctionEntries; i++)
                {
                    culvert.GateOpeningLossCoefficientFunction[relativeOpeningValues[i]] = lossCoefficientValues[i];
                }
            }
            catch (Exception)
            {
                // Skip setting values on culvert.GateOpeningLossCoefficientFunction, because there was something wrong when reading it.
                culvert.GateOpeningLossCoefficientFunction.RemoveValues();
            }
        }
    }
}