# Урок 1.6: Жизненный цикл акторов
Вах! Вы только посмотрите на себя! Сумели добраться до конца первого блока! Поздравляем. Серьезно. Мы очень ценим и благодарим вас за вашу настойчивость в учебе.

Данный урок будет завершающим в нашей серии "основы работы с акторами". И завершим мы одной из самых важных концепций - жизненным циклом акторов.

## Ключевые концепции / общая информация
### Что такое жизненный цикл актора?
У акторов есть жестко заданный жизненный цикл. Сначала актора создают, потом запускают, но основное время жизни он проводит за получением и обработкой сообщений. В случае, если актор вам больше не нужен, вы можете его остановить.

### Из каких этапов состоит жизненный цикл актора?
В Akka.NET существует 5 основных этапов жизненного цикла актора:

1. `Starting` (Актор в процессе запуска)
2. `Receiving` (Актор обрабатывает сообщения)
3. `Stopping` (Актор в процессе остановки)
4. `Terminated`, (Актор остановлен)
5. `Restarting` (Актор в процессе перезапуска)

![жизненный цикл актора в Akka.NET](Images/lifecycle.png)

Давайте разберем их по порядку.

#### `Starting` (Актор в процессе запуска)
Актор просыпается. Это первоначальное состояние актора, когда он инициализируется при помощи `ActorSystem`.

#### `Receiving` (Актор получает сообщения)
Актор готов принимать сообщения. Его почтовый ящик (`Mailbox` об этом чуть позже), начинает передавать сообщения для обработки в метод `OnReceive`.

#### `Stopping` (Актор в процессе остановки)
На протяжении этойго этапа актор очищает свое состояние. Что именно происходит - зависит от того, останавливается ли актор или же просто перезапускается.


Если актор перезапускается, он обычно сохраняет свое состояние и/или сообщения, чтобы обработать их после перезапуска.


Если актор останавливается, все сообщения из его почтового ящика (`Mailbox` ) пересылаются в специальное хранилище недоставленных сообщений ( `DeadLetters`) у `ActorSystem`. В `DeadLetters` сообщения, которые невозомжно доставить, например потому что актор-получатель уже не существует.

#### `Terminated` (Актор остановлен)
Актер мертв.  Любые сообщения, посылаемые его `IActorRef`, попадут в `DeadLetters`. Такого актора нельзя перезапустить. Однако можно создать нового актора с тем же адресом. У него будет другая  `IActorRef` , но тот же самый путь (`ActorPath`).

#### `Restarting` (Актор в состоянии перезапуска)
Актор будет перезапущен и скоро перейдет в состояние `Starting`

### Жизенный цикл и методы обработчики
Итак, как вы можете вклиниться в жизенный цикл аткоров ? Существует 4 места на которые вы можете подписаться.

#### `PreStart` (Предзапуск)
Код в методе `PreStart`  выполняется до того, как актор начнет получать сообщения. И это неплохое место для проведения инициализации. Этот метод также вызывается при перезапуске.

#### `PreRestart` (ПредПереЗапуск)
Если ваш актор случайно упал (т.е. бросил необработанное исключение), родитель перезапустит его. Внутри `PreRestart` можно почистить ресурсы перед перезапуском или сохранить текущее сообщения для последующей обработки.

#### `PostStop` (ПостОстановка)
`PostStop` вызыается в момент, когда актор уже остановлен и не получает сообщений.  В этом методе можно делать очистку ресурсов.  Этот метод также вызывается во время `PreRestart`. Но вы можете переопределить  `PreRestart`, и не вызывать `base.PreRestart` , если вас не устраивает такое поведение. 

`DeathWatch` также вызывается из метода `PostStop`. `DeathWatch` - система подписки, которая позволяет любому актору получить уведомление о завершении работы любого другого актора. 

#### `PostRestart` (Пост-перезапуск)
`PostRestart` вызывается после PreRestart, но перед PreStart. Здесь хорошо  добавлять логику и дополнительную диагностику о возможных причинах сбоя.

Вот как методы обработчики ложатся на жизненный цикл актора:

![Жизенный цикл актора Akka.NET с указанием методов](Images/lifecycle_methods.png)

### Как мне все-таки вклиниться в жизеннный цикл актора?
Для этого просто перегрузите необходимый метод, например вот так:

```csharp
 /// <summary>
/// Инициализация аткора
/// </summary>
protected override void PreStart()
{
    // здесь можно делать что угодно
}
```

### Which are the most commonly used life cycle methods?
#### `PreStart`
`PreStart` is far and away the most common hook method used. It is used to set up initial state for the actor and run any custom initialization logic your actor needs.

#### `PostStop`
The second most common place to hook into the life cycle is in `PostStop`, to do custom cleanup logic. For example, you may want to make sure your actor releases file system handles or any other resources it is consuming from the system before it terminates.

#### `PreRestart`
`PreRestart` is in a distant third to the above methods, but you will occasionally use it. What you use it for is highly dependent on what the actor does, but one common case is to stash a message or otherwise take steps to get it back for reprocessing once the actor restarts.

### How does this relate to supervision?
In the event that an actor accidentally crashes (i.e. throws an unhandled Exception,) the actor's supervisor will automatically restart the actor's lifecycle from scratch - without losing any of the remaining messages still in the actor's mailbox.

As we covered in lesson 4 on the actor hierarchy/supervision, what occurs in the case of an unhandled error is determined by the `SupervisionDirective` of the parent. That parent can instruct the child to terminate, restart, or ignore the error and pick up where it left off. The default is to restart, so that any bad state is blown away and the actor starts clean. Restarts are cheap.

## Exercise
This final exercise is very short, as our system is already complete. We're just going to use it to optimize the initialization and shutdown of `TailActor`.

### Move initialization logic from `TailActor` constructor to `PreStart()`
See all this in the constructor of `TailActor`?

```csharp
// TailActor.cs constructor
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
```

While it works, initialization logic really belongs in the `PreStart()` method.

Time to use your first life cycle method!

Pull all of the above init logic out of the `TailActor` constructor and move it into `PreStart()`. We'll also need to change `_observer`, `_fileStream`, and `_fileStreamReader` to non-readonly fields since they're moving out of the constructor.

The top of `TailActor.cs` should now look like this

```csharp
// TailActor.cs
private FileObserver _observer;
private Stream _fileStream;
private StreamReader _fileStreamReader;

public TailActor(IActorRef reporterActor, string filePath)
{
    _reporterActor = reporterActor;
    _filePath = filePath;
}

// we moved all the initialization logic from the constructor
// down below to PreStart!

/// <summary>
/// Initialization logic for actor that will tail changes to a file.
/// </summary>
protected override void PreStart()
{
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
```

Much better! Okay, what's next?

### Let's clean up and take good care of our `FileSystem` resources
`TailActor` instances are each storing OS handles in `_fileStreamReader` and `FileObserver`. Let's use `PostStop()` to make sure those handles are cleaned up and we are releasing all our resources back to the OS.

Add this to `TailActor`:

```csharp
// TailActor.cs
/// <summary>
/// Cleanup OS handles for <see cref="_fileStreamReader"/> and <see cref="FileObserver"/>.
/// </summary>
protected override void PostStop()
{
    _observer.Dispose();
    _observer = null;
    _fileStreamReader.Close();
    _fileStreamReader.Dispose();
    base.PostStop();
}
```

### Phase 4: Build and Run!
That's it! Hit `F5` to run the solution and it should work exactly the same as before, albeit a little more optimized. :)

### Once you're done
Compare your code to the solution in the [Completed](Completed/) folder to see what the instructors included in their samples.

## Great job!
### WOW! YOU WIN! Phenomenal work finishing Unit 1.

**Ready for more? [Start Unit 2 now](../../Unit-2 "Akka.NET Bootcamp Unit 2").**

## Any questions?
**Don't be afraid to ask questions** :).

Come ask any questions you have, big or small, [in this ongoing Bootcamp chat with the Petabridge & Akka.NET teams](https://gitter.im/petabridge/akka-bootcamp).

### Problems with the code?
If there is a problem with the code running, or something else that needs to be fixed in this lesson, please [create an issue](https://github.com/petabridge/akka-bootcamp/issues) and we'll get right on it. This will benefit everyone going through Bootcamp.
