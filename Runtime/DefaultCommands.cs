
namespace DeveloperConsole
{
    [CommandName("print", "prints text to the console")]
    [CommandSignature("Print", "print <text:s>", "prints everything, that goes after the 'print' keyword")]
    public class PrintDevConsoleCommand : DevConsoleCommand
    {
        public void Print(string[] parameters) => DevConsole.Print(parameters[0]);
    }

    [CommandName("clear", "clears the console")]
    [CommandSignature("Clear", "clear", "clears all contents of the console")]
    public class ClearDevConsoleCommand : DevConsoleCommand
    {
        public void Clear(string[] parameters) => DevConsole.Cls();
    }

    [CommandName("event", "adds various event callbacks")]
    [CommandSignature("OnStartup", "event onstartup <commands:s>", "executes provided commands on application startup")]
    [CommandSignature("OnShutdown", "event onshutdown <commands:s>", "executes provided commands on application shutdown")]
    public class EventDevConsoleCommand : DevConsoleCommand
    {
        public void OnStartup(string[] parameters) => DevConsoleBackend.RegisterEventCommand("OnStartup", parameters[0]);
        public void OnShutdown(string[] parameters) => DevConsoleBackend.RegisterEventCommand("OnShutdown", parameters[0]);
    }




    [CommandName("open", "opens the console (useful for automation)")]
    [CommandSignature("Open", "open", "opens a console. Useful for automation")]
    public class OpenDevConsoleCommand : DevConsoleCommand
    {
        public void Open(string[] parameters) => DevConsoleBackend.Open();
    }

    [CommandName("close", "closes the console (useful for automation)")]
    [CommandSignature("Close", "close", "closes the console.  Useful for automation")]
    public class CloseDevConsoleCommand : DevConsoleCommand
    {
        public void Close(string[] parameters) => DevConsoleBackend.Close();
    }
    
    
    
    [CommandName("fontsize", "changes the font size")]
    [CommandSignature("Reset", "fontsize reset", "resets font size to the default value")]
    [CommandSignature("Set", "fontsize <fs:i>", "sets font size to the specified value")]
    public class FontSizeDevConsoleCommand : DevConsoleCommand
    {
        public void Reset(string[] parameters) => DevConsoleBackend.SetFontSize(8);
        public void Set(string[] parameters) => DevConsoleBackend.SetFontSize(int.Parse(parameters[0]));
    }
    

    [CommandName("delay", "freezes the console for some time")]
    [CommandSignature("Delay", "delay <seconds:i>", "freezes the console for specified number of seconds")]
    public class DelayDevConsoleCommand : DevConsoleCommand
    {
        public void Delay(string[] parameters) => DevConsoleBackend.DelayExecution(int.Parse(parameters[0]));
    }
    
    
    
    [CommandName("scene", "scene manager access from the console")]
    [CommandSignature("Load", "scene load <scene-name:s>", "the name of the scene to load")]
    [CommandSignature("List", "scene list", "lists all available scenes")]
    public class SceneDevConsoleCommand : DevConsoleCommand
    {
        public void Load(string[] parameters)
        {
            var sceneName = parameters[1].Trim();
            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(sceneName))
            {
                DevConsole.Err($"Scene '{sceneName}' does not exist or is not in build settings");
                return;
            }

            DevConsole.Print($"Switching scene to '{sceneName}'...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        
        public void List(string[] parameters)
        {
            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (var i = 0; i < sceneCount; i++)
            {
                var scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                DevConsole.Print($"{i} -  {sceneName}");
            }
        }
    }
}
