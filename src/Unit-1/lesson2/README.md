# Урок 1.2: Создание и обработка сообщений

На этом уроке вы создадите свои собственные сообщения разных типов и научитесь контролировать процесс обработки их акторами. Таким образом вы познакомитесь с основами обмена информацией при помощи сообщеий  а также событийно-ориентированным подходом в рамках системы акторов.


Этот урок стартует в том месте, где мы завершили урок номер 1. Мы продложим расширять функционоальные возможности наших консольных акторов. Помимо определения собственных сообщений, мы также добавим доплнительную валидацию вводимых данных, на основе которой будем предпринимать различные действия.


## Ключевые идеи / общая информация
### Что такое сообщение?
Любой C# объект может быть сообщением. Сообщение можем иметь тип `string`, быть `int`-овым значением, типом, объектом реализцющим интерфейс... быть всем чем угодно.

Несмотря на полную свободу в определении сообщений, рекомендуемый подоход это создавать собственные сообщения внутри классов, которые обрабатывают эти сообщения. Также постарайтесь инкапсулировать любое состояние внутри этих классов (например сохраните причину ошибки валидации в поле `Reason` класса `ValidationFailed`... подсказка, подсказка)

### Как я могу отослать сообщение актору?
Как вы уже успели увидеть в первом уроке, достаточно использовать `Tell()` для отправки сообщений.

### Как мне обработать сообщение?

Это полностью зависит от вас. И практически никак не связано с Akka.NET. Вы можете обработать (или не обработать) сообщение которые вы получили в акторе.

### Что будет, если мой актор получит сообщение, которе не знает как обрабатывать?

Акторы игнорируют те сообщения, которые не знают как обрабатывать. Попадет ли это сообщение в лог зависит от типа актора.

Если использовать `UntypedActor`, необработанные сообщения не будут записаны в лог, только если вы явно не пометите их вроде такого:

```csharp

class MyActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
         if (message is Messages.InputError)
        {
            var msg = message as Messages.InputError;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg.Reason);
        }
        else
        {
            Unhandled(message);
        }
    }
}
```

Однако при использовании `ReceiveActor`, с которым мы разберемся в блоке 2, для необработанных сообщений автоматически вызывается  `Unhandled`, так что отправка в лог будет сделана автовматически..

### Как актор может ответить на сообщение?
Как захотите - вы можете ответить простой обработкой сообщения, отсылкой ответа `Sender`-у, пересылкой сообщения другому актору, а можете вообще ничего не делать.

> **Внимание:** Каждый раз когда актор получает сообщения, он может достучаться до отправителя, используя свойство `Sender`.

## Упражнение
В этом упражденнии, мы добавим в нашу систему базовую валидацию. Для этого мы будем использовать собственные типы сообщений, которые будут сигнализировать о корректности обработки сообщений.

### Шаг номер 1: Определение собственных типов сообщений
#### Создайте новый класс `Messages` в файле `Messages.cs`.
This is the class we'll use to define system-level messages that we can use to signal events. The pattern we'll be using is to turn events into messages. That is, when an event occurs, we will send an appropriate message class to the actor(s) that need to know about it, and then listen for / respond to that message as needed in the receiving actors.
Этот класс будет использоваться для определения системных сообщений, которые будут сигнализировать о наступлении событий. Этот паттерн мы будем использовать чтобы преобразовать события в сообщения. Таким образом, когда случится событие, мы пошлем соответственное сообщение актору(-ам), которым это интересно. А потом будем слушать и отвечать на это сообщение.

#### Add regions for each message type
Add three regions for different types of messages to the file. Next we'll be creating our own message classes that we'll use to signify events.

```csharp
// in Messages.cs
#region Neutral/system messages
#endregion

#region Success messages
#endregion

#region Error messages
#endregion
```

In these regions we will define custom message types to signal these situations:
	- user provided blank input
	- user provided invalid input
	- user provided valid input


#### Make `ContinueProcessing` message
Define a marker message class in the `Neutral/system messages` region that we'll use to signal to continue processing (the "blank input" case):

```csharp
// in Messages.cs
#region Neutral/system messages
/// <summary>
/// Marker class to continue processing.
/// </summary>
public class ContinueProcessing { }
#endregion
```

#### Make `InputSuccess` message
Define an `InputSuccess` class in the `Success messages` region. We'll use this to signal that the user's input was good and passed validation (the "valid input" case):

```csharp
#region Success messages
// in Messages.cs
/// <summary>
/// Base class for signalling that user input was valid.
/// </summary>
public class InputSuccess
{
    public InputSuccess(string reason)
    {
        Reason = reason;
    }

    public string Reason { get; private set; }
}
#endregion
```

#### Make `InputError` messages
Define the following `InputError` classes in the `Error messages` region. We'll use these messages to signal invalid input occurring (the "invalid input" cases):

```csharp
// in Messages.cs
#region Error messages
/// <summary>
/// Base class for signalling that user input was invalid.
/// </summary>
public class InputError
{
    public InputError(string reason)
    {
        Reason = reason;
    }

    public string Reason { get; private set; }
}

/// <summary>
/// User provided blank input.
/// </summary>
public class NullInputError : InputError
{
    public NullInputError(string reason) : base(reason) { }
}

/// <summary>
/// User provided invalid input (currently, input w/ odd # chars)
/// </summary>
public class ValidationError : InputError
{
    public ValidationError(string reason) : base(reason) { }
}
#endregion
```


> **NOTE:** You can compare your final `Messages.cs` to [Messages.cs](Completed/Messages.cs/) to make sure you're set up right before we go on.

### Phase 2: Turn events into messages and send them
Great! Now that we've got messages classes set up to wrap our events, let's use them in `ConsoleReaderActor` and `ConsoleWriterActor`.

#### Update `ConsoleReaderActor`
Add the following internal message type to `ConsoleReaderActor`:
```csharp
// in ConsoleReaderActor
public const string StartCommand = "start";
```

Update the `Main` method to use `ConsoleReaderActor.StartCommand`:

Replace this:

```csharp
// in Program.cs
// tell console reader to begin
consoleReaderActor.Tell("start");
```

with this:

```csharp
// in Program.cs
// tell console reader to begin
consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);
```

Replace the `OnReceive` method of `ConsoleReaderActor` as follows. Notice that we're now listening for our custom `InputError` messages, and taking action when we get an error.

```csharp
// in ConsoleReaderActor
protected override void OnReceive(object message)
{
    if (message.Equals(StartCommand))
    {
        DoPrintInstructions();
    }
    else if (message is Messages.InputError)
    {
        _consoleWriterActor.Tell(message as Messages.InputError);
    }

    GetAndValidateInput();
}
```

While we're at it, let's add `DoPrintInstructions()`, `GetAndValidateInput()`, `IsValid()` to `ConsoleReaderActor`. These are internal methods that our `ConsoleReaderActor` will use to get input from the console and determine if it is valid. (Currently, "valid" just means that the input had an even number of characters. It's an arbitrary placeholder.)

```csharp
// in ConsoleReaderActor, after OnReceive()
#region Internal methods
private void DoPrintInstructions()
{
    Console.WriteLine("Write whatever you want into the console!");
    Console.WriteLine("Some entries will pass validation, and some won't...\n\n");
    Console.WriteLine("Type 'exit' to quit this application at any time.\n");
}

/// <summary>
/// Reads input from console, validates it, then signals appropriate response
/// (continue processing, error, success, etc.).
/// </summary>
private void GetAndValidateInput()
{
    var message = Console.ReadLine();
    if (string.IsNullOrEmpty(message))
    {
        // signal that the user needs to supply an input, as previously
        // received input was blank
        Self.Tell(new Messages.NullInputError("No input received."));
    }
    else if (String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
    {
        // shut down the entire actor system (allows the process to exit)
        Context.System.Shutdown();
    }
    else
    {
        var valid = IsValid(message);
        if (valid)
        {
            _consoleWriterActor.Tell(new Messages.InputSuccess("Thank you! Message was valid."));

            // continue reading messages from console
            Self.Tell(new Messages.ContinueProcessing());
        }
        else
        {
        	Self.Tell(new Messages.ValidationError("Invalid: input had odd number of characters."));
        }
    }
}

/// <summary>
/// Validates <see cref="message"/>.
/// Currently says messages are valid if contain even number of characters.
/// </summary>
/// <param name="message"></param>
/// <returns></returns>
private static bool IsValid(string message)
{
    var valid = message.Length % 2 == 0;
    return valid;
}
#endregion
```

#### Update `Program`
First, remove the definition and call to `PrintInstructions()` from `Program.cs`.

Now that `ConsoleReaderActor` has its own well-defined `StartCommand`, let's go ahead and use that instead of hardcoding the string "start" into the message.

As a quick checkpoint, your `Main()` should now look like this:
```csharp
static void Main(string[] args)
{
    // initialize MyActorSystem
    MyActorSystem = ActorSystem.Create("MyActorSystem");

    var consoleWriterActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
    var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));

    // tell console reader to begin
    consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

    // blocks the main thread from exiting until the actor system is shut down
    MyActorSystem.AwaitTermination();
}
```

Not much has changed here, just a bit of cleanup.

#### Update `ConsoleWriterActor`
Now, let's get `ConsoleWriterActor` to handle these new types of messages.

Change the `OnReceive` method of `ConsoleWriterActor` as follows:

```csharp
// in ConsoleWriterActor.cs
protected override void OnReceive(object message)
{
    if (message is Messages.InputError)
    {
        var msg = message as Messages.InputError;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg.Reason);
    }
    else if (message is Messages.InputSuccess)
    {
        var msg = message as Messages.InputSuccess;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg.Reason);
    }
    else
    {
        Console.WriteLine(message);
    }

    Console.ResetColor();
}
```

As you can see here, we are making `ConsoleWriterActor` pattern match against the type of message it receives, and take different actions according to what type of message it receives.

### Phase 3: Build and run!
You should now have everything you need in place to be able to build and run. Give it a try!

If everything is working as it should, you should see an output like this:
![Petabridge Akka.NET Bootcamp Lesson 1.2 Correct Output](Images/working_lesson2.jpg)

### Once you're done
Compare your code to the solution in the [Completed](Completed/) folder to see what the instructors included in their samples.

##  Great job! Onto Lesson 3!
Awesome work! Well done on completing this lesson.

**Let's move onto [Lesson 3 - `Props` and `IActorRef`s](../lesson3).**

## Any questions?
**Don't be afraid to ask questions** :).

Come ask any questions you have, big or small, [in this ongoing Bootcamp chat with the Petabridge & Akka.NET teams](https://gitter.im/petabridge/akka-bootcamp).

### Problems with the code?
If there is a problem with the code running, or something else that needs to be fixed in this lesson, please [create an issue](https://github.com/petabridge/akka-bootcamp/issues) and we'll get right on it. This will benefit everyone going through Bootcamp.
