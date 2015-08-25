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
Акторы формируют иерархию супревизоров (мы разберемся с этим на уроке №5) . Это означает что существуют акторы "верхнего уровня",  которые отчитываются напрямую перед системой `ActorSystem`, а есть дочерние акторы, которые отчитываются перед другими акторами.

Чтобы создать актора, вы должны воплпользоваться конекстом. А **это вы уже  делали!** Припоминаете?
```csharp
// предположим, что система акторов "MyActorSystem" была создана ранее
IActorRef myFirstActor = MyActorSystem.ActorOf(Props.Create(() => new MyActorClass()), "myFirstActor")
```

Как показано в примере выше, вы содаете актора в контексте другого актора, который будет супервизором(почти всегда). Когда вы создаете актора напрямую в `ActorSystem`, это актор верхнего уровня.

Вы создаете дочерних акторов подобным образом. Вы просто делаете это изнутри другого актора.
Что-то вроде этого:
```csharp
// создаем дочернего актора где-то в глубине myFirstActor
// Обычно это делается в методах OnReceive или PreStart
class MyActorClass : UntypedActor{
	protected override void PreStart(){
		IActorRef myFirstChildActor = Context.ActorOf(Props.Create(() => new MyChildActorClass()), "myFirstChildActor")
	}
}
```

**\*ОСТОРОЖНО***: НЕ создавайте актора при помощи `new MyActorClass()`. Всегда пользуйтесь услугами `Props` и `ActorSystem` для создания акторов. Мы не будем сильно углубляться, но поступая так, актор будет создан вне контекста `ActorSystem`. И этот актор будет бесполезным объектом, который никто не сможет нормально исопльзовать.


##### 2) Поиск актора
У всех акторов есть адрес (технически, за это отвечает класс `ActorPath`) который описывает их место в иерархии. Вы можете получыить ссылку на них (`IActorRef`) используя соответствующий адрес.

Подробнее об этом на следующем уроке.

#### Надо ли давать акторам имена?
Вы могли заметить, что когда мы создавали акторов, мы передавали имена в `ActorSystem`:
```csharp
// последний аргумент метода ActorOf() это имя
IActorRef myFirstActor = MyActorSystem.ActorOf(Props.Create(() => new MyActorClass()), "myFirstActor")
```

Давать актору имя необязательно. Можно абсолютно спокойно создавать безымянных акторов, например так:
```csharp
// в этот раз имя не передаем 
IActorRef myFirstActor = MyActorSystem.ActorOf(Props.Create(() => new MyActorClass()))
```

НО, **правило хорошего тона - давать акторам осмысленные имена**. Почему?  Потому что имя актора используется для логирования собщеий, а также для идентификации акторов. Приобретите эту полезную привычку, и вы в будущем будете счастлиыв, когда придется отлаживать что-то, что имеет четкое имя и хорошо видно в логах :)

#### Существуют ли различные типы `IActorRef`-ов?
Строго говоря, да. Но в большинстве случаев, это просто старый добрый `IActorRef`.

Однако в рамках конекста актора, доступны другие `IActorRef`-ы. Как мы говорили ранее, у всех акторов есть конекст. В этом контексте содержатся метаданные, которые включают в себя информацию о сообщении, которое обрабатывается в данный момент. Эта информация включает в себя вещи вроде родителя и детей текущего актора (свойства `Parent` и `Children` соответственно). Также вы можете получить ссылку на отправителя сообщения (`Sender`).

`Parent`, `Children`, и `Sender` это обычные `IActorRef`-ы, которые можно использовать в ваших программах.

### Props-ы
#### Что такое `Props`?
Думайте о [`Props`](http://api.getakka.net/docs/stable/html/CA4B795B.htm "Akka.NET Stable API Documentation - Props class") как о рецепте создания актора. Технически, `Props` это конфигурационный класс, который содержит в себе всю информацию, необоходимую для создания экземпляра актора определенного типа.

#### Зачем нужны `Props`-ы?
`Props` это рецепты по созданию акторов. `Props`-ы передаются в `ActorSystem`, для того чтобы создать актора нужного типа .

На данный момент, вам может показаться, что использовние `Props`-ов приносит больше головной боли чем пользы. (Если вам действительно так кажется, не беспокойтесь.) Но вот в чем дело.

До сих пор, все `Props`-ы, которые мы видели, включали в себя только ингридиенты в виде типа актора и параметров для конструктора.
НО, вы еще не видели, как `Props`-ы можно расширить информацие об удаленном развертывании актора, и другими конфигурационными параметрами. Например, `Props`-ы являются сериализируемымы сущностями, так что их можно использовать для того, чтобы удаленно создавать и развертывать группы акторов, на другой машине на противоположном участке сети!

Мы сильно забегаем вперед, но вкратце - `Props`-ы нужны для поддержки продвинутых возможностей для создания акторов. Например кластеры, удаленные акторы и т.п. Эти вещи добавляют дополнительную мощность в движок Akka.NET.

#### Как мне создать `Props`?
Прежде, чем мы расскажем как воздавать `Props`-ы, давайте расскажем чего НЕ НАДО делать.

***НЕ ПЫТАЙТЕСЬ ИХ СОЗДАВАТЬ ПРИ ПОМОЩИ `new Props(...)`.*** Это то же самое, что создавать актора через `new MyActorClass()`. Действуя в обход фреймворка, вы лишаетесь всех возожностей `ActorSystem` , а также гарантий по перезапуску актора и управлением его времени жизни.

Существует  3 правильных способа создания `Props`, и все они включают вызов `Props.Create()`.

1. **Синтаксис `typeof`:**
  ```csharp
  Props props1 = Props.Create(typeof(MyActor));
  ```

  Хоть этот подоход кажется простым, **мы не рекомендуем вам его использовать.** Почему? *Потому что это не типобезопасно. И вы можете получить головную боль, когда все успешно скомпилируется, но грохнется во время выполнения.*.

1. **Лябмда-синтаксис**:
  ```csharp
  Props props2 = Props.Create(() => new MyActor(..), "...");
  ```

  Это простой и мощный вариант, наш любимчик. Вы можете указать как параметры конструктора, так и имя для акотора.

1. **Параметризированный синтаксис**:
  ```csharp
  Props props3 = Props.Create<MyActor>();
  ```

  Еще один подход, который мы можем рекомендовать от всего сердца.

#### Как правильно пользоваться `Props`-ами?
На самом деле вы это уже знаете и неоднократно использовали. Вы передаете `Props`(рецепт по созданию актора) в вызов метода `Context.ActorOf()`. Под капотом `ActorSystem` прочитает этот рецепт и вуаля! Подаст вам свежеприготовленного актора на блюдечке с голубой каемочкой.

Но хватит концептуальщины! Пора сделать что-то руками!

## Упражнение

Прежде, чем мы доберемя до самой вкусной части урока, нам необходимо провести небольшую уборку.

### Фаза №1: Переносим валидацию в отдельного актора
Валидация точно не должна находиться в `ConsoleReaderActor`. Каждый объект должен иметь одну зону ответственности, поэтому давайте переносим весь наш код валидации в специально созданного для этих целей актора. 

#### Создаем класс `ValidationActor`
Создайте новый класс `ValidationActor` в отдельном файел. Перенесите код валидации из `ConsoleReaderActor`:

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

### Фаза №2: Создание `Props`-ов, рецептов по созданию акторов
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
