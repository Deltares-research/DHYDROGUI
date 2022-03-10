using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekStructureFriction
    {
        internal readonly IFunction mainFrictionFuncPositive;
        private double mainConstPositive;
        private readonly IFunction groundLayerFriction;

        public SobekStructureFriction()
        {
            var flowUnit = new Unit("flow", "Q");
            var depthUnit = new Unit("depth", "H");

            mainFrictionFuncPositive = new Function("MainFriction");
            mainFrictionFuncPositive.Components.Add(new Variable<double>("water_flow", flowUnit));
            mainFrictionFuncPositive.Components.Add(new Variable<double>("water_depth", depthUnit));
            mainFrictionFuncPositive.Arguments.Add(new Variable<double>("Friction"));

            groundLayerFriction = new Function("GroundLayerFriction");
            groundLayerFriction.Components.Add(new Variable<double>("water_flow", flowUnit));
            groundLayerFriction.Components.Add(new Variable<double>("water_depth", depthUnit));
            groundLayerFriction.Arguments.Add(new Variable<double>("Friction"));
        }

        public string ID { get; set; }

        /// <summary>
        /// Id of the structure DEFINITION the friction data applies to
        /// </summary>
        public string StructureDefinitionID { get; set; }

        /// <summary>
        /// The friction type of the main channel. This value applies to the positive and the negeative direction.
        /// 0 = Chezy
        /// 1 = Manning
        /// 2 = Strickler Kn
        /// 3 = Strickler Ks
        /// 4 = White-Colebrook
        /// 7 = De Bos and Bijkerk
        /// </summary>
        public int MainFrictionType { get; set; }

        public int FloodPlain1FrictionType { get; set; }
        public int FloodPlain2FrictionType { get; set; }
        public int GroundLayerFrictionType { get; set; }
        public double GroundLayerFrictionValue { get; set; }
        public double MainFrictionConst
        {
            get { return mainConstPositive; }
            set { mainConstPositive = value; }
        }

        public SobekFrictionFunctionType MainFrictionFunctionType { get; set; }

        internal void AddMainPositiveFriction(string frictionRecord, string frictionType)
        {
            string[] results = Regex.Split(frictionRecord, @"[\t\s]+");
            double value = NumUtils.ConvertToDouble(results[1]);
            double friction = NumUtils.ConvertToDouble(results[2]);


            switch (frictionType)
            {
                case "Q":
                    MainFrictionFunctionType = SobekFrictionFunctionType.FunctionOfQ;
                    mainFrictionFuncPositive.Components[0][friction] = value;
                    break;
                case "H":
                    MainFrictionFunctionType = SobekFrictionFunctionType.FunctionOfH;
                    mainFrictionFuncPositive.Components[1][friction] = value;
                    break;
                case "CONST":
                    MainFrictionFunctionType = SobekFrictionFunctionType.Constant;
                    mainConstPositive = value;
                    break;
            }
        }
    }
}