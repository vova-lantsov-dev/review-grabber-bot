using System;

namespace ReviewGrabberBot.Exceptions
{
    public sealed class EnvironmentVariableFileNotFoundException : Exception
    {
        public EnvironmentVariableFileNotFoundException(string environmentVariableName)
            : base($"REVIEWBOT_{environmentVariableName} file was not found")
        {
        }
    }
}