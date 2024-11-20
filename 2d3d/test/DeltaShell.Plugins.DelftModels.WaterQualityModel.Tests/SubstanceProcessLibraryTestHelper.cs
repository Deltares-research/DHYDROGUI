using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    public static class SubstanceProcessLibraryTestHelper
    {
        public static SubstanceProcessLibrary CreateDefaultSubstanceProcessLibrary()
        {
            var substanceProcessLibrary = new SubstanceProcessLibrary();

            substanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "Salinity",
                Active = true,
                InitialValue = 30.0
            });

            substanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "Temperature",
                Active = true,
                InitialValue = 20.0
            });

            substanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "OXY",
                Active = true,
                InitialValue = 3.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "SaturOXY",
                Unit = "-"
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "ReaerOXY",
                Unit = "-"
            });

            substanceProcessLibrary.Processes.Add(new WaterQualityProcess {Name = "OXYSAT"});

            return substanceProcessLibrary;
        }

        public static SubstanceProcessLibrary CreateDemoSubstanceProcessLibrary()
        {
            var substanceProcessLibrary = new SubstanceProcessLibrary {Name = "Substances and Processes"};

            substanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "Cd",
                Description = "Cadmium",
                Active = true,
                ConcentrationUnit = "gCd/m3",
                InitialValue = 0.1
            });

            substanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "Cu",
                Description = "Copper",
                Active = false,
                ConcentrationUnit = "gCu/m3",
                InitialValue = 0.2
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "kCd",
                Description = "Cadmium coeff.",
                Unit = "(g/m3)",
                DefaultValue = 0.01
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "kCu",
                Description = "Copper coeff.",
                Unit = "(g/m3)",
                DefaultValue = 0.02
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "fDfwastCu1",
                Description = "Diffusive waste flux Cu",
                Unit = "(gCd/m2/d)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "fDfwastCd1",
                Description = "Diffusive waste flux Cd",
                Unit = "(gCd/m2/d)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "fDfwastCu2",
                Description = "Diffusive waste flux Cu",
                Unit = "(gCd/m2/d)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "fDfwastCd2",
                Description = "Diffusive waste flux Cd",
                Unit = "(gCd/m2/d)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "Depth",
                Description = "Depth of sediment",
                Unit = "(m)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "sw1DfwaCu",
                Description = "Load option 0=all, 1=top, 2= bottom seg",
                Unit = "(m)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "sw1DfwaCd",
                Description = "Load option 0=all, 1=top, 2= bottom seg",
                Unit = "(-)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "sw1AtmDCu",
                Description = "Load option 0=all, 1=top, 2= bottom seg",
                Unit = "(-)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "sw1AtmDCd",
                Description = "Load option 0=all, 1=top, 2= bottom seg",
                Unit = "(-)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Parameters.Add(new WaterQualityParameter
            {
                Name = "sw2DfwaCu",
                Description = "MaximiseWithdrawel to mass 0=no, 1=ye",
                Unit = "(-)",
                DefaultValue = 0.0
            });

            substanceProcessLibrary.Processes.Add(new WaterQualityProcess
            {
                Name = "AtmDep_Cd",
                Description = "Atmospheric deposition Cd"
            });

            substanceProcessLibrary.Processes.Add(new WaterQualityProcess
            {
                Name = "Dfwast_Cd",
                Description = "Diffusive waste Cd"
            });

            substanceProcessLibrary.Processes.Add(new WaterQualityProcess
            {
                Name = "AtmDep_Cu",
                Description = "Atmospheric deposition Cu"
            });

            substanceProcessLibrary.Processes.Add(new WaterQualityProcess
            {
                Name = "Dfwast_Cu",
                Description = "Diffusive waste Cu"
            });

            substanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
            {
                Name = "AlgN",
                Description = "total nitrogen in algae",
                ShowInHis = true,
                ShowInMap = true
            });

            substanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
            {
                Name = "TotN",
                Description = "total nitrogen (incl. algae)",
                ShowInHis = true,
                ShowInMap = false
            });

            substanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
            {
                Name = "KjelN",
                Description = "Kjeldahl nitrogen",
                ShowInHis = false,
                ShowInMap = true
            });

            return substanceProcessLibrary;
        }
    }
}