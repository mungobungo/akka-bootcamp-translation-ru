# Урок 1.4: Дочерние акторы, иерархия акторов, супервизоры
Этот урок будет гигантским рывком как в расширении возможностей нашей программы, так и в нашем понимании принципов работы модели акторов.

Это один из самых сложных уроков, поэтому будьте готовы!

## Ключевые идеи / общая информация
Перед тем как мы погрузимся в детали иерархии акоторов, остановимся и зададим себе вопрос : зачем нам в принципе нужна иерархия?

Вот две ключевых причины существования иерархии:

1. Разбить работу на независимые части и поделить огромные массивы данных на удобоваримые куски
1. Ограничить влияние ошибок и сделать систему стабильной

### Иерархии позволяют разбить задачу на части

Иерархия позволяет дробить нашу систему на все меньшие и меньшие части. Разные уровни иерархии могут отвечать за разный функционал (совсем как в жизни!).

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

### К чему эти приседания? Политика сдерживания.
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

Также убедимся, что мы обновлии `Props` в `Main`, который ссылается на наш класс:

```csharp
// Program.cs
Props validationActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor));
```

#### Обновим `DoPrintInstructions`
Немного обновим наши инструкции, поскольку теперь вместо пользовательсого ввода будет использоваться файл.

Исправьте `DoPrintInstructions()` следующим образом:

```csharp
// ConsoleReaderActor.cs
private void DoPrintInstructions()
{
    Console.WriteLine("пожалуйста укажите URI лог-файла на диске.\n");
}
```

#### Добавляем `FileObserver`
Этот класс мы будем использовать для отслеживания изменений в файле.

Создайте класс `FileObserver` и продублируйте код из [FileObserver.cs](Completed/FileObserver.cs). Если вы запускаете проект под Mono, раскомментируйте определение переменной окружения в методе `Start()`:

```csharp
// FileObserver.cs
using System;
using System.IO;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Превращает события от <see cref="FileSystemWatcher"/> в собщения для <see cref="TailActor"/>.
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
        /// Начинаем мониторить файл.
        /// </summary>
        public void Start()
        {
            // Нужно как костыль для Mono 3.12.0 
            // Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled"); // раскомментируйте эту строку, если запускаете программу под Mono

            // начинаем наблюдать за файлом
            _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);

            // нам интересны события изменения имени файла или добавления в него данных
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            // добавляем коллбеки
            _watcher.Changed += OnFileChanged;
            _watcher.Error += OnFileError;

            // начинаем наблюдать
            _watcher.EnableRaisingEvents = true;

        }

        /// <summary>
        /// Останавливаем мониторинг файла.
        /// </summary>
        public void Dispose()
        {
            _watcher.Dispose();
        }

        /// <summary>
        /// Коллбек для ошибок <see cref="FileSystemWatcher"/> .
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileError(object sender, ErrorEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileError(_fileNameOnly, e.GetException().Message), ActorRefs.NoSender);
        }

        /// <summary>
        /// Коллбек для изменений файла <see cref="FileSystemWatcher"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // Мы используем специальную ссылку ActorRefs.NoSender
                // небольшая микрооптимизация, поскольку событие может прийти несколько раз
                _tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
            }

        }

    }
}
```

### Фаза 2: Создадим родственные связи между акторами!
Супер! Теперь мы готовы создать акторов со связями..

В иерархии, которую мы собираемся построить, `TailCoordinatorActor` координирует работу акторов которые отслеживают изменения и читают данны из файлов. Пока этот актор будет контролироть только `TailActor`, но в будущем можно добавить поддержки большего количества дочерних акторов, каждый из которых может следить/читать свой личный файл.

#### Добавляем `TailCoordinatorActor`
Создайте класс `TailCoordinatorActor` в файле с соответствующим именем.

Добавьте код, который описывает актора-координатора( и который скоро станет нашим первым родительским актором).

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
        /// Начинаем читать изменения файла
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
        /// Перестаем читать изменения в файле
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
                // КОД ДОБАВЛЯТЬ СЮДА
            }

        }
    }
}



```

#### Добавляем `IActorRef` на `TailCoordinatorActor`
В `Main()`, создадим `IActorRef`, который ссылается на `TailCoordinatorActor` и передадим эту ссылку в `fileValidatorActorProps`. Как-то так:

```csharp
// Program.Main
// создаем tailCoordinatorActor
Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

// передаем tailCoordinatorActor в fileValidatorActorProps (просто добавляем еще один аргумент)
Props fileValidatorActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
IActorRef validationActor = MyActorSystem.ActorOf(fileValidatorActorProps, "validationActor");
```

#### Добавляем `TailActor`
Теперь создадим `TailActor`. Он будет отвечать за чтение последних изменений в файле. `TailActor` будет создан, и будет контролироваться `TailCoordinatorActor` одним движением руки.

Добавьте следующий код в  `TailActor.cs`:

```csharp
// TailActor.cs
using System.IO;
using System.Text;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Мониторит файл <see cref="_filePath"/> и отправляет изменения на консоль.
    /// </summary>
    public class TailActor : UntypedActor
    {
        #region Message types

        /// <summary>
        /// Сигнализирует о том, что файл изменился, и мы можем прочитать новую строку
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
        /// Сигнализирует об ошибке операционной системы при попытке доступа к файлу.
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
        /// Сигнализирует о необходимости считать первичное содержимое файла при запуске
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

            // начинаем наблюдать за изменениями в файле
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // открываем поток с правами на одновременные чтение/запись (чтобы в открытый файл можно было писать)
            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // Читаем первичное содержимое файла и выводим его на консоль
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
              
                // считываем данные от текущего положения до конца файла
                // (предполагается, что в лог-файл все изменения добавляются в конец)
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

#### Указываем, что `TailActor` является дочерним для `TailCoordinatorActor`
Напоминалка: `TailActor` является наследником `TailCoordinatorActor`, и следовательно, `TailCoordinatorActor` будет супервизором, который контролирует `TailActor`.

Это также означает, что `TailActor` должен быть создан в контексте `TailCoordinatorActor`.

Перейдите к `TailCoordinatorActor.cs` и замените `OnReceive()` кодом, которые создаст вашего первого дочернего актора!

```csharp
// TailCoordinatorActor.OnReceive
protected override void OnReceive(object message)
{
    if (message is StartTail)
    {
        var msg = message as StartTail;
		// Тут мы создаем пурвую связь родитель/наследник!
		// экземпляр TailActor создан как потомок экземпляра TailCoordinatorActor
        Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
    }

}
```

### ***БУММММ!***
Вы только что установили связь между родителем и дочерним актором!

### Фаза 3: Укажем `SupervisorStrategy`

Теперь самое время добавить стратегию супервизора `TailCoordinatorActor`-у.

По умолчанию `SupervisorStrategy` будет One-For-One (Каждый-сам-за-себя) ([docs](http://getakka.net/docs/Supervision#one-for-one-strategy-vs-all-for-one-strategy)) с директивой Restart ([docs](http://getakka.net/docs/Supervision#what-restarting-means)).

Добавьте в конец `TailCoordinatorActor`:

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

### Фаза 4: Собираем и запускаем!
Красотища! Теперь самое время посмотреть на эту детку в действии.

#### Укажите файл, который вы будете обрабатывать
Мы рекомендуем лог-файл [вроде этого](DoThis/sample_log_file.txt), но вы можете указать обычный текстовый файл, и записать туда что угодно.

Откройте текстовый файл и поместите окно в одну строну вашего экрана.

#### Запускаем
##### Проверьте сообщения при запуске
Запустите приложение, и вы должны увидеть в консоли содержимое лог-файла. Примерно как на картинке:
![Petabridge Akka.NET Bootcamp Actor Hierarchies](Images/working_tail_1.png)

**Оставьте консоль и файл открытыми ии....**

##### Добавьте текст в файл, и узрите работу `tail`!
Добавьте несколько строчек файл, сохраните его, и убедитесь что строки появились в  `tail`!

Все будет выглядеть следующим образом:
![Petabridge Akka.NET Bootcamp Actor Hierarchies](Images/working_tail_2.png)

Поздравляю! ВЫ ТОЛЬКО ЧТО ПОРТИРОВАЛИ `tail` НА .NET!

### Когда все сделано
Сравните код, который у вас вышел с примером [Completed](Completed/) , обратите внимание на комментарии в примере.

## Отлично поработали! Переходим к уроку №5!
Неплохо!  Поздравляем с завершение этого урока, мы знаем что это было непросто! Вы совершили действительно большой скачок в понимании акторов и их взаимодествия.

Вот высокоуровневая структура нашей системы!

![Akka.NET Unit 1 Tail System Diagram](Images/system_overview.png)

**Переходим к [Урок 5: Ищем акторов по адресу при помощи `ActorSelection`](../lesson5).**

---
## ЧАВО по супервизорам
### Как долго дочерние акторы должны ждать реакции супервизора?

Нам часто задают вопрос - что если пачка сообщений находится в ящике у супревизора в ожидании обработки, а в этот момент дочерний актор сигнализирует об ошибке? Неужели ему придется ждать пока супервизор обработает все остальные сообщения?


Вообще говоря, нет. Когда актор отправляет сообщение об ошибке своему супервизору, оно отправляется при помощи специального "системного сообщения".  *Системые сообщения обрабатываются супревизором в первую очередь, в обход остальных сообщений во входящем ящике.*

> * Системные сообщения обрабатываются в первую очередь, и супервизор не будет обрабатывать обычные сообщения, пока не разберется со всеми системными.*

У всех супервизоров указана стратегия (SupervisorStrategy) по умолчанию (вы можете написать свою) при помощи которой принимаются решения о том, как обрабатывать ошибки в наследниках.

### Но что случится с текущим сообщением, которое обрабатывает актор, если он остановит работу?
Сообщение, которое обрабатывается в момент остановки актора( вне зависимости от того, произошла ошибка в самом акторе или в его родителе) может быть сохранено и повторно обработано после перезапуска. Этого можно добиться несколькими способами. Наиболее популярный вариант  - воспользовавшись `preRestart()`, актор может сохранить сообщение в стек сообщений(stash),если он у него есть. Еще можно отправить сообщение другому актору который пришлет его обратно после перезапуска. Примечание: если у актора есть стек сообщений, он автоматически вытянет сообщение из стека после перезапуска.

## Есть вопросы?
**Не стесняйтесь задавать вопроосы** :).

Можете задавать любые вопросы, большие и маленькие, [в этом чате команд Petabridge и Akka.NET (английский)](https://gitter.im/petabridge/akka-bootcamp).

### Проблемы с кодом?
Если у вас возникил проблемы с запуском кода или чем-то другим, что необходимо починить в уроке, пожалуйста, [создайте issue](https://github.com/petabridge/akka-bootcamp/issues) и мы это пофиксим. Таким образом вы поможете всем кто будет проходить эту обучалку.
