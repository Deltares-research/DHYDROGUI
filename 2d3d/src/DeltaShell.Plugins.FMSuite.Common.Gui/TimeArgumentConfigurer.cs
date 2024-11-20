using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public class DateTimeGenerator : NextValueGenerator<DateTime>
    {
        private readonly IVariable<DateTime> variable;

        private readonly TimeSpan timeStep;

        public DateTimeGenerator(IVariable<DateTime> variable, TimeSpan timeStep)
        {
            this.variable = variable;
            this.timeStep = timeStep;
        }

        public override DateTime GetNextValue()
        {
            return variable.MaxValue + timeStep;
        }
    }

    public static class TimeArgumentConfigurer
    {
        public static void Configure(IFunction function, ITimeDependentModel model)
        {
            foreach (IVariable<DateTime> timeArgument in function.Arguments.OfType<IVariable<DateTime>>())
            {
                timeArgument.DefaultValue = model.StartTime;
                timeArgument.NextValueGenerator = new DateTimeGenerator(timeArgument, model.TimeStep);
            }
        }
    }
}