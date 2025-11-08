
namespace DeveloperConsole
{
    [CommandName("alias", "a shortcut for a line of user input")]
    [CommandSignature("New", "alias new <alias:s> to <commands:s>", "registers an alias to a command / commands separated by semicolons")]
    [CommandSignature("Print", "alias print <alias:s>", "prints the line of commands behind the specified alias")]
    [CommandSignature("Rem", "alias rem <alias:s>", "deregisters specified alias")]
    public class AliasDevConsoleCommand : DevConsoleCommand
    {
        public void New(string[] parameters) => DevConsoleBackend.RegisterAlias(parameters[0], parameters[1]);
        public void Print(string[] parameters) => DevConsoleBackend.PrintAlias(parameters[0]);
        public void Rem(string[] parameters) => DevConsoleBackend.DeregisterAlias(parameters[0]);
    }
}
