# Урок 1.5: Урок 5: Ищем акторов по адресу при помощи `ActorSelection`
Добро пожаловать на пятый урок! Хочу заметить что мы прошли немалый путь вместе. Потратьте **несолько секунд чтобы получить удовольствие от этого осознания, и поблагодарите самого себя за инвестиции времени и энегрии в учебу**.

Мммм... это было здорово!

Продолжим же движение к светлому будущему!

В этом уроке мы изучим как уменьшить связность между акторами, в также новый способ коммуникации между акторами: [`ActorSelection`](http://api.getakka.net/docs/stable/html/CC0731A6.htm "Akka.NET Stable API Docs - ActorSelection class"). Этот урок будет немного короче чем предыдущий, потому мы уже заложили основательный фундамент для дальнейшего развития.

## Ключевые концепии / общая информация
`ActorSelection` это естественное развитие иерархии акторов, с которой мы познакомились на прошлом уроке. Поскольку мы знаем, что акторы живут в иерархиях, возникает вопрос - как могут общаться акторы, которые НЕ находятся на одном и том же уровне?

Мы знаем, что  для того, чтобы послать сообщение актору, нам понадобится ссылка на него. Но теперь у нас акторы разбросаны по всей иерархии, и у нас не всегда есть возможность получить прямую ссылку (`IActorRef`) на актора, которому мы собираемся послать сообщение.  

*Как же мы можем послать сообщение актору в другой части иерархии, если у нас нет его `IActorRef`? Что же делать?*

Воспользоваться `ActorSelection`.

### Что такое `ActorSelection`?
`ActorSelection` дает возможность использовать адрес актора (`ActorPath`), чтобы получить ссылку на него. При этом вам не надо иметь прямую ссылку (`IActorRef`) на него.

Вместо того, чтобы получать ссылку в конструкторе или передавать ее как параметр, вы "находите" актора при помощи его `ActorPath` (напиминаем, что `ActorPath` это месторасположение актора в иерархии). Это чем-то похоже на поиск людей в Skype. Если вы не знаете его имени, вы можете искать по e-mail.

Будьте однако осторожны, потому что `ActorSelection` не всегда даст вам одну и только одну ссылку(`IActorRef`) на актора.


Технически, `ActorSelection` не обязательно указывает на конкретного актора и его `IActorRef`. На самом деле это структура, которая содержит ссылки на всех акторов, которые подходят под ваш запрос. Для этого можно использовать звездочки, так что выражение может выбрать от 0 до бесконечности акторов. (Об этом чуть позже).

`ActorSelection` также может получить две различные ссылки (`IActorRef`) с тем же самым именем актора, если первый из них умер, и был заменен другим ( не перезапущен, поскольку при перезапуске `IActorRef` сохраняется).

#### Так это объект? Или процесс? Или и то и другое?
Мы предпочитаем думать о  `ActorSelection` как о процессе И об объекте: процесс ищет актора(-ов) при помощи `ActorPath`, и возвращает объект, который позволяет нам отправлять сообщения всем акторам, которые попали в выборку..

### Почему меня должен волновать `ActorSelection`?
Спасибо, очень хороший вопрос!  `ActorSelection` дает вам несколько приятных возможностей.

#### Независимость от месторасположения (Location transparency)
 [Прозрачное расположение](http://getakka.net/docs/concepts/location-transparency) означате, что для того, чтобы послать сообщение актору, вам не надо знать его точное расположение в системе акторов. Потому что эта самая система может состоять из сотен машин. Вас не волнует, находятся акторы на одной физической машине или раскиданы по сотне машин на нескольких континентах. Все, что вам надо знать, это адрес актора (его `ActorPath`).

Можете провести аналогию с мобильным телефоном - вам не надо знать что ваш друг Вася сейчас селе Бродилово, Красносельского уезда, Полифонической губернии. Если вы знаете номер его телефона, вы можете ему дозвониться. Об всех сложностях маршрутизации позаботится мобильный оператор.

Прозрачное расположение (которое работает благодаря  `ActorSelection`) чрезвычайно важно для  создания масштабируемых систем, способных обеспечить высокую отказоустрочивость. Более подробно эту тему мы разберем в блоках 2 и 3.

#### Слабая связность
Поскольку вам теперь не надо постоянно держать ссылку на `IActorRef`, и передавать ее туда-обратно, связность у ваших акторов низкая. И как мы знаем из ООП, слабая свазность это ХО-РО-ШО!. Это означате, что компоненты вашей системы слабо завзаны друг на друга, и их можно леко адаптировать и исопльзовать. Это снижает трудозатраты на поддержку решения.

#### Динамическое поведение
Динамическое поведение актора - продвинутая концепция, в которую мы погрузимся с головой в начале второго блока. Пока просто имейте ввиду, что поведение кажого актора может быть очень гибким. ЭТо позволяет акторам работать как конечные автоматы.

Зачем для этого нужен `ActorSelection`? Нууу, если вы создаете чрезвычайно динамичную системы, где акторы постоянно появляются и исчезают, и при этом пытаются сохранять и передавать ссылки друг на друга, проблемы вам гарантированны. `ActorSelection` позволит вам отправлять сообщения только небольшому количеству действительно нужных вам акторов. И при этом вам не придется забивать себе голову проблемой корректной передачи ссылок.

Можно добавить еще один уровень динамики, и не хардкодить `ActorPath` внутрь актора. А получать путь внутри сообщений.

#### Гибкая схема общения == легко адаптируемая система
Давайте немного разовьем эту идею адаптируемости, поскольку это очень важно для вашей счастливой жизни в роли разработчка, отказоустрочивости вашей системы, и скорости, с которой может двигаться ваша организация.

Поскольку сильная связность между компонентами теперь отсутствует, вы получаете ускорение разработки. Вы можете добавлять совершенно новых акторов в совершенно новые части иерархии, без перелопачивания всего прежде написанного кода. Ваша система становится более гибкой в коммуникациях и в нее можно легко добавлять новых акторов (а.к.а требования).

#### В кратце: `ActorSelection` позволяет легко вносить изменения в вашу систему. 

### Ок, ясно. Так в каких случах все-таки пользоваться `ActorSelection`?
#### При общение с акторами верхнего уровня
Наиболее частый случай исопльзования `ActorSelection` - отправка сообщений акторам верхнего уровня с хорошо известными именами.

Представьте например, что у вас есть актор верхнего уровня, который отвечает за всю аутентификацию в вашей системе. Другие акторы должны отправлять ему сообщения для того, чтобы узнать, прошел ли пользователь аутентификацию, и есть ли у него права на выполнение данной операции. Пусть это будет `AuthenticationActor`.

Поскольку это актор верхнего уровня, мы знаем, что его место в иерархии будет `/user/AuthenticationActor`. Используя этот адрес, **ЛЮБОЙ** актор в системе может легко отправлять собщения, без необходимости хранить `IActorRef`. Например вот так:

```csharp
// посылаем имя пользователя актору AuthenticationActor для проведения аутентификации
// Это абсолютный путь к актору, поскольку он начинается с  /user/
Context.ActorSelection("akka://MyActorSystem/user/AuthenticationActor").Tell(username);
```

> Замечание: `ActorSelection` может быть абсолютным либо относительным. Абсолютный `ActorSelection` включает в себя корневого актора,  `/user/` .  Однако, есть и относительные пути. например `Context.ActorSelection("../validationActor")`.

#### Обработка больших потоков данных
Одним из очевидных расширений модели акторов являются акторы-роутеры ([docs](http://getakka.net/docs/Routing)). С акторами-роутетрами мы познакомимся во втором блоке, но небольшой пример на пальцах рассмотрим прямо сейчас.

Предположим, что ваша система должна обрабатвать большой поток данных в реальном времени. Также предположим, что у вас очень популярная система, которая переживает пиковые нагрузки дважды в день . Как например какое-нибудь приложение для соцсетей. Какждый польователь генерирует поток активностей, который надо обработать и отобразить в реальном времени. А еще мы предположим что за активность каждого пользователя у вас отвечает отдельный актор, который перенаправляет эту активность в другие части системы.
 
 (*Запомните, акторы дешевые! Создавать по одному актору на каждого пользователя может быть вполне адекватно. Когда мы проверяли последний раз, Akka.NET исползовала около 1 ГБ памяти на 2.5-3 миллиона акторов.*)

Итак, к вам приходит большой объем данных, и вы хотите быть уверены, что система остается отзывчивой под высокими нагрузками. Одним из вариантов решения может быть создание роутера (координатора заданий), который будут контролировать пул акторов, выполняющих обработку. Этот пул может динамически расширяться\сжиматься в зависимости от потребностей системы в вычислительных мощностях.

Поскольку каждый актор, привязанные к пользователю создается и уничтожается при входе\выходе пользователя из системы, как вы можете гарантировать, что данные попадут куда надо ? Вы просто посылаете необходимые данные роутерам, которые отвечают за создание\уничтожение акторов. Роутеры доступны через `ActorSelection`, поэтому динамическое создание\уничтожение рабочих акторов проходит для вас незаметно.

#### Когда просто отправить ответ недостаточно
Одним из наиболее исопльзуемых `IActorRef`-ов является свойво `Sender`(отправитель) у актора. Это свойство доступно в контексте каждого актора, и содержит отправителя текущего обрабатываемого сообщения. В предыдущих уровках мы использовали его внутри метода `OnReceive` актора  `FileValidatorActor`, чтобы отправить результат валидации файла отправителю. 

Но что делать, если во время обработки сообщения нам необходимо отправить сообщение другому актору, который не является отправителем текущего сообщения? Использовать `ActorSelection`, естественно.

#### Ответ нескольким акторам одновременно
Другим популярным сценарием может быть отправка сообщения сразу нескольким акторам. Например, у вас есть группа сервисов, отвечающих за сбор статистки. Используя `ActorSelection` вы можете послать сообщение одновременно всем акторам, используя звездочку в  `ActorSelection`. 

### Внимание: Не передавайте `ActorSelection` как ссылку.
Мы настойчиво НЕ СОВЕТУЕМ передавать `ActorSelection` как параметр. Потому что путь может быть не только абсолютным, но и относительным. А  относительный путь может добавить много сюрпризов, если будет расчитываться от неверного места в иерархии.

### Как создать `ActorSelection`?
Очень просто: `var selection = Context.ActorSelection("/path/to/actorName")`

> ВНИМАНИЕ: **путь к актору должен содержать имя, которое вы дали ему при создании. Если вы не укажете имя актора при создании, система сама сгенерирует для него уникальное имя.**. Например:

```csharp
class FooActor : UntypedActor {}
Props props = Props.Create<FooActor>();

//  ActorPath для myFooActor будет "/user/barBazActor"
// А НЕ "/user/myFooActor" или "/user/FooActor"
IActorRef myFooActor = MyActorSystem.ActorOf(props, "barBazActor");

// Если вы не указываете имя актора при создании, 
// система сгенерирует уникальное имя
// и путь к актору будет вроде такого "/user/$a"
IActorRef myFooActor = MyActorSystem.ActorOf(props);
```

### Отсылка сообщения через `ActorSelection` чем-то отличается от отсылки через `IActorRef`?
Неа. Вы просто вызываете  `Tell()` в `ActorSelection`, точно так же как и в  `IActorRef`:

```csharp
var selection = Context.ActorSelection("/path/to/actorName");
selection.Tell(message);
```

## Упражнение
Ну что ж, начнем. Это будет короткое и простое упражнение. Добавим в нашу системы несколько оптимизаций.

### Фаза 1: Уберем связность между `ConsoleReaderActor` и `FileValidatorActor`
Сейчас нашему актору `ConsoleReaderActor` нужна ссылка `IActorRef`, чтобы посылать сообщения, который он прочитал на валидацию. Сейчас это не сильно сложно.

НО представьте, что `ConsoleReaderActor` находится далеко от `FileValidatorActor` в рамках иерархии. В этом случае передать ссылку  без использования всех промежуточных звеньев будет довольно непросто. 

Без  `ActorSelection`,  вам бы пришлось передавать `IActorRef` через каждый объект, который находится по пути. Это бы превратило бы ваш код в огромную тарелку сильносвязанных спагетти. --**фи**!

Let's fix that by **removing the `validationActor` `IActorRef` that we're passing in**. The top of `ConsoleReaderActor` should now look like this:

```csharp
// ConsoleReaderActor.cs
// note: we don't even need our own constructor anymore!
public const string StartCommand = "start";
public const string ExitCommand = "exit";

protected override void OnReceive(object message)
{
    if (message.Equals(StartCommand))
    {
        DoPrintInstructions();
    }

    GetAndValidateInput();
}
```

Then, let's update the call for message validation inside `ConsoleReaderActor` so that the actor doesn't have to hold onto a specific `IActorRef` and can just forward the message read from the console onto an `ActorPath` where it knows validation occurs.

```csharp
// ConsoleReaderActor.GetAndValidateInput

// otherwise, just send the message off for validation
Context.ActorSelection("akka://MyActorSystem/user/validationActor").Tell(message);
```

Finally, let's update `consoleReaderProps` accordingly in `Program.cs` since its constructor no longer takes any arguments:
```csharp
// Program.Main
Props consoleReaderProps = Props.Create<ConsoleReaderActor>();
```

### Phase 2: Decouple `FileValidatorActor` and `TailCoordinatorActor`
Just as with `ConsoleReaderActor` and `FileValidatorActor`, the `FileValidatorActor` currently requires an `IActorRef` for the `TailCoordinatorActor` which it does not need. Let's fix that.

First, **remove the `tailCoordinatorActor` argument to the constructor of `FileValidatorActor` and remove the accompanying field on the class**. The top of `FileValidatorActor.cs` should now look like this:

```csharp
// FileValidatorActor.cs
// note that we're no longer storing _tailCoordinatorActor field
private readonly IActorRef _consoleWriterActor;

public FileValidatorActor(IActorRef consoleWriterActor)
{
    _consoleWriterActor = consoleWriterActor;
}
```

Then, let's use `ActorSelection` to communicate between `FileValidatorActor` and `TailCoordinatorActor`! Update `FileValidatorActor` like this:
```csharp
// FileValidatorActor.cs
// start coordinator
Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
```

And finally, let's update `fileValidatorProps` in `Program.cs` to reflect the different constructor arguments:

```csharp
// Program.Main
Props fileValidatorActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor));
```

### Phase 3: Build and Run!
Awesome! It's time to fire this baby up and see it in action.

Just as with the last lesson, you should be able to hit `F5` and run your log/text file and see additions to it appear in your console.

![Petabridge Akka.NET Bootcamp Actor Selection Working](Images/selection_working.png)

### Hey, wait, go back! What about that `consoleWriterActor` passed to `FileValidatorActor`? Wasn't that unnecessarily coupling actors?
Oh. You're good, you.

We assume you're talking about this `IActorRef` that is still getting passed into `FileValidatorActor`:

```csharp
// FileValidatorActor.cs
private readonly IActorRef _consoleWriterActor;

public FileValidatorActor(IActorRef consoleWriterActor)
{
    _consoleWriterActor = consoleWriterActor;
}
```

*This one is a little counter-intuitive*. Here's the deal.

In this case, we aren't using the handle for `consoleWriterActor` to talk directly to it. Instead we are putting that `IActorRef` inside a message that is getting sent somewhere else in the system for processing. When that message is received, the receiving actor will know everything it needs to in order to do its job.

This is actually a good design pattern in the actor model, because it makes the message being passed entirely self-contained and keeps the system as a whole flexible, even if this one actor (`FileValidatorActor`) needs an `IActorRef` passed in and is a little coupled.

Think about what is happening in the `TailCoordinatorActor` which is receiving this message: the job of the `TailCoordinatorActor` is to manage `TailActor`s which will actually observe and report file changes to... somewhere. We get to specify that somewhere up front.

`TailActor` should not have the reporting output location written directly into it. The reporting output location is a task-level detail that should be encapsulated as an instruction within the incoming message. In this case, that task is our custom `StartTail` message, which indeed contains the `IActorRef` for the previously mentioned `consoleWriterActor` as the `reporterActor`.

So, a little counter-intuitively, this pattern actually promotes loose coupling. You'll see it a lot as you go through Akka.NET, especially given the widespread use of the pattern of turning events into messages.

### Once you're done
Compare your code to the solution in the [Completed](Completed/) folder to see what the instructors included in their samples.

## Great job! Almost Done! Onto Lesson 6!
Awesome work! Well done on completing this lesson! We're on the home stretch of Unit 1, and you're doing awesome.


**Let's move onto [Lesson 6 - The Actor Lifecycle](../lesson6).**


## Any questions?
**Don't be afraid to ask questions** :).

Come ask any questions you have, big or small, [in this ongoing Bootcamp chat with the Petabridge & Akka.NET teams](https://gitter.im/petabridge/akka-bootcamp).

### Problems with the code?
If there is a problem with the code running, or something else that needs to be fixed in this lesson, please [create an issue](https://github.com/petabridge/akka-bootcamp/issues) and we'll get right on it. This will benefit everyone going through Bootcamp.
