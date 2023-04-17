# Computer Interface
Computer Interface is a library for Gorilla Tag which replaces the base computer with a modern desktop computer. This computer then can be utilized by both Developers to create their own computer tabs, and Modders in which they can make more out of the Computer.

If you want to contact me on Discord: ``Toni Macaroni#8970``

## Command Line
Computer Interface ships with a CLI that enables you to execute routines & change settings.<br>
Developers can also add their own commands, which is brought up later on.

By default Computer Interface ships with following commands:
  - <b>setcolor ``Int`` ``Int`` ``Int`` </b><br>   This command changes your gorilla color (e.g. setcolor 255 255 255)
  - <b>setname ``String`` </b><br>   This command changes your gorilla name (e.g. setname toni)
  - <b>join ``String`` </b><br>    This command connects to a room code (e.g. join dev123)
  - <b>leave </b><br>    This command disconnects you from the current room
  - <b>cam ``String`` </b><br>   This command changes your spectator camera's perspective to either First Person (fp) or Third Person (tp)
  - <b>setbg ``Int`` ``Int`` ``Int`` </b><br>    This command changes your computer's background color (e.g. setbg 40 70 40)

## Background Image
To use a custom background image, follow these steps in order from top to bottom:
  - <b>Launch the game, this will generate a config file.</b>
  - <b> Go to your Gorilla Tag folder, then ``BepInEx/config`` and open the ``tonimacaroni.computerinterface.cfg`` file.</b>
  - <b> Find the "ScreenBackgroundPath" config option, and replace the path with your own image path.</b>

Make sure when you're replacing the path, you have:
  - <b>Used forward slashes (/) instead of back slashes (\) in the path</b>
  - <b>Acknowledged that your background can be multiplied by the background's color</b>
  - <b>Paths can also either be relative to your Gorilla Tag folder or absolute.</b>
  
You can also run ``setbg 255 255 255`` to leave the background with no modified color.

## Additional Features
Computer Interface by itself also includes some exclusive content. This includes:
- <b>Room Browser (Alpha)</b>
- <b>Command Line</b>
- <b>Ability to toggle supported mods on and off</b>
- <b>Key animation when it's pressed</b>
- <b>Custom background (Image & Color)</b>

# For Developers

Before you begin reading I have created a very well documented example mod which you can use as a starting point.  
It shows examples for creating multiple views, navigating between those and creating your own commands.
https://github.com/ToniMacaroni/ComputerInterfaceExample

For more advanced examples check out the base library views here:  
https://github.com/ToniMacaroni/ComputerInterface/tree/main/ComputerInterface/Views

## Adding Views
Computer Interface works with "Views" (classes that inherit ComputerView or IComputerView).  
You navigate through the computer by specifying the type of your view.  
The instantiation / injection / Caching is handled by Computer Interface itself.
As stated you can normally ask for bound types in your views via zenject.

In your view you can for example navigate (go back, go to main menu, go to a specific view)  
and check for key presses (by overriding OnKeyPressed).  
All keys are wrapped by an EKeyboardKey enum to easier handle keys.

An example view may look like this:

```csharp
public class MyModView : ComputerView
    {
        // This is called when you view is opened
        public override void OnShow()
        {
            base.OnShow();
            // changing the Text property will fire an PropertyChanged event
            // which lets the computer know the text has changed and update it
            Text = "Monkey Computer\nMonkey Computer\nMounkey Computer";
        }

        // you can do something on keypresses by overriding "OnKeyPressed"
        // it get's an EKeyboardKey passed as a parameter which wraps the old character string
        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Delete:
                    // "ReturnToMainMenu" will basically switch to the main menu again
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

To actually create an entry in the main menu and specify an initial view  
you use a mod entry (inherits IComputerModEntry) and bind it via zenject.

Example ModEntry:
```csharp
public class MyModEntry : IComputerModEntry
    {
        // This is the mod name that is going to show up as a selectable mod
        public string EntryName => "MyMod";

        // This is the first view that is going to be shown if the user select you mod
        // The Computer Interface mod will instantiate your view 
        public Type EntryViewType => typeof(MyModView);
    }
```

Then you bind the mod entry like this:
```csharp
Container.Bind<IComputerModEntry>().To<MyModEntry>().AsSingle();
```

## Adding your own commands

Adding your own CLI commands is really easy.  
In a type that you bound via zenject request the CommandHandler and add your command.

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
                // args is an array of arguments (string) passed when entering the command
                // the command handler already checks if the correct amount of arguments is passed

                // the string you return is going to be shown in the terminal as a return message
                // you can break up the message into multiple lines by using \n
                return "MONKE";
            }));
        }
    }
```

I just created a dummy class called MyModCommandManager.  
But of course you can do this in any type as long as you request the CommandHandler.

## Disclaimers
This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. ©2021 Another Axiom LLC.
