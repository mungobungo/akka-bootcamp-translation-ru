# Урок 1.3: `Props` и `IActorRef`ы

На этом уроке мы более подробно разберем различные способы, которыми можно отправить сообщения. Этот урок больше концептуальный, поэтому много кода писать не придется. Но он чрезвычайно важен и дает понимание ключевых идей, с которыми вы столкнетесь позже. 

Мы немного изменим нашу программу. Изменения будут касаться `ConsoleReaderActor`. Ему больше не придется заниматься валидацеи. Вместо этого он будет пересылать собщения другому актору, который теперь отвечает за валидацию (а именно- `ValidationActor`). 

## Ключевые идеи / общая информация
### `IActorRef`ы
#### Что такое `IActorRef`?
Вообще говоря [`IActorRef`](http://api.getakka.net/docs/stable/html/56C46846.htm "Akka.NET Stable API Docs - IActorRef") это ссылка на актора. Цель `IActorRef`-а обеспечение поддержки отправки сообщений между акторами через `ActorSystem`. Вы никогда не общаетесь с актором напрямую. Вы посылаете сообщение `IActorRef`-у нужного актора и `ActorSystem` позаботится о доставке вашего сообщения.

#### Какого? Я не могу общаться со своими акторами? Почему это вдруг?
Вы можете общаться с ними, только не напрямую :) Вы должны говорить с ними через посредника в виде `ActorSystem`.

Вот вам пара причин, почему стоит посылать сообщения через `IActorRef`, и позволить `ActorSystem` выполнять работу по доставке сообщений до нужного актора.
  - Вы получаете дополнительную информацию о семантике сообщений. `ActorSystem` оборачивает все сообщения в конверт(`Envelope`)который содержит метаданные сообщения. Эти метаданные автоматически распаковываются и становятся доступны в контексте вашего актора.
  - Вы получаете "прозрачность местоположения": вам не надо беспокоиться на какой машине в сети запущен ваш актор. Заботиться об этом - задача системы. Это просто необходимо для удаленных аткоров, при помощи которых вы можете масштабировать вашу систему акторов для обработки больших объемов данных. (например можно сделать кластер из акторов, работающих на нескольких машинах). 

#### Как я знаю, что сообщение доставлено актору?
На данный момент вам не стоит беспокоиться об этом. Низкоуровневая  `ActorSystem` входящая в состав Akka.NET обеспечивает механизм подобной доставки. Но  акторы с гарантией доставки (`GuaranteedDeliveryActors`) это более продвинутая тема, которую мы разберем позже.

Сейсас просто поверьте, что доставлять сообщения это работа для `ActorSystem`, а не для вас. Вера двигает горы :)

#### ммм...ладненько, пусть система занимается доставкой сообщений. Так как вы говорите получить `IActorRef`?
Есть два основных способа получения `IActorRef`.

##### 1) Создание актора
Actors form intrinsic supervision hierarchies (we cover in detail in lesson 5). This means there are "top level" actors, which essentially report directly to the `ActorSystem` itself, and there are "child" actors, which report to other actors.

To make an actor, you have to create it from its context. And **you've already done this!** Remember this?
```csharp
// assume we have an existing actor system, "MyActorSystem"
IActorRef myFirstActor = MyActorSystem.ActorOf(Props.Create(() => new MyActorClass()), "myFirstActor")
```

As shown in the above example, you create an actor in the context of the actor that will supervise it (almost always). When you create the actor on the `ActorSystem` directly (as above), it is a top-level actor.

You make child actors the same way, except you create them from another actor, like so:
```csharp
// have to create the child actor somewhere inside myFirstActor
// usually happens inside OnReceive or PreStart
class MyActorClass : UntypedActor{
	protected override void PreStart(){
		IActorRef myFirstChildActor = Context.ActorOf(Props.Create(() => new MyChildActorClass()), "myFirstChildActor")
	}
}
```

**\*CAUTION***: Do NOT call `new MyActorClass()` outside of `Props` and the `ActorSystem` to make an actor. We can't go into all the details here, but essentially, by doing so you're trying to create an actor outside the context of the `ActorSystem`. This will produce a completely unusable, undesirable object.


##### 2) Look up the actor
All actors have an address (technically, an `ActorPath`) which represents where they are in the system hierarchy, and you can get a handle to them (get their `IActorRef`) by looking them up by their address.

We will cover this in much more detail in the next lesson.

#### Do I have to name my actors?
You may have noticed that we passed names into the `ActorSystem` when we were creating the above actors:
```csharp
// last arg to the call to ActorOf() is a name
IActorRef myFirstActor = MyActorSystem.ActorOf(Props.Create(() => new MyActorClass()), "myFirstActor")
```

This name is not required. It is perfectly valid to create an actor without a name, like so:
```csharp
// last arg to the call to ActorOf() is a name
IActorRef myFirstActor = MyActorSystem.ActorOf(Props.Create(() => new MyActorClass()))
```

That said, **the best practice is to name your actors**. Why? Because the name of your actor is used in log messages and in identifying actors. Get in the habit, and your future self will thank you when you have to debug something and it has a nice label on it :)

#### Are there different types of `IActorRef`s?
Actually, yes. The most common, by far, is just a plain-old `IActorRef` or handle to an actor, as above.

However, there are also some other `IActorRef`s available to you within the context of an actor. As we said, all actors have a context. That context holds metadata, which includes information  about the current message being processed. That information includes things like the `Parent` or `Children` of the current actor, as well as the `Sender` of the current message.

`Parent`, `Children`, and `Sender` all provide `IActorRef`s that you can use.

### Props
#### What are `Props`?
Think of [`Props`](http://api.getakka.net/docs/stable/html/CA4B795B.htm "Akka.NET Stable API Documentation - Props class") as a recipe for making an actor. Technically, `Props` is a configuration class that encapsulates all the information needed to make an instance of a given type of actor.

#### Why do we need `Props`?
`Props` objects are shareable recipes for creating an instance of an actor. `Props` get passed to the `ActorSystem` to generate an actor for your use.

Right now, `Props` probably feels like overkill. (If so, no worries.) But here's the deal.

The most basic `Props`, like we've seen, seem to only include the ingredients needed to make an actor—it's class and required args to its constructor.

BUT, what you haven't seen yet is that `Props` get extended to contain deployment information and other configuration details that are needed to do remote work. For example, `Props` are serializable, so they can be used to remotely create and deploy entire groups of actors on another machine somewhere on the network!

That's getting way ahead of ourselves though, but the short answer is that we need `Props` to support a lot of the advanced features (clustering, remote actors, etc) that give Akka.NET the serious horsepower which makes it interesting.

#### How do I make `Props`?
Before we tell you how to make `Props`, let me tell you what NOT to do.

***DO NOT TRY TO MAKE PROPS BY CALLING `new Props(...)`.*** Similar to trying to make an actor by calling `new MyActorClass()`, this is fighting the framework and not letting Akka's `ActorSystem` do its work under the hood to provide safe guarantees about actor restarts and lifecycle management.

There are 3 ways to properly create `Props`, and they all involve a call to `Props.Create()`.

1. **The `typeof` syntax:**
  ```csharp
  Props props1 = Props.Create(typeof(MyActor));
  ```

  While it looks simple, **we recommend that you do not use this approach.** Why? *Because it has no type safety and can easily introduce bugs where everything compiles fine, and then blows up at runtime*.

1. **The lambda syntax**:
  ```csharp
  Props props2 = Props.Create(() => new MyActor(..), "...");
  ```

  This is a mighty fine syntax, and our favorite. You can pass in the arguments required by the constructor of your actor class inline, along with a name.

1. **The generic syntax**:
  ```csharp
  Props props3 = Props.Create<MyActor>();
  ```

  Another fine syntax that we whole-heartedly recommend.

#### How do I use `Props`?
You actually already know this, and have done it. You pass the `Props`—the actor recipe—to the call to `Context.ActorOf()` and the underlying `ActorSystem` reads the recipe, et voila! Whips you up a fresh new actor.

Enough of this conceptual business. Let's get to it!

## Exercise
Before we can get into the meat of this lesson (`Props` and `IActorRef`s), we have to do a bit of cleanup.

### Phase 1: Move validation into its own actor
We're going to move all our validation code into its own actor. It really doesn't belong in the `ConsoleReaderActor`. Validation deserves to have its own actor (similar to how you want single-purpose objects in OOP).

#### Create `ValidationActor` class
Make a new class called `ValidationActor` and put it into its own file. Fill it with all the validation logic that is currently in `ConsoleReaderActor`:

```csharp
// ValidationActor.cs
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor that validates user input and signals result to others.
    /// </summary>
    public class ValidationActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public ValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("No input received."));
            }
            else
            {
                var valid = IsValid(msg);
                if (valid)
                {
                    // send success to console writer
                    _consoleWriterActor.Tell(new Messages.InputSuccess("Thank you! Message was valid."));
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationError("Invalid: input had odd number of characters."));
                }
            }

            // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
            Sender.Tell(new Messages.ContinueProcessing());

        }

        /// <summary>
        /// Determines if the message received is valid.
        /// Currently, arbitrarily checks if number of chars in message received is even.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static bool IsValid(string msg)
        {
            var valid = msg.Length % 2 == 0;
            return valid;
        }
    }
}
```

### Phase 2: Making `Props`, our actor recipes
Okay, now we can get to the good stuff! We are going to use what we've learned about `Props` and tweak the way we make our actors.

Again, we do not recommend using the `typeof` syntax. For practice, use both of the lambda and generic syntax!

> **Remember**: do NOT try to create `Props` by calling `new Props(...)`.
>
> When you do that, kittens die, unicorns vanish, Mordor wins and all manner of badness happens. Let's just not.

In this section, we're going to split out the `Props` objects onto their own lines for easier reading. In practice, we usually inline them into the call to `ActorOf`.

#### Delete existing `Props` and `IActorRef`s
In `Main()`, remove your existing actor declarations so we have a clean slate.

Your code should look like this right now:

```csharp
// Program.cs
static void Main(string[] args)
{
    // initialize MyActorSystem
    MyActorSystem = ActorSystem.Create("MyActorSystem");

    // nothing here where our actors used to be!

    // tell console reader to begin
    consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

    // blocks the main thread from exiting until the actor system is shut down
    MyActorSystem.AwaitTermination();
}
```


#### Make `consoleWriterProps`
Go to `Program.cs`. Inside of `Main()`, split out `consoleWriterProps` onto its own line like so:

```csharp
// Program.cs
Props consoleWriterProps = Props.Create(typeof (ConsoleWriterActor));
```

Here you can see we're using the typeof syntax, just to show you what it's like. But again, *we do not recommend using the `typeof` syntax in practice*.

Going forward, we'll only use the lambda and generic syntaxes for `Props`.

#### Make `validationActorProps`
Add this just to `Main()` also:

```csharp
// Program.cs
Props validationActorProps = Props.Create(() => new ValidationActor(consoleWriterActor));
```

As you can see, here we're using the lambda syntax.

#### Make `consoleReaderProps`
Add this just to `Main()` also:

```csharp
// Program.cs
Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
```

This is the generic syntax. `Props` accepts the actor class as a generic type argument, and then we pass in whatever the actor's constructor needs.

### Phase 3: Making `IActorRef`s using various `Props`
Great! Now that we've got `Props` for all the actors we want, let's go make some actors!

Remember: do not try to make an actor by calling `new Actor()` outside of a `Props` object and/or outside the context of the `ActorSystem` or another `IActorRef`. Mordor and all that, remember?

#### Make a new `IActorRef` for `consoleWriterActor`
Add this to `Main()` on the line after `consoleWriterProps`:
```csharp
// Program.cs
IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");
```


#### Make a new `IActorRef` for `validationActor`
Add this to `Main()` on the line after `validationActorProps`:

```csharp
// Program.cs
IActorRef validationActor = MyActorSystem.ActorOf(validationActorProps, "validationActor");
```

#### Make a new `IActorRef` for `consoleReaderActor`
Add this to `Main()` on the line after `consoleReaderProps`:

```csharp
// Program.cs
IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");
```

#### Calling out a special `IActorRef`: `Sender`
You may not have noticed it, but we actually are using a special `IActorRef` now: `Sender`. Go look for this in `ValidationActor.cs`:

```csharp
// tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
Sender.Tell(new Messages.ContinueProcessing());
```

This is the special `Sender` handle that is made available within an actors `Context` when it is processing a message. The `Context` always makes this reference available, along with some other metadata (more on that later).

### Phase 4: A bit of cleanup
Just a bit of cleanup since we've changed our class structure. Then we can run our app again!

#### Update `ConsoleReaderActor`
Now that `ValidationActor` is doing our validation work, we should really slim down `ConsoleReaderActor`. Let's clean it up and have it just hand the message off to the `ValidationActor` for validation.

We'll also need to store a reference to `ValidationActor` inside the `ConsoleReaderActor`, and we don't need a reference to the the `ConsoleWriterActor` anymore, so let's do some cleanup.

Modify your version of `ConsoleReaderActor` to match the below:

```csharp
// ConsoleReaderActor.cs
// removing validation logic and changing store actor references
using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console.
    /// Also responsible for calling <see cref="ActorSystem.Shutdown"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string StartCommand = "start";
        public const string ExitCommand = "exit";
        private readonly IActorRef _validationActor;

        public ConsoleReaderActor(IActorRef validationActor)
        {
            _validationActor = validationActor;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                DoPrintInstructions();
            }

            GetAndValidateInput();
        }


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
            if (!string.IsNullOrEmpty(message) && String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                // if user typed ExitCommand, shut down the entire actor system (allows the process to exit)
                Context.System.Shutdown();
                return;
            }

            // otherwise, just hand message off to validation actor (by telling its actor ref)
            _validationActor.Tell(message);
        }
        #endregion
    }
}

```

As you can see, we're now handing off the input from the console to the `ValidationActor` for validation and decisions. `ConsoleReaderActor` is now only responsible for reading from the console and handing the data off to another more sophisticated actor.

#### Fix that first `Props` call...
We can't very well recommend you not use the `typeof` syntax and then let it stay there. Real quick, go back to `Main()` and update `consoleWriterProps` to be use the generic syntax.

```csharp
Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
```

There. That's better.

### Once you're done
Compare your code to the solution in the [Completed](Completed/) folder to see what the instructors included in their samples.

If everything is working as it should, the output you see should be identical to last time:
![Petabridge Akka.NET Bootcamp Lesson 1.2 Correct Output](Images/working_lesson3.jpg)


#### Experience the danger of the `typeof` syntax for `Props` yourself
Since we harped on it earlier, let's illustrate the risk of using the `typeof` `Props` syntax and why we avoid it.

We've left a little landmine as a demonstration. You should blow it up just to see what happens.

1. Open up [Completed/Program.cs](Completed/Program.cs).
1. Find the lines containing `fakeActorProps` and `fakeActor` (should be around line 18).
2. Uncomment these lines.
	- Look at what we're doing here—intentionally substituting a non-actor class into a `Props` object! Ridiculous! Terrible!
	- While this is an unlikely and frankly ridiculous example, that is exactly the point. It's just leaving open the door for mistakes, even with good intentions.
1. Build the solution. Watch with horror as this ridiculous piece of code *compiles without error!*
1. Run the solution.
1. Try to shield yourself from everything melting down when your program reaches that line of code.

Okay, so what was the point of that? Contrived as that example was, it should show you that *using the `typeof` syntax for `Props` has no type safety and is best avoided unless you have a damn good reason to use it.*


## Отличная работа! Переходим к уроку №4 !
Ухх, это было непросто!

**Переходим к  [Уроку 4 - Дочерние акторы, иерархия акторов и супервизоры](../lesson4).**

## Есть вопросы?
**Не стесняйтесь задавать вопроосы** :).


Можете задавать любые вопросы, большие и маленькие, [в этом чате команд Petabridge и Akka.NET (английский)](https://gitter.im/petabridge/akka-bootcamp).

### Проблемы с кодом?
Если у вас возникил проблемы с запуском кода или чем-то другим, что необходимо починить в уроке, пожалуйста, [создайте issue](https://github.com/petabridge/akka-bootcamp/issues) и мы это пофиксим. Таким образом вы поможете всем кто будет проходить эту обучалку.
