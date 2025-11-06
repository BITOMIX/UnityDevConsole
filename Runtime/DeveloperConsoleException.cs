using System;

public class DeveloperConsoleException : Exception
{
    public DeveloperConsoleException(string message)  : base(message) {}
    
    public static DeveloperConsoleException NullInstance 
        => new DeveloperConsoleException("Developer Console has no instance");
}