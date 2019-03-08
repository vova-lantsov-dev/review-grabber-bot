using System;

namespace ReviewGrabberBot.Exceptions
{
    public sealed class EnvironmentVariableNotFoundException : Exception
    {
        public EnvironmentVariableNotFoundException(string environmentVariableName)
            : base($"REVIEWBOT_{environmentVariableName} environment variable was not found")
        {
        }
    }
}