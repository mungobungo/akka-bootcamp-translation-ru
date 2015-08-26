# Урок 1.4: Дочерние акторы, иерархия акторов, супервизоры
Этот урок будет гигантским рывком как в расширении возможностей нашей программы, так и в нашем понимании принципов работы модели акторов.

Это один из самых сложных уроков, поэтому будьте готовы!

## Ключевые идеи / общая информация
Перед тем как мы погрузимся в детали иерархии акоторов, остановимся и зададим себе вопрос : зачем нам в принципе нужна иерархия?

Вот две ключевых причины существования иерархии:

1. Разбить работу на независимые части и поделить огромные массивы данных на удобоваримые куски
1. Ограничить влияние ошибок и сделать систему стабильной

### Иерархии позволяют разбить задачу на части

Иерарзия позволяет дробить нашу систему на все меньшие и меньшие части. Разные уровни иерархии могут отвечать за разный функционал (совсем как в жизни!).

В основном в актор-ориентированых приложениях огромные потоки данных разбиваются на маленькие ручейки, с которыми можно легко разобраться небольшим куском кода.


Возьмем к примеру Twitter (они используют JVM Akka). При помощи Akka, Twitter может расперделить огромный объем входящих данных на небольшие блоки, которые можно обработать. Например - Twitter дробит чудовищное число твитов на инидвидуальные потоки для каждого пользователя, который в данный момент находится на сайте. Каждый поток доставляется пользователи при помощи вебсокетов.

Видите общую идею? Возмите гродадный кусок работы. Рекурсивно разбейте его на части, легкие для понимания и обработки. Готово.

### Иерархии позволяют делать стабильные системы
Иерархии позволюят гибко ограничивать уровень риска и ответствености.


Подумайте о том, как работает армия. В армии есть генерал, который задает стратегию и контролирует поле боя. Но обычно он не идет на передовую, подвергая свою жизнь риску. Но тем не менее у него очень широкие полномочия и возможности. В то же вермя есть рядовые солдаты, которые выполняют рискованные операции на передовой и выполняют те приказы, которые они получили.

Система акторов работает точно таким же образом.

Акторы верхнего уровня обычно являются супервизорами, и отправляют рискованные операции как можно ниже по иерархии. Таким образом система может минимизировать риск, и обеспечить восстановление от ошибок без падения всей системы.

Обе концепции важны но в этом уроке мы сделаем акцент на том, как системы акторов используют иерархии для обеспечения стабильности.

Как они этого добиваются? При помощи **Супервизоров**

### Что такое супервизоры ? Какое мне до них дело ?
Супервизоры это базовая концепция, коотрая позволяет вашей системе акторов быстро изолировать ошибки и восстанавливаться после сбоев.

У каждого актора есть другой актор(супервизор), который присматривает за ним. Также супервизор помогает вернуться в нормальное состояние после ошибки. Это правило справедливо для всей иерархии акторов сверху донизу.

Идея супервизоров позволяет ганатировать, что если часть вашего приложения свалится с неожиданной ошибкой(необработанное исключение, таймаут сети, и т.п.), ошибка повлияет только на часть акторов в вашей иерархии.

Остальные акторы будут продолжать работать в нормальном режиме. Мы называем это "изоляцией ошибок" или "политикой сдерживания".

Как это работает? Давайте выясним&hellip;

### иерархия акторов
Для начала ключевая мысль: У каждого актора есть родитель, у некоторых акторов есть дочерние акторы. Родители являются супервизоварми и присматривают за своими детьми.

Поскольку родители являются супервизорами для своих детей, это означает что *** у кажого актора есть супервизор, и каждый актор МОЖЕТ БЫТЬ супервизором. ***

В рамках вашей системы акторов, все акторы выстроены в иерархию. Это означает, что есть акторы "верхнего уровня", которые отчитываются напрямую преед `ActorSystem`, и есть "дочерние" акторы, которые отчитываются перед другими акторами.

Полная картина иерархии выглядит следующим образом (Скоро мы разберем эту картину по кусочкам):
![Petabridge Akka.NET Bootcamp Lesson 1.3 Actor Hierarchies](Images/hierarchy_overview.png)


### Как разобраться в этой иерархии?
#### Основа всего -  "Guardians" они же Хранитель
 "хранители" это самые главные акторы во всей системе.

Я имею ввиду этих троих на вершине пищевой цепочки:
![Petabridge Akka.NET Bootcamp Lesson 1.3 Actor Hierarchies](Images/guardians.png)

##### Актор `/` 

Актор по имине `/` - основной актор всей системы акторов. Его можно назвать "Главным хранителем." Он  контролирует акторов `/system` и `/user`  (других "хранителей").

Всем акторам кроме этого нужны родители. Он находится вне рамок системы координат обычных акторов. Но подробно обсуждать его мы не будем.

##### Актор `/system`

Актор `/system` - это "Хранитель Системы". Основаная его забота - обеспечить безопасное выключение системы. Также он контролирует акторов, которые обеспечивают дополнительные возможности системы (логирование и т.п.). 

##### Актор `/user` 

Вот где начинается реальная движуха! И основное время разработки  вы проведете именно здесь .

Актора `/user` можно назвать "Хранитель Акторов". С этой точки зрения, `/user` является корневым элеметом вашей системы акторов, и обычно его называеют "корневым актором."

> Обычно выражение "корневой актор" относится к `/user`.

Как пользователю, вам не часть придется иметь дело с Хранителями. Наша основная задача - обеспечить корректную работу супервизоров ниже `/user`,  так чтобы исключения не могли добраться до Хранителей и обрушить систему.

#### Иерархия под началом `/user`
Вот альфа и омега всей иерархии акторов. Все ваши акторы так или иначе подчинаются `/user`.
![Akka: User actor hierarchy](Images/user_actors.png)

> Прямые наследники `/user` называются "верхнеуровневыми акторами."

Акторы всегда создаются как наследники другого актора.

Когда вы создаете актора в контексте самой системы акторов, этот актор становится верхнеуровневым актором:

```csharp
// создаем акторов вверху диаграммы
IActorRef a1 = MyActorSystem.ActorOf(Props.Create<BasicActor>(), "a1");
IActorRef a2 = MyActorSystem.ActorOf(Props.Create<BasicActor>(), "a2");
```

Теперь добавим наследников `a2`,  создавая их в контексте нашего будущего родителя:

```csharp
// создаем наследников a2
// этот код находится внутри a2
IActorRef b1 = Context.ActorOf(Props.Create<BasicActor>(), "b1");
IActorRef b2 = Context.ActorOf(Props.Create<BasicActor>(), "b2");
```

#### Адрес актора == позиция актора в иерархии
У каждого актора есть свой адрес. Чтобы послать сообщение от одного актора к другому, вам необходимо знать этот адрес ("ActorPath"). Вот как выглядит полный адрес актора:

![Akka.NET actor address and path](Images/actor_path.png)

> * "Путь" - это часть адреса актора, которая описыает его местоположение в иерархии. Каждый уровень иерархии разделяется слешом ('/').*

Например, если мы запустили приложение на `localhost`, полным адресом актора `b2` будет `akka.tcp://MyActorSystem@localhost:9001/user/a2/b2`.

Вопрос который вертится на языке - "Должен ли актор обязательно находиться в определенной точке иерархии"?
Например у меня есть `FooActor`, должен ли он обязательно быть наследником  `BarActor`, или его можно запихнуть куда угодно?

Ответ - **Любой актор может занять любое место в иерархии**.

> *Любой актор может занять любое место в иерархии.*


Хорошо, теперь когда мы разобрались с иерархией, давайте сделаем что-то полезное. Например супервизоров!

### Как супервизоры работают в иерархии акторов?

Теперь когда вы в курсе огранизации акторов, вы знаете что акторы контролируют(являются супервизорами) своих потомков. *Но они являются супервизорами акторов ровно на один уровень ниже. они не контролируют своих внуков, правнуков и т.п.)*


> Акторы контролируют только своих детей, ровно на один уровень вниз по иерархии.

#### А когда супервизоры вступают в игру? В случае ошибок!
Когда что-то идет не так с этим разбираются супервизоры. Если дочерний актор выбрасывает необработанное исключение и падает, это исключение ловит родитель и принимает решение о том, что делать дальше.

В частности, наследник может послать родителю с ообщению с типом ошибки (`Failure`), которая произошла. 

#### Каким образом родительский актор разбирается с ошибкой?

Вот два основных фактора влияющих на то, как разрешится проблема:

1. В зависимости от того, как именно упал дочерний актор (какой `Exception` указан в сообщении `Failure` )
1. Какую директиву использует родитель в ответ на  `Failure` наследника. Это определяется стратегией супервизора (`SupervisionStrategy`).

##### В случае возникновения исключения события развиваются следующим образом:

1. Необработанное исключение возникает в акторе (`c1`), за которым наблюдает его родитель (`b1`).
2. `c1` останавливается.
3. Система посылает сообщение `Failure` от `c1` к `b1`, с указанием исключения(`Exception`) которое произошло.
4. На основании директивы `b1` дает указание `c1` о дальнейших действиях.
5. Жизнь идет своим чередом, часть системы которая поломалась самоисцелилась, не уничтожив при этом вселенную. Котята и единороги, получив бесплатное мороженое и кофе балдят на радуге. Ня!


##### Директивы супервизоров
Когда в дочернем акторе случается ошибка, родитель может принять решение на основе директивы ("directives"). Стратегия супервизора находит директиву соответствующую типу исклюяения, позволяя обрабатывать разные исключения подобающим образом.

Список доступных директив (т.е. какие решения может принять супервизор):

- **Restart** перезапуск дочернего актора: наиболее популярный вариант, применяемый по умолчанию.
- **Stop**  полная остановка дочернего актора.
- **Escalate**  эскалация ошибки  (и остановка супервизора): родительский актор-супервизор говорит "Я не знаю что делать дальше! Я бросаю все и предаю управление СВОЕМУ супервизору"
- **Resume** продолжаем работу(игнорируем ошибку) : в подавляющем большинстве случаев вам это не понадобится. Пока проигнорируем.

> *Критически важный момент *** любое действие над родителем распространяется и на его потомков***. Если родительский актор остановлен, его дочерние акторы также будут остановлены. Если родитель перезапущен, наследники тоже будут перезапущены.*

##### Стратегии супервизоров
Сущуствует 2 стратегии для супервизоров:

1. One-For-One (Каждый-сам-за-себя), которая применяется по умолчанию
2. All-For-One (Все-за-одного)

 Разница заключается в том, насколько широко будут распространяться действия по разрешению ошибки.
 

**One-For-One** (Каждый-сам-за-себя) говорит о том, что директива, которую применяет супервизор будет относиться только к сбойному актору. Другие потомки супервизора не будут затронуты. Если вы не укажете стратеги, по умолчанию применится One-For-One . (Существует возможность создать собственную стратегию для супервизора.)

**All-For-One** (Все-за-одного) говорит, что директива будет распространяться на актора, в котором произошла ошибка, *а также на всех остальных* потомков супервизора.

Дополнительный важный выбор, который вам предствоит сделать, сколько раз дочерний актор может сбоить в течение заданного промежутка времени, пока его не отключат. (например, "не более 10 ошибок в течение 60 секунд, или мы тебя вырубим").

Пример стратегии супервизора:

```csharp
public class MyActor : UntypedActor
{
    // Если какой-то наследник MyActor-а выбросит исключение, применяем следующие правила
    // Перезапускаем, если число исключений меньше 10 за 30 секунд
    // в противном случае останавливаем поломанного актора
    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(// or AllForOneStrategy
            maxNrOfRetries: 10,
            withinTimeRange: TimeSpan.FromSeconds(30),
            localOnlyDecider: x =>
            {
                // Может быть ArithmeticException не критично 
                // для нашего приложения, поэтому просто продолжаем.
                if (x is ArithmeticException) return Directive.Resume;

                // А с этим исключением мы понятия не имеем что делать
                else if (x is InsanelyBadException) return Directive.Escalate;

                // Это исключение мы не можем толком обработать, поэтому остановим актора
                else if (x is NotSupportedException) return Directive.Stop;

                // Во всех остальных случаях просто перезапустим блудного сына
                else return Directive.Restart;
            });
    }

    ...
}
```

### Зачем все эти приседания? Политика сдерживания.
Весь смыл стратегий супервизора вместе с директивами заключается в возможности ограничить ошибку в рамках системы и дать возможность ей самоисцелиться. Таким образом система в целом не упадет. Но как мы этого добьемся?

Мы спускаем потенциально опасные операции вниз по иерархии, до тех акторов, которые выполняют исключетельно одну опасную задачу.

Например, мы запустили систему статистики во время ЧМ по футболу. Эта система позволяет смотреть результаты матчей и статистику по игрокам.

Поскольку это чемпионат мира, может статься, что API может быть ограничено по количество запросов, может отвечать не быстро, а может и просто упасть. (Без обид ФИФА, я люблю вас ребята вместе с ЧМ). Для примера возьмем эпичный матч Германия-Гана.

Наш сервис хранения резульаттов матчей должен периодически обновляться в процессе игры. Давайте предположим, что он дергает внешиний API который используется ФИФА для получения этих данных.

***Обращение по сети опасная вещь!*** Если запрос завершится с ошибкой, актор, который выполнял его остановится. Так как же нам защититься от подобного?

Мы будем хранить всю статистику в родителе, а опасный сетевой вызов дадим на откуп дочернему актору. Таким образом, даже если наследник упадет, это не повляет на родителя, который отвечает за важные данные.
Благодаря такому подходу мы **локазилировали ошибку** и ограничили ее распространение по системе.

Вот пример иерархии, которая может помощь для решения подобной задачи:

![Akka: User actor hierarchy](Images/error_kernel.png)

Обратите внимание, что у нас может быть сколько угодно копий этой структуры, допустим по одной копии на каждую игру за которой мы следим. **И нам не придется писать новый код для горизонтального масштабирования!** Красота.

> Вы можете услышать, как люди употребляют термин "ядро ошибки", имея ввиду какая часть системы подвержена влиянию ошибки. Также говорят "паттерн ядро ошибки", пордазумевая подход который я только что описал. Мы отодвигаем опасное поведение как можно дальше по иерархии и изолируем/защищаем родительские процессы.

## Упражнение
To start off, we need to do some upgrading of our system. We are going to add in the components which will enable our actor system to actually monitor a file for changes. We have most of the classes we need, but there are a few pieces of utility code that we need to add.
Для начала, немного проапгрейдим нашу системц. Мы собираемся добавить компоненты, которые позволят нашей программе действительно отслеживать изменения в файлах. Большинство задач уже решены, нам нужны некоторые системные функции, котороые позволят все собрать в кучу.

Мы почти готовы к запуску! Осталось добавить `TailCoordinatorActor`, `TailActor`, и `FileObserver`.

Цель данного упражнения - показать вам как создавать связь родитель/дети.

### Фаза 1: Небольшая подготовка
#### Замените `ValidationActor` на `FileValidatorActor`
Поскольку мы собираемся работать с файлами, замените `ValidationActor` на `FileValidatorActor`.

Добавьте новый класс, `FileValidatorActor`, при помощи [этого кода](Completed/FileValidatorActor.cs):

```csharp
// FileValidatorActor.cs
using System.IO;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor that validates user input and signals result to others.
    /// </summary>
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        private readonly IActorRef _tailCoordinatorActor;

        public FileValidatorActor(IActorRef consoleWriterActor, IActorRef tailCoordinatorActor)
        {
            _consoleWriterActor = consoleWriterActor;
            _tailCoordinatorActor = tailCoordinatorActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank. Please try again.\n"));

                // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                var valid = IsFileUri(msg);
                if (valid)
                {
                    // signal successful input
                    _consoleWriterActor.Tell(new Messages.InputSuccess(string.Format("Starting processing for {0}", msg)));

                    // start coordinator
                    _tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationError(string.Format("{0} is not an existing URI on disk.", msg)));

                    // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }



        }

        /// <summary>
        /// Проверяет, существует ли указанный пользователем файл
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
```

You'll also want to make sure to update the `Props` instance in `Main` that references the class:

```csharp
// Program.cs
Props validationActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor));
```

#### Update `DoPrintInstructions`
Just making a slight tweak to our instructions here, since we'll be using a text file on disk going forward instead of prompting the user for input.

Update `DoPrintInstructions()` to this:

```csharp
// ConsoleReaderActor.cs
private void DoPrintInstructions()
{
    Console.WriteLine("Please provide the URI of a log file on disk.\n");
}
```

#### Add `FileObserver`
This is a utility class that we're providing for you to use. It does the low-level work of actually watching a file for changes.

Create a new class called `FileObserver` and type in the code for [FileObserver.cs](Completed/FileObserver.cs). If you're running this on Mono, note the extra environment variable that has to be uncommented in the `Start()` method:

```csharp
// FileObserver.cs
using System;
using System.IO;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Turns <see cref="FileSystemWatcher"/> events about a specific file into messages for <see cref="TailActor"/>.
    /// </summary>
    public class FileObserver : IDisposable
    {
        private readonly IActorRef _tailActor;
        private readonly string _absoluteFilePath;
        private FileSystemWatcher _watcher;
        private readonly string _fileDir;
        private readonly string _fileNameOnly;

        public FileObserver(IActorRef tailActor, string absoluteFilePath)
        {
            _tailActor = tailActor;
            _absoluteFilePath = absoluteFilePath;
            _fileDir = Path.GetDirectoryName(absoluteFilePath);
            _fileNameOnly = Path.GetFileName(absoluteFilePath);
        }

        /// <summary>
        /// Begin monitoring file.
        /// </summary>
        public void Start()
        {
            // Need this for Mono 3.12.0 workaround
            // Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled"); // uncomment this line if you're running on Mono!

            // make watcher to observe our specific file
            _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);

            // watch our file for changes to the file name, or new messages being written to file
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            // assign callbacks for event types
            _watcher.Changed += OnFileChanged;
            _watcher.Error += OnFileError;

            // start watching
            _watcher.EnableRaisingEvents = true;

        }

        /// <summary>
        /// Stop monitoring file.
        /// </summary>
        public void Dispose()
        {
            _watcher.Dispose();
        }

        /// <summary>
        /// Callback for <see cref="FileSystemWatcher"/> file error events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileError(object sender, ErrorEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileError(_fileNameOnly, e.GetException().Message), ActorRefs.NoSender);
        }

        /// <summary>
        /// Callback for <see cref="FileSystemWatcher"/> file change events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // here we use a special ActorRefs.NoSender
                // since this event can happen many times, this is a little microoptimization
                _tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
            }

        }

    }
}
```

### Phase 2: Make your first parent/child actors!
Great! Now we're ready to create our actor classes that will form a parent/child relationship.

Recall that in the hierarchy we're going for, there is a `TailCoordinatorActor` that coordinates child actors to actually monitor and tail files. For now it will only supervise one child, `TailActor`, but in the future it can easily expand to have many children, each observing/tailing a different file.

#### Add `TailCoordinatorActor`
Create a new class called `TailCoordinatorActor` in a file of the same name.

Add the following code, which defines our coordinator actor (which will soon be our first parent actor).

```csharp
// TailCoordinatorActor.cs
using System;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message types
        /// <summary>
        /// Start tailing the file at user-specified path.
        /// </summary>
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; private set; }

            public IActorRef ReporterActor { get; private set; }
        }

        /// <summary>
        /// Stop tailing the file at user-specified path.
        /// </summary>
        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; private set; }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                // YOU NEED TO FILL IN HERE
            }

        }
    }
}



```

#### Create `IActorRef` for `TailCoordinatorActor`
In `Main()`, create a new `IActorRef` for `TailCoordinatorActor` and then pass it into `fileValidatorActorProps`, like so:

```csharp
// Program.Main
// make tailCoordinatorActor
Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

// pass tailCoordinatorActor to fileValidatorActorProps (just adding one extra arg)
Props fileValidatorActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
IActorRef validationActor = MyActorSystem.ActorOf(fileValidatorActorProps, "validationActor");
```

#### Add `TailActor`
Now, add a class called `TailActor` in its own file. This actor is the actor that is actually responsible for tailing a given file. `TailActor` will be created and supervised by `TailCoordinatorActor` in a moment.

For now, add the following code in `TailActor.cs`:

```csharp
// TailActor.cs
using System.IO;
using System.Text;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Monitors the file at <see cref="_filePath"/> for changes and sends file updates to console.
    /// </summary>
    public class TailActor : UntypedActor
    {
        #region Message types

        /// <summary>
        /// Signal that the file has changed, and we need to read the next line of the file.
        /// </summary>
        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; private set; }
        }

        /// <summary>
        /// Signal that the OS had an error accessing the file.
        /// </summary>
        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public string FileName { get; private set; }

            public string Reason { get; private set; }
        }

        /// <summary>
        /// Signal to read the initial contents of the file at actor startup.
        /// </summary>
        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }

            public string FileName { get; private set; }
            public string Text { get; private set; }
        }

        #endregion

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            // start watching file for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first message
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                // move file cursor forward
                // pull results from cursor to end of file and write to output
                // (this is assuming a log file type format that is append-only)
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }

            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell(string.Format("Tail error: {0}", fe.Reason));
            }
            else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }
    }
}
```

#### Add `TailActor` as a child of `TailCoordinatorActor`
Quick review: `TailActor` is to be a child of `TailCoordinatorActor` and will therefore be supervised by `TailCoordinatorActor`.

This also means that `TailActor` must be created in the context of `TailCoordinatorActor`.

Go to `TailCoordinatorActor.cs` and replace `OnReceive()` with the following code to create your first child actor!

```csharp
// TailCoordinatorActor.OnReceive
protected override void OnReceive(object message)
{
    if (message is StartTail)
    {
        var msg = message as StartTail;
		// here we are creating our first parent/child relationship!
		// the TailActor instance created here is a child
		// of this instance of TailCoordinatorActor
        Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
    }

}
```

### ***BAM!***
You have just established your first parent/child actor relationship!

### Phase 3: Implement a `SupervisorStrategy`
Now it's time to add a supervision strategy to your new parent, `TailCoordinatorActor`.

The default `SupervisorStrategy` is a One-For-One strategy ([docs](http://getakka.net/docs/Supervision#one-for-one-strategy-vs-all-for-one-strategy)) w/ a Restart directive ([docs](http://getakka.net/docs/Supervision#what-restarting-means)).

Add this code to the bottom of `TailCoordinatorActor`:

```csharp
// TailCoordinatorActor.cs
protected override SupervisorStrategy SupervisorStrategy()
{
    return new OneForOneStrategy (
        10, // maxNumberOfRetries
        TimeSpan.FromSeconds(30), // withinTimeRange
        x => // localOnlyDecider
        {
            //Maybe we consider ArithmeticException to not be application critical
            //so we just ignore the error and keep going.
            if (x is ArithmeticException) return Directive.Resume;

            //Error that we cannot recover from, stop the failing actor
            else if (x is NotSupportedException) return Directive.Stop;

            //In all other cases, just restart the failing actor
            else return Directive.Restart;
        });
}
```

### Phase 4: Build and Run!
Awesome! It's time to fire this baby up and see it in action.

#### Get a text file you can tail
We recommend a log file like [this sample one](DoThis/sample_log_file.txt), but you can also just make a plain text file and fill it with whatever you want.

Open the text file up and put it on one side of your screen.

#### Fire it up
##### Check the starting output
Run the application and you should see a console window open up and print out the starting contents of your log file. The starting state should look like this if you're using the sample log file we provided:
![Petabridge Akka.NET Bootcamp Actor Hierarchies](Images/working_tail_1.png)

**Leave both the console and the file open, and then...**

##### Add text and see if the `tail` works!
Add some lines of text to the text file, save it, and watch it show up in the `tail`!

It should look something like this:
![Petabridge Akka.NET Bootcamp Actor Hierarchies](Images/working_tail_2.png)

Congrats! YOU HAVE JUST MADE A PORT OF `tail` IN .NET!

### Once you're done
Compare your code to the solution in the [Completed](Completed/) folder to see what the instructors included in their samples.

## Great job! Onto Lesson 5!
Awesome work! Well done on completing this lesson, we know it was a bear! It was a big jump forward for our system and in your understanding.

Here is a high-level overview of our working system!

![Akka.NET Unit 1 Tail System Diagram](Images/system_overview.png)

**Let's move onto [Lesson 5 - Looking up Actors by Address with `ActorSelection`](../lesson5).**

---
## Supervision FAQ
### How long do child actors have to wait for their supervisor?
This is a common question we get: What if there are a bunch of messages already in the supervisor's mailbox waiting to be processed when a child reports an error? Won't the crashing child actor have to wait until those are processed until it gets a response?

Actually, no. When an actor reports an error to its supervisor, it is sent as a special type of "system message." *System messages jump to the front of the supervisor's mailbox and are processed before the supervisor returns to its normal processing.*

> *System messages jump to the front of the supervisor's mailbox and are processed before the supervisor returns to its normal processing.*

Parents come with a default SupervisorStrategy object (or you can provide a custom one) that makes decisions on how to handle failures with their child actors.

### But what happens to the current message when an actor fails?
The current message being processed by an actor when it is halted (regardless of whether the failure happened to it or its parent) can be saved and re-processed after restarting. There are several ways to do this. The most common approach used is during `preRestart()`, the actor can stash the message (if it has a stash) or it can send the message to another actor that will send it back once restarted. (Note: If the actor has a stash, it will automatically unstash the message once it successfully restarts.)


## Any questions?
**Don't be afraid to ask questions** :).

Come ask any questions you have, big or small, [in this ongoing Bootcamp chat with the Petabridge & Akka.NET teams](https://gitter.im/petabridge/akka-bootcamp).

### Problems with the code?
If there is a problem with the code running, or something else that needs to be fixed in this lesson, please [create an issue](https://github.com/petabridge/akka-bootcamp/issues) and we'll get right on it. This will benefit everyone going through Bootcamp.
