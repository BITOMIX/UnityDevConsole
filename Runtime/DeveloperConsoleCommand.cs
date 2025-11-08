
using System;
using System.Text.RegularExpressions;

/// <summary>
/// Defines the root command
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandNameAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public CommandNameAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Defines the syntax for the command. A single command can have multiple signatures
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandSignatureAttribute : System.Attribute
{
    public string MethodName { get; }
    public string TextSignature { get; }
    public Regex RegexSignature { get; }
    public string Hint { get; }
    
    public delegate void ExecuteCallback(string[] parameters);
    public ExecuteCallback Callback;

    /// <summary>
    /// A simple constructor that provides an option of custom regex
    /// </summary>
    /// <param name="methodName">name of member method of command class that will be used to execute this signature</param>
    /// <param name="textFormSignature">signature that will be displayer when help is called on this command</param>
    /// <param name="regex">regular expression of this command</param>
    /// <param name="helpHint">information that will be displayer when help is called on this command</param>
    public CommandSignatureAttribute(string methodName, string textFormSignature, string regex, string helpHint)
    {
        Hint = helpHint;
        TextSignature = textFormSignature;
        RegexSignature = new Regex(regex);
        MethodName = methodName;
    }
    
    /// <summary>
    /// A recommended constructor that handles the creation of regex itself
    /// </summary>
    /// <param name="methodName">name of member method of command class that will be used to execute this signature</param>
    /// <param name="signature">syntax of the command. Example: <c>print &lt;text:s&gt;</c>, where 'text' will be the name
    /// and 's' will be the type of the parameter. Types of parameters: 's' - string; 'i' - integer; 'f' - float</param>
    /// <param name="helpHint">information that will be displayer when help is called on this command</param>
    /// <exception cref="DeveloperConsoleException">thrown if there is a mistake in the signature</exception>
    public CommandSignatureAttribute(string methodName, string signature, string helpHint)
    {
        var paramRegex = new Regex(@"<([^:>]+):([is])>$");
        var textRegex = new Regex(@"([a-zA-Z]+)$");
        var tokens = signature.Split(" ");

        var textSign = "";
        var regxSign = "";
        
        foreach (var token in tokens)
        {
            if (textRegex.IsMatch(token))
            {
                textSign += " " + token;
                regxSign += " " + token;
                
                continue;
            }

            if (paramRegex.IsMatch(token))
            {
                var match = paramRegex.Match(token);
            
                textSign += $" <{match.Groups[1].Value}|";
                textSign += match.Groups[2].Value switch
                {
                    "i" => "int",
                    "f" => "float",
                    "s" => "string",
                    "b" => "y/n",
                    _ => ""
                } + ">";
                
                regxSign += " " + match.Groups[2].Value switch
                {
                    "i" => "([0-9]+)",
                    "f" => "([0-9]+[.]*[0-9]*)",
                    "s" => "(.+)",
                    "b" => "([yn])",
                    _ => ""
                };
                
                continue;
            }
            
            throw new DeveloperConsoleException(
                $"Failed to parse command signature '{signature}'. " +
                $"Problematic token:  {token}");
        }

        TextSignature = textSign.Trim();
        RegexSignature = new Regex($"^{regxSign.Trim()}$");
        MethodName = methodName;
        Hint = helpHint;
    }
    
    public bool Test(string against) => RegexSignature.IsMatch(against);
}

/// <summary>
/// Base type for developer console commands
/// </summary>
public abstract class DevConsoleCommand { }