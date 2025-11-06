# DevConsole

Simple customizable developer console for Unity Engine

## Installation

In the Unity Package Manager, select add package by a GitHub link and past in the link to this repository

## Usage

The console will appear in the project automatically when the application is run (in playmode and build), no need to
add anything to the scene manually

## Features

- api for script interactions
- custom commands
- aliases
- customizability & input highlighting

## Script API

A DevConsole class is a high level tool for interacting with the console. It offers various methods:

| Method      | Description                                |
|-------------|--------------------------------------------|
| `Print()`   | Prints normal text to the console          |
| `Warn()`    | Prints a warning message to the console    |
| `Err()`     | Prints an error message to the console     |
| `Success()` | Prints a success message to the console    |
| `Execute()` | Executes a command as if a user entered it |
| `Cls()`     | Clears the console                         |

## Default console commands

| Command    | Description                                                     |
|------------|-----------------------------------------------------------------|
| `print`    | prints text to the console                                      |
| `clear`    | clears console                                                  |
| `open`     | opens the console (useful for automation)                       |
| `close`    | closes the console (useful for automation)                      |
| `alias`    | a shortcut for a line of user input                             |
| `event`    | allows to register various event callbacks like **on-startup**  |
| `fontsize` | sets the size of the font                                       |
| `delay`    | freezes the console for some time (for multiplayer for example) |
| `scene`    | scene manager access from the console                           |

## Defining custom commands

Below is an example of a custom command class

    [CommandName("example")]
    [CommandSignature("NoParam")]
    [CommandSignature("WithParam", "example <param:s>")]
    class ExampleCommand : DevConsoleCommand
    {
        public void NoParam(string[] parameters) { ... }
        public void WithParam(string[] parameters) { ... }
    }

Attributes:

- CommandName - defines the name of the command
- CommandSignature - defines an overload of a command. A single command can have multiple signatures which
  is great for grouping multiple related commands close together

> A command must have a CommandName and at least one CommandSignature attribute to be valid

### Command Name Attribute

Defines the name of the command. In a command like this:

    print "hello, world"

`print` will be the command name

### Command Signature Attribute

The `methodName` parameter of CommandSignature attribute is the name of the method, that defines the
behavior of the overload and will be called when the command is executed. The method must always match the
following signature:

    public void Foo(string[] variables)

The `signature` parameter of CommandSignature attribute is a signature. Inside of it is the syntax of the
command. It should always begin with the name of the command to avoid confusion and keep things consistent.
A parameter can be defined by writing its name and datatype separated by colon and enclosed in angle brackets:

    <param_name:s>

where 'param_name' is the name and 's' is the datatype of the parameter

> Parameter will be passed to the execution method inside a string  
> array and thus will be a string regardless of the datatype in the  
> signature. You can, however, always rely on the passed parameters to  
> be in a correct format for parsing without the need to peroform any  
> checks

Here is a list of supported datatypes:

- s - string
- i - integer
- f - float

> You can create a parameterless single-keyword command by leaving the
> signature parameter of the attribute empty

## Aliases

An alias is a way to create a shortcut for a line of user input.

### Managing aliases

Aliases have a pre-defined commands for their management:

| Command                               | Description                                                                            |
|:--------------------------------------|:---------------------------------------------------------------------------------------|
| `alias new <alias:s> to <commands:s>` | will register a new `alias` alias and when executed, will be interpreted as `commands` |
| `alias rem <alias:s>`                 | will deregister alias by the name from `alias` parameter                               |
| `alias print <alias:s>`               | prints what `alias` is interpreted as                                                  |

> You can make an alias execute multiple commands by writing them consecutively
> and separated by a semicolon ';' character

BITOMIX, 1:01 PM
