using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DeveloperConsole
{
    struct CommandRecord
    {
        private readonly DevConsoleCommand m_CommandInstance;
        private readonly CommandNameAttribute m_NameAttribute;
        private readonly CommandSignatureAttribute[] m_CommandSignatures;

        internal CommandRecord(Type commandType)
        {
            m_CommandInstance = Activator.CreateInstance(commandType) as DevConsoleCommand;
            m_CommandSignatures = commandType.GetCustomAttributes<CommandSignatureAttribute>().ToArray();
            m_NameAttribute = commandType.GetCustomAttribute<CommandNameAttribute>();

            foreach (var sig in m_CommandSignatures)
            {
                var method = commandType.GetMethod(sig.MethodName);
                if (method == null)
                    throw new DeveloperConsoleException(
                        $"Method not found: '{sig.MethodName}' " +
                        $"for console command signature '{sig.TextSignature}'");

                sig.Callback = (CommandSignatureAttribute.ExecuteCallback)Delegate.CreateDelegate(
                    typeof(CommandSignatureAttribute.ExecuteCallback), m_CommandInstance, method);
            }
        }

        public void PrintHelp()
        {
            var m =
                $"Command: {Name}\n" +
                $"  {m_NameAttribute.Description}\n\n" +
                $"  Variants:";

            m = m_CommandSignatures.Aggregate(m, (current, sig) =>
                current + $"\n" +
                $"  {sig.TextSignature}\n" +
                $"      {sig.Hint}\n");

            DevConsole.Print(m);
        }

        public void TryExecute(string userInput, out bool executed)
        {
            executed = true;
            foreach (var sign in m_CommandSignatures)
            {
                var match = sign.RegexSignature.Match(userInput);
                if (!match.Success) continue;

                var variables = new string[match.Groups.Count - 1];
                for (var i = 0; i < variables.Length; i++)
                    variables[i] = match.Groups[i + 1].Value;

                sign.Callback.Invoke(variables);
                return;
            }

            executed = false;
        }

        public bool ThisCommand(string input) =>
            m_CommandSignatures.Any(sig => sig.Test(input));

        public string Name => m_NameAttribute.Name.Trim();
        public Regex[] Regex => m_CommandSignatures.Select(sig => sig.RegexSignature).ToArray();
    }
}
