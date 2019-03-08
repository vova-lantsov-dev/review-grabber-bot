using System;

namespace ReviewGrabberBot.Exceptions
{
    public sealed class EnvironmentVariableDirectoryNotFoundException : Exception
    {
        public EnvironmentVariableDirectoryNotFoundException(string environmentVariableName)
            : base($"REVIEWBOT_{environmentVariableName} directory was not found")
        {
        }
    }
}