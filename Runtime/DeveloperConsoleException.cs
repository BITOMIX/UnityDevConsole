using System;

namespace DeveloperConsole
{
    public class DeveloperConsoleException : Exception
    {
        public DeveloperConsoleException(string message)  : base(message) {}
    
        public static DeveloperConsoleException NullInstance => new("Developer Console has no instance");
    }
}