using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace DeveloperConsole
{
    public class DevConsoleBackend : MonoBehaviour
    {
        private const string ALLOWED_CHARACTERS =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890-+*/_=<>!@#$%^&*(){}[]\"',.?;:|\\/ ";
        
        private const string SAVE_FILE_NAME = "/devconsole.json";
        private const string LOG_FILE_NAME = "/devconsolelog.txt";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnLoad() => Instantiate(Resources.Load("Developer Console"));
    
    
        private static DevConsoleBackend Instance;
        public static bool HasInstance => Instance;
        public static bool ConsoleOpen { get; private set; }
    
        [Header("Settings")]
        [SerializeField] private Text consoleText;
        [SerializeField] private RectTransform textRectTransform;
        [SerializeField] private RectTransform consoleRect;
        [SerializeField] private SmartScroll scroller;
        [SerializeField] private string inputLineHeader;
        [SerializeField] private int maxLines = 5;
    
        [Header("Preferences")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f);
        [SerializeField] private Color primaryColor = new Color(0.627f, 0.125f, 0.941f);
        [SerializeField] private Color successColor = new Color(0f, 1f, 0f);
        [SerializeField] private Color errorColor = new Color(1f, 0f, 0f);
        [SerializeField] private Color aliasColor = new Color(1f, 0.647f, 0f);
        [SerializeField] private Color helpColor = new Color(1f, 0.078f, 0.576f);

    
        public string DefaultColStr => $"#{ColorUtility.ToHtmlStringRGB(defaultColor)}";
        public string PrimaryColStr => $"#{ColorUtility.ToHtmlStringRGB(primaryColor)}";
        public string SuccessColStr => $"#{ColorUtility.ToHtmlStringRGB(successColor)}";
        public string ErrorColStr => $"#{ColorUtility.ToHtmlStringRGB(errorColor)}";
        public string AliasColStr => $"#{ColorUtility.ToHtmlStringRGB(aliasColor)}";
        public string HelpColStr => $"#{ColorUtility.ToHtmlStringRGB(helpColor)}";
    
    

        private readonly Queue<string> m_Lines = new();
        private static bool CanInput;
        private static string UserInput;
        private static int FontSize = 8;

        private static readonly List<CommandRecord> Commands = new();
        private static Dictionary<string, string> AliasMap = new();
        private static Dictionary<string, string> EventMap = new();

        private static void ExecuteEvent(string e)
        {
            if (EventMap.TryGetValue(e, out var c)) ExecuteCommand(c);
            if (EventMap.TryGetValue(e, out var x)) Debug.Log($"Trying to execute");
        }
    
        private PlayerInput m_PlayerInput;
        private List<InputActionMap> m_DisabledMaps = new();

        private readonly Stack<string> m_UndoStack = new();
        private readonly Stack<string> m_RedoStack = new();

        private float m_SequenceExecutionDelay;





        // === UNITY EVENTS ===

        private void Awake()
        {
            if (HasInstance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this);
            Instance = this;
        }

        private void Start()
        {
            if (TryGetComponent(out SmartScroll s)) scroller = s;
            m_PlayerInput = FindFirstObjectByType<PlayerInput>();
            Application.logMessageReceived += ReceiveDebugLogMessage;
            Load();
            SetConsoleInitialState();
            RenderConsole();
            AutoRegisterAllCommands();
            ExecuteEvent("OnStartup");
        }

        private void OnApplicationQuit()
        {
            ExecuteEvent("OnShutdown");
        }

        private void Update()
        {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                if (ConsoleOpen) Close();
                else Open();
            }

            if (m_SequenceExecutionDelay > 0) m_SequenceExecutionDelay -= Time.deltaTime;
            if (m_SequenceExecutionDelay < 0) m_SequenceExecutionDelay = 0;

            if (!ConsoleOpen) return;
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) GoFurtherInHistory();
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame) GoCloserInHistory();
        }





        // === INIT STATE ===

        private void SetConsoleInitialState()
        {
            ConsoleOpen = false;
            var height = consoleRect.sizeDelta.y;
            var targetY = height / 2 + 2;
            consoleRect.anchoredPosition = new Vector2(consoleRect.anchoredPosition.x, targetY);
            CanInput = false;
            consoleRect.gameObject.SetActive(false);
        }
    
        private static void AutoRegisterAllCommands()
        {
            var baseType = typeof(DevConsoleCommand);
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract);
            derivedTypes.ToList().ForEach(t => Commands.Add(new CommandRecord(t)));
        }

        private struct CommandRecord
        {
            internal CommandRecord(Type commandType)
            {
                CommandInstance = Activator.CreateInstance(commandType) as DevConsoleCommand;
                CommandSignatures = commandType.GetCustomAttributes<CommandSignatureAttribute>().ToArray();
                m_NameAttribute = commandType.GetCustomAttribute<CommandNameAttribute>();

                foreach (var sig in CommandSignatures)
                {
                    var method = commandType.GetMethod(sig.MethodName);
                    if (method == null) throw new DeveloperConsoleException(
                        $"Method not found: '{sig.MethodName}' " + 
                        $"for console command signature '{sig.TextSignature}'");

                    sig.Callback = (CommandSignatureAttribute.ExecuteCallback)Delegate.CreateDelegate(
                        typeof(CommandSignatureAttribute.ExecuteCallback), CommandInstance, method);
                }
            }

            public void PrintHelp()
            {
                var m = $"{Name} - {m_NameAttribute.Description}";
                m = CommandSignatures.Aggregate(m, (current, sig) => current + $"\n  {sig.TextSignature} - {sig.Hint}");
                DevConsole.Print(m);
            }
        
            public DevConsoleCommand CommandInstance;
            public CommandSignatureAttribute[] CommandSignatures;
            public bool ThisCommand(string input) =>
                CommandSignatures.Any(sig => sig.Test(input));
        
            private CommandNameAttribute m_NameAttribute;
            public string Name => m_NameAttribute.Name.Trim();
            public Regex[] Regex => CommandSignatures.Select(sig => sig.RegexSignature).ToArray();
        }





        // === OPEN / CLOSE ===

        public static void Open()
        {
            if (!HasInstance) throw DeveloperConsoleException.NullInstance;
            if (ConsoleOpen) return;
        
            ConsoleOpen = true;
            Keyboard.current.onTextInput += Instance.ProcessInput;
            Instance.StartCoroutine(Instance.OpenConsoleCoroutine());

            try
            {
                foreach (var map in Instance.m_PlayerInput.actions.actionMaps.Where(map => map.enabled))
                {
                    Instance.m_DisabledMaps.Add(map);
                    map.Disable();
                }
            }
            catch (Exception e) { _ = e; }
        }

        public static void Close()
        {
            if (!ConsoleOpen) return;
            ConsoleOpen = false;
            Keyboard.current.onTextInput -= Instance.ProcessInput;
            Instance.StartCoroutine(Instance.CloseConsoleCoroutine());

            Instance.m_DisabledMaps.ForEach(x => x.Enable());
            Instance.m_DisabledMaps.Clear();
        }





        // === INPUT ===

        public static void Clear()
        {
            if (!HasInstance) throw DeveloperConsoleException.NullInstance;
            Instance.m_Lines.Clear();
        }

        public static void Print(string text)
        {
            if (!HasInstance) throw DeveloperConsoleException.NullInstance;
            Instance.m_Lines.Enqueue(text.Trim().Replace("\n", "\n  "));
            if (Instance.m_Lines.Count > Instance.maxLines) Instance.m_Lines.Dequeue();
            Instance.RenderConsole();
            if (Instance.scroller) Instance.scroller.OnNewContent();
        }

        private void ProcessInput(char c)
        {
            if (!ConsoleOpen || !CanInput) return;

            switch (c)
            {
                // Backspace
                case '\b':
                    if (UserInput.Length > 0) UserInput = UserInput.Substring(0, UserInput.Length - 1);
                    break;
            
                // Enter
                case '\n':
                case '\r':
                    Print($"<color={PrimaryColStr}>{inputLineHeader}</color><color=#00FFFF>{UserInput}</color>");
                    ExecuteCommand(UserInput);
                    AddHistoryEntry(UserInput);
                    UserInput = string.Empty;
                    break;
            
                // Other input
                default:
                    if (!ALLOWED_CHARACTERS.Contains(c)) return;
                    UserInput += c;
                    ResetHistory();
                    break;
            }

            RenderConsole();
            if (scroller) scroller.OnNewContent();
        }

        private void RenderConsole()
        {
            var inputLine = $"<color={PrimaryColStr}>{inputLineHeader}</color>{SyntaxHighlight(UserInput)}";
            consoleText.text = $"{string.Join('\n', m_Lines)}\n{inputLine}_";
            consoleText.fontSize = FontSize;
            
            textRectTransform.sizeDelta = new Vector2(
                textRectTransform.sizeDelta.x,
                    Mathf.Max(consoleRect.sizeDelta.y, consoleText.preferredHeight));
        }

        private string SyntaxHighlight(string line)
        {
            if (string.IsNullOrEmpty(UserInput)) return string.Empty;
        
            // correct input
            if (Commands.Select(com => com.ThisCommand(line)).Any(result => result))
                return $"<color={SuccessColStr}>{line}</color>";

            // is alias
            if (AliasMap.ContainsKey(line))
                return $"<color={AliasColStr}>{line}</color>";
        
            // is call for help
            if (Commands.Select(com => new Regex($@"(^{com.Name}\s[?]$)|(^help\s{com.Name}$)").IsMatch(line))
                .Any(result => result)) return $"<color={HelpColStr}>{line}</color>";

            // wrong input
            return $"<color={ErrorColStr}>{line}</color>";
        }
        
        




        // === COMMANDS ===

        public static void ExecuteCommand(string line)
        {
            if (!HasInstance) throw DeveloperConsoleException.NullInstance;
            if (string.IsNullOrEmpty(line)) return;
            line = line.Trim();

            if (GetAliasedLine(line, out var aliasedLine))
            {
                foreach (var l in aliasedLine.Split(';'))
                    ExecuteCommand(l);
                return;
            }

            foreach (var com in Commands)
            {
                if (new Regex($@"(^{com.Name}\s[?]$)|(^help\s{com.Name}$)").IsMatch(line))
                {
                    com.PrintHelp();
                    return;
                }
                
                foreach (var sign in com.CommandSignatures)
                {
                    var match = sign.RegexSignature.Match(line);
                    if (!match.Success) continue;

                    var variables = new string[match.Groups.Count - 1];
                    for (var i = 0; i < variables.Length; i++)
                    {
                        variables[i] = match.Groups[i + 1].Value;
                    }

                    sign.Callback.Invoke(variables);
                    return;
                }
            }

            DevConsole.Err(Instance, "Bad command syntax");
        }

        private void ExecuteCommandSequence(string[] commands) =>
            StartCoroutine(ExecuteCommandSequenceCoroutine(commands));

        private IEnumerator ExecuteCommandSequenceCoroutine(string[] commands)
        {
            foreach (var cmd in commands)
            {
                while (m_SequenceExecutionDelay > 0) yield return null;
                ExecuteCommand(cmd.Trim());
            }
        }





        // === ALIASES ===

        public static void RegisterAlias(string alias, string command)
        {
            if (alias == "all")
            {
                DevConsole.Err($"Cannot register alias 'all'. It is a keyword representing all registered aliases");
                return;
            }
            
            AliasMap[alias] = command;
            Save();
            Print($"Registered alias '{alias}'");
        }

        public static void DeregisterAlias(string alias)
        {
            if (alias == "all") AliasMap.Clear();
            else if (AliasMap.Remove(alias))
                Print($"Removed alias '{alias}'");
            Save();
        }

        public static void PrintAlias(string alias)
        {
            if (alias.Equals("all"))
            {
                PrintAllAliases();
                return;
            }

            Print($"  {alias} => {AliasMap[alias]}");
        }

        private static void PrintAllAliases()
        {
            foreach (var kvp in AliasMap)
            {
                Print($"  {kvp.Key} => {kvp.Value}");
            }
        }
    
        private static bool GetAliasedLine(string alias, out string line) => AliasMap.TryGetValue(alias, out line);





        // === DELAY ===

        internal static void DelayExecution(float seconds)
        {
            if (!HasInstance) throw  DeveloperConsoleException.NullInstance;
            switch (seconds)
            {
                case <= 0:
                    return;
                case > 59:
                    DevConsole.Warn($"Delay of {seconds} is too much, don't you think? I will clamp it to 59, ok?");
                    seconds = 59;
                    break;
            }
            Instance.m_SequenceExecutionDelay += seconds;
        }





        // === ANIMATIONS ===

        private IEnumerator OpenConsoleCoroutine()
        {
            consoleRect.gameObject.SetActive(true);
            var startY = consoleRect.anchoredPosition.y;
            var targetY = -consoleRect.sizeDelta.y / 2;

            var t = 0f;
            while (t < animationDuration)
            {
                consoleRect.anchoredPosition = new Vector2(
                    consoleRect.anchoredPosition.x,
                    Mathf.Lerp(startY, targetY, t * (1 / animationDuration)));
            
                t += Time.deltaTime;
                yield return null;
            }
        
            consoleRect.anchoredPosition = new Vector2(consoleRect.anchoredPosition.x, targetY);
            CanInput = true;
        }

        private IEnumerator CloseConsoleCoroutine()
        {
            CanInput = false;
            var startY = consoleRect.anchoredPosition.y;
            var targetY = consoleRect.sizeDelta.y / 2 + 2;

            var t = 0f;
            while (t < animationDuration)
            {
                consoleRect.anchoredPosition = new Vector2(
                    consoleRect.anchoredPosition.x,
                    Mathf.Lerp(startY, targetY, t * (1 / animationDuration)));
            
                t += Time.deltaTime;
                yield return null;
            }
        
            consoleRect.anchoredPosition = new Vector2(consoleRect.anchoredPosition.x, targetY);
            consoleRect.gameObject.SetActive(false);
        }

        public static void SetFontSize(int fontSize)
        {
            FontSize = Mathf.Clamp(fontSize, 1, 50);
            if (HasInstance) 
                Instance.RenderConsole();
            Save();
        }

        public static void RegisterEventCommand(string e, string command)
        {
            EventMap[e] = e switch
            {
                "OnStartup" or "OnShutdown" => command,
                _ => throw new Exception($"Unknown event type '{e}'")
            };
            Print($"Event '{e}' registered");
        }





        // === HISTORY ===

        private void ResetHistory()
        {
            while (m_RedoStack.Count != 0)
            {
                m_UndoStack.Push(m_RedoStack.Pop());
            }
        }

        private void AddHistoryEntry(string input)
        {
            m_UndoStack.Push(input);
            m_RedoStack.Clear();
        }

        private void GoFurtherInHistory()
        {
            if (m_UndoStack.Count == 0) return;
            UserInput = GetCommandHistoryFurther();
            RenderConsole();
        }

        private void GoCloserInHistory()
        {
            UserInput = GetCommandHistoryCloser();
            RenderConsole();
        }

        private string GetCommandHistoryFurther()
        {
            if (m_UndoStack.Count > 0)
            {
                string cmd = m_UndoStack.Pop();
                m_RedoStack.Push(cmd);
                return cmd;
            }
            return string.Empty;
        }

        private string GetCommandHistoryCloser()
        {
            if (m_RedoStack.Count > 0)
            {
                string cmd = m_RedoStack.Pop();
                m_UndoStack.Push(cmd);
                return cmd;
            }
            return string.Empty;
        }





        // === UNITY'S DEBUG LOG ===

        private static void ReceiveDebugLogMessage(string logString, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                    DevConsole.Err($"{logString}"); //\n----------------------------\n{stackTrace}");
                    break;
            
                case LogType.Warning:
                    DevConsole.Warn(logString);
                    break;
            
                case LogType.Log:
                default:
                    DevConsole.Print(logString);
                    break;
            }
        }





        // === SAVE / LOAD ===

        private static void Save()
        {
            var filePath = $"{Application.persistentDataPath}/{SAVE_FILE_NAME}";

            SaveFile saveFile = new();
            if (HasInstance)
            {
                saveFile.fontSize = FontSize;
                saveFile.aliasMap = new SerializableDictionary<string, string>(AliasMap);
                saveFile.eventMap = new SerializableDictionary<string, string>(EventMap);;
            }

            var json = JsonUtility.ToJson(saveFile, true);
            File.WriteAllText(filePath, json);
        }

        private static void Load()
        {
            var filePath = $"{Application.persistentDataPath}/{SAVE_FILE_NAME}";
            SaveFile saveFile;

            if (!File.Exists(filePath)) saveFile = new SaveFile();
            else
            {
                var json = File.ReadAllText(filePath);
                saveFile = JsonUtility.FromJson<SaveFile>(json);
            }

            FontSize = saveFile.fontSize;
            AliasMap = saveFile.aliasMap;
            EventMap = saveFile.eventMap;

            if (HasInstance)
                Instance.RenderConsole();
        }


        [Serializable]
        private class SaveFile
        {
            public int fontSize = 8;
            public SerializableDictionary<string, string> aliasMap = new();
            public SerializableDictionary<string, string> eventMap = new();
        }
    }
}
