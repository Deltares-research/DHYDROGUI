using System;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// <see cref="EnvironmentSetter"/> implements the interface to set Environment variables.
    /// </summary>
    /// <seealso cref="IEnvironmentSetter" />
    public sealed class EnvironmentSetter : IEnvironmentSetter
    {
        public void SetVariable(string key, 
                                string value,
                                EnvironmentVariableTarget target = EnvironmentVariableTarget.Process) =>
            Environment.SetEnvironmentVariable(key, value, target);
    }
}