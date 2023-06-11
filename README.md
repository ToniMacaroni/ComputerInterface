# Computer Interface

Computer Interface is a library for Gorilla Tag which replaces the base computer with a custom computer, and allows developers to add functionality to it.

Main project contributors:

- [ToniMacaroni](https://github.com/ToniMacaroni)
- [Graic](https://github.com/Graicc)
- [Dev](https://github.com/developer9998)
- [A Haunted Army](https://github.com/AHauntedArmy)
- [Fchb1239](https://github.com/fchb1239)

You can find all of us on the [Gorilla Tag Modding Group Discord](http://discord.gg/monkemod).

## Table of Contents

- [Install](#install)
- [CommandLine](#commandline)
- [Background](#background)
- [Additional Features](#additional-features)
- [For Developers](#for-developers)
- [Disclaimers](#disclaimers)

## Install

The recommended way to install Computer Interface is through [MonkeModManager](https://github.com/DeadlyKitten/MonkeModManager/releases/latest). Simply select Computer Interface from the menu, and hit "Install/Update".
This will ensure you have all the necessary dependencies set up.

## CommandLine

Computer Interface ships with a CLI that enables you to execute routines & change settings.

Information on creating commands can be found in the [Adding Your Own Commands](#adding-your-own-commands) section.

By default Computer Interface ships with the following commands:

- **setcolor** ``int`` ``int`` ``int``  
  Changes your gorilla's color (e.g. setcolor 255 255 255)
- **setname** ``string``  
  Changes your gorilla's name (e.g. setname toni)
- **join** ``string``  
  Connects to a room code (e.g. join dev123)
- **leave**  
  Disconnects you from the current room
- **cam** ``string``  
  Changes your spectator camera's perspective to either First Person (fp) or Third Person (tp)
- **setbg** ``int`` ``int`` ``int``  
  Changes your computer's background color (e.g. setbg 40 70 40)

## Background

To use a custom background image:

- Go to your Gorilla Tag folder, and open ``BepInEx/config/tonimacaroni.computerinterface.cfg``.
- Find the `ScreenBackgroundPath` config option, and replace the path with your own image path.
  - Use forward slashes (/) instead of backslashes (\\) in the path
  - Your background will be multiplied by the background's color
  - Paths can either be relative to your Gorilla Tag folder or absolute.
  
You can also run ``setbg 255 255 255`` to leave the background with no modified color.

## Additional Features

- Command Line
- Ability to toggle supported mods on and off
- Animated keys
- Custom background (Image & Color)

## For Developers

Before you begin reading I have created a very well-documented example mod which you can use as a starting point.  
It shows examples for creating multiple views, navigating between those and creating your own commands:  
<https://github.com/ToniMacaroni/ComputerInterfaceExample>

For more advanced examples check out the base library views here:  
<https://github.com/ToniMacaroni/ComputerInterface/tree/main/ComputerInterface/Views>

### Adding Views

Computer Interface works with "Views" which are classes that inherit from `ComputerView` or `IComputerView`.
Instantiation, injection, and caching are all handled by Computer Interface with Zenject. You can get bound types through the constructor, and Zenject will handle the dependency injection.

Views can navigate to others views through `ShowView<TargetView>()`, or return to the main menu with `ReturnToMainMenu()`.
Views can check for key presses by overriding `OnKeyPressed`.

An example view may look like this:

```csharp
public class MyModView : ComputerView
    {
        // This is called when your view is opened
        public override void OnShow()
        {
            base.OnShow();
            // Changing the Text property will fire a PropertyChanged event
            // which lets the computer know the text has changed and updates it
            Text = "Monkey Computer\nMonkey Computer\nMounkey Computer";
        }

        // You can do something on keypresses by overriding "OnKeyPressed"
        // It gets an EKeyboardKey passed as a parameter which wraps the old character string
        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    // "ReturnToMainMenu" will switch to the main menu again
                    ReturnToMainMenu();
                    break;
                case EKeyboardKey.Option1:
                    // If you want to switch to another view you can do it like this
                    ShowView<MyOtherModView>();
                    break;
            }
        }
    }
```

To add a view to the main menu, you need to create a Mod Entry, and bind it with Zenject.
Mod Entries must implement `IComputerModEntry`, and provide the name type of the view to be shown.

For example:

```csharp
public class MyModEntry : IComputerModEntry
    {
        // This is the name that will be shown on the main menu
        public string EntryName => "MyMod";

        // This is the first view that is going to be shown if the user selects you mod
        // The Computer Interface mod will instantiate your view 
        public Type EntryViewType => typeof(MyModView);
    }
```

To tell Computer Interface that the entry exists, you must bind it with Zenject like so:

```csharp
Container.Bind<IComputerModEntry>().To<MyModEntry>().AsSingle();
```

This is generally done in your `MainInstaller`, see the example [MainInstaller.cs](https://github.com/ToniMacaroni/ComputerInterfaceExample/blob/main/ComputerModExample/MainInstaller.cs) for a full example.

### Adding Your Own Commands

Adding your own CLI commands is easy.  
In a type that you bound via Zenject request the `CommandHandler` and add your command.

For example:

```csharp
internal class MyModCommandManager : IInitializable
    {
        private readonly CommandHandler _commandHandler;

        // Request the CommandHandler
        // This gets resolved by zenject since we bind MyModCommandManager in the container
        public MyModCommandManager(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public void Initialize()
        {
            // Add a command
            _commandHandler.AddCommand(new Command(name: "whoami", argumentCount: 0, args =>
            {
                // Args is an array of arguments (string) passed when entering the command
                // The command handler already checks if the correct amount of arguments is passed

                // The string you return is going to be shown in the terminal as a return message
                // you can break up the message into multiple lines by using \n
                return "MONKE";
            }));
        }
    }
```

This used a dummy class `MyModCommandManager`, but of course, you can do this in any type as long as you request the `CommandHandler`.

## Disclaimers

This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. ©2021 Another Axiom LLC.
