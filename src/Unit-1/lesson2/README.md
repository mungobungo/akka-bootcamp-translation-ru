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

Этот класс будет использоваться для определения системных сообщений, которые будут сигнализировать о наступлении событий. Этот паттерн мы будем использовать чтобы преобразовать события в сообщения. Таким образом, когда случится событие, мы пошлем соответственное сообщение актору(-ам), которым это интересно. А потом будем слушать и отвечать на это сообщение.

#### Добавьте регионы для кажого типа сообщений

Добавьте три региона для различных типов сообщений. Потом мы создадим наши собственные классы сообщений, которые будем использовать для обработки событий.

```csharp
// in Messages.cs
#region Neutral/system messages
#endregion

#region Success messages
#endregion

#region Error messages
#endregion
```


В этих регионах мы напишем классы, которые будут сигнализировать о следующих ситуациях:
	- пользователь ввел пустую строку
	- пользователь ввел некорректные данные
	- пользователь ввел корректные данные


#### Создайте сообщение `ContinueProcessing`
Создайте класс для сообщения-маркера в регионе `Neutral/system messages`, мы будем исполозвать его для продолжения обработки в тех случая, когда пользователь ничего не ввел:

```csharp
// in Messages.cs
#region Neutral/system messages
/// <summary>
/// Класс-маркер для продолжения обработки.
/// </summary>
public class ContinueProcessing { }
#endregion
```

#### Создайте сообщение `InputSuccess`
Создайте класс `InputSuccess` в регионе `Success messages`. 
Мы используем его в тхе случаях, когда пользователь введет корректные данные, которые успешно пройдут валидацию.:

```csharp
#region Success messages
// in Messages.cs
/// <summary>
/// Базовый класс, сигнализирующий о том, что пользовательский ввод валиден.
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

#### Создайте сообщение `InputError`


Создайте несколько наследников `InputError` в регионе `Error messages`. Они будут нужны в случаях неверно введенных данных.
```csharp
// in Messages.cs
#region Error messages
/// <summary>
/// Базовый класс сигнализирующий об ошибке ввода
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
/// Пользователь ничего не ввел.
/// </summary>
public class NullInputError : InputError
{
    public NullInputError(string reason) : base(reason) { }
}

/// <summary>
/// Пользователь ввел неверные данные (в данном случае - нечетное число символов)
/// </summary>
public class ValidationError : InputError
{
    public ValidationError(string reason) : base(reason) { }
}
#endregion
```


> **Внимание:**  Вы можете сравнить ваш файл  `Messages.cs` с эталонным примером [Messages.cs](Completed/Messages.cs/). Убедитесь, что все сделано правильно и едем дальше.

### Фаза №2: Превращаем события в сообщения и посылаем их
Супер! Теперь мы можем обернуть события в наши классы-сообщения. Давайте используем их в `ConsoleReaderActor` и `ConsoleWriterActor`.

#### Обновляем `ConsoleReaderActor`
Добавьте внутреннее сообщение в `ConsoleReaderActor`:
```csharp
// in ConsoleReaderActor
public const string StartCommand = "start";
```

Обновите метод `Main`, таким образом, чтобы он использовал `ConsoleReaderActor.StartCommand`:

Замените это:

```csharp
// in Program.cs
// tell console reader to begin
consoleReaderActor.Tell("start");
```
вот этим:

```csharp
// in Program.cs
// tell console reader to begin
consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);
```

Замените метод `OnReceive` у актора `ConsoleReaderActor` как показано ниже. Обратите внимание на то, что мы уже обрабатываем специализировованные сообщения типа `InputError` ,  и предпренимаем различные дейтсвия в случае ошибкиы.

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

While we're at it, let's add `DoPrintInstructions()`, `GetAndValidateInput()`, `IsValid()` to `. These are internal methods that our `ConsoleReaderActor` will use to get input from the console and determine if it is valid. (Currently, "valid" just means that the input had an even number of characters. It's an arbitrary placeholder.)

Пока находимся этом файле, давайте добавим актору `ConsoleReaderActor`-у методы DoPrintInstructions()`, `GetAndValidateInput()`, `IsValid()`. Этими внутренними методами наш актор воспользуется для проверки корректности ввода. (Сейчас "правильным" вводом считается строка, состоящая из четного количества символов. Это просто заглушка, которую вы сможете заменить на то что вам больше нравится). 

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

#### Обновляем `Program`
Сначала уберем определение и вызов метода `PrintInstructions()` из файла `Program.cs`.

Теперь, когда у актора `ConsoleReaderActor` есть собственая команд `StartCommand`, воспользуемся ей вместо хардкода.

В результате изменений , ваша `Main()` должна выглядеть приблизительно так:
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

Не так уж много изменений, просто немного привели в порядок.

#### Обновляем `ConsoleWriterActor`
Теперь заставим `ConsoleWriterActor`-а обрабатывать новые типы сообщений.

Измените метод `OnReceive` в `ConsoleWriterActor` следующим образом:

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

Как вы видите, мы заставляем `ConsoleWriterActor`-а проверять  каждое сообщеник, которое он получает и выполнять различные действия в зависимости от типа этого сообщения.

### Фаза 3: Собираем и запускаем!
Все готово к сборке и запуска проекта. Давайте пробовать!

Если все отработало должным образом, вывод должен выглядеть приблизительно так::
![Petabridge Akka.NET Bootcamp Lesson 1.2 Correct Output](Images/working_lesson2.jpg)

### Когда все сделано
Сравните код, который у вас вышел с примером [Completed](Completed/) , обратите внимание на комментарии в примере.

## Хорошая работа! Переходим к уроку №3!
Супер! Поздравляем с завершением этого урока!.

**Двигаем к [Урок 3 - `Props` и `IActorRef`-ы](../lesson3).**

## Есть вопросы?
**Не стесняйтесь задавать вопроосы** :).


Можете задавать любые вопросы, большие и маленькие, [в этом чате команд Petabridge и Akka.NET (английский)](https://gitter.im/petabridge/akka-bootcamp).

### Проблемы с кодом?
Если у вас возникил проблемы с запуском кода или чем-то другим, что необходимо починить в уроке, пожалуйста, [создайте issue](https://github.com/petabridge/akka-bootcamp/issues) и мы это пофиксим. Таким образом вы поможете всем кто будет проходить эту обучалку.
