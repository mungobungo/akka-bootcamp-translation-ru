# Урок 1.1: Акторы и `ActorSystem`

С почином! Добро пожаловать на урок №1.

На этом уроке вы сможете создать ваших первых акторов и познакомитесь с основами [Akka.NET](http://getakka.net/).

## Ключевые идеи / общая информация

Во время этого урока вы получите практический опыт работы с акторами, в консопльном приложение. Для этого вы создадине систему акторов и несолько акторов в рамках этой системы. 

Мы напишем двух акторов, один з которых будет читать из консоли, а другой писать в консоль после небольшой обработки данных.

### Что такое актор?

"Актор" это просто аналог человека, выполняющего какую-то роль в системе. Этот человек является сущностью, объектом, и может проводить какие-то действия и общаться с другими людьми.

> Мы предполагаем, что вам знакомы идеи объектно ориентированного программирования (ООП).  Модель акторов очень похожа на объектно ориентированную модель - в рамках ООП все является объектами, в рамках модели акторов ***все является актором***.

Повторяйте про себя: все является актором. Все является актором. Все является актором! Представьте вашу систему как иерархию людей, в которой задачи разбиваются на части и делегируются до тех пор, пока не станут достаточно маленьким для того, чтобы быть качественно выоплненными даже одним актором.


На данный момент мы подоздерваем что вы думаете примерно так : в рамках ООП вы стараетесь дать каждому объекту простую и четко определенную цель существования. В принципе, система акторов ничем не отличается, за исключением того, что четкая цель теперь в зоне ответственности актором.

**Дополнительный материал: [Что такое актор в рамках Akka.NET](http://petabridge.com/blog/akkadotnet-what-is-an-actor/)?**

### How do actors communicate?
### Как акторы общаются?

Акторы общаются друг с другом точно так же, как и люди - обмениваясь сообщения. А сообщения - это старые добрые классы C#.


```csharp
//это сообщение !
public class SomeMessage{
	public int SomeValue {get; set}
}
```

Более детально мы разберем сообщения на следующем уроке, не волнуйтесь на этот счет. Все что вам надо сейчас знать, это то, что сообщения можно отправить другому актору при помощи метода `Tell()`

```csharp
//посылаем сообщение другому актору
someActorRef.Tell("и это тоже сообщение!");
```

### Что акторы могут делать?

Все что вы запрограммируюете. Серьезно :)

Вы создаете акторов, заставляете их обрабатывать сообщения, которые они получают, и акторы могут сделать все что угодно для того, тчобы обработать сообщение. Делать запросы к базе данных, писать в файл, изменять внутренние переменные, или любые другие вещи, которые могут вам понадобиться.

Помимо обработки сообщений, актор может:

1. Создавать других акторов

1. Посылать сообщения другим акторам (например отправителю текущего сообщения, используя свойство `Sender`)

1. Менять свое поведение и обрабатывать следующее полученное сообщение по-другому

Акторы по своей природе асинхронны(более подробно об этом на следующем уроке). И в [Модели акторов](https://en.wikipedia.org/wiki/Actor_model) не указано, какие из приведенных пунктов обязательны для актора, точно так же как не указан порядок, в котором эти действия следует выполнять. Все в ваших руках.

### Какие существуют типы акторов?

Все типы акторов наследуются от `UntypedActor`, но это пока не очень важно. Детальный разбор разных акторов будет чуть позже.

В блоке 1 все ваши акторы будут наследниками [`UntypedActor`](http://getakka.net/docs/Working%20with%20actors#untypedactor-api "Akka.NET - UntypedActor API").

### Как можно создать актора?

Для создания акотора необходимо знать две основные вещи:

1. Все аткоры создаются в рамках определенного контекста. 



1. При создании актора необходимо тажке создать `Props`. `Props` это объект, который инкапсулирует формулу создания определенного актора.

We'll be going into `Props` in depth in lesson 3, so for now don't worry about it much. We've provided the `Props` for you in the code, so you just have to figure out how to use `Props` to make an actor.

The hint we'll give you is that your first actors will be created within the context of your actor system itself. See the exercise instructions for more.

### What is an `ActorSystem`?
An `ActorSystem` is a reference to the underlying system and Akka.NET framework. All actors live within the context of this actor system. You'll need to create your first actors from the context of this `ActorSystem`.

By the way, the `ActorSystem` is a heavy object: create only one per application.

Aaaaaaand... go! That's enough conceptual stuff for now, so dive right in and make your first actors.

## Exercise
Let's dive in!

> Note: Within the sample code there are sections clearly marked `"YOU NEED TO FILL IN HERE"` - find those regions of code and begin filling them in with the appropriate functionality in order to complete your goals.

### Launch the fill-in-the-blank sample
Go to the [DoThis](../DoThis/) folder and open [WinTail](../DoThis/WinTail.sln) in Visual Studio. The solution consists of a simple console application and only one Visual Studio project file.

You will use this solution file through all of Unit 1.

### Install the latest Akka.NET NuGet package
In the Package Manager Console, type the following command:

```
Install-Package Akka
```

This will install the latest Akka.NET binaries, which you will need in order to compile this sample.

Then you'll need to add the `using` namespace to the top of `Program.cs`:


```csharp
// in Program.cs
using Akka.Actor;
```

### Make your first `ActorSystem`
Go to `Program.cs` and add this to create your first actor system:

```csharp
MyActorSystem = ActorSystem.Create("MyActorSystem");
```
>
> **NOTE:** When creating `Props`, `ActorSystem`, or `ActorRef` you will very rarely see the `new` keyword. These objects must be created through the factory methods built into Akka.NET. If you're using `new` you might be making a mistake.

### Make ConsoleReaderActor & ConsoleWriterActor
The actor classes themselves are already defined, but you will have to make your first actors.

Again, in `Program.cs`, add this just below where you made your `ActorSystem`:

```csharp
var consoleWriterActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));
```

We will get into the details of `Props` and `ActorRef`s in lesson 3, so don't worry about them much for now. Just know that this is how you make an actor.

### Have ConsoleReaderActor Send a Message to ConsoleWriterActor
Time to put your first actors to work!

You will need to do the following:

1. ConsoleReaderActor is set up to read from the console. Have it send a message to ConsoleWriterActor containing the content that it just read.

	```csharp
	// in ConsoleReaderActor.cs
	_consoleWriterActor.Tell(read);
	```

2. Have ConsoleReaderActor send a message to itself after sending a message to ConsoleWriterActor. This is what keeps the read loop going.

	```csharp
	// in ConsoleReaderActor.cs
	Self.Tell("continue");
	```
3. Send an initial message to ConsoleReaderActor in order to get it to start reading from the console.

	```csharp
	// in Program.cs
	consoleReaderActor.Tell("start");
	```

### Step 5: Build and Run!
Once you've made your edits, press `F5` to compile and run the sample in Visual Studio.

You should see something like this, when it is working correctly:
![Petabridge Akka.NET Bootcamp Lesson 1.1 Correct Output](Images/example.png)


### Once you're done
Compare your code to the code in the [Completed](Completed/) folder to see what the instructors included in their samples.

## Great job! Onto Lesson 2!
Awesome work! Well done on completing your first lesson.

**Let's move onto [Lesson 2 - Defining and Handling Different Types of Messages](../lesson2).**

## Any questions?
**Don't be afraid to ask questions** :).

Come ask any questions you have, big or small, [in this ongoing Bootcamp chat with the Petabridge & Akka.NET teams](https://gitter.im/petabridge/akka-bootcamp).

### Problems with the code?
If there is a problem with the code running, or something else that needs to be fixed in this lesson, please [create an issue](https://github.com/petabridge/akka-bootcamp/issues) and we'll get right on it. This will benefit everyone going through Bootcamp.
