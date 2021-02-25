# Computer Interface
## What is Computer Interface?
Computer Interface is a library for Gorilla Tag which enabled developers to write programs for the Gorilla Computer
and users to make more out of the computer.

If you want to contact me on discord: Toni Macaroni#8970

## Command Line
Computer Interface ships with a CLI that enables you to execute routine / change settings.  
Developers can also add their own commands which is discussed later.

By default Computer Interface ships with following commands:
- setcolor <r> <g> <b> //sets your gorilla color (e.g. setcolor 255 255 255)
- setname <name> //sets your gorilla name (e.g. setname toni)
- join <roomID> //joins a private room with the room id (e.g. join myroom)
- leave //disconnects from the current room
- cam <fp|tp> //sets the view mirrored to your monitor to eiter first person (fp) or third person (tp) (e.g. cam fp)
- setbg <r> <g> <b> //sets the background color of the computer screen (e.g. setbg 40 70 40)

## Additional Features
Computer Interface by iself also adds some changes.  
These currently are:
- visual queue for when a button is pressed (the button moves down and changes color)
- added cooldown to buttons to counter missclicks
- custom computer background

And many more to come...

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
