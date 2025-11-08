using UnityEngine;

namespace DeveloperConsole
{
    /// <summary>
    /// An API to the Developer Console provided by the package
    /// </summary>
    public static class DevConsole
    {
        /// <summary>Prints a message out to the dev console</summary>
        /// <param name="message">The message</param>
        public static void Print(string message) => DevConsoleBackend.Print(message);

        /// <summary>Prints a message out to the dev console</summary>
        /// <param name="source">Message sender</param>
        /// <param name="message">The message</param>
        public static void Print(UnityEngine.Object source, string message)
        {
            var sourceHeader = source ? $"<color=purple>[{source.name}]:</color>" : "";
            DevConsoleBackend.Print($"{sourceHeader}{message}");
        }

        /// <summary>Prints a message out to the dev console</summary>
        /// <param name="sourceType">Message sender</param>
        /// <param name="message">The message</param>
        public static void Print(System.Type sourceType, string message)
        {
            var sourceHeader = $"<color=purple>[{sourceType.Name}]:</color> ";
            DevConsoleBackend.Print($"{sourceHeader}{message}");
        }

        /// <summary>Prints a message out to the dev console</summary>
        /// <param name="source">Message sender</param>
        /// <param name="message">The message</param>
        /// <param name="color">The color of the message</param>
        public static void Print(UnityEngine.Object source, string message, Color color)
        {
            color.a = 1.0f;
            var sourceHeader = source ? $"<color=purple>[{source.name}]:</color>" : "";
            DevConsoleBackend.Print($"{sourceHeader}<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
        }



        /// <summary>Prints a warning message out to the dev console</summary>
        /// <param name="message">The message</param>
        public static void Warn(string message) => DevConsoleBackend.Print($"<color=yellow>{message}</color>");

        /// <summary>Prints a warning message out to the dev console</summary>
        /// <param name="sourceType">Message sender</param>
        /// <param name="message">The message</param>
        public static void Warn(System.Type sourceType, string message)
        {
            var sourceHeader = $"<color=purple>[{sourceType.Name}]:</color> ";
            DevConsoleBackend.Print($"{sourceHeader}<color=yellow>{message}</color>");
        }

        /// <summary>Prints a warning message out to the dev console</summary>
        /// <param name="source">Message sender</param>
        /// <param name="message">The message</param>
        public static void Warn(UnityEngine.Object source, string message)
        {
            var sourceHeader = source ? $"<color=purple>[{source.name}]:</color>" : "";
            DevConsoleBackend.Print($"{sourceHeader}<color=yellow>{message}</color>");
        }



        /// <summary>Prints an error message out to the dev console</summary>
        /// <param name="message">The message</param>
        public static void Err(string message) => DevConsoleBackend.Print($"<color=red>*{message}</color>");

        /// <summary>Prints an error message out to the dev console</summary>
        /// <param name="sourceType">Message sender</param>
        /// <param name="message">The message</param>
        public static void Err(System.Type sourceType, string message)
        {
            var sourceHeader = $"<color=purple>[{sourceType.Name}]:</color>";
            DevConsoleBackend.Print($"{sourceHeader}<color=red>*{message}</color>");
        }

        /// <summary>Prints an error message out to the dev console</summary>
        /// <param name="source">Message sender</param>
        /// <param name="message">The message</param>
        public static void Err(UnityEngine.Object source, string message)
        {
            var sourceHeader = source ? $"<color=purple>[{source.name}]:</color>" : "";
            DevConsoleBackend.Print($"{sourceHeader}<color=red>*{message}</color>");
        }



        /// <summary>Prints a success message out to the dev console</summary>
        /// <param name="message">The message</param>
        public static void Success(string message) => DevConsoleBackend.Print($"<color=green>{message}</color>");

        /// <summary>Prints a success message out to the dev console</summary>
        /// <param name="sourceType">Message sender</param>
        /// <param name="message">The message</param>
        public static void Success(System.Type sourceType, string message)
        {
            var sourceHeader = $"<color=purple>[{sourceType.Name}]:</color>";
            DevConsoleBackend.Print($"{sourceHeader}<color=green>{message}</color>");
        }

        /// <summary>Prints a success message out to the dev console</summary>
        /// <param name="source">Message sender</param>
        /// <param name="message">The message</param>
        public static void Success(UnityEngine.Object source, string message)
        {
            var sourceHeader = source ? $"<color=purple>[{source.name}]:</color> " : "";
            DevConsoleBackend.Print($"{sourceHeader}<color=green>{message}</color>");
        }



        /// <summary>Executes a message in the console as if a user had entered it</summary>
        /// <param name="command">The command</param>
        public static void Execute(string command) =>
            DevConsoleBackend.ExecuteCommand(command);



        /// <summary>Clears the console</summary>
        public static void Cls() =>
            DevConsoleBackend.Clear();
    }
}