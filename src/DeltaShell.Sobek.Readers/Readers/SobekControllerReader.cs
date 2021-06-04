using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekControllerReader : SobekReader<SobekController>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekControllerReader));

        public static bool Sobek2Import; //needed for PIDController
        public static TimeSpan TimeStepModel = new TimeSpan(0,0,1); //needed for PIDController

        public override IEnumerable<SobekController> Parse(string datFileText)
        {
            const string pattern = @"CNTL\s(?<row>(?'text'.*?))\scntl";

            foreach (Match controllerMatch in RegularExpression.GetMatches(pattern, datFileText))
            {
                SobekController sobekController;

                try
                {
                    sobekController = GetSobekController(controllerMatch.Groups["row"].Value);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Error while parsing controller string: {0}, skipping ...", controllerMatch.Groups["row"].Value);
                    continue;
                }

                if ( sobekController != null)
                {
                    yield return  sobekController;
                }
            }
        }

        private static SobekController GetSobekController(string rowText)
        {
            //id  = id of the controller definition
            //nm  = name of the controller definition
            //ct = controller type
            //0 = time controller
            //ca = controlled parameter
            //0 = crest level
            //1 = crest width; (not in Urban/Rural Controller)                    
            //2 = gate height
            //3 = pump capacity (SOBEK Urban/Rural; not implemented)
            //4 =  
            //5 = bed level of 2D grid cell
            //ac = controlled active yes/no
            //1 = active
            //0 = inactive
            //cf = update frequency (number of timesteps)
            //ta  =  trigger active (not in Urban/Rural Controller, 4 in River Controller)
            //0 = not active
            //1 = active
            //gi  = id of trigger description, -1 in case of non-active triggers (no triggers available for Urban/Rural Controller, maximum of 4 triggers for a River Controller)
            //ao = and(=1)/or(=0) relations when using more triggers (not in Urban/Rural Controller, maximum of 4 triggers for a River Controller)

            const string patternType212 = @"(ct\s(?<controllertype>" + RegularExpression.Integer + @")\s)?" +
                                       @"(ca\s(?<controlledparameter>" + RegularExpression.Integer + @")\s)?" +
                                       @"(ac\s(?<active>" + RegularExpression.Integer + @")\s)?" +
                                       @"(cf\s(?<cf>" + RegularExpression.Integer + @")\s)?";

            const string patternTypeRE = @"(ct\s(?<controllertype>" + RegularExpression.Integer + @")\s)?" +
                                        @"(ac\s(?<active>" + RegularExpression.Integer + @")\s)?" +
                                        @"(ca\s(?<controlledparameter>" + RegularExpression.Integer + @")\s)?" +
                                        @"(cf\s(?<cf>" + RegularExpression.Integer + @")\s)?";

            const string pattern = @"id\s'(?<id>" + RegularExpression.Characters + @")'\s" +
                                   @"(nm\s'(?<name>" + RegularExpression.ExtendedCharacters + @")'\s)?" +
                                   patternType212 +
                                   @"(ta\s(?<triggeractive0>" + RegularExpression.Integer +
                                   @")\s(?<triggeractive1>" + RegularExpression.Integer + @")\s(?<triggeractive2>" +
                                   RegularExpression.Integer + @")\s(?<triggeractive3>" +
                                   RegularExpression.Integer + @")\s)?" +
                                   @"(gi\s'(?<triggerid0>" + RegularExpression.ExtendedCharacters + @")'\s'(?<triggerid1>" +
                                   RegularExpression.ExtendedCharacters + @")'\s'(?<triggerid2>" + RegularExpression.ExtendedCharacters +
                                   @")'\s'(?<triggerid3>" + RegularExpression.ExtendedCharacters + @")'\s)?" +
                                   @"(ao\s(?<andorIgnore>" + RegularExpression.Integer + @")\s(?<andor0>" +
                                   RegularExpression.Integer + @")\s(?<andor1>" + RegularExpression.Integer +
                                   @")\s(?<andor2>" + RegularExpression.Integer + @")\s)?" +
                                   patternTypeRE +
                                   @"(?<properties>.*)";

            var matches = RegularExpression.GetMatches(pattern, rowText);

            if(matches.Count == 1)
            {
                var id = "CTR_" + matches[0].Groups["id"].Value;
                var name = matches[0].Groups["name"].Value;
                var controllertype = Convert.ToInt32((string) matches[0].Groups["controllertype"].Value);
                var controlledparameter = Convert.ToInt32((string) matches[0].Groups["controlledparameter"].Value);
                var active = matches[0].Groups["active"].Value;

                var triggeractive0 = matches[0].Groups["triggeractive0"].Value;
                var triggeractive1 = matches[0].Groups["triggeractive1"].Value;
                var triggeractive2 = matches[0].Groups["triggeractive2"].Value;
                var triggeractive3 = matches[0].Groups["triggeractive3"].Value;

                var triggerid0 = "TRG_" + matches[0].Groups["triggerid0"].Value;
                var triggerid1 = "TRG_" + matches[0].Groups["triggerid1"].Value;
                var triggerid2 = "TRG_" + matches[0].Groups["triggerid2"].Value;
                var triggerid3 = "TRG_" + matches[0].Groups["triggerid3"].Value;

                var andor0 = matches[0].Groups["andor0"].Value;
                var andor1 = matches[0].Groups["andor1"].Value;
                var andor2 = matches[0].Groups["andor2"].Value;

                var properties = matches[0].Groups["properties"].Value;

                var sobekController = new SobekController();

                sobekController.Id = id;
                sobekController.Name = name;
                sobekController.ControllerType = (SobekControllerType)controllertype;
                sobekController.SobekControllerParameterType = (SobekControllerParameter)controlledparameter;
                sobekController.IsActive = (active != "0");
                sobekController.Triggers = new List<Trigger>
                                               {
                                                  new Trigger{
                                                                Id = triggerid0,
                                                                Active = (triggeractive0 == "1"),
                                                                And = (andor0 == "1"),
                                                             },
                                                  new Trigger{
                                                                Id = triggerid1,
                                                                Active = (triggeractive1 == "1"),
                                                                And = (andor1 == "1"),
                                                             },
                                                  new Trigger{
                                                                Id = triggerid2,
                                                                Active = (triggeractive2 == "1"),
                                                                And = (andor2 == "1"),
                                                             }, 
                                                 new Trigger{
                                                                Id = triggerid3,
                                                                Active = (triggeractive3 == "1"),
                                                                And = true, //not relevant
                                                             } 
                                               };


                switch (sobekController.ControllerType)
                {
                    case SobekControllerType.HydraulicController:
                        ReadHydraulicControllerProperties(sobekController, properties);
                        break;
                    case SobekControllerType.IntervalController:
                        ReadIntervalControllerProperties(sobekController, properties);
                        break;
                    case SobekControllerType.PIDController:
                        ReadPIDControllerProperties(sobekController, properties);
                        break;
                    case SobekControllerType.RelativeFromValueController:
                        ReadRelativeTimeControllerProperties(sobekController,properties);
                        break;
                    case SobekControllerType.RelativeTimeController:
                        ReadRelativeFromValueControllerProperties(sobekController, properties);
                        RemoveInputLocationSobekController(sobekController);
                        break;
                    case SobekControllerType.TimeController:
                        ReadTimeControllerProperties(sobekController, properties);
                        RemoveInputLocationSobekController(sobekController);
                        break;
                    default:
                        throw new NotSupportedException(sobekController.ControllerType + " has not been supported");
                }


                return sobekController;
            }

            return null;
        }

        /// <summary>
        /// In Sobek files input locations can still be defined for controllers which don't have an input location (after changing type)
        /// </summary>
        /// <param name="sobekController"></param>
        private static void RemoveInputLocationSobekController(SobekController sobekController)
        {
            sobekController.MeasurementStationId = "";
            sobekController.StructureId = "";
        }

        private static void ReadHydraulicControllerProperties(SobekController sobekController, string properties)
        {
            //ml = id of measurement node (5 locations in River Controller, at present 1 in Urban/Rural Controller)
            //mp = time lag between controlling parameter and controlled parameter
            //cb = id of branch used for measuring (control branch) 
            //(5 locations in River Controller, not in Urban/Rural Controller)
            //cl  = location (relative to beginning of branch) used for measuring (control location)
            //(5 locations in River Controller, not in Urban/Rural Controller)
            //cp = type of measured parameter 
            //0 = water level (on branch cb location cl)
            //1 = discharge (on branch cb location cl)
            //The following types of control parameters are available in River but not in Urban/Rural Controller:
            //2 = head difference (at a structure)
            //3 = velocity (on branch cb location cl)
            //4 = flow direction 
            //5 = pressure difference 
            //b1   = Interpolation method table (only SOBEK Urban/Rural)
            //0 = none (block function)
            //1 = linear
            //hc ht  = control table with relation between measured and controller parameter
            //column 1 = measured parameter or summons of measured parameters
            //column 2 = settings of the controlled parameter
            //bl  = branch location used (not in Urban/Rural Controller)
            //0 = no
            //1 = yes
            //whether the 5 possible branch locations are being used by the hydraulic controller; the table contains de value of the controlled parameter for the total of the selected parameter. Note: this is sensitive to the branch direction.
            //si  = structure id (only for controlling head difference or pressure difference; not in Urban/Rural Controller)
            //ps  = positive stream (only for controlling flow direction: control parameter 4=stream direction; not in Urban/Rural Controller)
            //ns  = negative stream (only when using control parameter 4=stream direction; not in Urban/Rural Controller)


            sobekController.LookUpTable = GetControllerTableAndInterExtrapolation(sobekController, SobekController.LookUpTableStructure, properties);

            SetMeasuremetLocationAndParameter(sobekController, properties);

            const string propertiesPatternMP = @"mp\s(?<timelag>" + RegularExpression.Integer + @")\s";
            const string propertiesPatternSI = @"si\s'(?<structureid>" + RegularExpression.ExtendedCharacters + @")'\s";
            const string propertiesFlowDirection = @"ps\s*(?<flowPos>" + RegularExpression.Scientific + @")\s*ns\s*(?<flowNeg>" + RegularExpression.Scientific + @")";


            var matches = RegularExpression.GetMatches(propertiesPatternMP, properties);
            if (matches.Count == 1)
            {
                var specificProperties = new SobekHydraulicControllerProperties();
                specificProperties.TimeLag = Convert.ToInt32((string) matches[0].Groups["timelag"].Value);
                sobekController.SpecificProperties = specificProperties;
            }

            matches = RegularExpression.GetMatches(propertiesPatternSI, properties);
            if (matches.Count == 1)
            {
                sobekController.StructureId = matches[0].Groups["structureid"].Value.Replace("##", "~~");
            }

            matches = RegularExpression.GetMatches(propertiesFlowDirection, properties);
            if (matches.Count == 1)
            {
                sobekController.PositiveStream = Convert.ToDouble(matches[0].Groups["flowPos"].Value,CultureInfo.InvariantCulture);
                sobekController.NegativeStream = Convert.ToDouble(matches[0].Groups["flowNeg"].Value, CultureInfo.InvariantCulture);
            }
        }

        private static void ReadIntervalControllerProperties(SobekController sobekController, string properties)
        {

                //cb = id of branch used for measuring (control branch) 
                //(not in Urban/Rural Controller)
                //cl = location (relative to beginning of branch) used for measuring (control location) ) 
                //(not in Urban/Rural Controller)
                //ml = id of measurement node
                //cp = type of measured parameter 
                //0 = water level (on branch cb location cl)
                //1 = discharge (on branch cb location cl)
                //ui = Us minimum 
                //ua = Us maximum
                //cn  = control interval type
                //0 = fixed interval
                //1 = variable
                //du  = d(U) (fixed interval)
                //cv  = control velocity (variable interval)
                //dt   = dead band type 
                //0 = fixed
                //1 = as percentage of the discharge (Not in Urban/Rural Controller)
                //d_  = dead band step size (fixed)
                //pe  = dead band percentage D (not in Urban/Rural Controller)
                //di  = minimum dead band value (not in Urban/Rural Controller)
                //da  = maximum dead band value (not in Urban/Rural Controller)
                //bl   = interpolation method table (only in Urban/Rural Controller)
                //0 = none (block function)
                //1 = linear
                //sp tc 0 = constant set point
                //sp tc 1 = table with set point varying in time:
                //column 1 = date/time stamp, 
                //column 2 = set points of the controlled parameter


            SetMeasuremetLocationAndParameter(sobekController, properties);

            const string propertiesPatternUSminimum = @"ui\s(?<usminimum>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternUSmaximum = @"ua\s(?<usmaximum>" + RegularExpression.Scientific + @")\s";
            const string propertiesControllerIntervalType = @"cn\s(?<contrintervaltype>" + RegularExpression.Integer + @")\s";
            const string propertiesPatternIntervalType = @"sp tc\s(?<intervaltype>" + RegularExpression.Integer + @")\s";
            const string propertiesPatternFixedInterval = @"du\s(?<fixedinterval>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternControlVelocity = @"cv\s(?<controlvelocity>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternDeadBandType = @"dt\s(?<deadbandtype>" + RegularExpression.Integer + @")\s";
            const string propertiesPatternDeadBandFixedSize = @"\sd_\s*(?<deadbandfixedsize>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternDeadBandPecentage = @"pe\s(?<deadbandpercentage>" + RegularExpression.Float + @")\s";
            const string propertiesPatternDeadBandMin = @"di\s(?<deadbandmin>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternDeadBandMax = @"da\s(?<deadbandmax>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternConstantSetPoint = @"sp tc\s*0\s*(?<constantsetpoint>" + RegularExpression.Scientific + @")";

            MatchCollection matches;

            var specificProperties = new SobekIntervalControllerProperties();
            sobekController.SpecificProperties = specificProperties;

            matches = RegularExpression.GetMatches(propertiesPatternUSminimum, properties);
            if (matches.Count == 1)
            {
                specificProperties.USminimum = ConversionHelper.ToDouble(matches[0].Groups["usminimum"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternUSmaximum, properties);
            if (matches.Count == 1)
            {
                specificProperties.USmaximum = ConversionHelper.ToDouble(matches[0].Groups["usmaximum"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesControllerIntervalType, properties);
            if (matches.Count == 1)
            {
                specificProperties.ControllerIntervalType = (IntervalControllerIntervalType)Convert.ToInt32(matches[0].Groups["contrintervaltype"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternIntervalType, properties);
            if (matches.Count == 1)
            {
                specificProperties.SetPointType = (IntervalControllerSetPointType)Convert.ToInt32(matches[0].Groups["intervaltype"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternFixedInterval, properties);
            if (matches.Count == 1)
            {
                specificProperties.FixedInterval = ConversionHelper.ToDouble(matches[0].Groups["fixedinterval"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternControlVelocity, properties);
            if (matches.Count == 1)
            {
                specificProperties.ControlVelocity = ConversionHelper.ToDouble(matches[0].Groups["controlvelocity"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternDeadBandType, properties);
            if (matches.Count == 1)
            {
                specificProperties.DeadBandType = (IntervalControllerDeadBandType)Convert.ToInt32(matches[0].Groups["deadbandtype"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternDeadBandFixedSize, properties);
            if (matches.Count == 1)
            {
                specificProperties.DeadBandFixedSize = ConversionHelper.ToDouble(matches[0].Groups["deadbandfixedsize"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternDeadBandPecentage, properties);
            if (matches.Count == 1)
            {
                specificProperties.DeadBandPecentage = ConversionHelper.ToDouble(matches[0].Groups["deadbandpercentage"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternDeadBandMin, properties);
            if (matches.Count == 1)
            {
                specificProperties.DeadBandMin = ConversionHelper.ToDouble(matches[0].Groups["deadbandmin"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternDeadBandMax, properties);
            if (matches.Count == 1)
            {
                specificProperties.DeadBandMax = ConversionHelper.ToDouble(matches[0].Groups["deadbandmax"].Value);
            }

            matches = RegularExpression.GetMatches( propertiesPatternConstantSetPoint, properties);
            if (matches.Count == 1)
            {
                specificProperties.ConstantSetPoint = ConversionHelper.ToDouble(matches[0].Groups["constantsetpoint"].Value);
            }
            else
            {
                sobekController.TimeTable = GetControllerTableAndInterExtrapolation(sobekController, SobekController.TimeTableStructure, properties);
            }
        }

        private static void ReadPIDControllerProperties(SobekController sobekController, string properties)
        {

            //cb  =  id of branch used for measuring (control branch) (not in Urban/Rural Controller)
            //Only one location can be used here, as opposed to the hydraulic controller, where 5 locations can be entered.
            //cl  =  control location (relative to the beginning of the branch) (not in Urban/Rural Controller)
            //ml = id of measurement node
            //cp  =  type of measured parameter 
            //0 = water level (on branch cb location cl)
            //1 = discharge (on branch cb location cl)
            //ui = Us minimum 
            //ua  = Us maximum
            //u0 = Us initial
            //pf  =  K factor proportional
            //if  = K factor Integral
            //df  = K factor differential
            //va  =  maximum speed of change (i.e. m/s for the crest of a movable weir) 
            //bl   = interpolation method table (only Urban/Rural Controller)
            //0 = none (block function)
            //1 = linear
            //sp tc 0 = constant set point
            //sp tc 1 =  table with set point varying in time:
            //column 1 = date/time stamp
            //column 2 = set points of the controlled parameter


            SetMeasuremetLocationAndParameter(sobekController, properties);

            const string propertiesPatternUSminimum = @"ui\s(?<usminimum>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternUSmaximum = @"ua\s(?<usmaximum>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternUSinitial = @"u0\s(?<usinitial>" + RegularExpression.Scientific + @")\s";

            const string propertiesPatternKFactorProportional = @"pf\s(?<kfp>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternKFactorIntegral = @"if\s(?<kfi>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternKFactorDifferential = @"df\s(?<kfd>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternMaximumSpeed = @"va\s(?<maximumspeed>" + RegularExpression.Scientific + @")\s";

            const string propertiesPatternConstantSetPoint = @"sp tc\s*0\s*(?<constantsetpoint>" + RegularExpression.Scientific + @")";

            MatchCollection matches;

 
            var specificProperties = new SobekPidControllerProperties();
            sobekController.SpecificProperties = specificProperties;

            matches = RegularExpression.GetMatches(propertiesPatternUSminimum, properties);
            if (matches.Count == 1)
            {
                specificProperties.USminimum = ConversionHelper.ToDouble(matches[0].Groups["usminimum"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternUSmaximum, properties);
            if (matches.Count == 1)
            {
                specificProperties.USmaximum = ConversionHelper.ToDouble(matches[0].Groups["usmaximum"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternUSinitial, properties);
            if (matches.Count == 1)
            {
                specificProperties.USinitial = ConversionHelper.ToDouble(matches[0].Groups["usinitial"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternKFactorProportional, properties);
            if (matches.Count == 1)
            {
                specificProperties.KFactorProportional = ConversionHelper.ToDouble(matches[0].Groups["kfp"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternKFactorIntegral, properties);
            if (matches.Count == 1)
            {
                specificProperties.KFactorIntegral = ConversionHelper.ToDouble(matches[0].Groups["kfi"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternKFactorDifferential, properties);
            if (matches.Count == 1)
            {
                specificProperties.KFactorDifferential = ConversionHelper.ToDouble(matches[0].Groups["kfd"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternMaximumSpeed, properties);
            if (matches.Count == 1)
            {
                specificProperties.MaximumSpeed = ConversionHelper.ToDouble(matches[0].Groups["maximumspeed"].Value);
            }

            matches = RegularExpression.GetMatches(propertiesPatternConstantSetPoint, properties);
            if (matches.Count == 1)
            {
                specificProperties.ConstantSetPoint = ConversionHelper.ToDouble(matches[0].Groups["constantsetpoint"].Value);
            }
            else
            {
                sobekController.TimeTable = GetControllerTableAndInterExtrapolation(sobekController, SobekController.TimeTableStructure, properties);
            }

            specificProperties.FromSobekType = Sobek2Import ? SobekType.Sobek212 : SobekType.SobekRE;
            specificProperties.TimeStepModel = TimeStepModel;
        }

        private static void ReadRelativeTimeControllerProperties(SobekController sobekController, string properties)
        {
            //mc = max. change van dValue / dT 
            //mp  = minimum period between two active periods of the time controller 
            //ti vv  =  table with set point varying in time:
            //column 1 = relative time
            //column 2 = set point of the controlled parameter

            //LookUpTable -> seconds, value
            sobekController.LookUpTable = GetControllerTableAndInterExtrapolation(sobekController, SobekController.LookUpTableStructure, properties);

            const string propertiesPatternMC = @"mc\s(?<maxchangevelocity>" + RegularExpression.Scientific + @")\s";

            var matches = RegularExpression.GetMatches(propertiesPatternMC, properties);
            if (matches.Count == 1 && matches[0].Groups["maxchangevelocity"].Value != "")
            {
                sobekController.MaxChangeVelocity = ConversionHelper.ToDouble(matches[0].Groups["maxchangevelocity"].Value);
            }

            var patternMP = RegularExpression.GetInteger("mp");
            matches = RegularExpression.GetMatches(patternMP, properties);
            if (matches.Count == 1 && matches[0].Groups["mp"].Value != "")
            {
                sobekController.MinimumPeriod = Convert.ToInt32(matches[0].Groups["mp"].Value);
            }
        }

        private static void ReadRelativeFromValueControllerProperties(SobekController sobekController, string properties)
        {
            //mc = max. change van dValue / dT 
            //mp  = minimum period between two active periods of the time controller 
            //ti vv  =  table with set point varying in time:
            //column 1 = relative time
            //column 2 = set point of the controlled parameter

            //LookUpTable -> seconds, value
            sobekController.LookUpTable = GetControllerTableAndInterExtrapolation(sobekController, SobekController.LookUpTableStructure, properties);

            const string propertiesPatternMC = @"mc\s(?<maxchangevelocity>" + RegularExpression.Scientific + @")\s";

            var matches = RegularExpression.GetMatches(propertiesPatternMC, properties);
            if (matches.Count == 1 && matches[0].Groups["maxchangevelocity"].Value != "")
            {
                sobekController.MaxChangeVelocity = ConversionHelper.ToDouble(matches[0].Groups["maxchangevelocity"].Value);
            }

            var patternMP = RegularExpression.GetInteger("mp");
            matches = RegularExpression.GetMatches(patternMP, properties);
            if (matches.Count == 1 && matches[0].Groups["mp"].Value != "")
            {
                sobekController.MinimumPeriod = Convert.ToInt32(matches[0].Groups["mp"].Value);
            }
        }

        private static void ReadTimeControllerProperties(SobekController sobekController, string properties)
        {
            //mc = dValue/dt, denotes max. change velocity in controlled structure parameter (always 0 for Urban/Rural Controller)
            //bl  = interpolation method table (only for Urban/Rural Controller)
            //0 = no interpolation, a block function
            //1 = linear interpolation
            //ti tv = time table
            //PDIN  …. pdin = Characteristics of Time controller table 
            //1st : 0/1 = Lineair function/Block function
            //2st : 0/1 =  No periodicity/Use periodicity of 
            //3st : periodicity in ddd;hh;mm;ss (only in case 2st =1)         
            //TBLE … tble = Time controller table

            sobekController.TimeTable = GetControllerTableAndInterExtrapolation(sobekController, SobekController.TimeTableStructure, properties);

            SetMeasuremetLocationAndParameter(sobekController, properties);

            const string propertiesPatternMC = @"mc\s(?<maxchangevelocity>" + RegularExpression.Scientific + @")\s";

            var matches = RegularExpression.GetMatches(propertiesPatternMC, properties);
            if (matches.Count == 1)
            {
                if (matches[0].Groups["maxchangevelocity"].Value != "")
                {
                    sobekController.MaxChangeVelocity = ConversionHelper.ToDouble(matches[0].Groups["maxchangevelocity"].Value);
                }
            }

        }


        private static void SetMeasuremetLocationAndParameter(SobekController sobekController, string properties)
        {
            const string propertiesPatternMeasurementLocation = @"(ml\s*'(?<measurementLoacationid>" + RegularExpression.Characters + @")'\s)";
            const string propertiesPatternBranchLocation = @"cb\s'(?<branchid>" + RegularExpression.Characters + @")'\s" +
                                                           RegularExpression.CharactersAndQuote +
                                                           @"cl\s(?<chainage>" + RegularExpression.Scientific + @")\s";
            const string propertiesPatternParameter = @"(cp\s(?<measurementLocationParameter>" + RegularExpression.Integer + @")\s)";

            MatchCollection matches;

            matches = RegularExpression.GetMatches(propertiesPatternMeasurementLocation, properties);
            if (matches.Count == 1 && matches[0].Groups["measurementLoacationid"].Value != "")
            {
                sobekController.MeasurementStationId = matches[0].Groups["measurementLoacationid"].Value;
            }

            matches = RegularExpression.GetMatches(propertiesPatternBranchLocation, properties);
            if (matches.Count == 1)
            {
                sobekController.MeasurementStationId = MeasurementLocationIdGenerator.GetMeasurementLocationId(matches[0].Groups["branchid"].Value, ConversionHelper.ToDouble(matches[0].Groups["chainage"].Value));
            }

            matches = RegularExpression.GetMatches(propertiesPatternParameter, properties);
            if (matches.Count == 1)
            {
                sobekController.MeasurementLocationParameter = (SobekMeasurementLocationParameter)Convert.ToInt32(matches[0].Groups["measurementLocationParameter"].Value);
            }

        }

        private static DataTable GetControllerTableAndInterExtrapolation(SobekController sobekController,DataTable tableSchema, string fileText)
        {
            const string propertiesPatternTable = "(?<table>TBLE(?'text'.*?)tble)";
            const string propertiesPatternPDIN = @"(PDIN (?<pdin>" + RegularExpression.CharactersAndQuote + @") pdin)";
            const string propertiesPatternInterpolation = @"bl\s(?<interpolation>" + RegularExpression.Integer + @")\s";
            DataTable dataTable = null;
            MatchCollection matches;

            matches = RegularExpression.GetMatches(propertiesPatternTable, fileText);



            if(matches.Count == 1)
            {
                var tableText = matches[0].Groups["table"].Value;
                dataTable = SobekDataTableReader.GetTable(tableText, tableSchema);
            }
            else if (matches.Count > 1) //sometimes there is an extra 'not in use' table (after switch of controller type in sobek)
            {
                if (sobekController.ControllerType == SobekControllerType.IntervalController)
                {
                    var tableText = matches[1].Groups["table"].Value;
                    dataTable = SobekDataTableReader.GetTable(tableText, tableSchema);
                }
                else if(tableSchema.Columns[0].DataType == typeof(double))
                {
                    var tableText = matches[1].Groups["table"].Value;
                    dataTable = SobekDataTableReader.GetTable(tableText, tableSchema);
                }
                else
                {
                    var tableText = matches[0].Groups["table"].Value;
                    dataTable = SobekDataTableReader.GetTable(tableText, tableSchema); 
                }
            }

            //Only for Urban/Rural Controller
            matches = RegularExpression.GetMatches(propertiesPatternInterpolation, fileText);
            if (matches.Count == 1 && matches[0].Groups["interpolation"].Value != "")
            {
                sobekController.InterpolationType = (matches[0].Groups["interpolation"].Value == "1")
                                                        ? InterpolationType.Linear
                                                        : InterpolationType.Constant;
            }

            matches = RegularExpression.GetMatches(propertiesPatternPDIN, fileText);
            if (matches.Count == 1)
            {
                var pdinText = matches[0].Groups["pdin"].Value;
                SetInterAndExtrapolation(sobekController, pdinText);
            }

            return dataTable;
        }


        private static void SetInterAndExtrapolation(SobekController sobekController, string pdin)
        {
                const string pdinSubPattern =
                    @"(?<pdin1>" + RegularExpression.Integer + @")\s(?<pdin2>" + RegularExpression.Integer + @")" +
                    @"(\s(?<period>" + RegularExpression.CharactersAndQuote + @"))?";

                var pdinSubMatches = RegularExpression.GetMatches(pdinSubPattern, pdin);

                if (pdinSubMatches.Count > 0)
                {

                    string pdin1 = pdinSubMatches[0].Groups["pdin1"].Value;
                    string pdin2 = pdinSubMatches[0].Groups["pdin2"].Value;
                    string period = pdinSubMatches[0].Groups["period"].Value;
                    if (pdin1 == "0")
                    {
                        sobekController.InterpolationType = InterpolationType.Linear;
                    }
                    else
                    {
                        sobekController.InterpolationType = InterpolationType.Constant;
                    }

                    if (pdin2 == "1")
                    {
                        sobekController.ExtrapolationType = ExtrapolationType.Periodic;
                        if (!string.IsNullOrEmpty(period))
                        {
                            sobekController.ExtrapolationPeriod = period;
                        }
                    }
                    else
                    {
                        sobekController.ExtrapolationType = ExtrapolationType.Constant;
                    }
                }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "cntl";
        }
    }
}
