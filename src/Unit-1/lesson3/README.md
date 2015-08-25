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
Так, теперь займемся чем-то полезным! Мы применим наши знания о `Props`-ах и оптимизируем создание акторов.

Повторяем, мы НЕ рекомендуем ситнаксис с исползованием `typeof`. Лучше используйте лямбды!

> **Запомните**: НЕ создавайте `Props` через `new Props(...)`.
>
> Если вы так сделаете, котята умрут мучительной смертью, единороги исчезнут, Мордор победит и случатся всякие бяки. Просто не надо так делать.

В этом разделе мы разнесем определение  `Props`  на несколько строк, для того чтобы было проще читать. В боевом коде мы обычно пишем их в одну строку с `ActorOf`.

#### Удалите текущие `Props` и`IActorRef`ы
Уберите все строки, создающие акторов из `Main()`, так мы получим чистое состояние.

Ваш код должен выглядеть следующим образом:

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


#### Создайте `consoleWriterProps`
Перейдите в файл `Program.cs`. Внутри `Main()`, создайте `consoleWriterProps` на отдельной строке:

```csharp
// Program.cs
Props consoleWriterProps = Props.Create(typeof (ConsoleWriterActor));
```

Здесь мы воспользовались typeof-синтаксисом, чтобы показать что так тоже будет работать. Но еще раз *мы НЕ рекомендуем использовать `typeof` на практике*.


Далее по тексту мы будем использовать только лямбды или перегрузки по типам.

#### создаем `validationActorProps`
добавьте следующий код в`Main()`:

```csharp
// Program.cs
Props validationActorProps = Props.Create(() => new ValidationActor(consoleWriterActor));
```

Здесь, как вы видите мы воспользовались лябмдами.

#### Make `consoleReaderProps`
Add this just to `Main()` also:

```csharp
// Program.cs
Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
```

А здесь используем список шаблонов. `Props` специализируется типом нужного актора, а парамтером передаем необходимые для создания актора данные.

### Фаза 3: Получаем `IActorRef`-ы ипользуя разные `Props`-ы
Теперь у нас есть `Props` для всех акторов, которые нам нужны! Подходящее время для создания этих самых акторов!

Помните: не пытайтесь создать актора при поиощи `new Actor()`, вне контекста `ActorSystem` или другого `IActorRef`. Мордор и всякое такое прочее, помните?

#### Создаем `IActorRef` для `consoleWriterActor`
Добавьте этот код в `Main()` сразу после `consoleWriterProps`:
```csharp
// Program.cs
IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");
```

#### Создаем `IActorRef` для `validationActor`
Добавьте этот код в `Main()` сразу после `validationActorProps`:

```csharp
// Program.cs
IActorRef validationActor = MyActorSystem.ActorOf(validationActorProps, "validationActor");
```

#### Создаем `IActorRef` для `consoleReaderActor`
Добавьте этот код в  `Main()` сразу после `consoleReaderProps`:

```csharp
// Program.cs
IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");
```

#### Используем специальный `IActorRef` -  `Sender` (отправитель)
Вы могли и не заметить, но в нашей программе мы используем специальный тип `IActorRef` - `Sender`. Загляните в  `ValidationActor.cs`:

```csharp
// Говорим отправителю, чтоюы он продложал заниматься своими делами
Sender.Tell(new Messages.ContinueProcessing());
```

 `Sender` это специальная ссылка внутри  `Context`-а актора, доступная в момет обработки сообщения. Эта ссылка (вместе с парочкой других) всегда доступна через `Context` актора.

### Фаза 4: Подчищаем хвосты
Немного приберемся после того, как мы изменили структуру наших классов. После этого можно будет запускать нашу программу.

#### Обновим `ConsoleReaderActor`
Теперь когда `ValidationActor` занят всей валидацией, `ConsoleReaderActor` значительно упрощается. Давайте будем просто отправлять сообщения `ValidationActor`-у когда нам надо проверить входные данные.

Также мы храним ссылку на  `ValidationActor` внутри `ConsoleReaderActor`, а ссылка на `ConsoleWriterActor` нам больше не нужна.

После всех изменений ваша версия `ConsoleReaderActor` должна выглядеть следующим образом:

```csharp
// ConsoleReaderActor.cs
// Удаляем логику валидации и подчищаем ссылки на других акторов
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
Как можно видеть, мы теперь отправляем весь ввод с консоли напряямую `ValidationActor`.
 `ConsoleReaderActor` отвечает только за чтение данных с консоли и отправку их на обработку более умному актору.

#### Исправим первый вызов `Props`...
Мы запрещать вам пользоваться `typeof` для создания акторов, и тем не менее оставлять подобные штуки в коде.  Быстренько, вернитесь в `Main()` и сделайте так, чтобы `consoleWriterProps` создавался при помощи  шаблонов.

```csharp
Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
```

Вооот. Так-то лучше.

### Когда все готово
Сравните ваш код с решением в папке [Completed](Completed/), и проверьте наличие дополнительных подсказок в примере.

Если все работает как надо, вывод приложения будет примерно следующим:
![Petabridge Akka.NET Bootcamp Lesson 1.2 Correct Output](Images/working_lesson3.jpg)


#### Почувствуйте опасность `typeof` на собственной шкуре

Поскольку мы часто говорили об этом раньше, вот причина, почему пользоваться  `typeof` для создания `Props` очень рискованно.

В нашем примене заложена необольшая бомба. Вам предстоит ее взорвать и посмотреть что получится.

1. Откройте [Completed/Program.cs](Completed/Program.cs).
1. Найдите строки с `fakeActorProps` и `fakeActor` (должны быть в районе 18-й строки ).
2. Раскомментируйте эти строки.
	- Посмотрите что мы делам! Мы предаем в `Props` не класс актора, а обычный объект! Глупо! Ужасно!
	- Несмотря на то что пример выглядит искуственным, он отлично демонстрирует пробдему. Вы открываете дверь возможным ошибкам, несмотря на ваши хорошие намерения.
1. Скомпилируйте приложение. Посмотрите как этот ужасный кусок кода *скомпилируется без ошибок!*
1. Запустите приложение.
1. Попытайтесь защититься от осколков, когда ваша программа доберется до той злополучной строчки.

Ладненько, в чем был смысл всего этого действа? Несмотря на абсурдность примера, вы теперь знаете, что *надо избегать `typeof` при создании `Props`. *


## Отличная работа! Переходим к уроку №4 !
Ухх, это было непросто!

**Переходим к  [Уроку 4 - Дочерние акторы, иерархия акторов и супервизоры](../lesson4).**

## Есть вопросы?
**Не стесняйтесь задавать вопроосы** :).


Можете задавать любые вопросы, большие и маленькие, [в этом чате команд Petabridge и Akka.NET (английский)](https://gitter.im/petabridge/akka-bootcamp).

### Проблемы с кодом?
Если у вас возникил проблемы с запуском кода или чем-то другим, что необходимо починить в уроке, пожалуйста, [создайте issue](https://github.com/petabridge/akka-bootcamp/issues) и мы это пофиксим. Таким образом вы поможете всем кто будет проходить эту обучалку.
