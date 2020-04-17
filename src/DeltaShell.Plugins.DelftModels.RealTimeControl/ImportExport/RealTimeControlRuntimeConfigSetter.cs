using System;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>Responsible for setting all the data from the runtime config file on the RealTimeControlModel</summary>
    public class RealTimeControlRuntimeConfigSetter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlRuntimeConfigSetter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Sets the run time settings on the RealTimeControl Model.
        /// </summary>
        /// <param name="rtcModel">The RealTimeControl Model.</param>
        /// <param name="runTimeSettingsElement">The User Defined Runtime XML element.</param>
        /// <remarks>The rtcModel and runTimeSettingsElement parameter are expected to not be NULL.</remarks>
        public void SetRunTimeSettings(RealTimeControlModel rtcModel, UserDefinedRuntimeXML runTimeSettingsElement)
        {
            SetStartTime(rtcModel, runTimeSettingsElement);
            SetStopTime(rtcModel, runTimeSettingsElement);
            SetTimeStep(rtcModel, runTimeSettingsElement);
        }

        /// <summary>
        /// Sets the simulation mode settings on the RealTimeControl Model.
        /// </summary>
        /// <param name="rtcModel">The RealTimeControl Model.</param>
        /// <param name="simulationModeSettingsElement">The simulation Mode XML element.</param>
        /// <remarks>The rtcModel is expected to not be NULL. If simulationModeSettingsElement is NULL, no settings are set.</remarks>
        public void SetSimulationModeSettings(RealTimeControlModel rtcModel, ModeXML simulationModeSettingsElement)
        {
            if (simulationModeSettingsElement?.Item is ModeSimulationXML simulationMode)
            {
                rtcModel.LimitMemory = simulationMode.limitedMemory;
            }
        }

        /// <summary>
        /// Sets the restart settings on the RealTimeControl Model
        /// </summary>
        /// <param name="rtcModel">The RealTimeControl Model.</param>
        /// <param name="restartSettingsElement">The User Defined State Export XML element.</param>
        /// <remarks>The rtcModel and restartSettingsElement are expected to not be NULL.</remarks>
        public void SetRestartSettings(RealTimeControlModel rtcModel, UserDefinedStateExportXML restartSettingsElement)
        {
            if (restartSettingsElement.stateTimeStep == -1)
            {
                rtcModel.WriteRestart = false;
                rtcModel.SaveStateStartTime = rtcModel.StopTime;
                rtcModel.SaveStateStopTime = rtcModel.StopTime;
                rtcModel.SaveStateTimeStep = rtcModel.TimeStep;
                return;
            }

            rtcModel.WriteRestart = true;
            SetRestartStartTime(rtcModel, restartSettingsElement);
            SetRestartStopTime(rtcModel, restartSettingsElement);
            SetRestartTimeStep(rtcModel, restartSettingsElement);
        }

        private void SetTimeStep(ITimeDependentModel rtcModel, UserDefinedRuntimeXML settings)
        {
            var timeStepElement = settings.timeStep;
            var timeStep = GetTimeSpanFromTimeUnit(timeStepElement);
            var timeMultiplier = Convert.ToInt32(timeStepElement.multiplier);
            var timeDivider = Convert.ToInt32(timeStepElement.divider);

            rtcModel.TimeStep = MultiplyAndDivideTimeStepBy(timeStep, timeMultiplier, timeDivider);
        }

        private TimeSpan GetTimeSpanFromTimeUnit(TimeStepXML timeStepXml)
        {
            var timeUnit = timeStepXml.unit;
            TimeSpan timeStep;

            switch (timeUnit)
            {
                case timeStepUnitEnumStringType.second:
                    timeStep = new TimeSpan(0, 0, 0, 1);
                    break;
                case timeStepUnitEnumStringType.minute:
                    timeStep = new TimeSpan(0, 0, 1, 0);
                    break;
                case timeStepUnitEnumStringType.hour:
                    timeStep = new TimeSpan(0, 1, 0, 0);
                    break;
                case timeStepUnitEnumStringType.day:
                    timeStep = new TimeSpan(1, 0, 0, 0);
                    break;
                case timeStepUnitEnumStringType.week:
                    timeStep = new TimeSpan(7, 0, 0, 0);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return timeStep;
        }

        private TimeSpan MultiplyAndDivideTimeStepBy(TimeSpan t, int multiplier, int divider)
        {
            return new TimeSpan(t.Ticks * multiplier / divider);
        }

        private void SetStartTime(ITimeDependentModel rtcModel, UserDefinedRuntimeXML settings)
        {
            var startDateElement = settings.startDate;
            rtcModel.StartTime = CreateDateTimeFromDateAndTime(startDateElement.date, startDateElement.time);
        }

        private void SetStopTime(ITimeDependentModel rtcModel, UserDefinedRuntimeXML settings)
        {
            var endDateElement = settings.endDate;
            rtcModel.StopTime = CreateDateTimeFromDateAndTime(endDateElement.date, endDateElement.time);
        }

        private DateTime CreateDateTimeFromDateAndTime(DateTime dateDateTime, DateTime timeDateTime)
        {
            var time = timeDateTime.TimeOfDay;
            var date = dateDateTime.Date;
            var dateTime = date.Add(time);

            return dateTime;
        }

        private void SetRestartStartTime(RealTimeControlModel rtcModel, UserDefinedStateExportXML settings)
        {
            var startDateElement = settings.startDate;
            rtcModel.SaveStateStartTime = CreateDateTimeFromDateAndTime(startDateElement.date, startDateElement.time);
        }

        private void SetRestartStopTime(RealTimeControlModel rtcModel, UserDefinedStateExportXML settings)
        {
            var endDateElement = settings.endDate;
            rtcModel.SaveStateStopTime = CreateDateTimeFromDateAndTime(endDateElement.date, endDateElement.time);
        }

        private void SetRestartTimeStep(RealTimeControlModel rtcModel, UserDefinedStateExportXML settings)
        {
            var timeStepDouble = settings.stateTimeStep;
            rtcModel.SaveStateTimeStep = TimeSpan.FromSeconds(timeStepDouble);
        }
    }
}
