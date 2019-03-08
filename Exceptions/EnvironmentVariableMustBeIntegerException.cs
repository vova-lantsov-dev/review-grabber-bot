using System;

namespace ReviewGrabberBot.Exceptions
{
    public sealed class EnvironmentVariableMustBeIntegerException : Exception
    {
        public EnvironmentVariableMustBeIntegerException(string environmentVariableName)
            : base($"REVIEWBOT_{environmentVariableName} environment variable must be an integer")
        {
        }
    }
}