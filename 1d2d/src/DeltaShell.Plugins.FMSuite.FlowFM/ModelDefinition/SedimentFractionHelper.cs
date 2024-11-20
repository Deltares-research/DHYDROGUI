using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public static class SedimentFractionHelper
    {
        public static EventedList<ISedimentProperty> GetSedimentationOverAllProperties()
        {
            return new EventedList<ISedimentProperty>()
            {
                new SedimentProperty<double>("Cref", 1600, 0, true, 9999.9999, false, "kg/m³","Reference density for hindered settling calculations", true)
            };
        }

        public static List<ISedimentType> GetSedimentationTypes()
        {
            return new List<ISedimentType>()
            {
                new SedimentType()
                {
                    Name = "Sand",
                    Key = "sand",
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, double.MaxValue, true, "kg/m³", "Initial Concentration", true, false),
                        new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, double.MaxValue, true, "m","Initial sediment layer thickness at bed", false, false),
                        new SedimentProperty<double>("FacDss", 1, 0.6, false, 1, false, string.Empty, "Factor for suspended sediment diameter", true),
                        new SedimentProperty<double>("RhoSol", 2650, 0, true, 10000, true, "kg/m³", "Specific density", false),
                        new SedimentProperty<int>("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true),
                        new SedimentProperty<double>("CDryB", 1600, 0, true, 10000, true, "kg/m³", "Dry bed density", false),
                        new SedimentProperty<double>("SedDia", 0.0002, 0.000063, false, 0.002, false, "m", "Median sediment diameter (D50)", false),
                    }
                },
                new SedimentType()
                {
                    Name = "Mud",
                    Key = "mud",
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, double.MaxValue, true, "kg/m³", "Initial Concentration", true, false),
                        new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, double.MaxValue, true, "m", "Initial sediment layer thickness at bed", false, false),
                        new SedimentProperty<double>("FacDss", 1, 0.6, false, 1, false, string.Empty, "Factor for suspended sediment diameter", true),
                        new SedimentProperty<double>("RhoSol", 2650, 0, true, 10000, true, "kg/m³", "Specific density", false),
                        new SedimentProperty<int>("TraFrm", -3, -3, false, -3, false, string.Empty, "Integer selecting the transport formula", true),
                        new SedimentProperty<double>("CDryB", 500, 0, true, 10000, true, "kg/m³", "Dry bed density", false),
                        
                        // Should these be in TransportFormula? (-3)
                        new SedimentProperty<double>("SalMax", 31, 0.01, false, 391, true, "ppt", "Salinity for saline settling velocity", false),
                        new SedimentProperty<double>("WS0", 0.00025, 0, true, 1, true, "m/s", "Settling velocity fresh water", false),
                        new SedimentProperty<double>("WSM", 0.00025, 0, true, 1, true, "m/s", "Settling velocity saline water", false),
                    }
                },
                new SedimentType()
                {
                    Name = "Bed-load",
                    Key = "bedload",
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, double.MaxValue, true, "m", "Initial sediment layer thickness at bed", false, false),
                        new SedimentProperty<double>("RhoSol", 2650, 0, true, 10000, true, "kg/m³", "Specific density", false),
                        new SedimentProperty<int>("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true),
                        new SedimentProperty<double>("CDryB", 1600, 0, true, 10000, true, "kg/m³", "Dry bed density", false),
                        new SedimentProperty<double>("SedDia", 0.0002, 0.000063, false, 0.002, false, "m", "Median sediment diameter (D50)", false),
                    }
                },
            };
        }

        public static List<ISedimentFormulaType> GetSedimentationFormulas()
        {
            return new List<ISedimentFormulaType>()
            {
                new SedimentFormulaType()
                {
                    Name = "Partheniades-Krone",
                    TraFrm = -3,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("EroPar", 0.0001, 0, true,  double.MaxValue, true, "kg/m²s", "Erosion parameter", false),
                        new SpatiallyVaryingSedimentProperty<double>("TcrSed", 1000, 0, true, double.MaxValue, true, "N/m²", "Critical stress for sedimentation", false, false),
                        new SpatiallyVaryingSedimentProperty<double>("TcrEro", 0.5, 0, true, double.MaxValue, true, "N/m²", "Critical stress for erosion", false, false),
                        new SedimentProperty<double>("TcrFluff", Double.Epsilon, 0, true, double.MaxValue, true, "N/m²", "Critical stress for fluff layer erosion", false, null,
                            sediments =>
                            {
                                var sedimentType = sediments.OfType<ISedimentType>().FirstOrDefault();
                                if (sedimentType == null) return true;
                                if (sedimentType.Key == "mud")
                                    return false;
                                return true;
                            }),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Van Rijn (2007): TRANSPOR2004",
                    TraFrm = -2,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<int>("IopSus", 0, 0, false, 1, false, string.Empty, "Option for determining suspended sediment diameter", false),
                        new SedimentProperty<double>("Pangle", 0, 0, false, 360, false, "degrees", "Phase lead angle", false),
                        new SedimentProperty<double>("Fpco", 1, -1, false, 1, false, string.Empty, "Coefficient for phase lag effects", false),
                        new SedimentProperty<double>("Subiw", 51, 0, true, double.MaxValue, true, string.Empty, "Wave period subdivision", false),
                        new SedimentProperty<bool>("EpsPar", false, false, false, true, false, string.Empty, "Use Van Rijn's parabolic mixing coefficient", false),
                        new SedimentProperty<double>("GamTcr", 1.5, 1, false, double.MaxValue, true, string.Empty, "Coefficient for grain size effect", false),
                        new SedimentProperty<double>("SalMax", 0, 0, false, double.MaxValue, true, "ppt", "Salinity for saline settling velocity", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Van Rijn (1993)",
                    TraFrm = -1,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<int>("IopSus", 0, 0, false, 1, false, string.Empty, "Option for determining suspended sediment diameter", false),
                        new SedimentProperty<double>("AksFac", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration factor for Van Rijn’s reference height", false),
                        new SedimentProperty<double>("Rwave", 2, 0, true, double.MaxValue, true, string.Empty, "Calibration factor wave roughness height", false),
                        new SedimentProperty<double>("RDC", 0.01, 0, true, double.MaxValue, true, "m", "Current related roughness ks", false),
                        new SedimentProperty<double>("RDW", 0.02, 0, true, double.MaxValue, true, "m", "Wave related roughness kw", false),
                        new SedimentProperty<int>("IopKCW", 1, 0, false, 1, false, string.Empty, "Option for ks and kw", true),
                        new SedimentProperty<bool>("EpsPar", false, false, false, true, false, string.Empty, "Use Van Rijn's parabolic mixing coefficient", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Engelund-Hansen (1967)",
                    TraFrm = 1,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                        new SedimentProperty<double>("SusFac", 0, 0, false, 1, false, string.Empty, "Fraction transported as suspended load (0 to 1)", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Meyer-Peter-Mueller (1948)",
                    TraFrm = 2,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Swanby / Ackers-White",
                    TraFrm = 3,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "General formula",
                    TraFrm = 4,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 8, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                        new SedimentProperty<double>("PowerB", 0, 0, false, double.MaxValue, true, string.Empty, "Power b", false),
                        new SedimentProperty<double>("PowerC", 1.5, 0, false, double.MaxValue, true, string.Empty, "power c", false),
                        new SedimentProperty<double>("RipFac", 1, 0, true, double.MaxValue, true, string.Empty, "Ripple factor or efficiency factor", false),
                        new SedimentProperty<double>("ThetaC", 0.047, 0, true, double.MaxValue, true, string.Empty, "Critical mobility factor", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Bijker (1971)",
                    TraFrm = 5,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("CalBs", 5, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient b for shallow water", false),
                        new SedimentProperty<double>("CalBd", 2, 0, true,double.MaxValue, true, string.Empty, "Calibration coefficient b for deep water", false),
                        new SedimentProperty<double>("CritCs", 0.05, 0, true,1, true, string.Empty, "Shallow water(hw/h) criterion", false),
                        new SedimentProperty<double>("CritCd", 0.4, 0, true, 1, true, string.Empty, "Deep water(hw/h) criterion", false),
                        new SedimentProperty<double>("RouKs", 0.01, 0, true, double.MaxValue, true, "m", "Bed roughness height", false),
                        new SedimentProperty<double>("Wsettle", 0.001, 0, true, double.MaxValue, true, "m/s", "Settling velocity", false),
                        new SedimentProperty<double>("Porosity", 0.4, 0, true, 1, true, string.Empty, "Bed porosity", false),
                        new SedimentProperty<double>("Twave", 5, 0, true, double.MaxValue, true, "s", "Default wave period", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Van Rijn (1984)",
                    TraFrm = 7,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                        new SedimentProperty<double>("Aks", 0.1, 0, true, double.MaxValue, true, "m", "Reference height", false),
                        new SedimentProperty<double>("Wsettle", 0.001, 0, true, double.MaxValue, true, "m/s", "Settling velocity", false),
                        new SedimentProperty<double>("BetaM", 0, 0, false, double.MaxValue, true, string.Empty, "Effect of mud on critical bed shear stress", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Soulsby / Van Rijn",
                    TraFrm = 11,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                        new SedimentProperty<double>("RatioD90D50", 1.5, 1, true, double.MaxValue, true, string.Empty, "D90/D50 ratio", false),
                        new SedimentProperty<double>("RouZ0", 0.01, 0, true, double.MaxValue, true, "m", "Roughness height", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Soulsby",
                    TraFrm = 12,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                        new SedimentProperty<int>("ModInd", 1, 1, false, 8, false, string.Empty, "Model index", false),
                        new SedimentProperty<double>("RatioD50Z0", 0.2, 0, true, double.MaxValue,true,  string.Empty, "D50/z0 ratio", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Ashida-Michiue (1974)",
                    TraFrm = 14,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                        new SedimentProperty<double>("ThetaC", 0.05, 0, false, double.MaxValue, true, string.Empty, "Critical mobility factor", false),
                        new SedimentProperty<double>("PowerM", 1.5, 0, false, double.MaxValue, true, string.Empty, "Power m", false),
                        new SedimentProperty<double>("PowerP", 1, 0, false, double.MaxValue, true, string.Empty, "Power p", false),
                        new SedimentProperty<double>("PowerQ", 1, 0, false, double.MaxValue, true, string.Empty, "Power q", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Wilcock-Crowe (2003)",
                    TraFrm = 16,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ACal", 1, 0, true, double.MaxValue, true, string.Empty, "Calibration coefficient", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Gaeuman et. al. (2009) lab calibration",
                    TraFrm = 17,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ThetaC0", 0.021, 0, false, double.MaxValue, true, string.Empty, "Calibration coefficient theta", false),
                        new SedimentProperty<double>("Alpha0", 0.33, 0, false, 1, true, string.Empty, "Calibration coefficient alpha", false),
                    }
                },
                new SedimentFormulaType()
                {
                    Name = "Gaeuman et. al. (2009) Trinity River",
                    TraFrm = 18,
                    Properties = new EventedList<ISedimentProperty>()
                    {
                        new SedimentProperty<double>("ThetaC0", 0.03, 0, false, double.MaxValue, true, string.Empty, "Calibration coefficient theta", false),
                        new SedimentProperty<double>("Alpha0", 0.3, 0, false, 1, true, string.Empty, "Calibration coefficient alpha", false),
                    }
                },
            };
        }
    }
}
