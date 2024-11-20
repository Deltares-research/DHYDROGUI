using System;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// <see cref="SystemEnvironment"/> implements the interface to set Environment variables.
    /// </summary>
    /// <seealso cref="IEnvironment"/>
    public sealed class SystemEnvironment : IEnvironment
    {
        public void SetVariable(string key,
                                string value,
                                EnvironmentVariableTarget target = EnvironmentVariableTarget.Process) =>
            Environment.SetEnvironmentVariable(key, value, target);

        public string GetVariable(string key, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process) =>
            Environment.GetEnvironmentVariable(key, target);
    }
}